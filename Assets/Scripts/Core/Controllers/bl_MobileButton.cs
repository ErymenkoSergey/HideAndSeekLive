using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class bl_MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string ButtonName;
    public int WeaponId;
    public KeyCode fallBackKey = KeyCode.None;
    public bool blockTouchPad = false;
    [SerializeField] private OnClick onClick;

    [Header("Transition")]
    public Graphic[] buttonGraphics;
    public Transform animatedChild;
    public Color normalColor = Color.white;
    public Color touchColor = Color.white;
    public AnimationCurve animationCurve;
    [Range(0.1f, 2)] public float transitionDuration = 0.25f;

    public ButtonState buttonState { get; private set; } = ButtonState.Idle;
    [Serializable] public class OnClick : UnityEvent { }
    private bool hasDispatchClick = false;
    private bool hasDispatchUp = false;
    private bool isRegistered = false;

    private void Awake()
    {
        Registre();
    }

    private void OnEnable()
    {
        //also try in OnEnable cuz in some Unity versions OnEnable calls before Awake :/
        Registre();
    }

    private void Registre()
    {
        if (!isRegistered)
        {
            foreach (var item in buttonGraphics)
            {
                //item.color = Color.white;
                item.canvasRenderer.SetColor(normalColor);
            }

            if (string.IsNullOrEmpty(ButtonName)) return;
            bl_MobileInput.AddMobileButton(this);
            isRegistered = true;
        }
    }

    private void OnDestroy()
    {
        if (isRegistered)
        {
            bl_MobileInput.RemoveMobileButton(this);
        }
    }

    //public bool isButton(int gun)
    //{
    //    Debug.Log($"isButtonisButton 0 {gun}, WeaponId {WeaponId}");

    //    if (gun == WeaponId)
    //    {
    //        Debug.Log($"isButtonisButton 1 {gun}, WeaponId {WeaponId}");
    //        if (buttonState == ButtonState.Down)
    //        {
    //            Debug.Log($"isButtonisButton 2 {gun}, WeaponId {WeaponId}");
    //            //bl_MobileInput.GetButton(fallBackKey , GameInputType.Down);
    //            Input.GetKeyDown(fallBackKey);
    //            return true;
    //        }
    //        else
    //            return false;
    //    }
    //    else
    //        return false;

    //    //return buttonState == ButtonState.Down || buttonState == ButtonState.Click;


    //    //if (buttonState == ButtonState.Idle || buttonState == ButtonState.Up)
    //    //    return false;



    //}

    public bool isButtonDown()
    {
        if (buttonState == ButtonState.Idle || buttonState == ButtonState.Up)
            return false;

        hasDispatchUp = false;

        if (buttonState == ButtonState.Down) //туту было
            return true;

        if (hasDispatchClick)
        {
            buttonState = ButtonState.Down;
            hasDispatchClick = false;
            return false;
        }
        else
        {
            hasDispatchClick = true;
            return true;
        }

        //if (buttonState == ButtonState.Down)
        //    return true;
    }

    public bool isButtonUp()
    {
        if (buttonState == ButtonState.Idle || buttonState != ButtonState.Up) return false;
        hasDispatchClick = false;
        if (hasDispatchUp) { buttonState = ButtonState.Idle; hasDispatchUp = false; return false; }
        hasDispatchUp = true;
        return true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //buttonState = ButtonState.Click;
        buttonState = ButtonState.Down;
        onClick?.Invoke();

        for (int i = 0; i < buttonGraphics.Length; i++)
        {
            buttonGraphics[i]?.CrossFadeColor(touchColor, transitionDuration, true, true);
        }
        
        if (blockTouchPad && !bl_MobileInput.ignoredTouches.Contains(eventData.pointerId))
        {
            bl_MobileInput.ignoredTouches.Add(eventData.pointerId);
        }

        if (animatedChild != null) 
        {
            StopAllCoroutines();
            StartCoroutine(DoAnimation(true)); 
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonState = ButtonState.Up;

        for (int i = 0; i < buttonGraphics.Length; i++)
        {
            buttonGraphics[i]?.CrossFadeColor(normalColor, transitionDuration, true, true);
        }

        if (blockTouchPad && bl_MobileInput.ignoredTouches.Contains(eventData.pointerId))
        {
            bl_MobileInput.ignoredTouches.Remove(eventData.pointerId);
        }

        if (animatedChild != null)
        { 
            StopAllCoroutines(); 
            StartCoroutine(DoAnimation(false));
        }
    }

    public void AddOnClickListener(UnityAction callback)
    {
        onClick.AddListener(callback);
    }

    public void RemoveOnClickListener(UnityAction callback)
    {
        onClick.RemoveListener(callback);
    }

    private IEnumerator DoAnimation(bool forward)
    {
        float d = 0;
        while (d < 1)
        {
            d += Time.deltaTime / transitionDuration;
            if (forward)
            {
                animatedChild.localScale = Vector3.one * animationCurve.Evaluate(d);
            }
            else
            {
                animatedChild.localScale = Vector3.one * animationCurve.Evaluate(1 - d);
            }
            yield return null;
        }
    }

//#if UNITY_EDITOR
//    void OnValidate()
//    {
//        if (buttonGraphics == null) return;
//        if (buttonGraphics.Length == 0)
//        {
//            Graphic g = GetComponent<Graphic>();
//            if (g != null) { buttonGraphics = new Graphic[1]; buttonGraphics[0] = g; }
//            return;
//        }
//        foreach (var item in buttonGraphics)
//        {
//            item.canvasRenderer.SetColor(normalColor);
//        }
//    }
//#endif

    [Serializable]
    public enum ButtonState
    {
        Idle,
        Click,
        Down,
        Up,
    }
}
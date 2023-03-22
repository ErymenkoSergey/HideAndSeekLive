using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class bl_MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string ButtonName;
    public KeyCode fallBackKey = KeyCode.None;
    public bool blockTouchPad = false;
    [SerializeField] public OnClick onClick;

    [Header("Transition")]
    public Graphic[] buttonGraphics;
    public Transform animatedChild;
    public Color normalColor = Color.white;
    public Color touchColor = Color.white;
    public AnimationCurve animationCurve;
    [Range(0.1f, 2)] public float transitionDuration = 0.25f;

    public ButtonState buttonState { get; private set; } = ButtonState.Idle;
    [Serializable]public class OnClick : UnityEvent { }
    private bool hasDispatchClick = false;
    private bool hasDispatchUp = false;
    private bool isRegistered = false;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        Registre();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        //also try in OnEnable cuz in some Unity versions OnEnable calls before Awake :/
        Registre();
    }

    /// <summary>
    /// 
    /// </summary>
    void Registre()
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

    /// <summary>
    /// 
    /// </summary>
    void OnDestroy()
    {
        if (isRegistered)
        {
            bl_MobileInput.RemoveMobileButton(this);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool isButton()
    {
        return buttonState == ButtonState.Down || buttonState == ButtonState.Click;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool isButtonDown()
    {
        Debug.Log($"isButtonDown 0 {ButtonName}");
        if (buttonState == ButtonState.Idle || buttonState == ButtonState.Up)
            return false;
        Debug.Log($"isButtonDown 1 {ButtonName}");
        hasDispatchUp = false;

        if (buttonState == ButtonState.Down)
            return false;
        Debug.Log($"isButtonDown 2 {ButtonName}");
        if (hasDispatchClick)
        {
            Debug.Log($"isButtonDown 3 {ButtonName}");
            buttonState = ButtonState.Down;
            hasDispatchClick = false;
            return false;
        }
        else
        {
            Debug.Log($"isButtonDown 4 {ButtonName}");
            hasDispatchClick = true;
            return true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool isButtonUp()
    {
        Debug.Log($"isButtonDown 5 {ButtonName}");
        if (buttonState == ButtonState.Idle || buttonState != ButtonState.Up) return false;
        hasDispatchClick = false;
        if (hasDispatchUp) { buttonState = ButtonState.Idle; hasDispatchUp = false; return false; }
        hasDispatchUp = true;
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPointerDown(PointerEventData eventData) // нажатия теперь работают
    {
        Debug.Log($"OnPointerDown 0 ButtonName {ButtonName}");
        buttonState = ButtonState.Click;
        onClick?.Invoke();
        for (int i = 0; i < buttonGraphics.Length; i++)
        {
            buttonGraphics[i]?.CrossFadeColor(touchColor, transitionDuration, true, true);
        }

        if (blockTouchPad && !bl_MobileInput.ignoredTouches.Contains(eventData.pointerId))
        {
            Debug.Log($"OnPointerDown 1 ButtonName {ButtonName}");
            bl_MobileInput.ignoredTouches.Add(eventData.pointerId);
        }
        if(animatedChild != null) { StopAllCoroutines();StartCoroutine(DoAnimation(true)); }
    }

    /// <summary>
    /// 
    /// </summary>
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

        if (animatedChild != null) { StopAllCoroutines(); StartCoroutine(DoAnimation(false)); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callback"></param>
    public void AddOnClickListener(UnityAction callback)
    {
        onClick.AddListener(callback);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callback"></param>
    public void RemoveOnClickListener(UnityAction callback)
    {
        onClick.RemoveListener(callback);
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator DoAnimation(bool forward)
    {
        float d = 0;
        while(d < 1)
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

#if UNITY_EDITOR
    void OnValidate()
    {
        if (buttonGraphics == null) return;
        if(buttonGraphics.Length == 0)
        {
            Graphic g = GetComponent<Graphic>();
            if(g != null) { buttonGraphics = new Graphic[1]; buttonGraphics[0] = g; }
            return;
        }
        foreach (var item in buttonGraphics)
        {
            item.canvasRenderer.SetColor(normalColor);
        }
    }
#endif

    [Serializable]
    public enum ButtonState
    {
        Idle,
        Click,
        Down,
        Up,
    }
}
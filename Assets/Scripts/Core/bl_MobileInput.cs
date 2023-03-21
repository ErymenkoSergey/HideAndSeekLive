using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Lovatto.MobileInput;
//using static bl_GameInput;
#if INPUT_MANAGER
using MFPS.InputManager;
#endif

public class bl_MobileInput
{
    // Set this to false to make the input unresponsive
    public static bool Interactable = true;
    public static List<int> ignoredTouches { get; private set; } = new List<int>();

    private static int m_Touch = -1;
    private static List<int> touchesList;
    private static Dictionary<string, bl_MobileButton> mobileButtons = new Dictionary<string, bl_MobileButton>();

    public static void Initialize()
    {
        touchesList = new List<int>();
        ignoredTouches = new List<int>();
        m_Touch = -1;
        Interactable = true;
    }

    public static void AddMobileButton(bl_MobileButton button)
    {
        if (mobileButtons.ContainsKey(button.ButtonName))
        {
            Debug.LogWarning($"A button with the name '{button.ButtonName}' is already registered, buttons with the same name are not allowed.");
            return;
        }
        Debug.Log($"bl_MobileInput AddMobileButton button {button.ButtonName}");
        mobileButtons.Add(button.ButtonName, button);
    }

    public static void RemoveMobileButton(bl_MobileButton button)
    {
        if (!mobileButtons.ContainsKey(button.ButtonName)) { return; }

        mobileButtons.Remove(button.ButtonName);
    }

    public static bl_MobileButton Button(string buttonName)
    {
        if (!mobileButtons.ContainsKey(buttonName))
        { /*Debug.LogWarning($"The button '{buttonName}' is not registered in the mobile input buttons.");*/
            return null;
        }
        return mobileButtons[buttonName];
    }

    /// <summary>
    /// is the button pressed
    /// </summary>
    /// <param name="buttonName"></param>
    /// <returns></returns>
    public static bool GetButton(string buttonName)
    {
        if (!Interactable)
            return false;

        if (!mobileButtons.ContainsKey(buttonName))
        {
            Debug.LogWarning($"The button '{buttonName}' is not registered in the mobile input buttons."); return false;
        }
        Debug.Log($"MOBILE GetButton {buttonName}");
#if UNITY_EDITOR
        if (bl_MobileInputSettings.Instance.UseKeyboardOnEditor)
        {
            return Input.GetKey(mobileButtons[buttonName].fallBackKey);
        }
#endif
        return mobileButtons[buttonName].isButton();
    }

    public static bool GetButtonDown(string buttonName)
    {
        if (!Interactable)
            return false;

        if (!mobileButtons.ContainsKey(buttonName)) { Debug.LogWarning($"The button '{buttonName}' is not registered in the mobile input buttons."); return false; }
#if UNITY_EDITOR
        if (bl_MobileInputSettings.Instance.UseKeyboardOnEditor)
        {
            return Input.GetKeyDown(mobileButtons[buttonName].fallBackKey);
        }
#endif
        Debug.Log($"MOBILE GetButtonDown {buttonName}");
        ActionButton(buttonName);

        return mobileButtons[buttonName].isButtonDown();
    }

    private static void ActionButton(string name)
    {
        Debug.Log($"ActionButton {name}");

        if (name == "Fire") Fire();
        if (name == "Aim") Aim();
        if (name == "Crouch") Crouch();
        if (name == "Jump") Jump();

        if (name == "Interact") Interact();
        if (name == "Reload") Reload();
        if (name == "WeaponSlot") WeaponSlot(1);
        if (name == "QuickMelee") QuickMelee();

        if (name == "QuickNade") QuickNade();
        if (name == "Pause") Pause();
        if (name == "Scoreboard") Scoreboard();
        if (name == "SwitchFireMode") SwitchFireMode();

        if (name == "GeneralChat") GeneralChat();
        if (name == "TeamChat") TeamChat();
    }

    public static bool GetButtonUp(string buttonName)
    {
        if (!Interactable)
            return false;

        if (!mobileButtons.ContainsKey(buttonName)) { Debug.LogWarning($"The button '{buttonName}' is not registered in the mobile input buttons."); return false; }
#if UNITY_EDITOR
        if (bl_MobileInputSettings.Instance.UseKeyboardOnEditor)
        {
            return Input.GetKeyUp(mobileButtons[buttonName].fallBackKey);
        }
#endif
        Debug.Log($"MOBILE GetButtonDown {buttonName}");

        return mobileButtons[buttonName].isButtonUp();
    }

    /// <summary>
    /// Detect is the auto fire is triggered (lets say like if it's pressed)
    /// </summary>
    /// <returns></returns>
    public static bool AutoFireTriggered()
    {
        if (!Interactable)
            return false;

        if (bl_AutoFire.Instance == null)
            return false;

        return bl_AutoFire.Instance.isTriggered();
    }

    public static int GetUsableTouch()
    {
        if (Input.touches.Length <= 0)
        {
            m_Touch = -1;
            return m_Touch;
        }
        List<int> list = GetValuesFromTouches(Input.touches).Except<int>(ignoredTouches).ToList<int>();
        if (list.Count <= 0)
        {
            m_Touch = -1;
            return m_Touch;
        }
        if (!list.Contains(m_Touch))
        {
            m_Touch = list[0];
        }
        return m_Touch;
    }

    public static List<int> GetValuesFromTouches(Touch[] touches)
    {
        if (touchesList == null)
        {
            touchesList = new List<int>();
        }
        else
        {
            touchesList.Clear();
        }
        for (int i = 0; i < touches.Length; i++)
        {
            touchesList.Add(touches[i].fingerId);
        }
        return touchesList;
    }

    public static float TouchPadSensitivity { get => bl_MobileInputSettings.Instance.touchPadHorizontalSensitivity; set => bl_MobileInputSettings.Instance.touchPadHorizontalSensitivity = value; }

    public static bool EnableAutoFire { get => bl_MobileInputSettings.Instance.useAutoFire; set => bl_MobileInputSettings.Instance.useAutoFire = value; }

    //bl_GameInput 
    public static bool Fire(GameInputType inputType = GameInputType.Hold)
    {
#if INPUT_MANAGER
        if(inputType == GameInputType.Down)return bl_Input.isButton("SingleFire");
        else return GetInputManager("Fire", inputType);
#else
        return GetButton(KeyCode.Mouse0, inputType);
#endif
    }

    public static bool Run(GameInputType inputType = GameInputType.Hold)
    {
#if INPUT_MANAGER
        if (bl_InputData.Instance.runWithButton)
            return GetInputManager("Run", inputType);
        else
            return Input.GetAxis("Vertical") >= 1f;
#else
        return GetButton(KeyCode.LeftShift, inputType);
#endif
    }

    public static bool Aim(GameInputType inputType = GameInputType.Hold)
    {
#if INPUT_MANAGER
        return GetInputManager("Aim", inputType);
#else
        return GetButton(KeyCode.Mouse1, inputType);
#endif
    }

    public static bool Crouch(GameInputType inputType = GameInputType.Hold)
    {
#if INPUT_MANAGER
        return GetInputManager("Crouch", inputType);
#else
        return GetButton(KeyCode.C, inputType);
#endif
    }

    public static bool Jump(GameInputType inputType = GameInputType.Down)
    {
#if INPUT_MANAGER
        return GetInputManager("Jump", inputType);
#else
        return GetButton(KeyCode.Space, inputType);
#endif
    }

    public static bool Interact(GameInputType inputType = GameInputType.Down)
    {
#if INPUT_MANAGER
        return GetInputManager("Interact", inputType);
#else
        return GetButton(KeyCode.F, inputType);
#endif
    }

    public static bool Reload(GameInputType inputType = GameInputType.Down)
    {
#if INPUT_MANAGER
        return GetInputManager("Reload", inputType);
#else
        return GetButton(KeyCode.R, inputType);
#endif
    }

    public static bool WeaponSlot(int slotID, GameInputType inputType = GameInputType.Down) // смена оружия 
    {
#if INPUT_MANAGER
        return GetInputManager($"Weapon{slotID}", inputType);
#else
        return GetButton($"{slotID}", inputType);
#endif
    }

    public static bool QuickMelee(GameInputType inputType = GameInputType.Down)
    {
#if INPUT_MANAGER
        return GetInputManager("FastKnife", inputType);
#else
        return GetButton(KeyCode.V, inputType);
#endif
    }

    public static bool QuickNade(GameInputType inputType = GameInputType.Down)
    {
#if INPUT_MANAGER
        return GetInputManager("QuickNade", inputType);
#else
        return GetButton(KeyCode.G, inputType);
#endif
    }

    public static bool Pause(GameInputType inputType = GameInputType.Down)
    {
        //Debug.LogError($"Pause Pause ");
#if INPUT_MANAGER
        if (bl_Input.isGamePad)
        {
            return bl_Input.isStartPad;
        }
#endif
        return GetButton(KeyCode.Escape, inputType);
    }

    public static bool Scoreboard(GameInputType inputType = GameInputType.Down)
    {
#if INPUT_MANAGER
        return GetInputManager("Scoreboard", inputType);
#else
        return GetButton(KeyCode.Tab, inputType);
#endif
    }

    public static bool SwitchFireMode(GameInputType inputType = GameInputType.Down)
    {
#if INPUT_MANAGER
        return GetInputManager("FireType", inputType);
#else
        return GetButton(KeyCode.B, inputType);
#endif
    }

    public static bool GeneralChat(GameInputType inputType = GameInputType.Down)
    {
#if INPUT_MANAGER
        return GetInputManager("GeneralChat", inputType);
#else
        return GetButton(KeyCode.T, inputType);
#endif
    }

    public static bool TeamChat(GameInputType inputType = GameInputType.Down)
    {
#if INPUT_MANAGER
        return GetInputManager("TeamChat", inputType);
#else
        return GetButton(KeyCode.Y, inputType);
#endif
    }

    public static float MobileVertical;
    public static float MobileHorizontal;

    public static float MobileMouseX;
    public static float MobileMouseY;

    public static float Vertical
    {
        get
        {
            if (!bl_RoomMenu.Instance.isCursorLocked || bl_GameData.Instance.isChating) return 0;
#if !INPUT_MANAGER
            if (bl_GameData.Instance.MobileInput)
                return MobileVertical;
            else
                return Input.GetAxis("Vertical");
#else
            return bl_Input.VerticalAxis;
#endif
        }
    }

    public static float Horizontal
    {
        get
        {
            if (!bl_RoomMenu.Instance.isCursorLocked || bl_GameData.Instance.isChating) return 0;
#if !INPUT_MANAGER
            if (bl_GameData.Instance.MobileInput)
                return MobileHorizontal;
            else
                return Input.GetAxis("Horizontal");
#else
            return bl_Input.HorizontalAxis;
#endif
        }
    }

    public static float MouseX // мышка 
    {
        get
        {
            if (!bl_RoomMenu.Instance.isCursorLocked || bl_GameData.Instance.isChating) return 0;

            if (bl_GameData.Instance.MobileInput)
                return MobileMouseX;
            else
                return Input.GetAxis("Mouse X");
        }
    }

    public static float MouseY
    {
        get
        {
            if (!bl_RoomMenu.Instance.isCursorLocked || bl_GameData.Instance.isChating)
                return 0;
            if (bl_GameData.Instance.MobileInput)
                return MobileMouseY;
            else
                return Input.GetAxis("Mouse Y");
        }
    }

    public static bool GetButton(KeyCode key, GameInputType inputType)
    {
        if (!bl_RoomMenu.Instance.isCursorLocked || bl_GameData.Instance.isChating)
            return false;

        if (inputType == GameInputType.Hold) { return Input.GetKey(key); }
        else if (inputType == GameInputType.Down) { return Input.GetKeyDown(key); }
        else { return Input.GetKeyUp(key); }
    }

    public static bool GetButton(string key, GameInputType inputType)
    {
        if (!bl_RoomMenu.Instance.isCursorLocked || bl_GameData.Instance.isChating) return false;

        if (inputType == GameInputType.Hold) { return Input.GetKey(key); }
        else if (inputType == GameInputType.Down) { return Input.GetKeyDown(key); }
        else { return Input.GetKeyUp(key); }
    }

#if INPUT_MANAGER
    public static bool GetInputManager(string key, GameInputType inputType)
    {
    if (!bl_RoomMenu.Instance.isCursorLocked || bl_GameData.Instance.isChating) return false;
        if(inputType == GameInputType.Hold) { return bl_Input.isButton(key); }
        else if (inputType == GameInputType.Down) { return bl_Input.isButtonDown(key); }
        else { return bl_Input.isButtonUp(key); }
    }
#endif
}

public enum GameInputType
{
    Down,
    Up,
    Hold,
}
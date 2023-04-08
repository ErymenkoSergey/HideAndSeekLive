using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonMonoBehaviour : MonoBehaviour
{
    [Serializable]
    public enum ButtonState
    {
        Idle,
        Click,
        Down,
        Up,
    }
}

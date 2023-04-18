using System;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnimation : MonoBehaviour
{
    [SerializeField] private Animator animation;
    void Start()
    {
        animation.SetBool("Dance", true);
    }

}

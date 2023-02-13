using UnityEngine;

public class JumpBooster : MonoBehaviour
{
    [Range(0, 125)] public float JumpForce;
    [Range(-90, 90)] public float JumpAngle = 45;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(bl_PlayerSettings.LocalTag))
        {
            bl_FirstPersonController fpc = other.GetComponent<bl_FirstPersonController>();
            fpc.PlatformJump(JumpForce, JumpAngle);
        }
    }
}

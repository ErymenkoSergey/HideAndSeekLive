using System.Collections;
using UnityEngine;

public class InteractivePlace : MonoBehaviour
{
    private BoxCollider _placeInteraction;
    [SerializeField] private AudioClip _teleportSound;
    private bool _isTeleporting;
    private Transform _spawnPoint;

    public void SetNewPlace(Transform transform)
    {
        Debug.Log($"OnTriggerStay SetNewPlace 0 ");
        _spawnPoint = transform;
        _isTeleporting = true;
        StartCoroutine(Rollback());
    }

    private IEnumerator Rollback()
    {
        yield return new WaitForSeconds(1);
        _isTeleporting = false;
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"OnTriggerStay {other.gameObject.name} 1 ");
        if (!_isTeleporting)
            return;

        Debug.Log($"OnTriggerStay {other.gameObject.name} 2 ");
        if (other.CompareTag(bl_PlayerSettings.RemoteTag)) // bl_PlayerSettings - этот есть 
        {
            bl_PlayerNetwork fpc = other.GetComponent<bl_PlayerNetwork>(); // он выключен вообще 
            Debug.Log($"OnTriggerStay {other.gameObject.name} 3 ");
            fpc.SetNewPosition(_spawnPoint);

            if (_teleportSound != null)
                AudioSource.PlayClipAtPoint(_teleportSound, transform.position);
        }

        //if (other.CompareTag(bl_PlayerSettings.LocalTag))
        //{
        //    bl_FirstPersonController fpc = other.GetComponent<bl_FirstPersonController>();
        //    Debug.Log($"OnTriggerStay {other.gameObject.name} 3.1 ");
        //    fpc.SetPosition(_spawnPoint);

        //    if (_teleportSound != null)
        //        AudioSource.PlayClipAtPoint(_teleportSound, transform.position);
        //}
    }
}

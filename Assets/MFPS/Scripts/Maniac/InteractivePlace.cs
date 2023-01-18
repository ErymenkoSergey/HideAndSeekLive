using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractivePlace : MonoBehaviour
{
    private BoxCollider _placeInteraction;
    [SerializeField] private AudioClip _teleportSound;
    private bool _isTeleporting;
    private Transform _spawnPoint;

    public void SetNewPlace(Transform transform)
    {
        _spawnPoint = transform;
        _isTeleporting = true;
        StartCoroutine(Rollback());
    }

    private IEnumerator Rollback()
    {
        yield return new WaitForSeconds(1);
        _isTeleporting = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(bl_PlayerSettings.LocalTag))
        {
            bl_FirstPersonController fpc = other.GetComponent<bl_FirstPersonController>();

            fpc.SetPosition(_spawnPoint);

            if (_teleportSound != null)
                AudioSource.PlayClipAtPoint(_teleportSound, _spawnPoint.position);
        }
    }
}

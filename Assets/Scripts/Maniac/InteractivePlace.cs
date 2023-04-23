using System.Collections;
using UnityEngine;

public class InteractivePlace : MonoBehaviour
{
    [SerializeField] private AudioClip _teleportSound;
    [SerializeField] private bool _isTeleporting;
    [SerializeField] private float _timeTeleport = 1f;
    private Transform _spawnPoint;

    public void SetNewPlace(Transform transform)
    {
        _spawnPoint = transform;
        _isTeleporting = true;
        StartCoroutine(Rollback());
    }

    private IEnumerator Rollback()
    {
        yield return new WaitForSeconds(_timeTeleport);
        _isTeleporting = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_isTeleporting)
            return;

        if (other.CompareTag(bl_PlayerSettings.LocalTag))
        {
            bl_FirstPersonController fpc = other.GetComponent<bl_FirstPersonController>();

            {
                fpc.SetPosition(_spawnPoint);
            }

            if (_teleportSound != null)
                AudioSource.PlayClipAtPoint(_teleportSound, transform.position);
        }
    }
}

using UnityEngine;

public class Teleport : MonoBehaviour
{
    [SerializeField] private bool _isRandomPoint;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private AudioClip _teleportSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(bl_PlayerSettings.LocalTag))
        {
            bl_FirstPersonController fpc = other.GetComponent<bl_FirstPersonController>();
            
            if (_isRandomPoint)
            {
                var randomPoint = Random.Range(0, _spawnPoints.Length);
                fpc.SetPosition(_spawnPoints[randomPoint]);
            }
            else
            {
                fpc.SetPosition(_spawnPoint);
            }

            if (_teleportSound != null) 
                AudioSource.PlayClipAtPoint(_teleportSound, transform.position);
        }
    }
}

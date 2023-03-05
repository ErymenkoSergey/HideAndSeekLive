using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CheckedManiac : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _clip;
    [SerializeField] private float _volume;
    [SerializeField] private UnityEvent _event;


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<bl_PlayerSettings>().PlayerTeam == Team.Maniac)
        {
            AudioSignal();
        }
        else
        {

        }
    }

    private void AudioSignal()
    {
        _audioSource.PlayOneShot(_clip, _volume);
        _event?.Invoke();
        StartCoroutine(Destroyng());
    }

    private IEnumerator Destroyng()
    {
        yield return new WaitForSeconds(_clip.length);
        Destroy(gameObject);
    }
}

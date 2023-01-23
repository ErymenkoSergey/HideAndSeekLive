using UnityEngine;

public class Interactive : bl_MonoBehaviour
{
    [SerializeField] private InteractivePlace _placeInteraction;
    [SerializeField] private Transform _pointSpawn;

    public void UseObject()
    {
        _placeInteraction.SetNewPlace(_pointSpawn);
    }
}

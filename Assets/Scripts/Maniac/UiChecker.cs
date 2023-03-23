using UnityEngine;

public class UiChecker : MonoBehaviour
{
    [SerializeField] private GameObject _mobileControler;
    [SerializeField] private GameObject[] _pcControlerWeapons;
    //[SerializeField] private GameObject _pcControlerBullets;

    private void Awake()
    {
        //Debug.Log($"Application {Application.platform}");
        ////CheckPlatform();

        SetControl(true);
    }

    //private void CheckPlatform()
    //{
    //    if (bl_GameData.Instance.MobileInput)
    //    {
    //        bl_GameData.Instance.MobileInput = true;
    //        SetControl(true);
    //    }
    //    else
    //    if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
    //    {
    //        bl_GameData.Instance.MobileInput = false;
    //        SetControl(false);
    //    }
    //}

    private void SetControl(bool isTach)
    {
        _mobileControler.SetActive(isTach);

        for (int i = 0; i < _pcControlerWeapons.Length; i++)
        {
            _pcControlerWeapons[i].SetActive(!isTach);
        }
    }
}

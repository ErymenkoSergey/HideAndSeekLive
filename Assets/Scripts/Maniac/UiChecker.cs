using System.Threading.Tasks;
using UnityEngine;

public class UiChecker : CommonMonoBehaviour
{
    [SerializeField] private GameObject _mobileControler;
    [SerializeField] private GameObject[] _pcControlerWeapons;
    [SerializeField] private GameObject[] _iconsHide;
    //[SerializeField] private GameObject _pcControlerBullets;

    private void Awake()
    {
        //Debug.Log($"Application {Application.platform}");
        ////CheckPlatform();

        SetControl(true);
        ClousedIcons();
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

    private async void ClousedIcons()
    {
        await Task.Delay(1000);

        for (int i = 0; i < _iconsHide.Length; i++)
        {
            _iconsHide[i].SetActive(false);
        }
    }
}

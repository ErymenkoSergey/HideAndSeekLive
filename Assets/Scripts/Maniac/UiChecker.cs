using System.Threading.Tasks;
using UnityEngine;

public class UiChecker : CommonMonoBehaviour
{
    [SerializeField] private GameObject _mobileControler;
    [SerializeField] private GameObject[] _pcControlerWeapons;
    [SerializeField] private GameObject[] _iconsHide;

    private bool _isMobileInput = false;

    private void Awake()
    {
        _isMobileInput = bl_GameData.Instance.MobileInput;
        CheckPlatform();
    }

    private void CheckPlatform()
    {
        if (_isMobileInput)
        {
            SetControl(true);
            ClousedIcons();
        }
        else
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            SetControl(false);
        }
    }

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
        await Task.Delay(100);

        for (int i = 0; i < _iconsHide.Length; i++)
        {
            _iconsHide[i].SetActive(false);
        }
    }
}

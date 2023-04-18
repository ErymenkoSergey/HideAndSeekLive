using UnityEngine;
using UnityEngine.UI;

public class bl_PlayerUIBank : MonoBehaviour
{
    [Header("REFERENCES")]
    public Canvas PlayerUICanvas;
    public GameObject KillZoneUI;
    public GameObject WeaponStatsUI;
    public GameObject playerStatsUI;
    public GameObject MaxKillsUI;
    public GameObject SpeakerIcon;
    public GameObject TimeUIRoot;
    public GameObject MaxKillsUIRoot;
    public Image PlayerStateIcon;
    public Image SniperScope;
    public Image HealthBar;

    [Space(10)]
    public Text AmmoText;
    public Text ClipText;
    public Text FireTypeText;
    [Space(10)]
    public Text HealthText;
    public Text TimeText;
    
    public CanvasGroup DamageAlpha;
    public bl_WeaponLoadoutUI LoadoutUI;
    public GameObject ModileLoadoutUI;
    public GameObject PcLoadoutUI;
    public Gradient AmmoTextColorGradient;

    private bool _isMobileInput = false;

    private void Awake()
    {
        UpdateUIDisplay();
    }

    public void UpdateUIDisplay()
    {
        _isMobileInput = bl_GameData.Instance.MobileInput;
        SetUILoadOut();

        TimeUIRoot.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.Time));
        WeaponStatsUI.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.WeaponData));
        playerStatsUI.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.PlayerStats));
        if (LoadoutUI != null) 
            LoadoutUI.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.Loadout));
        bl_EventHandler.DispatchUIMaskChange(bl_UIReferences.Instance.UIMask);
    }

    private void SetUILoadOut()
    {
        if (_isMobileInput)
        {
            ModileLoadoutUI.SetActive(true);
            PcLoadoutUI.SetActive(false);
            LoadoutUI = ModileLoadoutUI.GetComponent<bl_WeaponLoadoutUI>();
        }
        else
        {
            PcLoadoutUI.SetActive(true);
            ModileLoadoutUI.SetActive(false);
            LoadoutUI = PcLoadoutUI.GetComponent<bl_WeaponLoadoutUI>();
        }
    }

    public void UpdateWeaponState(bl_Gun gun)
    {
        int bullets = gun.bulletsLeft;
        int clips = gun.numberOfClips;
        float per = (float)bullets / (float)gun.bulletsPerClip;
        Color c = AmmoTextColorGradient.Evaluate(per);

        if (gun.Info.Type != GunType.Knife)
        {
            AmmoText.text = bullets.ToString();
            if (gun.HaveInfinityAmmo)
                ClipText.text = "∞";
            else
                ClipText.text = ClipText.text = clips.ToString("F0");
            AmmoText.color = c;
            ClipText.color = c;
        }
        else
        {
            AmmoText.text = "--";
            ClipText.text = ClipText.text = "--";
            AmmoText.color = Color.white;
            ClipText.color = Color.white;
        }
    }
}
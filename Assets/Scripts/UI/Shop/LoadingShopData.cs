using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ShopData;

public class LoadingShopData : MonoBehaviour
{
    [SerializeField] private ShopData _shopData;
    [SerializeField] private GameObject _uiPrefabContent;
    [SerializeField] private Transform _contentSkins, _contentWeapon;

    private void Start()
    {
        LoadUIContent(_shopData.SkinShopItems, _contentSkins);
    }

    private void LoadUIContent(ShopItem[] items, Transform transform)
    {
        for (int i = 0; i < items.Length; i++)
        {
            Instantiate(_uiPrefabContent, transform).GetComponent<UIShopPrefab>().SetData(items[i], i, this);
        }
    }

    public void SelectSkin(int id)
    {
        _shopData.SetSkinPlayer(id);
    }
}

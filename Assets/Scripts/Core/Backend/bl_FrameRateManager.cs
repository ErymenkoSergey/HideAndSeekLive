﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_FrameRateManager : MonoBehaviour
{
    public Dropdown optionDropdown;

    void OnStart()
    {
        if(optionDropdown != null && bl_MFPS.Settings != null)
        {
            optionDropdown.ClearOptions();
            List<Dropdown.OptionData> ol = new List<Dropdown.OptionData>();
            int[] options = bl_MFPS.Settings.RefreshRates;
            for (int i = 0; i < options.Length; i++)
            {
                if(options[i] == 0) { ol.Add(new Dropdown.OptionData() { text = "UNLIMITED" }); continue; }
                ol.Add(new Dropdown.OptionData() { text = options[i].ToString() });
            }
            optionDropdown.AddOptions(ol);
            int df = PlayerPrefs.GetInt(PropertiesKeys.FrameRateOption, (int)bl_MFPS.Settings.GetSettingOf("Frame Rate"));
            optionDropdown.value = df;
            Application.targetFrameRate = -1; //bl_MFPS.Settings.RefreshRates[df]; //
        }
    }

    public void OnChange(int option)
    {
        Application.targetFrameRate = -1;// bl_MFPS.Settings.RefreshRates[option]; //-1
        PlayerPrefs.SetInt(PropertiesKeys.FrameRateOption, option);
    }

    public void OnChangeCustom(int option)
    {
        Application.targetFrameRate = -1;// bl_MFPS.Settings.RefreshRates[option];
    }
}
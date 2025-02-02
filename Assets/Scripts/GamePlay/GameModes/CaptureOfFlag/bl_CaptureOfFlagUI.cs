﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.GameModes.CaptureOfFlag
{
    public class bl_CaptureOfFlagUI : MonoBehaviour
    {

        public GameObject Content;
        public Text Team1ScoreText, Team2ScoreText;
        public Image FlagImg1, FlagImg2;

        public void SetScores(int team1, int team2)
        {
            Team1ScoreText.text = team1.ToString();
            Team2ScoreText.text = team2.ToString();
        }

        public void ShowUp()
        {
            if (bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.TopScoreBoard))
                Content.SetActive(true);

            Team1ScoreText.color = Team.Hiding.GetTeamColor();
            Team2ScoreText.color = Team.Maniac.GetTeamColor();
            FlagImg1.color = Team.Hiding.GetTeamColor();
            FlagImg2.color = Team.Maniac.GetTeamColor();
        }

        public void Hide()
        {
            Content.SetActive(false);
        }

        private static bl_CaptureOfFlagUI _instance;
        public static bl_CaptureOfFlagUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<bl_CaptureOfFlagUI>();
                }
                return _instance;
            }
        }
    }
}
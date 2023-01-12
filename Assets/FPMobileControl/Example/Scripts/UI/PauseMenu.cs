using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lovatto.MobileInput
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject pauseMenuUI;

        private bool isPaused = false;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            pauseMenuUI.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            if (bl_MobileInput.GetButtonDown("Pause"))
            {
                Pause();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void TogglePause()
        {
            isPaused = !isPaused;
            if (isPaused) Pause();
            else Resume();
        }

        /// <summary>
        /// 
        /// </summary>
        void Pause()
        {
            pauseMenuUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isPaused = true;

            bl_TouchPad.Interactable = false;
            bl_MobileInput.Interactable = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Resume()
        {
            pauseMenuUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isPaused = false;

            bl_TouchPad.Interactable = true;
            bl_MobileInput.Interactable = true;
        }
    }
}
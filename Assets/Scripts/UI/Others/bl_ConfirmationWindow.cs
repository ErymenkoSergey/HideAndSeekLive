using System;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Runtime.UI
{
    public class bl_ConfirmationWindow : MonoBehaviour
    {
        public Text descriptionText;
        public GameObject content;

        [Header("Events")]
        public bl_EventHandler.UEvent onConfirm;
        public bl_EventHandler.UEvent onCancel;

        private Action callback;
        private Action cancelCallback;

        public void AskConfirmation(string description, Action onAccept, Action onCancel = null)
        {
            callback = onAccept;
            cancelCallback = onCancel;
            if(!string.IsNullOrEmpty(description))
            descriptionText.text = description;

            content.SetActive(true);
        }

        public void Confirm()
        {
            callback?.Invoke();
            onConfirm?.Invoke();
            content.SetActive(false);
        }

        public void Cancel()
        {
            callback = null;
            cancelCallback?.Invoke();
            onCancel?.Invoke();
            cancelCallback = null;
            content.SetActive(false);
        }
    }
}
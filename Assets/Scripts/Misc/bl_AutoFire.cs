using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Lovatto.MobileInput
{
    public class bl_AutoFire : MonoBehaviour
    {
        public Transform playerCamera;
        public FillBarMethod fillBarMethod = FillBarMethod.ImageWidth;
        public float roundProgressBy = 10;
        public string[] detectionTags;
        public Team _myTeam = Team.None;
        [Header("References")]
        public GameObject waitProgressUI;
        public Image waitProgressBar;

        private Ray ray;
        private RaycastHit raycastHit;
        private float detectTime = 0;
        private bool hasDetectedSomething = false;
        private RectTransform barFillRect;
        private Vector2 defaultSize;

        private void Awake()
        {
            waitProgressUI?.SetActive(false);
            if (waitProgressBar)
            {
                barFillRect = waitProgressBar.GetComponent<RectTransform>();
                defaultSize = barFillRect.sizeDelta;
            }
        }

        private void Update()
        {
            if (playerCamera == null)
                return;

            //if (!bl_MobileInputSettings.Instance.useAutoFire)
            //    return;
            Detect();
        }

        private void Detect()
        {
            //that is a simple but useful optimization method to avoid fire a raycast each frame :)
            //if ((Time.frameCount % bl_MobileInputSettings.Instance.detectRate) != 0)
            //    return;

            //fire a ray from the camera position to the front of the camera view
            ray = new Ray(playerCamera.position, playerCamera.forward);
            
            if (Physics.Raycast(ray, out raycastHit, bl_MobileInputSettings.Instance.detectRate))
            {
                Debug.Log($"bl_PlayerSettings SetTeam  Detect 2 ");

                bool detected = false;
                //check if the hitted object contains any of the trigger tags
                Debug.Log($"bl_PlayerSettings 2.1 Tag: {raycastHit.transform.tag}");
                Debug.DrawRay(playerCamera.position, playerCamera.forward, Color.green, bl_MobileInputSettings.Instance.detectRate);
                for (int i = 0; i < detectionTags.Length; i++)
                {
                    //if is so
                    if (raycastHit.transform.CompareTag(detectionTags[i]))
                    //if (raycastHit.transform.CompareTag(bl_PlayerSettings.RemoteTag))
                    {
                        Debug.Log($"bl_PlayerSettings SetTeam  Detect 2 1");

                        if (raycastHit.transform.TryGetComponent(out bl_PlayerSettings component))
                        {
                            Debug.Log($"bl_PlayerSettings SetTeam  Detect 3 ");
                            if (component != null)
                            {
                                Debug.Log($"bl_PlayerSettings SetTeam  Detect 3.1 ");
                            }
                        }

                        bl_FirstPersonController fpc = raycastHit.transform.GetComponent<bl_FirstPersonController>();

                        if (fpc != null)
                        {
                            Debug.Log($"bl_PlayerSettings SetTeam  Detect 3.2 ");
                        }

                        //and was not detecting anything previously
                        if (!hasDetectedSomething)
                        {
                            Debug.Log($"bl_PlayerSettings SetTeam  Detect 2 2");
                            //cache the detection time to wait until first fire
                            detectTime = Time.time;
                            StartCoroutine(DisplayProgress());
                        }
                        detected = true;
                        break;
                    }
                }


                //if (raycastHit.transform.CompareTag(bl_PlayerSettings.LocalTag))
                //{
                //    Debug.Log($"bl_PlayerSettings SetTeam  Detect 2.1 ");

                //    bl_PlayerSettings fpc = raycastHit.transform.GetComponent<bl_PlayerSettings>();
                //    Debug.Log($"bl_PlayerSettings SetTeam  Detect 3 ");

                //    if (fpc != null)
                //    {
                //        Debug.Log($"bl_PlayerSettings SetTeam  Detect 3.1 ");

                //        if (raycastHit.transform.GetComponent<bl_PlayerSettings>().PlayerTeam != _myTeam)
                //        {
                //            Debug.Log($"bl_PlayerSettings SetTeam  Detect 4 ");
                //            if (!hasDetectedSomething)
                //            {
                //                Debug.Log($"bl_PlayerSettings SetTeam  Detect 5 ");
                //                detectTime = Time.time;
                //                StartCoroutine(DisplayProgress());
                //            }
                //        }

                //        detected = true;
                //    }
                //    else
                //    {
                //        Debug.Log($"bl_PlayerSettings SetTeam  Detect 3.2 ");
                //    }
                //}



                //if you want detect the target in some other way like for example by checking if contains a specific script
                //there is the place where you should do it, example:
                //////if (raycastHit.transform.TryGetComponent(out bl_PlayerSettings component))
                //////{
                //////    Debug.Log($"bl_PlayerSettings SetTeam  Detect 3 ");
                //////    if (component != null)
                //////    {
                //////        Debug.Log($"bl_PlayerSettings SetTeam  Detect 3.1 ");

                //////        if (raycastHit.transform.GetComponent<bl_PlayerSettings>().PlayerTeam != _myTeam)
                //////        {
                //////            Debug.Log($"bl_PlayerSettings SetTeam  Detect 4 ");
                //////            if (!hasDetectedSomething)
                //////            {
                //////                Debug.Log($"bl_PlayerSettings SetTeam  Detect 5 ");
                //////                detectTime = Time.time;
                //////                StartCoroutine(DisplayProgress());
                //////            }
                //////        }

                //////        detected = true;
                //////    }
                //////    else
                //////    {
                //////        Debug.Log($"bl_PlayerSettings SetTeam  Detect 3.2 ");
                //////    }
                //////}

                //if before was detecting something but now doesn't
                if (hasDetectedSomething && !detected)
                    Undetect();

                hasDetectedSomething = detected;
            }
            else
            {
                if (hasDetectedSomething)
                    Undetect();

                hasDetectedSomething = false;
            }
        }


        public bool isTriggered()
        {
            //if raycast has hit nothing
            if (!hasDetectedSomething)
                return false;

            return
                (Time.time - detectTime) >= bl_MobileInputSettings.Instance.waitBeforeFire;
        }

        private void Undetect()
        {
            StopAllCoroutines();
            waitProgressUI?.SetActive(false);
        }

        private IEnumerator DisplayProgress()
        {
            if (waitProgressUI == null)
                yield break;

            waitProgressUI?.SetActive(true);
            float duration = 0;

            Vector2 currentSize = defaultSize;
            while (duration < 1)
            {
                duration += Time.deltaTime / bl_MobileInputSettings.Instance.waitBeforeFire;
                if (fillBarMethod == FillBarMethod.ImageFill) { waitProgressBar.fillAmount = duration; }
                else if (fillBarMethod == FillBarMethod.ImageWidth)
                {
                    currentSize.x = RoundOff(defaultSize.x * duration, roundProgressBy);
                    barFillRect.sizeDelta = currentSize;
                }
                yield return null;
            }
            waitProgressUI?.SetActive(false);
        }

        public float RoundOff(float i, float roundBy)
        {
            return (Mathf.Round(i / roundBy)) * roundBy;
        }

        [Serializable]
        public enum FillBarMethod
        {
            ImageFill,
            ImageWidth,
        }

        private static bl_AutoFire _instance;
        public static bl_AutoFire Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_AutoFire>(); }
                return _instance;
            }
        }

        public async void SetTeam(Transform camera, Team team)
        {
            await Task.Delay(2000);
            _myTeam = team;
            playerCamera = camera;


            Debug.Log($"bl_PlayerSettings SetTeam  team {team}");
        }
    }
}
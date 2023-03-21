using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lovatto.MobileInput
{
    public class bl_PersitentButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public bool Clamped = true;
        public float ClampArea = 100;

        private Vector3 defaultPosition, defaultRawPosition;
        private RectTransform m_Transform;
        private int touchID = 0;
        private Touch m_Touch;
        private bool init = false;

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            m_Transform = GetComponent<RectTransform>();
            GetDeafultPosition();
            init = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetDeafultPosition()
        {
            defaultPosition = m_Transform.anchoredPosition;
            defaultRawPosition = m_Transform.position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!init) return;
            touchID = eventData.pointerId;
#if !UNITY_EDITOR
          //  StartCoroutine(OnUpdate());
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!init) return;
#if !UNITY_EDITOR
           // StopAllCoroutines();
#endif
            m_Transform.anchoredPosition = defaultPosition;
            SetMouse(0f, 0f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData) // this mouse?
        {
            if (!init) return;

            
            //SetMouse(eventD);

            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Transform, eventData.position, null, out pos))
            {
                pos.x = (pos.x / m_Transform.sizeDelta.x);
                pos.y = (pos.y / m_Transform.sizeDelta.y);

                //pos.x = (m_Transform.sizeDelta.x);
                //pos.y = (m_Transform.sizeDelta.y);

                inputVector = new Vector3(pos.x, 0, pos.y);
                inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized * 10f : inputVector;

                //stickPosition.x = inputVector.x * (m_Transform.sizeDelta.x * ClampArea);
                //stickPosition.y = inputVector.z * (m_Transform.sizeDelta.y * ClampArea);
                SetMouse(inputVector.x, inputVector.z);
                //StickRect.anchoredPosition = stickPosition;
            }

            if (Clamped)
            {
                if (Vector3.Distance(eventData.position, defaultRawPosition) > ClampArea)
                {
                    return;
                }
            }

            m_Transform.position = eventData.position;
        }
        //private Vector2 stickPosition = Vector2.zero;
        public Vector3 inputVector { get; set; }
        //private void ConvertVector()
        //{
        //    inputVector = new Vector3(pos.x, 0, pos.y);
        //    inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

        //    stickPosition.x = inputVector.x * (joystickRoot.sizeDelta.x * stickArea);
        //    stickPosition.y = inputVector.z * (joystickRoot.sizeDelta.y * stickArea);
        //    SetMouse(inputVector.x, inputVector.z);
        //}

        private void SetMouse(float vectorX, float vectorY)
        {
            Debug.Log($"bl_PersitentButton vectorX {vectorX}, vectorY {vectorY}");
            bl_MobileInput.MobileMouseX = vectorX;
            bl_MobileInput.MobileMouseY = vectorY;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator OnUpdate()
        {
            while (true)
            {
                Follow();
                yield return null;
            }
        }

        void Follow()
        {
            m_Touch = Input.GetTouch(touchID);
            m_Transform.position = new Vector3(m_Touch.position.x, m_Touch.position.y, transform.position.z);
        }
    }
}
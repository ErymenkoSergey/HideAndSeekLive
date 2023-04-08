using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lovatto.MobileInput
{
    public class bl_PersitentButton : CommonMonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public bool Clamped = true;
        public float ClampArea = 100;

        private Vector3 defaultPosition, defaultRawPosition;
        private RectTransform m_Transform;
        private int touchID = 0;
        private Touch m_Touch;
        private bool init = false;

        void Start()
        {
            m_Transform = GetComponent<RectTransform>();
            GetDeafultPosition();
            init = true;
        }

        public void GetDeafultPosition()
        {
            defaultPosition = m_Transform.anchoredPosition;
            defaultRawPosition = m_Transform.position;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!init) return;
            touchID = eventData.pointerId;
#if !UNITY_EDITOR
          //  StartCoroutine(OnUpdate());
#endif

            if (lastId == -2)
            {
                lastId = eventData.pointerId;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!init) return;
#if !UNITY_EDITOR
           // StopAllCoroutines();
#endif
            m_Transform.anchoredPosition = defaultPosition;

            if (eventData.pointerId == lastId)
            {
                lastId = -2;
                SetMouse(0f, 0f);
                
            } 
        }
        private int lastId = -2;

        public void OnDrag(PointerEventData eventData)
        {
            if (!init)
                return;

            if (eventData.pointerId == lastId)
            {
                Vector2 pos;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Transform, eventData.position, null, out pos))
                {
                    pos.x = (pos.x / m_Transform.sizeDelta.x);
                    pos.y = (pos.y / m_Transform.sizeDelta.y);

                    Vector3 inputVector = new Vector3(pos.x, 0, pos.y);
                    //inputVector = (inputVector.magnitude > .1f) ? inputVector.normalized : inputVector;

                    SetMouse(inputVector.x, inputVector.z);
                }
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

        private void SetMouse(float vectorX, float vectorY)
        {
            bl_MobileInput.MobileMouseX = vectorX;
            bl_MobileInput.MobileMouseY = vectorY;
        }

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
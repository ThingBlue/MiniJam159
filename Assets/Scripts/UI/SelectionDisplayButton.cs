using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniJam159.UI
{
    public class SelectionDisplayButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Inspector members

        public float smoothTime;

        #endregion

        public string selectedObjectName;
        public int selectedIndex;
        public List<GameObject> row;
        public Vector3 targetLocalPosition;
        public Vector2 targetSize;

        private Vector3 localPositionVelocity;
        private Vector2 sizeVelocity;

        private void FixedUpdate()
        {


            // Move towards target position and size
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.localPosition = Vector3.SmoothDamp(rectTransform.localPosition, targetLocalPosition, ref localPositionVelocity, smoothTime);
            rectTransform.sizeDelta = Vector2.SmoothDamp(rectTransform.sizeDelta, targetSize, ref sizeVelocity, smoothTime);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //TooltipManager.instance.toggleTooltip(selectedObjectName, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //TooltipManager.instance.toggleTooltip("", false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniJam159.UI
{
    public enum SelectStatus
    {
        DEFAULT = 0,
        RESELECT,
        DESELECT
    }

    public class SelectionDisplayButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Inspector members

        public Image frameImage;

        public float smoothTime;

        public float defaultWidth;
        public float hoveredWidth;

        public Color defaultColor;
        public Color reselectColor;
        public Color deselectColor;

        #endregion

        public string selectedObjectName;
        public int selectedIndex;
        public List<GameObject> row;
        public Vector3 targetLocalPosition;
        public Vector2 targetSize = new Vector2(32f, 32f);
        public bool hovered = false;

        private Vector3 localPositionVelocity;
        private Vector2 sizeVelocity;

        private void FixedUpdate()
        {
            if (hovered) targetSize = new Vector2(hoveredWidth, hoveredWidth);
            else targetSize = new Vector2(defaultWidth, defaultWidth);

            // Move towards target position and size
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.localPosition = Vector3.SmoothDamp(rectTransform.localPosition, targetLocalPosition, ref localPositionVelocity, smoothTime);
            rectTransform.sizeDelta = Vector2.SmoothDamp(rectTransform.sizeDelta, targetSize, ref sizeVelocity, smoothTime);
        }

        public void setSelectStatus(SelectStatus status)
        {
            switch (status)
            {
                case SelectStatus.DEFAULT:
                    frameImage.color = defaultColor;
                    break;
                case SelectStatus.RESELECT:
                    frameImage.color = reselectColor;
                    break;
                case SelectStatus.DESELECT:
                    frameImage.color = deselectColor;
                    break;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;

            // Trigger UI update
            UIManager.instance.updateDisplayBoxes();

            // Show tooltip
            //TooltipManager.instance.toggleTooltip(selectedObjectName, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;

            // Trigger UI update
            UIManager.instance.updateDisplayBoxes();

            // Hide tooltip
            //TooltipManager.instance.toggleTooltip("", false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniJam159.UICore
{
    public class SelectionDisplayButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Inspector members

        public Image frameImage;

        public float smoothTime;

        public Color defaultColor;
        public Color focusedColor;
        public Color reselectColor;
        public Color deselectColor;

        #endregion

        public string selectedObjectName;
        public int selectedIndex;

        public float defaultSize;
        public float hoveredSize;

        public Vector3 targetLocalPosition;
        public Vector2 targetSize = new Vector2(32f, 32f);
        public bool hovered = false;

        private Vector3 localPositionVelocity;
        private Vector2 sizeVelocity;

        private void FixedUpdate()
        {
            if (hovered) targetSize = new Vector2(hoveredSize, hoveredSize);
            else targetSize = new Vector2(defaultSize, defaultSize);

            // Move towards target position and size
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.localPosition = Vector3.SmoothDamp(rectTransform.localPosition, targetLocalPosition, ref localPositionVelocity, smoothTime);
            rectTransform.sizeDelta = Vector2.SmoothDamp(rectTransform.sizeDelta, targetSize, ref sizeVelocity, smoothTime);
        }

        public void setFrameColour(HoverStatus status)
        {
            switch (status)
            {
                case HoverStatus.DEFAULT:
                    frameImage.color = defaultColor;
                    break;
                case HoverStatus.FOCUS:
                    frameImage.color = focusedColor;
                    break;
                case HoverStatus.HOVER:
                    frameImage.color = reselectColor;
                    break;
                case HoverStatus.REMOVE:
                    frameImage.color = deselectColor;
                    break;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;

            // Trigger UI update
            SelectionDisplayManagerBase.instance.updateSelectionDisplayBoxes();

            /*
            // Add hovered outline to corresponding entity
            GameObject selectedObject = SelectionManager.instance.selectedObjects[selectedIndex];
            if (selectedObject != null)
            {
                selectedObject.GetComponent<Entity>().setOutline(SelectionManager.instance.selectedOutlineMaterial, SelectionManager.instance.hoveredOutlineColor);
            }
            */

            // Show tooltip
            //TooltipManager.instance.toggleTooltip(selectedObjectName, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;

            // Trigger UI update
            SelectionDisplayManagerBase.instance.updateSelectionDisplayBoxes();

            /*
            // Remove hovered outline from corresponding entity
            GameObject selectedObject = SelectionManager.instance.selectedObjects[selectedIndex];
            if (selectedObject != null)
            {
                selectedObject.GetComponent<Entity>().setOutline(SelectionManager.instance.selectedOutlineMaterial, SelectionManager.instance.selectedOutlineColor);
            }
            */

            // Hide tooltip
            //TooltipManager.instance.toggleTooltip("", false);
        }
    }
}

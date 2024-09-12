using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MiniJam159.GameCore;
using UnityEngine.EventSystems;
using MiniJam159.PlayerCore;

namespace MiniJam159.UICore
{
    public class SquadDisplayBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        #region Inspector members

        public CanvasGroup canvasGroup;

        public Image frameImage;

        public Color defaultColor;
        public Color hoverColor;
        public Color focusColor;
        public Color deselectColor;

        #endregion

        public Squad squad;

        private Vector3 originalPosition;
        private Vector3 mouseOffset;

        public void setFrameColour(HoverStatus status)
        {
            switch (status)
            {
                case HoverStatus.DEFAULT:
                    frameImage.color = defaultColor;
                    break;
                case HoverStatus.HOVER:
                    frameImage.color = hoverColor;
                    break;
                case HoverStatus.FOCUS:
                    frameImage.color = focusColor;
                    break;
                case HoverStatus.REMOVE:
                    frameImage.color = deselectColor;
                    break;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            setFrameColour(HoverStatus.HOVER);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            setFrameColour(HoverStatus.DEFAULT);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalPosition = GetComponent<RectTransform>().localPosition;
            mouseOffset = GetComponent<RectTransform>().position - Input.mousePosition;

            // Add transparency effect and disable raycasts
            canvasGroup.alpha = 0.25f;
            canvasGroup.blocksRaycasts = false;

            // Allow raycasts on unbind and delete boxes
            SquadPanelManagerBase.instance.toggleRaycastBoxes(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            GetComponent<RectTransform>().position = Input.mousePosition + mouseOffset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Check to see where the drag ends
            GameObject dropTargetObject = null;
            int dropSlot = SquadPanelManagerBase.instance.getDropTarget(out dropTargetObject);

            // Not dropped over useful UI object
            if (dropSlot == -1)
            {
                // Reset position and return
                GetComponent<RectTransform>().localPosition = originalPosition;
            }
            // Dropped over main area
            else if (dropSlot == -2)
            {
                // Unbind
                unbindSquad(squad);

                // Update positions of unbound squad display boxes
                SquadPanelManagerBase.instance.updateUnboundBoxes();
            }
            // Dropped over delete box
            else if (dropSlot == -3)
            {
                // Remove from slot if bound
                unbindSquad(squad);

                // Remove from lists
                SelectionManager.instance.squads.Remove(squad);
                SquadPanelManagerBase.instance.squadDisplayBoxes.Remove(gameObject);

                // Update positions of unbound squad display boxes
                SquadPanelManagerBase.instance.updateUnboundBoxes();
            }
            // Dropped over squad slot box
            else
            {
                // Remove from previous slot if bound
                unbindSquad(squad);

                // Kick out previous squad in new slot if occupied
                if (SelectionManager.instance.boundSquads[dropSlot] != null)
                {
                    unbindSquad(SelectionManager.instance.boundSquads[dropSlot]);
                }

                // Assign to corresponding slot
                SelectionManager.instance.boundSquads[dropSlot] = squad;

                // Update positions of unbound squad display boxes
                SquadPanelManagerBase.instance.updateUnboundBoxes();

                // Set position
                GetComponent<RectTransform>().localPosition = dropTargetObject.GetComponent<RectTransform>().localPosition;
            }

            // Reset canvas group modifiers
            canvasGroup.alpha = 0.75f;
            canvasGroup.blocksRaycasts = true;

            // Reset unbind and delete boxes
            SquadPanelManagerBase.instance.toggleRaycastBoxes(false);

            /*
            // DEBUG
            Debug.Log("Squad count: " + SelectionManager.instance.squads.Count);
            foreach (Squad squad in SelectionManager.instance.boundSquads)
            {
                if (squad == null) Debug.Log("NULL");
                else Debug.Log(squad.id);
            }
            */

            // Delete this box
            if (dropSlot == -3) Destroy(gameObject);
        }

        public void unbindSquad(Squad matchingSquad)
        {
            if (matchingSquad == null) return;

            // Remove from squad binds list
            int bindSlot = SelectionManager.instance.boundSquads.IndexOf(matchingSquad);
            if (bindSlot != -1) SelectionManager.instance.boundSquads[bindSlot] = null;

            /*
            // Move corresponding display box back to unbound area
            GameObject squadDisplayBoxObject = SquadPanelManagerBase.instance.getSquadDisplayBox(matchingSquad);
            if (!squadDisplayBoxObject)
            {
                Debug.LogError("Error: No squad display box associated with squad: " + squad);
                return;
            }

            squadDisplayBoxObject.GetComponent<RectTransform>().localPosition = ;
            */
        }
    }
}

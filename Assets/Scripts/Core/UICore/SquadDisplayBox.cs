using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MiniJam159.GameCore;
using UnityEngine.EventSystems;
using MiniJam159.PlayerCore;

namespace MiniJam159.UICore
{
    public class SquadDisplayBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
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

        public bool hovered = false;
        private float timeSinceLastMouse0 = 0;
        private bool mouse1Down = false;

        private Vector3 originalPosition;
        private Vector3 mouseOffset;

        private void Update()
        {
            // Quick bind
            // Check for squad bind toggle (Right click on default)
            if (InputManager.instance.getKeyDown("Mouse1") && hovered) mouse1Down = true;
            if (InputManager.instance.getKeyUp("Mouse1") && mouse1Down && hovered)
            {
                toggleSquadBind();
                mouse1Down = false;
            }

            // Reset clicking if button is released while not hovering
            if (InputManager.instance.getKeyUp("Mouse1") && mouse1Down && !hovered) mouse1Down = false;

            // Increment double click timer
            timeSinceLastMouse0 += Time.deltaTime;
        }

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

        public void unbindSquad(Squad matchingSquad)
        {
            if (matchingSquad == null) return;

            // Remove from squad binds list
            int bindSlot = SelectionManager.instance.boundSquads.IndexOf(matchingSquad);
            if (bindSlot != -1) SelectionManager.instance.boundSquads[bindSlot] = null;
        }

        public void deleteSquad()
        {
            // Remove from slot if bound
            unbindSquad(squad);

            // Remove from lists
            SelectionManager.instance.squads.Remove(squad);
            SquadPanelManagerBase.instance.squadDisplayBoxes.Remove(gameObject);

            // Update positions of unbound squad display boxes
            SquadPanelManagerBase.instance.updateSquadDisplayBoxes();

            // IMPORTANT: Need to call Destroy(gameObject) after calling this function
        }

        private void toggleSquadBind()
        {
            // Already bound
            if (SelectionManager.instance.boundSquads.Contains(squad))
            {
                // Unbind
                unbindSquad(squad);

                // Update positions of unbound squad display boxes
                SquadPanelManagerBase.instance.updateSquadDisplayBoxes();
            }
            // Not bound yet, bind to first open slot
            else
            {
                // Get first open slot
                int firstOpenSlot = -1;
                for (int i = 0; i < SelectionManager.instance.boundSquads.Count; i++)
                {
                    if (SelectionManager.instance.boundSquads[i] == null)
                    {
                        firstOpenSlot = i;
                        break;
                    }
                }

                // All slots occupied, do nothing
                if (firstOpenSlot == -1) return;

                // Assign to corresponding slot
                SelectionManager.instance.boundSquads[firstOpenSlot] = squad;

                // Update positions of unbound squad display boxes
                SquadPanelManagerBase.instance.updateSquadDisplayBoxes();

                // Set position
                GetComponent<RectTransform>().localPosition = SquadPanelManagerBase.instance.squadSlotBoxes[firstOpenSlot].GetComponent<RectTransform>().localPosition;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            SquadPanelManagerBase.instance.updateSquadDisplayBoxes();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            SquadPanelManagerBase.instance.updateSquadDisplayBoxes();
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
                SquadPanelManagerBase.instance.updateSquadDisplayBoxes();
            }
            // Dropped over delete box
            else if (dropSlot == -3)
            {
                // Delete this squad
                deleteSquad();
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
                SquadPanelManagerBase.instance.updateSquadDisplayBoxes();

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

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // Quick delete
                if (InputManager.instance.getKey("Deselect"))
                {
                    deleteSquad();
                    Destroy(gameObject);
                }

                // Quick retrieve
                // Execute double click
                if (timeSinceLastMouse0 < SettingsManager.instance.settingsData.doubleClickMaxDelay)
                {
                    // Retrieve squad
                    SelectionControllerBase.instance.retrieveSquad(squad);

                    // Make sure we can't double click again on the next click
                    timeSinceLastMouse0 = SettingsManager.instance.settingsData.doubleClickMaxDelay + 1f;
                }
                else
                {
                    // Reset timer
                    timeSinceLastMouse0 = 0;
                }
            }
        }
    }
}

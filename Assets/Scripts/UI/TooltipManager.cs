using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MiniJam159.UI
{
    public class TooltipManager : MonoBehaviour
    {
        #region Inspector members

        public float textPaddingSize = 8f;

        #endregion

        private TMP_Text tooltipText;

        // Singleton
        public static TooltipManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            tooltipText = transform.Find("TooltipText").GetComponent<TMP_Text>();
            toggleTooltip("", false);
        }

        private void Update()
        {
            // Make tooltip follow mouse
            transform.position = Input.mousePosition;
        }

        public void toggleTooltip(string text, bool visible)
        {
            tooltipText.text = text;
            Vector2 backgroundSize = new Vector2(tooltipText.preferredWidth + textPaddingSize * 2f, tooltipText.preferredHeight + textPaddingSize * 2f);
            GetComponent<RectTransform>().sizeDelta = backgroundSize;

            gameObject.SetActive(visible);
        }
    }
}

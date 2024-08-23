using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MiniJam159.GameCore
{
    public class HealthBar : MonoBehaviour
    {
        #region Inspector members

        public RectTransform healthBarCanvasTransform;
        public RectTransform healthBarTransform;
        public float maxHealth;

        public float minSize;
        public float sizeScaling;

        #endregion

        public float health;

        private void Start()
        {
            // Default to start at max health
            // Units will start at max, structure will start at 1
            setMaxHealth(maxHealth);
            setHealth(maxHealth);
        }

        public void setMaxHealth(float maxHealth)
        {
            this.maxHealth = maxHealth;

            // Refresh health bar background width
            healthBarCanvasTransform.sizeDelta = new Vector2(minSize + (maxHealth * sizeScaling), healthBarCanvasTransform.sizeDelta.y);

            // Make sure health bar is still at the correct width
            healthBarTransform.offsetMin = new Vector2(0, healthBarTransform.offsetMin.y);
        }

        public void setHealth(float health)
        {
            this.health = health;

            // Calculate percentage, capped at 1
            float healthPercentage = Mathf.Min(health / maxHealth, 1f);

            // Refresh health bar width by changing right offset
            float missingWidth = (1f - healthPercentage) * healthBarCanvasTransform.sizeDelta.x;
            healthBarTransform.offsetMax = new Vector2(-missingWidth, healthBarTransform.offsetMax.y);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MiniJam159.EntityCore
{
    public class HealthBar : MonoBehaviour
    {
        #region Inspector members

        public RectTransform healthBarCanvasTransform;
        public RectTransform healthBarTransform;

        public float minSize;
        public float sizeScaling;

        #endregion

        public float maxHealth;
        public Entity attachedEntity;

        private void Start()
        {
            // Try getting entity from parent object
            if (transform && transform.parent) attachedEntity = transform.parent.GetComponent<Entity>();
            // Found nothing, throw error
            if (!attachedEntity) throw new System.Exception("Health bar on " + gameObject.name + " cannot find entity on parent object");

            // Get max health value from entity
            maxHealth = attachedEntity.maxHealth;
            // Refresh health bar background width
            healthBarCanvasTransform.sizeDelta = new Vector2(minSize + (maxHealth * sizeScaling), healthBarCanvasTransform.sizeDelta.y);
            // Make sure health bar is still at the correct width
            healthBarTransform.offsetMin = new Vector2(0, healthBarTransform.offsetMin.y);

            // Subscribe to health changed event
            attachedEntity.onHealthChangedEvent += onHealthChanged;
            // Immediately perform health change once
            onHealthChanged(attachedEntity.health);
        }

        public void onHealthChanged(float newValue)
        {
            // Calculate clamped percentage
            float healthPercentage = Mathf.Clamp(0f, newValue / maxHealth, 1f);

            // Refresh health bar width by changing right offset
            float missingWidth = (1f - healthPercentage) * healthBarCanvasTransform.sizeDelta.x;
            healthBarTransform.offsetMax = new Vector2(-missingWidth, healthBarTransform.offsetMax.y);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MiniJam159.UI
{
    public class InkSpread : MonoBehaviour
    {
        #region Inspector members

        public InkParticleEmitter inkParticleEmitter;

        public Image image;

        public float maxZoom;
        public float zoomSmoothTime;

        public float minNoise1Zoom;
        public float maxNoise1Zoom;
        public float noise1ZoomSmoothTime;

        public float minNoise2Zoom;
        public float maxNoise2Zoom;
        public float noise2ZoomSmoothTime;

        public float fadeOutLerpFactor;

        #endregion

        private float currentZoom;
        private float zoomVelocity;

        private float currentNoise1Zoom;
        private float noise1ZoomVelocity;

        private float currentNoise2Zoom;
        private float noise2ZoomVelocity;

        private bool fadingIn = false;
        private bool fadingOut = false;

        private void Update()
        {
            // DEBUG
            if (Input.GetKeyDown(KeyCode.I))
            {
                FadeIn();
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                FadeOut();
            }
        }

        private void FixedUpdate()
        {
            // Fade in
            if (fadingIn)
            {
                Material material = image.material;

                // Zoom
                currentZoom = Mathf.SmoothDamp(currentZoom, maxZoom, ref zoomVelocity, zoomSmoothTime);
                material.SetFloat("_Zoom", currentZoom);

                // Noise zoom
                currentNoise1Zoom = Mathf.SmoothDamp(currentNoise1Zoom, maxNoise1Zoom, ref noise1ZoomVelocity, noise1ZoomSmoothTime);
                material.SetFloat("_Noise1Zoom", currentNoise1Zoom);

                currentNoise2Zoom = Mathf.SmoothDamp(currentNoise2Zoom, maxNoise2Zoom, ref noise2ZoomVelocity, noise2ZoomSmoothTime);
                material.SetFloat("_Noise2eZoom", currentNoise2Zoom);
            }

            // Fade out
            if (fadingOut)
            {
                float newAlpha = Mathf.Lerp(image.color.a, 0, fadeOutLerpFactor);
                image.color = new Color(image.color.r, image.color.g, image.color.b, newAlpha);
            }
        }

        public void FadeIn()
        {
            fadingIn = true;
            fadingOut = false;

            currentZoom = 0;
            currentNoise1Zoom = minNoise1Zoom;
            currentNoise2Zoom = minNoise2Zoom;

            // Immediately set zoom so that resetting alpha doesn't pop ink back in for a frame
            Material material = image.material;
            material.SetFloat("_Zoom", currentZoom);

            // Reset alpha
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);

            inkParticleEmitter.startEmission();
        }

        public void FadeOut()
        {
            fadingOut = true;
            // Continue to fade in if already started, don't want to stop halfway with a half sized ink blot

            inkParticleEmitter.stopEmission();
        }
    }
}

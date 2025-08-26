using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MiniJam159.UI
{
    public class InkSpreadAnimator : MonoBehaviour
    {
        #region Inspector members

        public InkParticleEmitter inkSplashParticleEmitter;
        public InkParticleEmitter inkSmokeParticleEmitter;

        public Image layer1Image;
        public Image layer2Image;
        public Image layer3Image;

        // Fade in
        public float fadeInTime;
        public float fadeOutTime;

        public float maxZoom;
        public float zoomSmoothTime;

        public float layer1NoisePan;
        public float layer1NoisePanLerpFactor;
        public float layer2NoisePan;
        public float layer2NoisePanLerpFactor;
        public float layer3NoisePan;
        public float layer3NoisePanLerpFactor;

        public float inkSmokeBurstTime = 0.2f;
        public int inkSmokeBurstCount = 4;

        // Fade out
        public float fadeOutLerpFactor;

        public float layer1FadeOutTime;
        public float layer2FadeOutTime;
        public float layer3FadeOutTime;

        #endregion

        private bool fadingIn = false;
        private bool fadingOut = false;
        private float fadeInTimer;
        private float fadeOutTimer;

        // Fade in
        private float currentZoom;
        private float zoomSmoothVelocity;

        private float currentLayer1NoisePan;
        private float currentLayer2NoisePan;
        private float currentLayer3NoisePan;

        private bool inkSplashBurstDone = false;
        private bool inkSmokeBurstDone = false;

        // Fade out
        private float currentLayer1FadeOut = 0;
        private float currentLayer2FadeOut = 0;
        private float currentLayer3FadeOut = 0;

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

            // Handle fade in and fade out timers
            if (fadingIn) fadeInTimer += Time.deltaTime;
            else fadeInTimer = 0.0f;

            if (fadingOut) fadeOutTimer += Time.deltaTime;
            else fadeOutTimer = 0.0f;
        }

        private void FixedUpdate()
        {
            // Fade in
            if (fadingIn)
            {
                Material layer1Material = layer1Image.material;
                Material layer2Material = layer2Image.material;
                Material layer3Material = layer3Image.material;

                // Zoom
                currentZoom = Mathf.SmoothDamp(currentZoom, maxZoom, ref zoomSmoothVelocity, zoomSmoothTime);
                layer1Material.SetFloat("_Zoom", currentZoom);
                layer2Material.SetFloat("_Zoom", currentZoom);
                layer3Material.SetFloat("_Zoom", currentZoom);

                // Upwards pan
                //currentLayer1NoisePan = Mathf.SmoothDamp(currentLayer1NoisePan, layer1NoisePan, ref layer1NoisePanSmoothVelocity, layer1NoisePanTime);
                currentLayer1NoisePan = Mathf.Lerp(currentLayer1NoisePan, layer1NoisePan, layer1NoisePanLerpFactor);
                currentLayer2NoisePan = Mathf.Lerp(currentLayer2NoisePan, layer2NoisePan, layer2NoisePanLerpFactor);
                currentLayer3NoisePan = Mathf.Lerp(currentLayer3NoisePan, layer3NoisePan, layer3NoisePanLerpFactor);
                layer1Material.SetVector("_Noise2Pan", new Vector4(0, currentLayer1NoisePan, 0, 0));
                layer2Material.SetVector("_Noise2Pan", new Vector4(0, currentLayer2NoisePan, 0, 0));
                layer3Material.SetVector("_Noise2Pan", new Vector4(0, currentLayer3NoisePan, 0, 0));

                // Launch ink smoke burst
                if (fadeInTimer > inkSmokeBurstTime && !inkSmokeBurstDone)
                {
                    inkSmokeParticleEmitter.burstEmit(inkSmokeBurstCount);
                    inkSmokeBurstDone = true;
                }

                if (fadeInTimer > fadeInTime) fadingIn = false;
            }
            else
            {
                inkSplashBurstDone = false;
                inkSmokeBurstDone = false;
            }

            // Fade out
            if (fadingOut)
            {
                Material layer1Material = layer1Image.material;
                Material layer2Material = layer2Image.material;
                Material layer3Material = layer3Image.material;

                currentLayer1FadeOut = Mathf.MoveTowards(currentLayer1FadeOut, 1, Time.fixedDeltaTime / layer1FadeOutTime);
                currentLayer2FadeOut = Mathf.MoveTowards(currentLayer2FadeOut, 1, Time.fixedDeltaTime / layer1FadeOutTime);
                currentLayer3FadeOut = Mathf.MoveTowards(currentLayer3FadeOut, 1, Time.fixedDeltaTime / layer1FadeOutTime);
                layer1Material.SetFloat("_FadeOut", currentLayer1FadeOut);
                layer2Material.SetFloat("_FadeOut", currentLayer2FadeOut);
                layer3Material.SetFloat("_FadeOut", currentLayer3FadeOut);

                inkSplashBurstDone = false;
                inkSmokeBurstDone = false;

                if (fadeOutTimer > fadeOutTime) fadingOut = false;
            }
        }

        public void FadeIn()
        {
            fadingIn = true;
            fadingOut = false;

            currentZoom = 0;
            currentLayer1NoisePan = 0;
            currentLayer2NoisePan = 0;
            currentLayer3NoisePan = 0;

            // Immediately set zoom so that resetting alpha doesn't pop ink back in for a frame
            Material layer1Material = layer1Image.material;
            Material layer2Material = layer2Image.material;
            Material layer3Material = layer3Image.material;

            layer1Material.SetFloat("_Zoom", currentZoom);
            layer2Material.SetFloat("_Zoom", currentZoom);
            layer3Material.SetFloat("_Zoom", currentZoom);

            layer1Material.SetVector("_Noise2Pan", new Vector4());
            layer2Material.SetVector("_Noise2Pan", new Vector4());
            layer3Material.SetVector("_Noise2Pan", new Vector4());


            // Reset fade out
            currentLayer1FadeOut = 0;
            currentLayer2FadeOut = 0;
            currentLayer3FadeOut = 0;
            layer1Material.SetFloat("_FadeOut", 0);
            layer2Material.SetFloat("_FadeOut", 0);
            layer3Material.SetFloat("_FadeOut", 0);

            // Start particle system
            inkSplashParticleEmitter.startEmission();
        }

        public void FadeOut()
        {
            fadingOut = true;
            // Continue to fade in if already started, don't want to stop halfway with a half sized ink blot

            inkSplashParticleEmitter.stopEmission();
        }
    }
}

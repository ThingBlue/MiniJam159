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

        public Image layer1Image;
        public Image layer2Image;
        public Image layer3Image;

        public float layer1Alpha;
        public float layer2Alpha;
        public float layer3Alpha;

        // Fade in
        public float maxZoom;
        public float zoomSmoothTime;

        public float layer1NoisePan;
        public float layer1NoisePanLerpFactor;
        public float layer2NoisePan;
        public float layer2NoisePanLerpFactor;
        public float layer3NoisePan;
        public float layer3NoisePanLerpFactor;

        // Fade out
        public float fadeOutLerpFactor;

        #endregion

        private bool fadingIn = false;
        private bool fadingOut = false;

        // Fade in
        private float currentZoom;
        private float zoomSmoothVelocity;

        private float currentLayer1NoisePan;
        private float currentLayer2NoisePan;
        private float currentLayer3NoisePan;

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
                layer1Material.SetVector("_Noise2Pan", new Vector4(0, currentLayer1NoisePan, 0, 0));
                currentLayer2NoisePan = Mathf.Lerp(currentLayer2NoisePan, layer2NoisePan, layer2NoisePanLerpFactor);
                layer2Material.SetVector("_Noise2Pan", new Vector4(0, currentLayer2NoisePan, 0, 0));
                currentLayer3NoisePan = Mathf.Lerp(currentLayer3NoisePan, layer3NoisePan, layer3NoisePanLerpFactor);
                layer3Material.SetVector("_Noise2Pan", new Vector4(0, currentLayer3NoisePan, 0, 0));
            }

            // Fade out
            if (fadingOut)
            {
                float layer1NewAlpha = Mathf.Lerp(layer1Image.color.a, 0, fadeOutLerpFactor);
                float layer2NewAlpha = Mathf.Lerp(layer2Image.color.a, 0, fadeOutLerpFactor);
                float layer3NewAlpha = Mathf.Lerp(layer3Image.color.a, 0, fadeOutLerpFactor);
                layer1Image.color = new Color(layer1Image.color.r, layer1Image.color.g, layer1Image.color.b, layer1NewAlpha);
                layer2Image.color = new Color(layer2Image.color.r, layer2Image.color.g, layer2Image.color.b, layer2NewAlpha);
                layer3Image.color = new Color(layer3Image.color.r, layer3Image.color.g, layer3Image.color.b, layer3NewAlpha);
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

            // Reset alpha
            layer1Image.color = new Color(layer1Image.color.r, layer1Image.color.g, layer1Image.color.b, layer1Alpha);
            layer2Image.color = new Color(layer2Image.color.r, layer2Image.color.g, layer2Image.color.b, layer2Alpha);
            layer3Image.color = new Color(layer3Image.color.r, layer3Image.color.g, layer3Image.color.b, layer3Alpha);

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

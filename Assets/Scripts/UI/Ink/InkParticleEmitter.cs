using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MiniJam159.UI
{
    // Used for both ink smoke and ink splashes
    public class InkParticleEmitter : MonoBehaviour
    {
        #region Inspector members

        public ParticleSystem particleSystem;

        public float edgeLength = 5f;
        public float standardDeviation = 0.3f; // Controls how tight the center bias is
        public int particlesPerSecond = 10;

        public float heightAmplitude = 2f;  // Max height at the center
        public float heightStdDev = 2f;     // Controls the spread of the height curve

        public float minVelocity = 1f; // Velocity at the edge
        public float maxVelocity = 5f; // Velocity at the center

        public ParticleSystemRenderer particleSystemRenderer;
        public int renderQueueOrder = 4000;

        #endregion

        private float emissionTimer;

        [ReadOnly] public bool emitting = false;

        private void Start()
        {
            // Set render queue order
            particleSystemRenderer.material.renderQueue = renderQueueOrder;
        }

        private void FixedUpdate()
        {
            // Increment timer
            if (emitting && particlesPerSecond > 0)
            {
                emissionTimer += Time.fixedDeltaTime;
            }
            else
            {
                emissionTimer = 0;
            }

            // Emit particles based on timer
            float emissionInterval = 1.0f / particlesPerSecond;
            while (emissionTimer >= emissionInterval)
            {
                gaussianEmit();
                emissionTimer -= emissionInterval;
            }
        }

        private void gaussianEmit()
        {
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

            // Generate a Gaussian-biased value centered at 0
            float x = sampleGaussian(0f, standardDeviation);
            x = Mathf.Clamp(x, -edgeLength / 2f, edgeLength / 2f);

            // Compute height (Z) using a Gaussian curve centered at X = 0
            float maxHeight = heightAmplitude * Mathf.Exp(-Mathf.Pow(x, 2f) / (2f * Mathf.Pow(heightStdDev, 2f)));
            float z = Random.Range(0, maxHeight);

            // Velocity (more at center, less at edge)
            float distanceFromCenter = Mathf.Abs(x);
            float halfLength = edgeLength / 2f;
            float normalizedDistance = distanceFromCenter / halfLength;
            float velocityMagnitude = Mathf.Lerp(maxVelocity, minVelocity, normalizedDistance);
            Vector3 velocity = new Vector3(0f, 0f, velocityMagnitude); // e.g. shoot upward

            emitParams.position = new Vector3(x, 0f, z);
            emitParams.velocity = velocity;

            particleSystem.Emit(emitParams, 1);
        }

        // Box-Muller transform to generate a Gaussian distributed float
        private float sampleGaussian(float mean, float standardDeviation)
        {
            float u1 = 1.0f - Random.Range(0f, 1f); // Uniform(0,1] random doubles
            float u2 = 1.0f - Random.Range(0f, 1f);
            float randonStandardNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                                  Mathf.Sin(2.0f * Mathf.PI * u2); // Standard normal (0, 1)
            return mean + standardDeviation * randonStandardNormal;
        }

        public void startEmission()
        {
            emitting = true;
        }

        public void stopEmission()
        {
            emitting = false;
        }

        public void burstEmit(int count)
        {
            for (int i = 0; i < count; i++)
            {
                gaussianEmit();
            }
        }
    }
}

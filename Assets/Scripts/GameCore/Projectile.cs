using UnityEngine;

namespace MiniJam159.GameCore
{
    public class Projectile : MonoBehaviour
    {
        public float speed = 10f;
        private Vector3 targetPosition;
        private int damage;

        public void Initialize(Vector3 target, int damageAmount)
        {
            targetPosition = target;
            damage = damageAmount;
        }

        void Update()
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                Destroy(gameObject);
                // Handle collision with target and apply damage
            }
        }

        // EDIT EDIT EDIT
        void OnTriggerEnter2D(Collider2D other)
        {
            // Apply damage
            Debug.Log("Hit an object for " + damage + " damage.");
            Destroy(gameObject);
        }
    }
}


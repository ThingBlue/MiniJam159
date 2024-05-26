using UnityEngine;

namespace MiniJam159.GameCore
{
    public class Projectile : MonoBehaviour
    {
        public float speed = 10f;
        private Vector2 targetPosition;
        private int damage;

        public void Initialize(Vector2 target, int damageAmount)
        {
            targetPosition = target;
            damage = damageAmount;
        }

        void Update()
        {
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            {
                Destroy(gameObject);
                // Handle collision with target and apply damage
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Apply damage
            Debug.Log("Hit an object for " + damage + " damage.");
            Destroy(gameObject);
        }
    }
}


using UnityEngine;

public class GolemLaser : MonoBehaviour
{
    private int damageAmount = 20;
    private BoxCollider2D laserCollider;
    private Transform aimTransform;
    private ParticleSystem particles;

    private float damageCooldown = 0.5f;
    private float lastDamageTime = -999f;

    void Start()
    {
        laserCollider = GetComponent<BoxCollider2D>();
        aimTransform = GetComponentInParent<Transform>();
        particles = GetComponentInParent<ParticleSystem>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerHealth = other.GetComponent<PlayerMovement>();
            Transform playerTransform = other.transform;

            if (playerHealth != null && Time.time - lastDamageTime >= damageCooldown)
            {
                lastDamageTime = Time.time;
                playerHealth.takeDamage(damageAmount);

                float laserAngleDeg = aimTransform.transform.eulerAngles.z;
                float laserAngleRad = laserAngleDeg * Mathf.Deg2Rad;

                Vector2 laserDir = new Vector2(Mathf.Cos(laserAngleRad), Mathf.Sin(laserAngleRad));
                Vector2 knockbackDir = new Vector2(-laserDir.y, laserDir.x);

                float side = Mathf.Sign(Vector2.Dot(knockbackDir, playerTransform.position - transform.position));
                knockbackDir *= side;
                float knockbackForce = 10f;
                playerHealth.ApplyKnockback(knockbackDir, knockbackForce);
            }
        }
    }

    public void EnableLaserCollider()
    {
        laserCollider.enabled = true;
        particles.Play();
    }

    public void DisableLaserCollider()
    {
        laserCollider.enabled = false;
        particles.Stop();
    }
}

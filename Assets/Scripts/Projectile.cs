using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public GameObject hitEffect;

    private Vector3 direction;
    private int damage;
    private Rigidbody2D rb;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        //rb.linearVelocity = direction * speed;
        rb.AddForce(direction * speed, ForceMode2D.Impulse);
        //BulletEnd(5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            collision.GetComponent<EnemyController>().TakeDamage(damage);
            BulletEnd();
        }
        else if (collision.tag == "Player")
        {
            collision.GetComponent<PlayerMovement>().takeDamage(damage);
            BulletEnd();
        }
        else if (collision.tag == "Walls")
        {
            BulletEnd();
        }
    }

    private void BulletEnd(float time = 0f)
    {
        Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(gameObject, time);
    }
}

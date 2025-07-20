using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public GameObject hitEffect;

    private Vector3 direction;
    private int damage;
    private Rigidbody2D rb;
    private bool playersBullet = false;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    public void SetPlayersBullet(bool val)
    {
        playersBullet = val;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        //rb.linearVelocity = direction * speed;
        rb.AddForce(direction * speed, ForceMode2D.Impulse);
        BulletEnd(5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playersBullet && collision.tag == "Enemy")
        {
            collision.GetComponent<EnemyController>().TakeDamage(damage);
            BulletEnd();
        }
        else if (!playersBullet && collision.tag == "Enemy")
        {
            //collision.GetComponent<EnemyController>().heal(damage);
            //BulletEnd();
        }
        else if (!playersBullet && collision.tag == "Player" && !collision.GetComponent<PlayerMovement>().getIsDashing())
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
        Invoke("InstantiateHitEffect", time);
        Destroy(gameObject, time);
    }

    private void InstantiateHitEffect()
    {
        Instantiate(hitEffect, transform.position, Quaternion.identity);
    }
}

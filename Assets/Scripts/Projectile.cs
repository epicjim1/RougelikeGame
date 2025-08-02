using UnityEngine;

public enum BulletType
{
    Normal,
    Explosive,
    Poison
}

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public GameObject hitEffect;

    public BulletType bulletType = BulletType.Normal;
    public bool isSpeedRandom = false;

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
        if (isSpeedRandom)
        {
            rb.AddForce(direction * Random.Range(speed, speed * 2), ForceMode2D.Impulse);
        }
        else
        {
            rb.AddForce(direction * speed, ForceMode2D.Impulse);
        }
        BulletEnd(5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playersBullet && collision.tag == "Enemy")
        {
            collision.GetComponent<EnemyController>().TakeDamage(damage);
            Hit();
        }
        else if (playersBullet && collision.tag == "Boss")
        {
            collision.GetComponent<GolemBoss>().TakeDamage(damage);
            Hit();
        }
        else if (!playersBullet && collision.tag == "Enemy")
        {
            //collision.GetComponent<EnemyController>().heal(damage);
            //BulletEnd();
        }
        else if (!playersBullet && collision.tag == "Player" && !collision.GetComponent<PlayerMovement>().getIsDashing())
        {
            collision.GetComponent<PlayerMovement>().takeDamage(damage);
            Hit();
        }
        else if (collision.tag == "Walls")
        {
            Hit();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!playersBullet && collision.tag == "Player" && !collision.GetComponent<PlayerMovement>().getIsDashing())
        {
            collision.GetComponent<PlayerMovement>().takeDamage(damage);
            Hit();
        }
    }

    private void Hit()
    {
        if (bulletType == BulletType.Explosive)
        {
            Explode();
        }
        BulletEnd();
    }

    private void Explode()
    {
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, 3f);
        foreach (Collider2D obj in hitObjects)
        {
            if (playersBullet && obj.CompareTag("Enemy"))
            {
                obj.GetComponent<EnemyController>().TakeDamage(damage);
            }
            else if (!playersBullet && obj.CompareTag("Player"))
            {
                obj.GetComponent<PlayerMovement>().takeDamage(damage);
            }
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

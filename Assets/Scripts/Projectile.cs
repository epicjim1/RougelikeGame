using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 direction;
    private int damage;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(collision);
        Destroy(gameObject);
    }
}

using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyData enemyData;

    private PlayerMovement playerMovement;
    private Transform playerTransform;
    private int currentHealth;
    private float lastAttackTime;
    private bool hittingWall = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        playerTransform = player.transform;
        currentHealth = enemyData.maxHealth;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance < enemyData.attackRange)
        {
            // Attack
            animator.SetBool("running", false);
            if (Time.time > lastAttackTime + enemyData.attackCooldown)
            {
                if (enemyData.isRanged)
                    Shoot();
                else
                    MeleeAttack();

                lastAttackTime = Time.time;
            }
        }
        else if (distance < enemyData.chaseRange && !hittingWall)
        {
            animator.SetBool("running", true);

            Vector2 direction = (playerTransform.position - transform.position).normalized;
            Vector2 newPosition = rb.position + direction * enemyData.moveSpeed * Time.deltaTime;
            rb.MovePosition(newPosition);
            //transform.Translate(direction * enemyData.moveSpeed * Time.deltaTime);

            // Flip sprite if needed
            if (direction.x < 0)
                spriteRenderer.flipX = true;
            else if (direction.x > 0)
                spriteRenderer.flipX = false;
        }
        else
        {
            animator.SetBool("running", false);
        }
    }

    void MeleeAttack()
    {
        animator.SetTrigger("attack");
        Debug.Log($"{enemyData.enemyName} performs melee attack!");
        // Deal damage to playerTransform here
        if (!playerMovement.getIsDashing())
        {
            playerMovement.takeDamage(enemyData.damage);

            // Knockback direction from enemy to player
            Vector2 knockbackDir = (playerTransform.position - transform.position).normalized;

            // Knockback force
            float knockbackForce = enemyData.knockbackStrength; // Adjust this value as needed

            // Apply knockback to the player
            playerMovement.ApplyKnockback(knockbackDir, knockbackForce);
        }
    }

    void Shoot()
    {
        if (enemyData.projectilePrefab == null) return;

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        GameObject proj = Instantiate(enemyData.projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().linearVelocity = direction * 5f;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Walls"))
        {
            hittingWall = true;
            animator.SetBool("running", false);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Walls"))
        {
            hittingWall = false;
        }
    }
}

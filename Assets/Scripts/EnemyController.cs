using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathfinding;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyController : MonoBehaviour
{
    public EnemyData enemyData;
    public GameObject weapon;
    public WeaponData weaponData;
    public Transform aimTransform;

    private PlayerMovement playerMovement;
    private Transform playerTransform;
    private BoxCollider2D playerCollider;
    private int currentHealth;
    private float lastAttackTime;
    private bool isChasing = false;
    private float visionOffsetY = 0.7f;

    /*private float nextWaypointDistance = 3f;
    private int currentWaypoint = 0;
    private float pathUpdateRate = 0.5f;
    private float lastPathUpdate = 0f;
    private bool reachedEndOfPath = false;

    private Path path;
    private Seeker seeker;*/
    private AIPath aiPath;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        playerTransform = player.transform;
        playerCollider = player.GetComponent<BoxCollider2D>();
        currentHealth = enemyData.maxHealth;

        //seeker = GetComponent<Seeker>();
        aiPath = GetComponent<AIPath>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        aiPath.canMove = false;
        aiPath.maxSpeed = enemyData.moveSpeed;

        if (weapon != null)
        {
            weapon.GetComponent<Weapon>().SetData(weaponData);
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        //if (isChasing && visionOffsetY != 0)
        //    visionOffsetY = 0;

        //Vector2 playerTarget = new Vector2(playerTransform.position.x, playerTransform.position.y - visionOffsetY);
        Vector2 origin = new Vector2(transform.position.x, transform.position.y -.0f);
        Vector2 playerTarget = playerCollider.ClosestPoint(origin);
        Vector2 directionToPlayer = (playerTarget - origin).normalized;
        float distance = Vector2.Distance(origin, playerTarget);

        RaycastHit2D hit = Physics2D.Raycast(origin, directionToPlayer, distance, LayerMask.GetMask("Walls"));
        bool playerVisible = hit.collider == null;
        Debug.DrawRay(origin, directionToPlayer * distance, playerVisible ? Color.green : Color.red);

        if (!enemyData.isRanged)
        {
            if (distance < enemyData.chaseRange && playerVisible && !isChasing)
            {
                isChasing = true;
            }

            if (distance < enemyData.attackRange)
            {
                aiPath.canMove = false;
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
            else if (isChasing)
            {
                //aiPath.canMove = true;
                animator.SetBool("running", true);

                /*if (Time.time > lastPathUpdate + pathUpdateRate)
                {
                    UpdatePath();
                    lastPathUpdate = Time.time;
                }

                FollowPath();*/
                //aiPath.destination = playerTransform.position;

                if (aiPath.desiredVelocity.x <= -0.01f)
                    spriteRenderer.flipX = true;
                else if (aiPath.desiredVelocity.x > -0.01f)
                    spriteRenderer.flipX = false;

                if (distance < 1.1f && aiPath.reachedEndOfPath)
                {
                    aiPath.canMove = false;
                    Vector2 newPosition = rb.position + directionToPlayer * enemyData.moveSpeed * 2 * Time.deltaTime;
                    rb.MovePosition(newPosition);
                }
                else
                {
                    aiPath.canMove = true;
                    aiPath.destination = playerTransform.position;
                }
            }
            else
            {
                aiPath.canMove = false;
                animator.SetBool("running", false);
                //Patrol or return to original spot
            }
        }
        else
        {
            /*Vector2 playerTargetToHead = new Vector2(playerTransform.position.x + 0f, playerTransform.position.y - 0.55f);
            Vector2 originFromFeet = new Vector2(transform.position.x, transform.position.y - .7f);
            Vector2 directionToPlayerFeet = (playerTargetToHead - originFromFeet).normalized;
            float distanceFromFeet = Vector2.Distance(originFromFeet, playerTargetToHead);

            RaycastHit2D hit2 = Physics2D.Raycast(originFromFeet, directionToPlayerFeet, distanceFromFeet, LayerMask.GetMask("Walls"));
            bool playerVisibleFromFeet = hit2.collider == null;
            Debug.DrawRay(originFromFeet, directionToPlayerFeet * distanceFromFeet, playerVisibleFromFeet ? Color.green : Color.red);*/

            if (distance < enemyData.chaseRange && playerVisible && !isChasing)
            {
                isChasing = true;
            }

            if (distance < enemyData.attackRange && playerVisible)// && playerVisibleFromFeet)
            {
                aiPath.canMove = false;
                animator.SetBool("running", false);
                Aiming();

                if (Time.time > lastAttackTime + enemyData.attackCooldown)
                {
                    Shoot();
                    lastAttackTime = Time.time;
                }
            }
            else if (isChasing)
            {
                aiPath.canMove = true;
                aiPath.destination = playerTransform.position;
                animator.SetBool("running", true);
                Aiming();

                if (aiPath.desiredVelocity.x <= -0.01f)
                    spriteRenderer.flipX = true;
                else if (aiPath.desiredVelocity.x > -0.01f)
                    spriteRenderer.flipX = false;
            }
            else
            {
                aiPath.canMove = false;
                animator.SetBool("running", false);
                //Patrol or return to original spot
            }
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
        /*if (enemyData.projectilePrefab == null) return;

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        GameObject proj = Instantiate(enemyData.projectilePrefab, firePoint.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().linearVelocity = direction * 5f;*/

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        weapon?.GetComponent<Weapon>().Shoot(direction);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{enemyData.enemyName} has died.");
        aiPath.canMove = false;
        GetComponent<BoxCollider2D>().enabled = false;
        rb.simulated = false;
        this.enabled = false;
        Destroy(gameObject, 2f);
    }

    /* void OnCollisionEnter2D(Collision2D collision)
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
    }*/

    /*private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void UpdatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, playerTransform.position, OnPathComplete);
        }
    }

    private void FollowPath()
    {
        // Check if we have a valid path
        if (path == null || path.vectorPath == null || path.vectorPath.Count == 0)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 pathDirection = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        //rb.AddForce(pathDirection * enemyData.moveSpeed * Time.deltaTime);

       // Vector2 targetPosition = (Vector2)path.vectorPath[currentWaypoint];
        //Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, enemyData.moveSpeed * Time.fixedDeltaTime);
        //rb.MovePosition(newPosition);

        Vector2 newPosition = rb.position + pathDirection * enemyData.moveSpeed * Time.deltaTime;
        rb.MovePosition(newPosition);
        rb.linearVelocity = pathDirection * enemyData.moveSpeed;

        if (pathDirection.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (pathDirection.x > 0)
        {
            spriteRenderer.flipX = false;
        }

        // Check if we're close enough to the current waypoint to move to the next one
        float pathDistance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (pathDistance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }*/

    private void Aiming()
    {
        Vector3 aimDir = (playerTransform.position - aimTransform.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        aimTransform.eulerAngles = new Vector3(0, 0, angle);

        Vector3 localScale = Vector3.one;
        localScale.y = (angle > 90 || angle < -90) ? -1f : 1f;
        aimTransform.localScale = localScale;
    }
}

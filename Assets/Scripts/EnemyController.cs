using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Pathfinding;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.UI.Image;

public class EnemyController : MonoBehaviour
{
    public EnemyData enemyData;
    public GameObject weapon;
    public WeaponData weaponData;
    public Transform aimTransform;
    public float separationRadius = 1f;
    public float separationForce = 5f;

    private PlayerMovement playerMovement;
    private Transform playerTransform;
    private BoxCollider2D playerCollider;
    private int currentHealth;
    private float lastAttackTime;
    private bool canMove = true;
    [HideInInspector] public bool isChasing = false;
    private float shootRayRadius = 1;
    private Color originalColor;
    private bool isFlashing = false;

    private float visionCheckInterval = 0.2f;
    private float lastVisionCheckTime;
    private float distanceCheckInterval = 0.2f;
    private float lastDistanceCheckTime;
    private bool playerVisible;
    private float distance;
    private Vector2 directionToPlayer;

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

        aiPath = GetComponent<AIPath>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        originalColor = spriteRenderer.color;

        aiPath.canMove = false;
        aiPath.maxSpeed = enemyData.moveSpeed;
        spriteRenderer.flipX = Random.value < 0.5f;
        if (weapon != null)
        {
            weapon.GetComponent<Weapon>().SetData(weaponData);
        }
    }

    void Update()
    {
        if (playerTransform == null || !canMove) return;

        if (Time.time > lastVisionCheckTime + visionCheckInterval)
        {
            lastVisionCheckTime = Time.time;
            //Vector2 playerTarget = new Vector2(playerTransform.position.x, playerTransform.position.y - 0.7f);
            Vector2 origin = new Vector2(transform.position.x, transform.position.y);
            Vector2 playerTarget = playerCollider.ClosestPoint(origin);
            directionToPlayer = (playerTarget - origin).normalized;
            distance = Vector2.Distance(origin, playerTarget);

            /*if (distance < enemyData.chaseRange)
            {
                visionCheckInterval = 0;
            }
            else
            {
                visionCheckInterval = 0.2f;
            }*/

            //if (!isChasing)
            //{
                RaycastHit2D hit;
                if (distance > 4 && enemyData.isRanged && !isChasing)
                {
                    shootRayRadius = 1;
                    hit = Physics2D.CircleCast(origin, shootRayRadius, directionToPlayer, distance, LayerMask.GetMask("Walls"));
                    playerVisible = hit.collider == null;
                }
                else
                {
                    hit = Physics2D.Raycast(origin, directionToPlayer, distance, LayerMask.GetMask("Walls"));
                    playerVisible = hit.collider == null;
                }
            Debug.DrawRay(origin, directionToPlayer * distance, playerVisible ? Color.green : Color.red);
            //}
        }


        if (!enemyData.isRanged)
        {
            //Debug.DrawRay(origin, directionToPlayer * distance, playerVisible ? Color.green : Color.red);

            if (distance < enemyData.chaseRange && playerVisible && !isChasing)
            {
                Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, 5f);
                foreach (Collider2D obj in hitObjects)
                {
                    if (obj.CompareTag("Enemy"))
                    {
                        obj.GetComponent<EnemyController>().isChasing = true;
                    }
                }
                isChasing = true;
            }

            if (distance < enemyData.attackRange)
            {
                aiPath.canMove = false;
                animator.SetBool("running", false);

                if (Time.time > lastAttackTime + enemyData.attackCooldown)
                {
                    MeleeAttack();
                    lastAttackTime = Time.time;
                }
            }
            else if (isChasing)
            {
                animator.SetBool("running", true);

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
                    if (Time.time > lastDistanceCheckTime + distanceCheckInterval)
                    {
                        lastDistanceCheckTime = Time.time;
                        aiPath.canMove = true;
                        Vector2 separationOffset = ComputeSeparationOffset();
                        Vector3 flockedDestination = playerTransform.position + (Vector3)separationOffset;

                        aiPath.destination = flockedDestination;
                        //aiPath.destination = playerTransform.position;
                    }
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
            //Debug.DrawRay(origin, directionToPlayer * distance, playerVisibleRanged ? Color.green : Color.red);

            if (distance < enemyData.chaseRange && playerVisible && !isChasing)
            {
                Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, 5f);
                foreach (Collider2D obj in hitObjects)
                {
                    if (obj.CompareTag("Enemy"))
                    {
                        obj.GetComponent<EnemyController>().isChasing = true;
                    }
                }
                isChasing = true;
            }

            if (distance < enemyData.attackRange && playerVisible)
            {
                aiPath.canMove = false;
                animator.SetBool("running", false);
                Aiming();

                if (Time.time > lastAttackTime + enemyData.attackCooldown)
                {
                    Shoot(distance);
                    if (distance < .5f)
                    {
                        MeleeAttack();
                    }
                    lastAttackTime = Time.time;
                }
            }
            else if (isChasing)
            {
                aiPath.canMove = true;
                Vector2 separationOffset = ComputeSeparationOffset();
                Vector3 flockedDestination = playerTransform.position + (Vector3)separationOffset;

                aiPath.destination = flockedDestination;
                //aiPath.destination = playerTransform.position;
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
        //Debug.Log($"{enemyData.enemyName} performs melee attack!");

        if (!playerMovement.getIsDashing())
        {
            playerMovement.takeDamage(enemyData.damage);

            Vector2 knockbackDir = (playerTransform.position - transform.position).normalized;
            float knockbackForce = enemyData.knockbackStrength;
            playerMovement.ApplyKnockback(knockbackDir, knockbackForce);
        }
    }

    void Shoot(float distance)
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Transform weaponFirePoint = weapon.GetComponent<Weapon>().firePoint.transform;
        Vector2 aimDir = (playerCollider.ClosestPoint(origin) - (new Vector2(weaponFirePoint.position.x, weaponFirePoint.position.y))).normalized;

        weapon?.GetComponent<Weapon>().Shoot(aimDir);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (!isFlashing)
        {
            StartCoroutine(FlashCoroutine(Color.red));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void heal(int amount)
    {
        currentHealth += amount;
        if (!isFlashing)
        {
            StartCoroutine(FlashCoroutine(Color.green));
        }

        if (currentHealth > enemyData.maxHealth)
        {
            currentHealth = enemyData.maxHealth;
        }
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration = .1f)
    {
        StartCoroutine(KnockbackCoroutine(direction, force, duration));
    }

    private System.Collections.IEnumerator KnockbackCoroutine(Vector2 direction, float force, float duration)
    {
        canMove = false;
        aiPath.canMove = false;
        Debug.Log("Enemy knocked back");

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(.5f);

        aiPath.canMove = true;
        canMove = true;
    }

    private System.Collections.IEnumerator FlashCoroutine(Color c)
    {
        isFlashing = true;
        //spriteRenderer.color = c;
        spriteRenderer.material.SetColor("_Color", c);
        spriteRenderer.material.SetInt("_Flash", 1);
        yield return new WaitForSeconds(.1f);
        spriteRenderer.material.SetInt("_Flash", 0);
        //spriteRenderer.color = originalColor;
        isFlashing = false;
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

    private void Aiming()
    {
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Vector2 aimDir = (playerCollider.ClosestPoint(origin) - (new Vector2(aimTransform.position.x, aimTransform.position.y))).normalized;
        //Vector3 aimDir = (playerTransform.position - aimTransform.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        aimTransform.eulerAngles = new Vector3(0, 0, angle);

        Vector3 localScale = Vector3.one;
        localScale.y = (angle > 90 || angle < -90) ? -1f : 1f;
        aimTransform.localScale = localScale;
    }

    Vector2 ComputeSeparationOffset()
    {
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(transform.position, separationRadius, LayerMask.GetMask("Enemy"));
        Vector2 separation = Vector2.zero;
        int count = 0;

        foreach (Collider2D neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;

            Vector2 diff = (Vector2)(transform.position - neighbor.transform.position);
            float distance = diff.magnitude;

            if (distance < 0.1f)
            {
                // Small random push to break perfect overlap
                Debug.Log("enemy too close to each other");
                diff = Random.insideUnitCircle.normalized * 2f;
                distance = 1f;
            }

            separation += diff.normalized / distance;
            count++;
        }

        if (count > 0)
        {
            separation /= count;
            separation *= separationForce;
        }

        return separation;
    }
}

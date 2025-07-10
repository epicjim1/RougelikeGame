using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Pathfinding;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.UI.Image;

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
    private float shootRayRadius = 1;
    private Color originalColor;
    private bool isFlashing = false;

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
        Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerTarget = playerCollider.ClosestPoint(origin);
        Vector2 directionToPlayer = (playerTarget - origin).normalized;
        float distance = Vector2.Distance(origin, playerTarget);

        RaycastHit2D hit = Physics2D.Raycast(origin, directionToPlayer, distance, LayerMask.GetMask("Walls"));
        bool playerVisible = hit.collider == null;

        if (!enemyData.isRanged)
        {
            Debug.DrawRay(origin, directionToPlayer * distance, playerVisible ? Color.green : Color.red);

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
                        Shoot(distance);
                    else
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

            //float newDistance;
            if (distance > 4)
            {
                shootRayRadius = 1;
                //newDistance = distance - 3;
                hit = Physics2D.CircleCast(origin, shootRayRadius, directionToPlayer, distance, LayerMask.GetMask("Walls"));
            }
            else
            {
                //shootRayRadius = 0.1f;
                //newDistance = distance;

            }
            bool playerVisibleRanged = hit.collider == null;
            Debug.DrawRay(origin, directionToPlayer * distance, playerVisibleRanged ? Color.green : Color.red);

            if (distance < enemyData.chaseRange && playerVisibleRanged && !isChasing)
            {
                isChasing = true;
            }

            if (distance < enemyData.attackRange && playerVisibleRanged)// && playerVisibleFromFeet)
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
        /*Vector2 origin = new Vector2(transform.position.x, transform.position.y);
        Vector2 directionToPlayer;

        if (distance > 4)
        {
            directionToPlayer = (playerTransform.position - transform.position).normalized;
        }
        else
        {
            directionToPlayer = (playerCollider.ClosestPoint(origin) - origin).normalized;
        }*/
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
            StartCoroutine(FlashCoroutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashCoroutine()
    {
        isFlashing = true;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(.1f);
        spriteRenderer.color = originalColor;
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
}

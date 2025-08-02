using Pathfinding;
using Pathfinding.Examples;
using TMPro;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public enum GolemPhase { Phase1, Phase2, BulletHell, Dead }

public class GolemBoss : MonoBehaviour
{
    public string bossName;
    public float health = 100;
    private Transform player;
    public float meleeRange = 2f;
    public float meleeRadius = 3f;
    public float aimSpeed = 180f;
    public Transform[] corners;
    public GameObject projectilePrefab;
    public GameObject laser;
    public GameObject meleePoint;

    public Color mainFill;
    public Color EaseFill;
    public Color bgFill;

    private GolemPhase currentPhase = GolemPhase.Phase1;
    private bool fightStarted = false;
    private bool isDoingAttack = false;
    private bool isLaserAttacking = false;
    private AIPath aiPath;
    private Animator anim;
    private SpriteRenderer sp;
    private CapsuleCollider2D capsuleCollider;
    private GameObject bossBar;
    private Healthbar healthBar;
    [HideInInspector] public RoomController roomController;

    private void Awake()
    {
        bossBar = GameObject.FindGameObjectWithTag("bossBar");
        bossBar.SetActive(false);
        healthBar = bossBar.GetComponent<Healthbar>();
        healthBar.maxHealth = health;
        healthBar.Starting();
        bossBar.GetComponentInChildren<TMP_Text>().text = bossName;
        healthBar.healthbarImage.color = mainFill;
        healthBar.easeHealthbarImage.color = EaseFill;
        healthBar.bgHealthbarImage.color = bgFill;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        aiPath = GetComponent<AIPath>();
        anim = GetComponent<Animator>();
        sp = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        Random.InitState(System.DateTime.Now.Millisecond);
        aiPath.canMove = false;
        capsuleCollider.enabled = false;
    }

    private void Update()
    {
        if (!fightStarted || currentPhase == GolemPhase.Dead) //|| isDoingAttack)
            return;

        if (isLaserAttacking)
        {
            Aiming();
        }

        float directionToPlayer = player.position.x - transform.position.x;
        if (directionToPlayer < 0)
        {
            sp.flipX = true;
            meleePoint.transform.localPosition = new Vector3(-1.81f, -0.83f, 0);
        }
        else if (directionToPlayer > 0)
        {
            sp.flipX = false;
            meleePoint.transform.localPosition = new Vector3(1.81f, -0.83f, 0);
        }

        switch (currentPhase)
        {
            case GolemPhase.Phase1:
            case GolemPhase.Phase2:
                break;
            case GolemPhase.BulletHell:
                break;
        }
    }

    public System.Collections.IEnumerator StartBossFight()
    {
        bossBar.SetActive(true);
        anim.SetTrigger("entry");
        yield return new WaitForSeconds(1.5f);
        fightStarted = true;
        aiPath.canMove = true;
        capsuleCollider.enabled = true;
        StartCoroutine(ActionDecisionLoop());
    }

    public void TakeDamage(int amount)
    {
        if (currentPhase == GolemPhase.Dead || currentPhase == GolemPhase.BulletHell)
            return;

        healthBar.takeDamage(amount);
        health -= amount;
        /*if (!isFlashing)
        {
            StartCoroutine(FlashCoroutine(Color.red));
        }*/

        if (health <= healthBar.maxHealth / 2 && currentPhase == GolemPhase.Phase1)
        {
            StartCoroutine(EnterBulletHellPhase());
        }
        else if (health <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator ActionDecisionLoop()
    {
        while (currentPhase != GolemPhase.Dead)
        {
            yield return new WaitUntil(() => (currentPhase == GolemPhase.Phase1 || currentPhase == GolemPhase.Phase2) && !isDoingAttack);

            float decisionWaitTime = Random.Range(5, 10);
            //yield return new WaitForSeconds(decisionWaitTime);

            if (!isDoingAttack)
            {
                bool randomBool = Random.value > 0.5f;

                if (randomBool)
                {
                    yield return StartCoroutine(DoMeleeAttack(decisionWaitTime));
                }
                else
                {
                    yield return StartCoroutine(DoLaserAttack());
                }
            }
        }
    }

    System.Collections.IEnumerator DoMeleeAttack(float decisionDuration)
    {
        float startTime = Time.time;
        isDoingAttack = true;

        while (Time.time - startTime < decisionDuration && currentPhase != GolemPhase.BulletHell && currentPhase != GolemPhase.Dead)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= meleeRange)
            {
                aiPath.canMove = false;
                anim.SetTrigger("melee");
                yield return new WaitForSeconds(1f);
                aiPath.canMove = true;
            }
            else
            {
                aiPath.canMove = true;
                aiPath.destination = player.position;
                yield return null;
            }
        }

        aiPath.canMove = true;
        isDoingAttack = false;
    }

    public void MeleeAttack()
    {
        meleePoint.GetComponent<ParticleSystem>().Play();
        Collider2D hit = Physics2D.OverlapCircle(meleePoint.transform.position, meleeRadius, LayerMask.GetMask("Player"));

        if (hit != null && hit.CompareTag("Player"))
        {
            PlayerMovement player = hit.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.takeDamage(15);

                Vector2 knockbackDir = (hit.transform.position - meleePoint.transform.position).normalized;
                player.ApplyKnockback(knockbackDir, 30);
            }
        }
    }

    System.Collections.IEnumerator DoLaserAttack()
    {
        isDoingAttack = true;
        aiPath.canMove = false;

        Vector3 aimDir = (player.position - laser.transform.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        laser.transform.eulerAngles = new Vector3(0, 0, angle);

        isLaserAttacking = true;
        laser.SetActive(true);
        anim.SetTrigger("laserOn");
        yield return new WaitForSeconds(3.125f);
        
        laser.SetActive(false);
        isLaserAttacking = false;
        anim.SetTrigger("laserOff");
        yield return new WaitForSeconds(1f);
        aiPath.canMove = true;
        isDoingAttack = false;
    }

    System.Collections.IEnumerator EnterBulletHellPhase()
    {
        //StopAllCoroutines();
        isDoingAttack = true;
        currentPhase = GolemPhase.BulletHell;

        anim.SetTrigger("shieldOn");
        Transform corner = GetRandomCorner();
        aiPath.destination = corner.position;
        yield return new WaitUntil(() => aiPath.reachedDestination || Vector3.Distance(transform.position, corner.position) < 0.5f);
        aiPath.canMove = false;

        yield return StartCoroutine(FireBulletHellPattern());

        yield return new WaitForSeconds(1f);

        anim.SetTrigger("shieldOff");
        currentPhase = GolemPhase.Phase2;
        aiPath.maxSpeed = 5f;
        //aimSpeed = 30f;
        aiPath.canMove = true;
        isDoingAttack = false;
    }

    /*System.Collections.IEnumerator FireBulletHellPattern()
    {
        int bursts = 5;
        float interval = 2f;
        int bulletsPerBurst = 20;

        for (int i = 0; i < bursts; i++)
        {
            for (int j = 0; j < bulletsPerBurst; j++)
            {
                float angle = j * (360f / bulletsPerBurst);
                Quaternion rotation = Quaternion.Euler(0, angle, 0);
                Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
                Debug.Log("proj fired");
                //FireProjectile(transform.position, dir);

                GameObject projectile = Instantiate(
                    projectilePrefab,
                    transform.position,
                    rotation
                );

                Projectile projScript = projectile.GetComponent<Projectile>();
                if (projScript != null)
                {
                    projScript.SetPlayersBullet(false);
                    projScript.SetDirection(dir);
                    projScript.SetDamage(10);
                }
            }
            yield return new WaitForSeconds(interval);
        }
    }*/

    private System.Collections.IEnumerator FireBulletHellPattern()
    {
        if (projectilePrefab == null || player == null)
        {
            Debug.LogError("Required references for bullet hell are not set!");
            yield break;
        }

        int bursts = 10;
        float interval = .7f;

        for (int i = 0; i < bursts; i++)
        {
            int bulletsPerBurst = Random.Range(10, 25);
            float bulletHellSpreadAngle = Random.Range(180f, 240f);

            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float centerAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            float startAngle = centerAngle - bulletHellSpreadAngle / 2;
            float angleStep = bulletHellSpreadAngle / (bulletsPerBurst - 1);

            for (int j = 0; j < bulletsPerBurst; j++)
            {
                float angle = startAngle + (j * angleStep);
                float angleRad = angle * Mathf.Deg2Rad;

                Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0);

                Quaternion rot = Quaternion.Euler(0, 0, angle);
                GameObject projectile = Instantiate(projectilePrefab, transform.position, rot);


                Projectile projScript = projectile.GetComponent<Projectile>();
                if (projScript != null)
                {
                    projScript.SetPlayersBullet(false);
                    projScript.SetDirection(direction);
                    projScript.SetDamage(10);
                }
            }
            yield return new WaitForSeconds(interval); // Interval between bursts
        }
    }

    private Transform GetRandomCorner()
    {
        return corners[Random.Range(0, corners.Length)];
    }

    private void Die()
    {
        StopAllCoroutines();
        isDoingAttack = true;
        laser.SetActive(false);
        currentPhase = GolemPhase.Dead;
        aiPath.canMove = false;
        capsuleCollider.enabled = false;
        if (roomController != null) roomController.OnEnemyDefeated(this.gameObject);
        bossBar.GetComponentInChildren<Animator>().SetTrigger("bossDied");
        anim.SetTrigger("die");
    }

    private void Aiming()
    {
        Vector3 aimDir = (player.position - laser.transform.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        //Vector3 localScale = Vector3.one;
        //localScale.x = (angle >= 90 || angle <= -90) ? -1f : 1f;
        //laser.transform.localScale = localScale;

        /*if (transform.localScale.x < 0)
        {
            angle -= 180f;
        }*/

        float currentAngle = laser.transform.eulerAngles.z;
        // smoothedAngle = Mathf.LerpAngle(currentAngle, angle, aimSpeed * Time.deltaTime / 360f);
        float smoothedAngle = Mathf.MoveTowardsAngle(currentAngle, angle, aimSpeed * Time.deltaTime);

        laser.transform.eulerAngles = new Vector3(0, 0, smoothedAngle);
    }

    private void OnDrawGizmos()
    {
        if (meleePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleePoint.transform.position, meleeRadius);
        }
    }
}

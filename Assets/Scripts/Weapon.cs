using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class Weapon : MonoBehaviour
{
    [HideInInspector]
    public WeaponData weaponData;

    [Header("Projectile Settings")]
    [SerializeField]
    private bool isPlayer = true;
    public Transform firePoint;
    public ParticleSystem muzzleFlash;

    private LineRenderer aimLine;
    private float lastShotTime;
    [HideInInspector] public int currentAmmo;
    [HideInInspector] public bool isReloading = false;
    private Quaternion originalRotation;

    private void Start()
    {
        originalRotation = transform.rotation;
        //aimLine = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (weaponData.isRanged)
        {
            //Laser();
        }
        if (isPlayer)
        {
            if (weaponData.isRanged && Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < weaponData.maxAmmo)
            {
                StartCoroutine(Reload());
            }
        }
    }

    public void SetData(WeaponData data)
    {
        weaponData = data;
        currentAmmo = weaponData.maxAmmo;
    }

    public void Shoot(Vector3 direction)
    {
        if (isReloading)
            return;

        if (weaponData.isRanged)
        {
            if (currentAmmo <= 0)
            {
                if (!isPlayer)
                {
                    StartCoroutine(Reload());
                }
                Debug.Log("Out of ammo. Press R to reload.");
                return;
            }

            if (Time.time - lastShotTime < 1f / weaponData.fireRate)
                return;

            currentAmmo--;
            lastShotTime = Time.time;

            this.gameObject.GetComponent<Animator>().SetTrigger("Shoot");
            TempCamShake.Instance.Shake(0.1f, 0.1f);
            if (muzzleFlash != null)
                muzzleFlash.Emit(30);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            if (weaponData.fireType == GunFireType.Normal)
            {
                Quaternion rot = Quaternion.Euler(0, 0, angle);

                GameObject projectile = Instantiate(
                    weaponData.projectilePrefab,
                    firePoint.position,
                    rot
                );

                Projectile projScript = projectile.GetComponent<Projectile>();
                if (projScript != null)
                {
                    projScript.SetPlayersBullet(isPlayer);
                    projScript.SetDirection(direction);
                    projScript.SetDamage(weaponData.damage);
                }
            }
            else if (weaponData.fireType == GunFireType.Multiple)
            {
                int pellets = 8; // You can expose this in weaponData if needed
                float spreadAngle = 5f; // total spread in degrees

                for (int i = 0; i < pellets; i++)
                {
                    // Evenly distribute pellets within spread angle
                    float offset = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, (float)i / (pellets - 1));
                    float finalAngle = angle + offset;

                    Quaternion rot = Quaternion.Euler(0, 0, finalAngle);
                    Vector2 spreadDir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad)).normalized;

                    GameObject pellet = Instantiate(weaponData.projectilePrefab, firePoint.position, rot);
                    Projectile projScript = pellet.GetComponent<Projectile>();
                    if (projScript != null)
                    {
                        projScript.SetPlayersBullet(isPlayer);
                        projScript.SetDirection(spreadDir);
                        projScript.SetDamage(weaponData.damage);
                    }
                }
            }
        }
        else
        {
            if (Time.time - lastShotTime < 1f / weaponData.fireRate)
                return;

            lastShotTime = Time.time;
            this.gameObject.GetComponent<Animator>().SetTrigger("Shoot");
            //PerformMeleeAttack();
        }
    }

    private void PerformMeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(firePoint.position, 1, LayerMask.GetMask("Enemy"));

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyController controller = enemy.GetComponent<EnemyController>();
            controller.TakeDamage(weaponData.damage);
            Vector2 knockbackDir = (enemy.transform.position - firePoint.position).normalized;
            float knockbackForce = weaponData.knockbackStrength;
            controller.ApplyKnockback(knockbackDir, knockbackForce);
            Debug.Log($"Hit {enemy.name} with melee");
        }
    }

    private System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        this.gameObject.GetComponent<Animator>().enabled = false;
        //transform.localRotation = Quaternion.Euler(0, 0, -45f);
        Debug.Log("Reloading...");
        float rotationDuration = 0.1f;
        float timer = 0f;
        while (timer < rotationDuration)
        {
            transform.localRotation = Quaternion.Slerp(Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 0, -45f), timer / rotationDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = Quaternion.Euler(0, 0, -45f);

        yield return new WaitForSeconds(weaponData.reloadTime);

        Quaternion startLerpBackRotation = transform.localRotation;
        timer = 0f;
        while (timer < rotationDuration)
        {
            transform.localRotation = Quaternion.Slerp(startLerpBackRotation, Quaternion.Euler(0, 0, 0), timer / rotationDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = Quaternion.Euler(0, 0, 0);

        currentAmmo = weaponData.maxAmmo;
        this.gameObject.GetComponent<Animator>().enabled = true;
        isReloading = false;
        //transform.localRotation = Quaternion.Euler(0, 0, 0); ;
        Debug.Log("Reload complete.");
    }

    private void Laser()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        aimLine.SetPosition(0, firePoint.position);
        aimLine.SetPosition(1, mousePos);
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 1);
        }
    }
}

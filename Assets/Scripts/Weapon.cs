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
        else
        {
            if (Time.time - lastShotTime < 1f / weaponData.fireRate)
                return;

            lastShotTime = Time.time;
            this.gameObject.GetComponent<Animator>().SetTrigger("Shoot");
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
}

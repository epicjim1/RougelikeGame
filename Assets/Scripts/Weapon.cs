using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class Weapon : MonoBehaviour
{
    private WeaponData weaponData;
    private float lastShotTime;

    [Header("Projectile Settings")]
    public Transform firePoint; // assign this in prefab, typically at the gun barrel
    public ParticleSystem muzzleFlash;
    private LineRenderer aimLine;

    private void Start()
    {
        aimLine = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (weaponData.isRanged)
        {
            Laser();
        }
    }

    public void SetData(WeaponData data)
    {
        weaponData = data;

        // Optional: Change visuals based on weaponData (e.g., mesh, sprite)
    }

    public void Shoot(Vector3 direction)
    {
        if (Time.time - lastShotTime < 1f / weaponData.fireRate)
            return;

        lastShotTime = Time.time;

        if (weaponData.isRanged)
        {
            this.gameObject.GetComponent<Animator>().SetTrigger("Shoot");
            TempCamShake.Instance.Shake(0.1f, 0.1f);
            muzzleFlash.Emit(30);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0, 0, angle);

            GameObject projectile = Instantiate(
                weaponData.projectilePrefab,
                firePoint.position,
                rot
            );

            // Apply direction and damage to the projectile
            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null)
            {
                projScript.SetDirection(direction);
                projScript.SetDamage(weaponData.damage);
            }
        }
        else
        {
            //Debug.Log("IsSword");

            this.gameObject.GetComponent<Animator>().SetTrigger("Shoot");
        }
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

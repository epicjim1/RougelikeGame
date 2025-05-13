using UnityEngine;

public class Weapon : MonoBehaviour
{
    private WeaponData weaponData;
    private float lastShotTime;

    [Header("Projectile Settings")]
    public Transform firePoint; // assign this in prefab, typically at the gun barrel

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

        if (weaponData.projectilePrefab != null && firePoint != null)
        {
            this.gameObject.GetComponent<Animator>().SetTrigger("Shoot");

            GameObject projectile = Instantiate(
                weaponData.projectilePrefab,
                firePoint.position,
                Quaternion.LookRotation(Vector3.forward, direction) // 2D: can also use Quaternion.identity and rotate manually
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
            Debug.LogWarning("Weapon prefab or fire point not set up.");
        }
    }
}

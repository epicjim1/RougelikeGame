using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [Header("Weapon Management")]
    public List<WeaponData> allWeapons; // All possible weapons (assigned in Inspector)
    public List<WeaponData> unlockedWeapons = new List<WeaponData>();
    private int currentWeaponIndex = 0;

    [Header("References")]
    public Transform weaponHolder;
    public GameObject weaponPickupPrefab;
    public Transform weaponDropPoint;
    public WeaponUI weaponUI;

    private Weapon currentWeapon;

    private void Start()
    {
        // Get starting weapon from GameManager
        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.startingWeapon))
        {
            WeaponData startingWeapon = FindWeaponByName(GameManager.Instance.startingWeapon);
            if (startingWeapon != null)
            {
                UnlockWeapon(startingWeapon);
                EquipWeapon(0);
            }
            else
            {
                Debug.LogWarning($"Starting weapon '{GameManager.Instance.startingWeapon}' not found in allWeapons list!");
                // Fallback to first weapon if starting weapon not found
                if (allWeapons.Count > 0)
                {
                    UnlockWeapon(allWeapons[0]);
                    EquipWeapon(0);
                }
            }
        }
        else
        {
            if (unlockedWeapons.Count == 1)
            {
                UnlockWeapon(unlockedWeapons[0]);
                EquipWeapon(0);
            }
            else
            {
                UnlockWeapon(allWeapons[0]);
                EquipWeapon(0);
            }
        }
    }

    private void Update()
    {
        HandleShooting();
        HandleWeaponSwap();

        if (!currentWeapon.isReloading && currentWeapon.currentAmmo == currentWeapon.weaponData.maxAmmo)
        {
            weaponUI.currAmmoText.text = currentWeapon.currentAmmo.ToString();
        }
    }

    private WeaponData FindWeaponByName(string weaponName)
    {
        foreach (WeaponData weapon in allWeapons)
        {
            if (weapon != null && weapon.name.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase))
            {
                return weapon;
            }
        }
        return null;
    }

    void HandleShooting()
    {
        if (!GameManager.Instance.GameIsPaused && !GameManager.Instance.GameIsLost && Input.GetMouseButton(0))
        {
            Vector3 dir = (GetMouseWorldPosition() - weaponHolder.position).normalized;
            currentWeapon?.Shoot(dir);
            weaponUI.currAmmoText.text = currentWeapon.currentAmmo.ToString();
        }
    }

    void HandleWeaponSwap()
    {
        if (!GameManager.Instance.GameIsPaused && !GameManager.Instance.GameIsLost && Input.GetKeyDown(KeyCode.Q) && unlockedWeapons.Count > 1)
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % unlockedWeapons.Count;
            EquipWeapon(currentWeaponIndex);
        }
    }

    public void EquipWeapon(int index)
    {
        if (currentWeapon != null)
            Destroy(currentWeapon.gameObject);

        currentWeaponIndex = index;
        WeaponData weaponData = unlockedWeapons[index];
        currentWeapon = Instantiate(weaponData.weaponPrefab, weaponHolder).GetComponent<Weapon>();
        currentWeapon.SetData(weaponData);

        if (weaponUI != null)
            weaponUI.SetWeaponSprite(weaponData.weaponSprite, !weaponData.isRanged);

        weaponUI.maxAmmoText.text = currentWeapon.weaponData.maxAmmo.ToString();
        weaponUI.currAmmoText.text = currentWeapon.currentAmmo.ToString();
    }

    public void UnlockWeapon(WeaponData newWeapon)
    {
        // Already owned
        if (unlockedWeapons.Contains(newWeapon))
            return;

        // If under 2 weapons, just add
        if (unlockedWeapons.Count < 2)
        {
            unlockedWeapons.Add(newWeapon);
            EquipWeapon(unlockedWeapons.Count - 1);
            //currentWeaponIndex = (currentWeaponIndex + 1) % unlockedWeapons.Count;
            return;
        }

        // Remove currently equipped weapon
        WeaponData removedWeapon = unlockedWeapons[currentWeaponIndex];
        unlockedWeapons[currentWeaponIndex] = newWeapon;

        // Drop the removed weapon as a pickup
        if (weaponPickupPrefab != null)
        {
            GameObject pickup = Instantiate(weaponPickupPrefab, weaponDropPoint.position, Quaternion.identity);
            WeaponPickup pickupScript = pickup.GetComponent<WeaponPickup>();
            pickupScript.weaponToUnlock = removedWeapon;
        }

        EquipWeapon(currentWeaponIndex); // Equip the new one in the same slot
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}

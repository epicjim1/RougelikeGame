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
    //public Animator weaponAnimator;
    public GameObject weaponPickupPrefab;
    public Transform weaponDropPoint;
    public WeaponUI weaponUI;

    private Weapon currentWeapon;

    private void Start()
    {
        /*if (allWeapons.Count > 0)
        {
            UnlockWeapon(allWeapons[0]);
            EquipWeapon(0);
        }*/
    }

    private void Update()
    {
        HandleShooting();
        HandleWeaponSwap();
    }

    void HandleShooting()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3 dir = (GetMouseWorldPosition() - weaponHolder.position).normalized;
            currentWeapon?.Shoot(dir);
            //weaponAnimator.SetTrigger("Shoot");
            //currentWeapon?.GetComponent<Animator>().SetTrigger("Shoot");
        }
    }

    void HandleWeaponSwap()
    {
        if (Input.GetKeyDown(KeyCode.Q) && unlockedWeapons.Count > 1)
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
        currentWeapon.SetData(weaponData); // pass data in

        // Apply animations
        //if (weaponData.weaponAnimOverride != null)
        //    weaponAnimator.runtimeAnimatorController = weaponData.weaponAnimOverride;

        if (weaponUI != null)
            weaponUI.SetWeaponSprite(weaponData.weaponSprite);
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

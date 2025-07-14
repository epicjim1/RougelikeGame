using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon")]
public class WeaponData : ScriptableObject
{
    public bool isRanged;
    public string weaponName;
    public Sprite weaponSprite;
    public GameObject weaponPrefab;
    public float fireRate;
    public int maxAmmo;
    public float reloadTime;
    public int damage;
    public float knockbackStrength;
    public GameObject projectilePrefab;
}

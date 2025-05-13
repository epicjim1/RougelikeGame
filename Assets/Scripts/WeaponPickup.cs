using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponData weaponToUnlock;

    private bool playerInRange = false;
    private GameObject player;

    private Vector3 startPos;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (weaponToUnlock != null && weaponToUnlock.weaponSprite != null)
        {
            sr.sprite = weaponToUnlock.weaponSprite;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            player.GetComponent<WeaponHandler>().UnlockWeapon(weaponToUnlock);
            Destroy(gameObject);
        }

        //float newY = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        //transform.position = startPos + new Vector3(startPos.x, startPos.y + newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }
}

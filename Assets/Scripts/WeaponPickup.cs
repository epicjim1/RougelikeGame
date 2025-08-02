using TMPro;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponData weaponToUnlock;

    private bool playerInRange = false;
    private GameObject player;
    private SpriteRenderer sr;

    public GameObject promptObject;
    public GameObject promptPrefab; // assign in Inspector
    private GameObject promptInstance;

    private Vector3 startPos;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;

    private void Start()
    {
        startPos = transform.position;
        promptObject.SetActive(false);
        sr = GetComponent<SpriteRenderer>();
        if (weaponToUnlock != null && weaponToUnlock.weaponSprite != null)
        {
            sr.sprite = weaponToUnlock.weaponSprite;

            //float spriteTop = sr.bounds.extents.y;
            //promptObject.transform.localPosition = new Vector3(0, spriteTop, 0);
            promptObject.GetComponentInChildren<TMP_Text>().text = weaponToUnlock.name;

            float pivotOffset = sr.bounds.center.y - transform.position.y;
            float localTopY = pivotOffset + sr.bounds.extents.y;
            promptObject.transform.localPosition = new Vector3(0, localTopY + .2f, 0);
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
        float newY = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + new Vector3(0f, newY, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.gameObject;

            sr.material.SetFloat("_OutlineThickness", 1);
            promptObject.SetActive(true);
            /*if (promptPrefab != null && promptInstance == null)
            {
                promptInstance = Instantiate(promptPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
                promptInstance.transform.SetParent(transform); // optional: follow bobbing
            }*/
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;

            sr.material.SetFloat("_OutlineThickness", 0);
            promptObject.SetActive(false);
            /*if (promptInstance != null)
                Destroy(promptInstance);*/
        }
    }
}

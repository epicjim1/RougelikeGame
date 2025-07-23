using UnityEngine;

public enum ChestType
{
    Full,
    Empty,
    Gun,
    Mimic
}

public class Chest : MonoBehaviour
{
    public GameObject promptObject;
    public ChestType chestType = ChestType.Full;

    public int coinValue = 50;
    public float damage = 30f;
    public GameObject weaponPickup;

    private bool playerInRange = false;
    private bool isOpened = false;
    private GameObject player;
    private Animator anim;
    private WeaponHandler weaponHandler = null;
    private PlayerMovement playerMovement = null;

    void Start()
    {
        anim = GetComponent<Animator>();
        promptObject.SetActive(false);

        if (chestType == ChestType.Gun || chestType == ChestType.Mimic)
        {
            player = GameObject.FindWithTag("Player");
            weaponHandler = player.GetComponent<WeaponHandler>();
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        if (!isOpened && playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            isOpened = true;
            promptObject.SetActive(false);

            if (chestType == ChestType.Full)
            {
                anim.SetTrigger("chestOpenFull");
                GameManager.Instance.AddCoinsToCurrentRun(coinValue);
            }
            else if (chestType == ChestType.Empty)
            {
                anim.SetTrigger("chestOpenEmpty");
            }
            else if (chestType == ChestType.Gun)
            {
                anim.SetTrigger("chestOpenEmpty");
                GameObject pickup = Instantiate(weaponPickup, transform.position + new Vector3(1.5f, 0, 0), Quaternion.identity);
                pickup.GetComponent<WeaponPickup>().weaponToUnlock = weaponHandler.allWeapons[Random.Range(0, weaponHandler.allWeapons.Count)];
            }
            else if (chestType == ChestType.Mimic)
            {
                anim.SetBool("chestOpenMimic", true);
                attackPlayer();
                //MimicAnim();
            }
        }
    }

    private System.Collections.IEnumerator MimicAnim()
    {
        anim.SetBool("chestOpenMimic", true);
        yield return new WaitForSeconds(5f);
        anim.SetBool("chestOpenMimic", false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isOpened && other.CompareTag("Player"))
        {
            playerInRange = true;
            //player = other.gameObject;

            promptObject.SetActive(true);
        }
        else if (chestType == ChestType.Mimic && isOpened && other.CompareTag("Player"))
        {
            attackPlayer();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isOpened && other.CompareTag("Player"))
        {
            playerInRange = false;
            //player = null;

            promptObject.SetActive(false);
        }
    }

    private void attackPlayer()
    {
        if (!playerMovement.getIsDashing())
        {
            playerMovement.takeDamage(damage);

            Vector2 knockbackDir = (player.transform.position - transform.position).normalized;
            float knockbackForce = 40f;
            playerMovement.ApplyKnockback(knockbackDir, knockbackForce);
        }
    }
}

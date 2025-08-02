using UnityEngine;

public class exitController : MonoBehaviour
{
    private CircleCollider2D CircleCollider2D;

    private void Start()
    {
        CircleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerMovement>().PlayerWinStage();
        }
    }
}

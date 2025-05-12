using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    public Rigidbody2D rb;
    public Animator anim;
    public SpriteRenderer spriteRenderer;

    Vector2 movement;
    private bool isDashing = false;
    private float lastDashTime = -Mathf.Infinity;

    void Update()
    {
        if (!isDashing)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            anim.SetFloat("Speed", movement.sqrMagnitude);

            //Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //spriteRenderer.flipX = mouseWorldPos.x < transform.position.x;

            if (movement.x < 0)
                spriteRenderer.flipX = true;
            else if (movement.x > 0)
                spriteRenderer.flipX = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }

    private System.Collections.IEnumerator Dash()
    {
        isDashing = true;
        anim.SetBool("isDashing", true);
        lastDashTime = Time.time;

        Vector2 dashDirection = movement.normalized;
        if (dashDirection == Vector2.zero) dashDirection = Vector2.right * (spriteRenderer.flipX ? -1 : 1); // Default direction

        if (dashDirection.x < 0)
            spriteRenderer.flipX = true;
        else if (dashDirection.x > 0)
            spriteRenderer.flipX = false;

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            rb.MovePosition(rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;
        anim.SetBool("isDashing", false);
    }
}

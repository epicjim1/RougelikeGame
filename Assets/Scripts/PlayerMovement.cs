using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    public Healthbar healthbar;

    Vector2 movement;
    private bool isDashing = false;
    private bool canMove = true;
    private float lastDashTime = -Mathf.Infinity;
    private Color originalColor;

    public GameObject pauseMenuUI;
    public GameObject loseMenuUI;
    public GameObject winStageMenuUI;
    public GameObject winGameMenuUI;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        pauseMenuUI.SetActive(false);
        loseMenuUI.SetActive(false);
        winStageMenuUI.SetActive(false);
        winGameMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        if (!isDashing && !GameManager.Instance.GameIsLost && !GameManager.Instance.GameIsPaused)
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

        if (canMove && Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing && canMove)
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

    public void takeDamage(float amount)
    {
        healthbar.takeDamage(amount);
        StartCoroutine(FlashCoroutine());

        if (healthbar.health <= 0)
        {
            PlayerDie();
        }
    }

    private System.Collections.IEnumerator FlashCoroutine()
    {
        //spriteRenderer.color = Color.red;
        spriteRenderer.material.SetInt("_Flash", 1);
        yield return new WaitForSeconds(.1f);
        spriteRenderer.material.SetInt("_Flash", 0);
        //spriteRenderer.color = originalColor;
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration = 0.1f)
    {
        StartCoroutine(KnockbackCoroutine(direction, force, duration));
    }

    private System.Collections.IEnumerator KnockbackCoroutine(Vector2 direction, float force, float duration)
    {
        // Disable player input/movement
        //bool wasInputEnabled = canMove; // assuming you have this flag
        canMove = false;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(duration);

        rb.linearVelocity = Vector2.zero;
        canMove = true; // Re-enable input
    }

    public bool getIsDashing()
    {
        return isDashing;
    }

    public void PlayerWinStage()
    {
        if (GameManager.Instance.stage == 2)
        {
            winGameMenuUI.SetActive(true);
        }
        else
        {
            winStageMenuUI.GetComponentInChildren<TMP_Text>().text = "You Beat Stage: " + (GameManager.Instance.stage + 1);
            winStageMenuUI.SetActive(true);
        }
        StopAllCoroutines();
        canMove = false;
        rb.simulated = false;
    }

    public void ContinueToNextStage()
    {
        GameManager.Instance.NextStage(healthbar.maxHealth);
    }

    public void PlayerDie ()
    {
        GameManager.Instance.GameIsLost = true;
        loseMenuUI.SetActive(true);
        StopAllCoroutines();
        canMove = false;
        rb.simulated = false;
        //canvasAnim.SetTrigger("LoseOn");
    }

    public void Resume()
    {
        GameManager.Instance.GameIsPaused = false;
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        //canvasAnim.SetTrigger("PauseOff");
    }

    public void Pause()
    {
        GameManager.Instance.GameIsPaused = true;
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        //canvasAnim.SetTrigger("PauseOn");
    }

    public void ReturnHome()
    {
        Time.timeScale = 1f;
        canMove = true;
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene(0);
    }

    public void ReturnHomeWithCoins()
    {
        Time.timeScale = 1f;
        canMove = true;
        GameManager.Instance.EndRunAndSaveCoins();
        GameManager.Instance.ResetGame();
        SceneManager.LoadScene(0);
    }

    /*private System.Collections.IEnumerator Blur()
    {
        startingFocalLength = Mathf.Lerp(startingFocalLength, 13f, Time.deltaTime * 4f);
        yield return new WaitForSeconds(.5f);
        depthOfField.focalLength.value = startingFocalLength;
    }

    private System.Collections.IEnumerator UnBlur()
    {
        startingFocalLength = Mathf.Lerp(startingFocalLength, 1f, Time.deltaTime * 4f);
        yield return new WaitForSeconds(.5f);
        depthOfField.focalLength.value = startingFocalLength;
    }*/
}

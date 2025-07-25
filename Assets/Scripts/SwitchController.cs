using UnityEngine;

public class SwitchController : MonoBehaviour
{
    public static event System.Action<SwitchController> OnSwitchActivated;

    public string switchID;
    public Sprite onSprite;
    private Sprite offSprite;
    private SpriteRenderer myRenderer;

    private void Start()
    {
        myRenderer = GetComponent<SpriteRenderer>();
        offSprite = myRenderer.sprite;
    }

    private bool switchedOn = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!switchedOn && other.CompareTag("Player"))
        {
            switchedOn = true;
            myRenderer.sprite = onSprite;
            OnSwitchActivated?.Invoke(this);
        }
    }
}

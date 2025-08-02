using Unity.VisualScripting;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Tooltip("The unique ID for the door-key pairing.")]
    public string doorID;

    private bool isUnlocked = false;
    public Sprite openDoor;
    [HideInInspector] public Sprite closedDoor;
    [HideInInspector] public SpriteRenderer myRenderer;
    [HideInInspector] public BoxCollider2D myCollider;

    private void Start()
    {
        myRenderer = this.GetComponent<SpriteRenderer>();
        myCollider = this.GetComponent<BoxCollider2D>();
        closedDoor = myRenderer.sprite;
        myCollider.enabled = true;
    }

    private void OnEnable()
    {
        SwitchController.OnSwitchActivated += HandleSwitchOn;
    }

    private void OnDisable()
    {
        SwitchController.OnSwitchActivated -= HandleSwitchOn;
    }

    private void HandleSwitchOn(SwitchController sw)
    {
        // Compare key and door by shared identifier
        if (sw.switchID == this.doorID)
        {
            UnlockDoor();
        }
    }

    private void UnlockDoor()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        myRenderer.sprite = openDoor;
        myCollider.enabled = false;
        Debug.Log($"Door with ID {doorID} unlocked!");
    }
}

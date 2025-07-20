using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject settingsMenuUI;
    public GameObject[] slotScreens;

    public GameObject gunIndicator; // assign this to the gun UI Image
    public Button[] menuButtons; 

    void Start()
    {
        foreach (GameObject screen in slotScreens)
        {
            screen.SetActive(false);
        }
        settingsMenuUI.SetActive(false);
        gunIndicator.SetActive(false); // hide at start
        // Add hover events to each button
        foreach (Button btn in menuButtons)
        {
            AddHoverEvents(btn);
        }

        mainMenuUI.SetActive(true);
    }

    void Update()
    {
        
    }

    void AddHoverEvents(Button button)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // OnPointerEnter
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) =>
        {
            MoveGunToButton(button.gameObject);
        });
        trigger.triggers.Add(enterEntry);

        // OnPointerExit
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) =>
        {
            gunIndicator.SetActive(false);
        });
        trigger.triggers.Add(exitEntry);
    }

    void MoveGunToButton(GameObject target)
    {
        RectTransform targetRect = target.GetComponent<RectTransform>();
        RectTransform gunRect = gunIndicator.GetComponent<RectTransform>();

        Vector3 offset = new Vector3(-146f, 0f, 0f); // Adjust offset as needed
        gunRect.localPosition = targetRect.localPosition + offset;
        gunIndicator.SetActive(true);
    }

    public void SlotScreen()
    {
        mainMenuUI.SetActive(false);
        foreach (GameObject screen in slotScreens)
        {
            screen.SetActive(true);
        }
    }

    public void SettingsMenu()
    {
        mainMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }

    public void MenuScreen()
    {
        foreach (GameObject screen in slotScreens)
        {
            screen.SetActive(false);
        }
        settingsMenuUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}

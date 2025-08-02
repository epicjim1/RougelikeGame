using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpinController : MonoBehaviour
{
    public Button spinBtn;
    public TMP_Text spinBtnText;

    ColorBlock cb;
    public Color spinColor = new Color32(209, 0, 1, 255);
    public Color playColor = new Color32(0, 209, 10, 255);

    public RowSpinner rowLevel;
    public RowSpinner rowStartingWeapon;
    public RowSpinner rowBossType;
    public RowSpinner rowModifier;

    public float minSpeed = 15f;
    public float maxSpeed = 35f;

    private bool hasSpun = false;
    private bool hasCollectedVals = false;
    private Coroutine loadingDotsCoroutine;

    private void Start()
    {
        ResetButton();
        ColorBlock cb = spinBtn.colors;
    }
        
    private void Update()
    {
        if (hasSpun && !hasCollectedVals)
        {
            if (rowLevel.GetSelectedValue() != "" &&
                rowStartingWeapon.GetSelectedValue() != "" &&
                rowBossType.GetSelectedValue() != "" &&
                rowModifier.GetSelectedValue() != "")
            {
                hasCollectedVals = true;
                CollectResults();
            }
        }
    }

    public void StartSpin()
    {
        if (!hasSpun)
        {
            hasSpun = true;
            spinBtn.interactable = false;

            // Start each row with different speeds and deceleration for variety
            rowLevel.StartSpin(Random.Range(minSpeed, maxSpeed), 1000f, 1f);
            rowStartingWeapon.StartSpin(Random.Range(minSpeed, maxSpeed), 1000f, 2f);
            rowBossType.StartSpin(Random.Range(minSpeed, maxSpeed), 1000f, 2.5f);
            rowModifier.StartSpin(Random.Range(minSpeed, maxSpeed), 1000f, 3f);

            // Start loading dots animation
            loadingDotsCoroutine = StartCoroutine(LoadingDots());
        }
    }

    private void CollectResults()
    {
        string levelValue = rowLevel.GetSelectedValue();
        string startingWeaponValue = rowStartingWeapon.GetSelectedValue();
        string bossType = rowBossType.GetSelectedValue();
        string modifierValue = rowModifier.GetSelectedValue();

        // Parse them if needed
        GameManager.Instance.SetSpinResults(levelValue, startingWeaponValue, bossType, modifierValue);

        // Stop dots and update button
        if (loadingDotsCoroutine != null)
            StopCoroutine(loadingDotsCoroutine);

        spinBtnText.text = "Play";
        cb.normalColor = playColor;
        cb.highlightedColor = new Color32(0, 184, 9, 255);
        cb.pressedColor = new Color32(0, 184, 9, 255);
        cb.colorMultiplier = 1f;
        spinBtn.colors = cb;
        spinBtn.interactable = true;
    }

    public void OnButtonPressed()
    {
        if (!hasSpun)
        {
            StartSpin();
        }
        else if (hasCollectedVals)
        {
            LoadNextScene();
        }
    }

    IEnumerator LoadingDots()
    {
        string baseText = "";
        int dotCount = 0;

        cb.disabledColor = Color.gray;
        cb.colorMultiplier = 1f;
        spinBtn.colors = cb;

        while (true)
        {
            spinBtnText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSeconds(0.3f);
        }
    }

    private void ResetButton()
    {
        spinBtnText.text = "Spin";
        cb.normalColor = spinColor;
        cb.highlightedColor = new Color32(184, 0, 2, 255);
        cb.pressedColor = new Color32(184, 0, 2, 255);
        cb.colorMultiplier = 1f;
        spinBtn.colors = cb;
        spinBtn.interactable = true;
        Random.InitState(System.DateTime.Now.Millisecond);
    }

    //Temp
    private void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

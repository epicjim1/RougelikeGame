using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public Slider loadingSlider;
    public TMP_Text loadingText;

    private void Awake()
    {
        this.gameObject.SetActive(true);
        loadingSlider.value = 0;
    }

    public void UpdateProgress(float progress, string message = null)
    {
        loadingSlider.value = progress;
        if (loadingText != null && !string.IsNullOrEmpty(message))
        {
            loadingText.text = message;
        }
    }

    public void HideLoadingScreen()
    {
        this.gameObject.SetActive(false);
    }
}

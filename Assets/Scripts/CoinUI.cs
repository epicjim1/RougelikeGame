using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    private TMP_Text thisText;
    private int displayedCoins = 0;
    private Coroutine coinChangeCoroutine;

    void Start()
    {
        thisText = GetComponent<TMP_Text>();
        thisText.text = displayedCoins.ToString();
    }

    void Update()
    {
        if (GameManager.Instance == null)
            return;

        int runCoinCount = GameManager.Instance.currentRunPlayerCoins;

        if (displayedCoins != runCoinCount && coinChangeCoroutine == null)
        {
            coinChangeCoroutine = StartCoroutine(AnimateCoinChange(runCoinCount));
        }
    }

    private System.Collections.IEnumerator AnimateCoinChange(int targetCoins)
    {
        while (displayedCoins != targetCoins)
        {
            if (displayedCoins < targetCoins)
                displayedCoins++;
            else
                displayedCoins--;

            thisText.text = displayedCoins.ToString();
            yield return new WaitForSeconds(0.02f); // controls the speed of change
        }

        coinChangeCoroutine = null;
    }
}

using UnityEngine;

public class SpinController : MonoBehaviour
{
    public RowSpinner rowLevel;
    public RowSpinner rowStartingWeapon;
    public RowSpinner rowModifier;

    public float minSpeed = 15f;
    public float maxSpeed = 35f;

    private bool hasSpun = false;
    private bool hasCollectedVals = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !hasSpun)
        {
            StartSpin();
        }

        if (hasSpun && !hasCollectedVals)
        {
            if (rowLevel.GetSelectedValue() != "" &&
                rowStartingWeapon.GetSelectedValue() != "" &&
                rowModifier.GetSelectedValue() != "")
            {
                hasCollectedVals = true;
                CollectResults();
            }
        }
    }

    private void StartSpin()
    {
        hasSpun = true;

        // Start each row with different speeds and deceleration for variety
        rowLevel.StartSpin(Random.Range(minSpeed, maxSpeed), 10f, 1f);
        rowStartingWeapon.StartSpin(Random.Range(minSpeed, maxSpeed), 10f, 2f);
        rowModifier.StartSpin(Random.Range(minSpeed, maxSpeed), 10f, 3f);

        //Invoke(nameof(CollectResults), 5f);  // Enough time for all to stop
    }

    private void CollectResults()
    {
        string levelValue = rowLevel.GetSelectedValue();
        string startingWeaponValue = rowStartingWeapon.GetSelectedValue();
        string modifierValue = rowModifier.GetSelectedValue();

        // Parse them if needed
        //GameManager.Instance.SetSpinResults(levelValue, mapSizeValue, enemyValue);
    }
}

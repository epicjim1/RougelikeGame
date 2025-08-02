using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class RowSpinner : MonoBehaviour
{
    [System.Serializable]
    public class SpinItem
    {
        public Sprite sprite;
        public string value;  // use string so you can store anything (int, bool, etc as string)
        public bool rotateDisplay;
    }

    public SpinItem[] items;               // list of possible items
    public GameObject itemPrefab;          // prefab containing Image component
    public int visualLoops = 3;          // total number of visual items to instantiate
    public float itemSpacing = 1f;         // vertical distance between items

    private List<GameObject> spawnedItems = new List<GameObject>();
    private float spinSpeed = 5f;
    private bool spinning = false;
    private float deceleration;
    private string selectedValue = "";
    private float spinTimer = 0f;
    private bool decelerating = false;

    public float upperLimit = 21f;
    public float lowerLimit = 7f;
    public Transform selectionArrow;

    private Vector3 targetPosition;
    private bool aligning = false;
    private float alignSpeed = 5f;  // adjust for smoothness

    private void Start()
    {
        Populate();
    }

    private void Populate()
    {
        int totalCopies = items.Length * visualLoops;

        for (int i = 0; i < totalCopies; i++)
        {
            int itemIndex = i % items.Length;

            GameObject obj = Instantiate(itemPrefab, transform);
            obj.transform.localPosition = Vector3.down * i * itemSpacing;
            SetVisual(obj, items[itemIndex].sprite);

            obj.name = items[itemIndex].value;
            if (items[itemIndex].rotateDisplay)
                obj.transform.rotation = Quaternion.Euler(0,0,270);
            spawnedItems.Add(obj);
        }

        // Center the row so that some items are both above and below
        float totalHeight = totalCopies * itemSpacing;
        transform.localPosition = new Vector3(transform.localPosition.x, totalHeight / 2f, transform.localPosition.z);
    }

    private void SetVisual(GameObject obj, Sprite sprite)
    {
        // Support both UI (Image) and 2D (SpriteRenderer)
        var img = obj.GetComponent<Image>();
        if (img != null)
            img.sprite = sprite;
        else
        {
            obj.GetComponent<SpriteRenderer>().sprite = sprite;
            //float targetHeight = 1f;
            //float scale = targetHeight / sprite.bounds.size.y;
            //obj.transform.localScale = new Vector3(scale, scale, 1f);
        }
            
    }

    public void StartSpin(float speed, float slowDownRate, float spinDuration)
    {
        spinSpeed = speed;
        deceleration = slowDownRate;
        spinning = true;
        spinTimer = spinDuration;
        decelerating = false;
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.E))
        {
            StartSpin(spinSpeed);
        }*/

        if (spinning)
        {
            transform.localPosition -= Vector3.up * spinSpeed * Time.deltaTime;

            if (transform.localPosition.y <= lowerLimit)
            {
                transform.localPosition = new Vector3(transform.localPosition.x, upperLimit, transform.localPosition.z);
            }

            if (!decelerating)
            {
                spinTimer -= Time.deltaTime;
                if (spinTimer <= 0f)
                {
                    decelerating = true;
                }
            }
            else
            {
                spinSpeed -= deceleration * Time.deltaTime;

                if (spinSpeed <= 0f)
                {
                    spinSpeed = 0f;
                    spinning = false;
                    AlignToClosestItem();
                }
            }
        }

        if (aligning)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * alignSpeed);

            if (Vector3.Distance(transform.localPosition, targetPosition) < 0.001f)
            {
                transform.localPosition = targetPosition;
                aligning = false;
            }
        }
    }

    public void StopSpin()
    {
        spinning = false;
        AlignToClosestItem();
    }

    private void AlignToClosestItem()
    {
        float arrowY = selectionArrow.position.y;

        float closestDistance = float.MaxValue;
        GameObject closestItem = null;

        foreach (var obj in spawnedItems)
        {
            float itemY = obj.transform.position.y;
            float distance = Mathf.Abs(itemY - arrowY);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = obj;
            }
        }

        selectedValue = closestItem.name;
        //Debug.Log("Selected Item: " + selectedValue);

        float yOffset = arrowY - closestItem.transform.position.y;
        targetPosition = transform.localPosition + new Vector3(0, yOffset, 0);
        aligning = true;
    }

    public string GetSelectedValue()
    {
        return selectedValue;
    }
}

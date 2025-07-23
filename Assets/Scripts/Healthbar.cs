using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public GameObject healthbar;
    public GameObject easeHealthbar;

    public float maxHealth = 50f;
    public float health;

    private Slider healthslider;
    private Slider easeHealthSlider;
    private float lerpSpeed = 0.05f;

    // start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance != null)
        {
            maxHealth = GameManager.Instance.maxHealth;
        }
        health = maxHealth;

        healthslider = healthbar.GetComponent<Slider>();
        easeHealthSlider = easeHealthbar.GetComponent<Slider>();

        healthslider.maxValue = maxHealth;
        easeHealthSlider.maxValue = maxHealth;
        healthbar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxHealth * 2);
        easeHealthbar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxHealth * 2);
    }

    // Update is called once per frame
    void Update()
    {
        if (healthslider.value != health)
        {
            healthslider.value = health;
        }

        /*if (Input.GetKeyDown(KeyCode.E))
        {
            takeDamage(10f);
        }*/
        if (Input.GetKeyDown(KeyCode.F))
        {
            heal(maxHealth);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            permHealthIncrease(25f);
        }

        if (healthslider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, health, lerpSpeed);
        }
    }

    public void takeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            health = 0;
        }
    }

    public void heal(float healVal)
    {
        health += healVal;

        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    public void permHealthIncrease(float amount)
    {
        if (maxHealth + amount <= 375)
        {
            maxHealth += amount;
            healthslider.maxValue = maxHealth;
            easeHealthSlider.maxValue = maxHealth;

            float currentWidth = healthbar.GetComponent<RectTransform>().sizeDelta.x;
            float easeCurrentWidth = easeHealthbar.GetComponent<RectTransform>().sizeDelta.x;

            healthbar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth + amount * 2);
            easeHealthbar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, easeCurrentWidth + amount * 2);

            health = maxHealth;
        }
    }
}


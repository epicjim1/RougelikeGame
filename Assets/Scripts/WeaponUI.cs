using UnityEngine;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    public Image weaponImage; // Assign in Inspector

    public Vector2 maxSize = new Vector2(100, 50);

    public void SetWeaponSprite(Sprite sprite)
    {
        if (sprite != null)
        {
            weaponImage.sprite = sprite;
            weaponImage.SetNativeSize();

            // Optional scaling
            RectTransform rt = weaponImage.rectTransform;
            Vector2 size = rt.sizeDelta;

            float scale = Mathf.Min(maxSize.x / size.x, maxSize.y / size.y, 1f);
            rt.sizeDelta = size * scale;

            weaponImage.enabled = true;
        }
        else
        {
            weaponImage.enabled = false;
        }
    }
}

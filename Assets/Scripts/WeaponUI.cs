using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    public Image weaponImage;
    public Vector2 maxSize = new Vector2(100, 50);

    public TMP_Text currAmmoText;
    public TMP_Text maxAmmoText;

    public void SetWeaponSprite(Sprite sprite, bool spriteIsVertical)
    {
        if (sprite != null)
        {
            weaponImage.sprite = sprite;
            weaponImage.SetNativeSize();
            weaponImage.enabled = true;

            RectTransform rt = weaponImage.rectTransform;

            Vector2 nativeSize = rt.sizeDelta;
            Vector2 sizeForScaling;

            if (!spriteIsVertical)
            {
                // If not vertical, we'll rotate it 90 degrees.
                // The effective size for scaling is native (width, height) swapped.
                sizeForScaling = new Vector2(nativeSize.y, nativeSize.x);
            }
            else
            {
                // If vertical, no rotation needed.
                sizeForScaling = nativeSize;
            }

            float scale = Mathf.Min(maxSize.x / sizeForScaling.x, maxSize.y / sizeForScaling.y, 1f);

            Vector2 finalScaledSize = nativeSize * scale;

            if (!spriteIsVertical)
            {
                rt.rotation = Quaternion.Euler(0, 0, 90);
            }
            else
            {
                rt.rotation = Quaternion.identity;
            }

            rt.sizeDelta = finalScaledSize;
            float effectiveHeightForAlignment;

            if (!spriteIsVertical)
            {
                // When rotated 90 degrees, the original width becomes the effective height for vertical positioning.
                effectiveHeightForAlignment = finalScaledSize.x;
            }
            else
            {
                // For vertical sprites, the original height is the effective height.
                effectiveHeightForAlignment = finalScaledSize.y;
            }

            float newYPos = -(maxSize.y / 2f) + (effectiveHeightForAlignment / 2f);
            float targetYOffsetFromCenter = -maxSize.y / 2f + effectiveHeightForAlignment / 2f;
            float calculatedYOffsetFromCenterOfMaxSize = (effectiveHeightForAlignment / 2f) - (maxSize.y / 2f);
            rt.anchoredPosition = new Vector2(35, 110 + calculatedYOffsetFromCenterOfMaxSize);

        }
        else
        {
            weaponImage.enabled = false;
        }
    }
}

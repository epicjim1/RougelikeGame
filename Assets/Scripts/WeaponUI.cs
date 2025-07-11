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

            // --- STEP 1: Determine effective size for scaling based on orientation ---
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

            // --- STEP 2: Scale the image correctly to fit maxSize ---
            float scale = Mathf.Min(maxSize.x / sizeForScaling.x, maxSize.y / sizeForScaling.y, 1f);

            // Apply scale to the native size to get the actual size the image will be rendered at
            // before considering the rotation of the RectTransform itself.
            Vector2 finalScaledSize = nativeSize * scale;

            // --- STEP 3: Apply rotation ---
            if (!spriteIsVertical)
            {
                rt.rotation = Quaternion.Euler(0, 0, 90);
            }
            else
            {
                rt.rotation = Quaternion.identity;
            }

            // --- STEP 4: Set the sizeDelta (this is the size of the RectTransform, unrotated) ---
            rt.sizeDelta = finalScaledSize;

            // --- STEP 5: Adjust position for bottom alignment ---
            // Assuming the pivot is (0.5, 0.5) (center).
            // The image should be centered horizontally within maxSize.x.
            // The newYPos should place the bottom edge of the image at the bottom of the maxSize area.

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

            // Calculate the Y position to move the image from the center down to the bottom
            // of the maxSize area.
            // (maxSize.y / 2f) is half the container height.
            // (effectiveHeightForAlignment / 2f) is half the actual displayed image height.
            // The sum of these two, negative, positions the center such that the bottom aligns.
            float newYPos = -(maxSize.y / 2f) + (effectiveHeightForAlignment / 2f);

            // Apply the new position. X remains 35 (or whatever desired horizontal offset).
            // Y is relative to the anchor (which we assume is center 0,0 for now, then adjust).
            // If your parent anchors are not at (0.5, 0.5) and your RectTransform's anchors
            // are also not (0.5, 0.5), this might need further adjustment.
            // For simplicity, let's assume `weaponImage` has its anchors set to stretch/center horizontally
            // and center vertically within its direct parent.

            // To align to the bottom of the *parent container* (not just maxSize),
            // and assuming the parent is designed to contain the maxSize, you should adjust
            // based on the parent's actual height or the intended bottom of the UI element.

            // Let's refine the final anchoredPosition.
            // Current setup has an offset of (35, 110) plus the calculated newYPos.
            // This suggests the parent or reference point for the image is at (35, 110) from some origin.

            // If you want the image to be centered horizontally within an area,
            // and bottom-aligned within a max height of `maxSize.y` relative to its own `anchoredPosition`,
            // then the X should be 0 (if anchors are center-x) or adjusted based on your UI layout.
            // The Y should bring the bottom of the image to the bottom of the available space.

            // Let's assume the overall UI element containing this WeaponUI
            // has some fixed position for the image.
            // If the Image's RectTransform's anchors are (0.5, 0.5) (center),
            // then a Y value of `-(maxSize.y / 2f) + (effectiveHeightForAlignment / 2f)`
            // will place the center of the image correctly such that its bottom is at the bottom
            // of the `maxSize.y` area.

            // The '110' in your original code seems to be an additional vertical offset.
            // Let's integrate that correctly. If 110 is the bottom edge of your overall UI container
            // for this weapon image, then we should align to that.

            // Re-evaluating the desired outcome: "still not bottom aligning for nonVertical sprites"
            // This implies the non-vertical sprites, when rotated, are not sitting at the bottom
            // of the visual area you've defined for them (which is `maxSize.y`).

            // Let's ensure the image's anchor is set to bottom-center for simplicity.
            // Or, if it's center-center, calculate the offset from there.

            // Assuming `weaponImage`'s `RectTransform` has its anchors set to (0.5, 0) (bottom-center)
            // or you want to align it relative to a bottom-center concept.
            // If anchors are (0.5, 0.5) (center), then:
            float targetYOffsetFromCenter = -maxSize.y / 2f + effectiveHeightForAlignment / 2f;

            // Your original code had `new Vector2(35, 110 + newYPos);`
            // Let's keep the X at 35 and adjust the Y to place the bottom of the image correctly.
            // The '110' seems to be a base Y position for the entire weapon UI image component.
            // If 110 is the *bottom* of where the weapon image should be, then we need to account for it.

            // Let's reconsider the coordinate system.
            // If (35, 110) is the *bottom-left* corner of the logical area for the image,
            // and `maxSize` defines the extent from there, then we want the bottom of the image
            // to be at Y=110.

            // Let's work with `pivot` and `anchoredPosition` relative to the anchor point.
            // If anchors are (0.5, 0.5) (center):
            // The center of the image should be at Y = 110 (bottom of container) + (effectiveHeightForAlignment / 2)
            // relative to the parent's origin, then minus (parent center Y - weaponImage anchor Y).

            // A more robust approach:
            // 1. Set anchors to (0.5, 0) (bottom-center of parent).
            // 2. Set pivot to (0.5, 0) (bottom-center of image itself).
            // 3. Then `anchoredPosition`'s Y component will directly control the image's bottom edge.

            // If you can't change anchors/pivot easily or don't want to,
            // then we need to calculate the offset from the current pivot/anchor setup.

            // Let's assume the `weaponImage` `RectTransform` has its anchors set to (0.5, 0.5) (center)
            // and its pivot set to (0.5, 0.5) (center).
            // The `anchoredPosition` is then the offset of the image's center from its parent's center.
            // You want the *bottom* of the image to align with a certain Y coordinate.

            // Desired bottom Y coordinate = 110.
            // The center of the image (pivot) needs to be at Y = 110 + (effectiveHeightForAlignment / 2).

            // The Y-coordinate for `anchoredPosition` is the offset of the image's pivot from its anchors.
            // If anchors are (0.5, 0.5) (parent center), and image pivot is (0.5, 0.5) (image center),
            // then `anchoredPosition.y` moves the image's center relative to parent's center.

            // So, `anchoredPosition.y` should be:
            // `(Desired Y for bottom) + (effectiveHeightForAlignment / 2) - (parent_rect.height / 2)` (if parent center is 0,0)
            // This is getting complicated due to the fixed '110' offset.

            // Let's try to simplify the `newYPos` calculation assuming the `maxSize`
            // is the conceptual box that the image should fit within and align to.
            // And that `maxSize` itself is positioned somewhere.

            // If `maxSize` describes a conceptual box, and you want to align to its bottom,
            // then the final `anchoredPosition` (relative to whatever parent origin) needs to be:
            // X: `35` (your fixed horizontal offset)
            // Y: `110` (your fixed base Y) + `(effectiveHeightForAlignment / 2) - (maxSize.y / 2)`

            // Let's try this:
            float calculatedYOffsetFromCenterOfMaxSize = (effectiveHeightForAlignment / 2f) - (maxSize.y / 2f);
            rt.anchoredPosition = new Vector2(35, 110 + calculatedYOffsetFromCenterOfMaxSize);

        }
        else
        {
            weaponImage.enabled = false;
        }
    }
}

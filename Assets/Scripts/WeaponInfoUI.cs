using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInfoUI : MonoBehaviour
{
    public RectTransform cooldown_mask_ref;
    private float ui_height = 100;

    private void Start()
    {
        ui_height = cooldown_mask_ref.rect.height;
    }

    public void SetCooldownProgress(float progress)
    {
        progress = Mathf.Clamp(progress, 0, 1);

        cooldown_mask_ref.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, progress * ui_height);
    }

    public void SetCooldownProgress2(float progress, float maxprogress)
    {
        SetCooldownProgress(progress / maxprogress);
    }
}

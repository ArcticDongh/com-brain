using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInfoUI : MonoBehaviour
{
    public RectTransform cooldown_mask_ref;
    public TMPro.TMP_Text ui_text;
    public TMPro.TMP_Text ui_weaponname;

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

    public void SetText(string text)
    {
        ui_text.text = text;
    }

    public void SetRemainingText(int remain, int maxium)
    {
        SetText(string.Format("{0:D}/{1:D}", remain, maxium));
    }

    public void SetWeaponName(string text)
    {
        ui_weaponname.text = text;
    }
}

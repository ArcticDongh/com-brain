using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeWarpControl : UniqueMono<TimeWarpControl>
{
    public TMPro.TMP_Text[] UITexts;

    private int saveCount = 0;
    private float[] savedTimes = new float[256];

    const string UIPATTERN = "Slot:{0:d}\nTime:{1:f}";
    const int MAX_SLOT = 4;

    private void Start()
    {
        if (UITexts.Length < MAX_SLOT)
        {
            Debug.LogWarning("TimeWarpUI���Ƚ��٣���ʾ���ܲ�����");
        }
    }

    public string SetText(int slot, float time)
    {
        if (slot >= UITexts.Length)
        {
            Debug.LogWarning(string.Format("slot������Χ:{0:d} out of {1:d}", slot, UITexts.Length));
            return "<invalid>";
        }

        var result = string.Format(UIPATTERN, slot + 1, time);
        UITexts[slot].text = result;
        return result;
    }

    public void Save()
    {
        FW.TimelineManager.Save();

        savedTimes[saveCount++] = Time.time;

        if (saveCount >= 256) Debug.LogWarning("�浵��������");

        UpdateTexts();
    }

    public void LoadFrom(int index)
    {
        if (index > saveCount || index > MAX_SLOT)
        {
            Debug.LogWarning("���볬����Χ");
            return;
        }

        FW.TimelineManager.Load(-index);    // ������ʾ������������

        saveCount -= index - 1;

        UpdateTexts();
    }

    private void UpdateTexts()
    {
        int i;
        for (i = 0; i < MAX_SLOT; i++)
        {
            if (saveCount - i - 1 < 0) break;
            SetText(i, savedTimes[saveCount - i - 1]);
        }
        for (; i < MAX_SLOT; i++)
        {
            SetText(i, 0);
        }
    }
}

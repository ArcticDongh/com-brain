using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public string weapon_name;
    public float cooldown_time = 0.6f;
    public int lasting_tick = 3;
    public GameObject weapon_appearance;
    public WeaponInfoUI infoUI;
    [Header("Limits")]
    public bool limited_use;
    public int use_times_limit;

    // ��ʱ�����㲻Ӧ���ڼ�ʱ֮���޸���Щֵ��
    private float timer_cooldown = 0f;
    private int timer_lasting = 0;
    private int remaining_use_times;

    // Start is called before the first frame update
    void Start()
    {
        DisableWeapon();
        if (limited_use)
        {
            remaining_use_times = use_times_limit;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (infoUI is not null)
        {
            infoUI.SetCooldownProgress2(timer_cooldown, cooldown_time);
            infoUI.SetWeaponName(weapon_name);
            if (limited_use)
            {
                infoUI.SetRemainingText(remaining_use_times, use_times_limit);
            }
            else
            {
                infoUI.SetText("");
            }
        }
    }
    // ����֡
    protected virtual void FixedUpdate()
    {
        // ��������ʱ���ʱ����ʱ����ʱ����������
        if (timer_lasting > 0)
        {
            if (--timer_lasting <= 0)
            {
                DisableWeapon();
            }
        }
        // ��ͨ��ȴ��ʱ
        if (timer_cooldown > 0)
        {
            timer_cooldown -= Time.fixedDeltaTime;
        }
    }
    // �Ƿ���ȴ���
    public bool IsWeaponReady()
    {
        return timer_cooldown <= 0;
    }
    // ��չ����ʱӦ����д��������������Ӧ������ۺ͹���
    protected virtual void EnableWeapon()
    {
        if (weapon_appearance is not null)
        {
            weapon_appearance.SetActive(true);
        }
    }

    protected virtual void DisableWeapon()
    {
        if (weapon_appearance is not null)
        {
            weapon_appearance.SetActive(false);
        }
    }

    public void OnWeaponTriggered()
    {
        if (!IsWeaponReady()) return;
        // ����ʹ�ô���
        if (limited_use)
        {
            if (remaining_use_times <= 0) return;
            remaining_use_times--;
        }

        timer_cooldown = cooldown_time;
        timer_lasting = lasting_tick;
        EnableWeapon();
    }

    public void BindUI(WeaponInfoUI ui)
    {
        infoUI = ui;
    }

    public void DestroyWeapon()
    {
        Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public float cooldown_time = 0.6f;
    public int lasting_tick = 3;
    public GameObject weapon_appearance;

    // ��ʱ�����㲻Ӧ���ڼ�ʱ֮���޸���Щֵ��
    private float timer_cooldown = 0f;
    private int timer_lasting = 0;

    // Start is called before the first frame update
    void Start()
    {
        DisableWeapon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // ����֡
    private void FixedUpdate()
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
        // throw new System.NotImplementedException();
        weapon_appearance.SetActive(true);
    }

    protected virtual void DisableWeapon()
    {
        weapon_appearance.SetActive(false);
    }

    public void OnWeaponTriggered()
    {
        if (IsWeaponReady())
        {
            timer_cooldown = cooldown_time;
            timer_lasting = lasting_tick;
            EnableWeapon();
        }
    }
}

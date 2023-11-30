using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public float cooldown_time = 0.6f;
    public int lasting_tick = 3;
    public GameObject weapon_appearance;

    // 计时器，你不应该在计时之外修改这些值。
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
    // 物理帧
    private void FixedUpdate()
    {
        // 武器持续时间计时，计时结束时禁用武器。
        if (timer_lasting > 0)
        {
            if (--timer_lasting <= 0)
            {
                DisableWeapon();
            }
        }
        // 普通冷却计时
        if (timer_cooldown > 0)
        {
            timer_cooldown -= Time.fixedDeltaTime;
        }
    }
    // 是否冷却完成
    public bool IsWeaponReady()
    {
        return timer_cooldown <= 0;
    }
    // 拓展武器时应当重写这两个函数以适应武器外观和功能
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

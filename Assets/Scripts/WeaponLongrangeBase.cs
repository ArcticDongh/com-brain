using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponLongrangeBase : WeaponBase
{
    private class ProjectionData
    {
        private float timestamp;
        public GameObject projection_ref;

        public ProjectionData()
        {
            timestamp = Time.time;
        }

        public bool IsOutOfTime(float time)
        {
            return (Time.time - timestamp) > time;
        }
    }
    [Header("WeaponLongrange")]
    public GameObject projection_prefab;
    public float projection_speed = 2f;
    public float projection_lasting_time = 1f;
    public bool do_inherit_player_velocity = true;

    private readonly List<ProjectionData> projection_datas = new();

    protected override void EnableWeapon()
    {
        base.EnableWeapon();    // display
        // 实例化
        var inst = Instantiate(projection_prefab);
        inst.transform.SetPositionAndRotation(transform.position, PlayerControl.Instance.transform.rotation);
        var rb = inst.GetComponent<Rigidbody2D>();
        var angle = PlayerControl.Instance.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        var v = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
        rb.velocity = v * projection_speed;

        if (do_inherit_player_velocity)
        {
            rb.velocity += PlayerControl.Instance.GetComponent<Rigidbody2D>().velocity;
        }

        projection_datas.Add(new ProjectionData() { projection_ref = inst });
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        // 移除所有超时项，同时销毁对象。内有魔法lambda函数。有种JavaScript的美（划掉）
        projection_datas.RemoveAll((data) => { 
            if (data.IsOutOfTime(projection_lasting_time))
            {
                if (data.projection_ref is not null) Destroy(data.projection_ref);  // 销毁存在的对象。
                return true; 
            }
            return false;
        });
    }

}

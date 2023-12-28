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
    public bool do_inherit_source_velocity = true;

    private readonly List<ProjectionData> projection_datas = new();

    protected override void EnableWeapon()
    {
        base.EnableWeapon();    // display

        Shoot();

        /*
        // ʵ����
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
        */
    }

    protected virtual void Shoot()
    {
        ShootTowards(PlayerControl.Instance.transform, FW.Utilities.GetMouseWorldCoordinate());
    }

    protected void ShootTowards(Transform source, Vector2 target)
    {
        var inst = Instantiate(projection_prefab);
        inst.transform.position = source.position;
        var rb = inst.GetComponent<Rigidbody2D>();
        var delta = target - (Vector2)source.position;
        rb.velocity = delta.normalized * projection_speed;

        var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        rb.rotation = angle;

        if (do_inherit_source_velocity)
        {
            if (source.TryGetComponent<Rigidbody2D>(out Rigidbody2D source_rb))
            {
                rb.velocity += source_rb.velocity;
            }
        }

        projection_datas.Add(new ProjectionData() { projection_ref = inst });
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        // �Ƴ����г�ʱ�ͬʱ���ٶ�������ħ��lambda����������JavaScript������������
        projection_datas.RemoveAll((data) => { 
            if (data.IsOutOfTime(projection_lasting_time))
            {
                if (data.projection_ref is not null) Destroy(data.projection_ref);  // ���ٴ��ڵĶ���
                return true; 
            }
            return false;
        });
    }

}

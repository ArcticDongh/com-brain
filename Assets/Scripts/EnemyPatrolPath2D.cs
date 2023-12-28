using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolPath2D : EnemyBase
{
    [Header("EnemyPatrol")]
    public float path_node_close_enough_length = 0.05f;
    public float patrol_speed = 2f;

    // protected int patrol_index = 0;
    protected FW.Path2D path2d;

    private float ai_patrol_stay_timer = 0;
    private bool ai_patrol_is_staying = false;

    protected override void Start()
    {
        base.Start();

        TestPath2D();
    }

    private void Update()
    {
        if (PlayerControl.Instance.DebugShow && path2d is not null)
        {
            path2d.DebugDraw();
        }
    }

    private void TestPath2D()
    {
        Vector2[] vecs = { new Vector2(1f, 1f), new Vector2(7f, 2f), new Vector2(3f, 8f) };
        float[] fc = { 90, 270, 0 };
        float[] wait_times = { 2, 2, 4 };
        bool[] looks = { false, false, false };

        path2d = new FW.Path2D(vecs, fc, wait_times, looks);
    }

    protected override void AIBehaviorModeIdle()
    {
        if (path2d is null) return;

        var cNode = path2d.CurrentNode;

        if (ai_patrol_is_staying)   // 停留模式
        {
            if ((ai_patrol_stay_timer += Time.fixedDeltaTime) > cNode.stay_time)
            {
                // 结束停留，前往下一个点。
                ai_patrol_is_staying = false;
                path2d.CurrentIndex++;
            }
            ai_move_direction = Vector2.zero;
            ai_face_degree = cNode.faceing_direction;

            return;
        }

        speed = patrol_speed;

        var move_target = cNode.point;
        var delta = move_target - (Vector2)transform.position;
        ai_move_direction = delta.normalized;

        ai_face_degree = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90;

        if (delta.magnitude <= path_node_close_enough_length)
        {
            // 到达地点
            // patrol_index = (patrol_index + 1) % path2d.Size;

            ai_patrol_is_staying = true;    // 开始停留
            ai_patrol_stay_timer = 0;
        }
    }

    private class SaveData : Object
    {
        // 巡逻AI数据
        public float ai_patrol_stay_timer;
        public bool ai_patrol_is_staying;
        public int path_current_index;
        // 额外数据，考虑用于继承
        public Object extra_data;
    }

    public override Object Serialize()
    {
        var basedata = base.Serialize();
        var data = new SaveData()
        {
            ai_patrol_stay_timer = ai_patrol_stay_timer,
            ai_patrol_is_staying = ai_patrol_is_staying,
            path_current_index = path2d.CurrentIndex,

            extra_data = basedata,
        };
        return data;
    }

    public override void Deserialize(Object saved_data)
    {
        SaveData data = saved_data as SaveData;
        if (data is null)
        {
            Debug.LogError("拆箱失败：" + saved_data.ToString());
        }

        ai_patrol_stay_timer = data.ai_patrol_stay_timer;
        ai_patrol_is_staying = data.ai_patrol_is_staying;
        path2d.CurrentIndex = data.path_current_index;

        base.Deserialize(data.extra_data);
    }
}

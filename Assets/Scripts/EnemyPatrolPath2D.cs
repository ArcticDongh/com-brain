using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolPath2D : EnemyBase
{
    public float path_node_close_enough_length = 0.05f;
    public float patrol_speed = 2f;

    protected int patrol_index = 0;
    protected FW.Path2D path2d;

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
        float[] fc = { 0, 0, 0 };

        path2d = new FW.Path2D(vecs, fc);
    }

    protected override void AIBehaviorModeIdle()
    {
        if (path2d is null) return;

        speed = patrol_speed;

        var move_target = path2d.GetPoint(patrol_index);
        var delta = move_target - (Vector2)transform.position;
        ai_move_direction = delta.normalized;

        ai_face_degree = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90;

        if (delta.magnitude <= path_node_close_enough_length)
        {
            patrol_index = (patrol_index + 1) % path2d.Size;
        }
    }
}

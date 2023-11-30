using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour
{
    public enum AIMode { IDLE, PATROL, SUSPECT, ALARM, DISABLED, PRESERVED1, PRESERVED2, PRESERVED3 };

    public float sight_range = 4.8f;
    public float sight_angle = 30f;
    public float acceleration = 0.45f;
    public float sticky_rate = 0.9f;
    public int ai_sight_stagesize = 60;
    public float rotate_speed = 2f;
    public SpriteRenderer sprite_ref;
    public GameObject sight_visual_ref;

    private AIMode ai_mode;
    private bool is_alive = true;
    private int ai_sight_progress = 0;
    private Vector2 ai_last_spot;
    private Vector2 ai_move_direction;
    private float ai_face_degree;   // AI控制的目标朝向（rotation）
    private Rigidbody2D rb;
    private ParticleSystem particle_blood_ref;  // 死亡时的粒子效果，目前通过在子物体中寻找ParticleSystem获得。
    private GameObject hide_behind_wall_ref;
    private EnemyInfo enemy_info;

    private static List<EnemyBase> enemies = new();
    public static List<EnemyBase> Enemies { get { return enemies; } }

    private void Awake()
    {
        enemies.Add(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        particle_blood_ref = GetComponentInChildren<ParticleSystem>();
        var t_transform = FW.Utilities.GetChildByName(transform, "HideBehindWall");
        if (t_transform is null)
        {
            Debug.LogError("找不到子对象: HideBehindWall");
        }
        else
        {
            hide_behind_wall_ref = t_transform.gameObject;
        }
        // 创建UI信息
        enemy_info = CanvasEnemyInfo.Instance.CreateEnemyInfoInstance();
        enemy_info.SetTrack(transform);
    }

    private void OnDestroy()
    {
        enemies.Remove(this);
    }

    private void FixedUpdate()
    {
        AIBehavior();
        ProcessSight();

        // 提示敌人是否能看见玩家
        // IsSeePlayer();

        // 移动
        rb.velocity *= sticky_rate;
        var direction = ai_move_direction;
        if (direction.magnitude > 1) direction = ai_move_direction.normalized;
        rb.velocity += direction * acceleration;

        // 旋转
        var rdelta = ((ai_face_degree - rb.rotation + 180) % 360) - 180;
        /*if (Mathf.Abs(rdelta) <= rotate_speed)
        {
            rb.angularVelocity = 0;
            rb.SetRotation(ai_face_degree);
        }
        else */
        if (rdelta >= 0)
        {
            // rb.SetRotation(rb.rotation + rotate_speed);
            rb.angularVelocity = rotate_speed;
        }
        else
        {
            // rb.SetRotation(rb.rotation - rotate_speed);
            rb.angularVelocity = -rotate_speed;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AIBehaviorSight()
    {
        if (!IsSeePlayer()) return;

        ai_last_spot = PlayerControl.Instance.transform.position;
        ai_sight_progress = Mathf.Min(ai_sight_progress + 1, ai_sight_stagesize);

        var delta = ai_last_spot - (Vector2)transform.position;
        ai_face_degree = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90;

        if (ai_sight_progress < ai_sight_stagesize) return;
        // 警戒等级提升
        if (ai_mode == AIMode.IDLE)
        {
            ai_mode = AIMode.SUSPECT;
            ai_sight_progress = 0;
        }
        else if (ai_mode == AIMode.SUSPECT)
        {
            ai_mode = AIMode.ALARM;
            ai_sight_progress = 0;
        }
        
    }

    private void AIBehaviorModeIdle()
    {
        ai_move_direction = Vector2.zero;
    }

    private void AIBehaviorModeSuspect()
    {
        ai_move_direction = (ai_last_spot - (Vector2)transform.position).normalized;
    }
    // AI行为应当写在这里
    private void AIBehavior()
    {
        if (!is_alive)
        {
            return;
        }

        AIBehaviorSight();
        switch (ai_mode)
        {
            case AIMode.IDLE:
                AIBehaviorModeIdle();
                break;
            case AIMode.SUSPECT:
                AIBehaviorModeSuspect();
                break;
            case AIMode.ALARM:
                AIBehaviorModeSuspect();
                break;
            default:
                Debug.LogError("未定义的AIMode：" + ai_mode.ToString());
                throw new System.NotImplementedException(ai_mode.ToString());
                // break;
        }
    }



    private void ProcessSight()
    {
        // 提示状态
        enemy_info.SightProgress = (float)ai_sight_progress / ai_sight_stagesize;

        enemy_info.TextInfo = ai_mode.ToString();
    }

    public void Kill()
    {
        OnKilled();
    }

    private void OnKilled()
    {
        if (!is_alive)
        {
            return;
        }

        // GetComponent<SpriteRenderer>().color = Color.Lerp(Color.black, Color.blue, 0.2f);
        if (sprite_ref is not null)
        {
            sprite_ref.color = Color.Lerp(Color.black, Color.blue, 0.2f);
        }

        if (sight_visual_ref is not null)
        {
            sight_visual_ref.SetActive(false);
        }

        if (particle_blood_ref) particle_blood_ref.Play();

        ai_move_direction = Vector2.zero;

        is_alive = false;
    }
    // 计算自身到目标位置的角度，以Right方向为0度角，逆时针角度增加。
    public float GetAngleTo(Vector3 target)
    {
        var v = target - transform.position;
        return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
    }
    // 检查是否能看到玩家，可能有1帧的延迟
    // 条件：敌人存活，与玩家的距离在视野范围内，玩家在视野角度内，射线检测路径上没有terrian（墙壁等）
    public bool IsSeePlayer()
    {
        if (!is_alive)
        {
            return false;
        }
        var delta = PlayerControl.Instance.transform.position - transform.position;
        delta.z = 0;
        // 检查距离
        if (delta.magnitude > sight_range)
        {
            return false;
        }
        // 检查视野角度
        // 
        var t_angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        // if (Mathf.Abs(t - 90 - transform.rotation.eulerAngles.z) > sight_angle)
        if (FW.Utilities.GetDeltaAngleCircularDeg(t_angle, 90 + transform.rotation.eulerAngles.z) > sight_angle)
        {
            return false;
        }
        // 检查路径
        var t_range = Mathf.Min(sight_range, delta.magnitude);
        var ray_result = Physics2D.Raycast(transform.position, delta, t_range, LayerMask.GetMask("Terrain"));
    
        Debug.DrawLine(transform.position, transform.position + delta.normalized * t_range, Color.cyan);

        if (ray_result.collider is null)
        {
            return true;
        }

        return false;
    }
    // 主动设置隐藏项目（包括敌人的贴图和可视化视线）
    // 用于玩家发现敌人和隐藏未发现的敌人
    public void SetShowEx(bool visible)
    {
        if (hide_behind_wall_ref is null) return;
        hide_behind_wall_ref.SetActive(visible);

        enemy_info.SetActive(visible);
    }
}

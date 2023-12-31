using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour, FW.ISerializable
{
    public enum AIMode { IDLE, PATROL, SUSPECT, ALARM, DISABLED, PRESERVED1, PRESERVED2, PRESERVED3 };

    public float sight_range = 4.8f;
    public float sight_angle = 30f;
 //   public float acceleration = 0.45f;
    public float sticky_rate = 0.9f;
    public float speed = 3f;

    public float searching_speed = 1f;  //搜查时速度
    public float chasing_speed = 5f;    //追击时速度

    public float sight_progress_up_speed = 1.0f;//警戒值上升速度
    public float sight_progress_down_speed = 0.3f;//警戒值下降速度
    public float ai_sight_stagesize = 60f;
    public float rotate_speed = 75f;
    public float searching_rotate_speed = 75f;//搜查时旋转速度
    public float chasing_rotate_speed = 200f;//追击时旋转速度
    public SpriteRenderer sprite_ref;
    public GameObject sight_visual_ref;

    public UnityEngine.Events.UnityEvent trigger_weapon;

    protected AIMode ai_mode;
    protected bool is_alive = true;
    protected float ai_sight_progress = 0f;
    protected Vector2 ai_last_spot;
    protected Vector2 ai_move_direction;
    protected float ai_face_degree;   // AI控制的目标朝向（rotation）
    protected Rigidbody2D rb;
    protected ParticleSystem particle_blood_ref;  // 死亡时的粒子效果，目前通过在子物体中寻找ParticleSystem获得。
    protected GameObject hide_behind_wall_ref;
    protected EnemyInfo enemy_info;

    protected static List<EnemyBase> enemies = new();
    public static List<EnemyBase> Enemies { get { return enemies; } }

    private static readonly Color color_alarm = new(0.82f, 0, 0);
    private static readonly Color color_suspect = new(0.4f, 0.34f, 0);
    private static readonly Color color_common = new(0, 0.26f, 0.4f);

    private bool cacheSeePlayer;
    private bool useCacheSeePlayer;

    // private static GameObject enemyAttackPrefab = FW.Utilities.LoadPrefab("WeaponProjectionEnemy");

    protected virtual void Awake()
    {
        enemies.Add(this);
        FW.TimelineManager.Register(this);
    }
    // Start is called before the first frame update
    protected virtual void Start()
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

        AAIMode = AAIMode;
        SetSightVisualAngle(sight_angle * 2);
    }

    private void OnDestroy()
    {
        enemies.Remove(this);
    }

    private void FixedUpdate()
    {
        useCacheSeePlayer = false;

        AIBehavior();
        ProcessSight();

        // 提示敌人是否能看见玩家
        // IsSeePlayerCached();

        // 移动

        var direction = ai_move_direction;
        if (direction.magnitude > 1) direction = ai_move_direction.normalized;
        //        rb.velocity += direction * acceleration;
        //尝试直接改变速度
        rb.velocity = direction * speed;
        rb.velocity *= sticky_rate;
        // 旋转  乱转圈bug修好了
        // rb.rotation 计量重复圈数。
        var rdelta = ShakeFix(((ai_face_degree - transform.rotation.eulerAngles.z + 900) % 360) - 180); // 内有魔法数字，你不需要知道为什么，只要能运行就好了。
        // Debug.Log(rdelta);
        /*if (Mathf.Abs(rdelta) <= rotate_speed)
        {
            rb.angularVelocity = 0;
            rb.SetRotation(ai_face_degree);
        }
        else */
        if (rdelta > 0.1f)//这里也做了防抖处理，但是写死了
        {
            // rb.SetRotation(rb.rotation + rotate_speed);
            rb.angularVelocity = rotate_speed;
        }
        else if(rdelta<-0.1f)
        {
            // rb.SetRotation(rb.rotation - rotate_speed);
            rb.angularVelocity = -rotate_speed;
        }
        else
        {
            rb.angularVelocity = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected AIMode AAIMode
    {
        get { return ai_mode; }
        set
        {
            ai_mode = value;
            if (value == AIMode.ALARM)
            {
                SetSightVisualColor(color_alarm);
            }
            else if (value == AIMode.SUSPECT)
            {
                SetSightVisualColor(color_suspect);
            }
            else if (value == AIMode.IDLE)
            {
                SetSightVisualColor(color_common);
            }
        }
    }

    protected void AIBehaviorSight()
    {
        if (!IsSeePlayerCached())
        {
            ai_sight_progress = Mathf.Max(ai_sight_progress - sight_progress_down_speed, 0);
            // 警戒等级降低
            if (ai_sight_progress > 0) return;
            if (AAIMode == AIMode.ALARM)
            {
                AAIMode = AIMode.SUSPECT;
                ai_sight_progress = ai_sight_stagesize;
            }
            else if (AAIMode == AIMode.SUSPECT)
            {
                AAIMode = AIMode.IDLE;
                ai_sight_progress = ai_sight_stagesize;
            }

            return;
        }
        //如果在视野范围内
        ai_last_spot = PlayerControl.Instance.transform.position;
        var delta = ai_last_spot - (Vector2)transform.position;
        sight_progress_up_speed = sight_range / Mathf.Abs(delta.magnitude);//距离越近警戒条进度越快
        ai_sight_progress = Mathf.Min(ai_sight_progress + sight_progress_up_speed, ai_sight_stagesize);

        ai_face_degree = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90;    // 实体默认朝上，与默认方向右有90°的相位差。

        if (ai_sight_progress < ai_sight_stagesize) return;
        // 警戒等级提升
        if (AAIMode == AIMode.IDLE)
        {
            AAIMode = AIMode.SUSPECT;
            ai_sight_progress = 0;
        }
        else if (AAIMode == AIMode.SUSPECT)
        {
            AAIMode = AIMode.ALARM;
            ai_sight_progress = 0;
        }
        
    }

    protected virtual void AIBehaviorModeIdle()
    {
        ai_move_direction = Vector2.zero;
    }

    protected virtual void AIBehaviorModeSuspect()
    {
        speed = searching_speed;
        rotate_speed = searching_rotate_speed;
        ai_move_direction = ShakeFix((ai_last_spot - (Vector2)transform.position)).normalized;
    }
    protected virtual void AIBehaviorModeAlarm()
    {
        speed = chasing_speed;
        rotate_speed = chasing_rotate_speed;
        ai_move_direction = ShakeFix((ai_last_spot - (Vector2)transform.position)).normalized;

        if (IsSeePlayerCached())
        {
            trigger_weapon.Invoke();
        }
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
                AIBehaviorModeAlarm();
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
        enemy_info.SightProgress = ai_sight_progress / ai_sight_stagesize;

        enemy_info.TextInfo = ai_mode.ToString();
    }

    public void Kill()
    {
        OnKilled();
    }

    protected virtual void OnKilled()
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

    // 功能与IsSeePlayer一致，添加了缓存以避免重复执行射线检测。
    public bool IsSeePlayerCached()
    {
        if (!useCacheSeePlayer)
        {
            cacheSeePlayer = IsSeePlayer();
            useCacheSeePlayer = true;
        }
            
        return cacheSeePlayer;
    }

    // 检查是否能看到玩家，可能有1帧的延迟
    // 条件：敌人存活，与玩家的距离在视野范围内，玩家在视野角度内，射线检测路径上没有terrian（墙壁等）
    public bool IsSeePlayer()
    {
        if (!is_alive || !PlayerControl.Instance.IsAlive)
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
        var ray_result = Physics2D.Raycast(transform.position, delta, t_range, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Hideout"));    // 仅检测地形（墙壁等）以及藏身处（草丛等）
    
        Debug.DrawLine(transform.position, transform.position + delta.normalized * t_range, Color.cyan);

        if (ray_result.collider is null)
        {
            return true;
        }

        Debug.DrawLine(ray_result.point, transform.position + delta.normalized * t_range, Color.red);

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

    public Vector2 ShakeFix(Vector2 rawVec)//防抖
    {
        if(rawVec.magnitude<0.1f)
        {
            return Vector2.zero;
        }
        return rawVec;
    }
    public float ShakeFix(float rawFloat)//防抖
    {
        if (Mathf.Abs(rawFloat) < 5)
        {
            return 0;
        }
        return rawFloat;
    }

    private class SaveData : Object
    {
        // 空间属性
        public Vector3 position;
        public Quaternion rotation;
        public Vector2 velocity;
        // 内部属性
        public bool isAlive;
    }
    // 接口函数。
    // 通常子类需要重写这两个函数以适配新增属性。
    public virtual Object Serialize()
    {
        var data = new SaveData
        {
            position = transform.position,
            rotation = transform.rotation,
            velocity = rb.velocity,
            isAlive = is_alive,
        };

        return data;
    }

    public virtual void Deserialize(Object saved_data)
    {
        SaveData data = saved_data as SaveData;
        if (data is null)
        {
            Debug.LogError("拆箱失败：" + saved_data.ToString());
        }

        transform.SetPositionAndRotation(data.position, data.rotation);
        rb.velocity = data.velocity;

        if (!is_alive && data.isAlive) Resurrect();
    }

    public void SetSightVisualColor(Color color)
    {
        if (sight_visual_ref is null) return;
        var light = sight_visual_ref.GetComponent<Light2D>();   // URP的Light2D
        if (light is null) return;
        light.color = color;
    }

    public void SetSightVisualAngle(float angle)
    {
        if (sight_visual_ref is null) return;
        var light = sight_visual_ref.GetComponent<Light2D>();   // URP的Light2D
        if (light is null) return;
        light.pointLightOuterAngle = angle;
        light.pointLightInnerAngle = angle;
        light.pointLightOuterAngle = angle;     // 三次赋值，防止可能的数值clamp
    }

    public void Resurrect()
    {
        ;//
    }
}

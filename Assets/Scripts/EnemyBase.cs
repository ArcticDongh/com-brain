using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour, FW.ISerializable
{
    public enum AIMode { IDLE, PATROL, SUSPECT, ALARM, DISABLED, PRESERVED1, PRESERVED2, PRESERVED3 };

    public float sight_range = 4.8f;
    public float sight_angle = 30f;
    public float acceleration = 0.45f;
    public float sticky_rate = 0.9f;
    public float speed = 3f;

    public float searching_speed = 1f;  //�Ѳ�ʱ�ٶ�
    public float chasing_speed = 5f;    //׷��ʱ�ٶ�
    
    public int ai_sight_stagesize = 60;
    public float rotate_speed = 75f;
    public float searching_rotate_speed = 75f;//�Ѳ�ʱ��ת�ٶ�
    public float chasing_rotate_speed = 200f;//׷��ʱ��ת�ٶ�
    public SpriteRenderer sprite_ref;
    public GameObject sight_visual_ref;

    protected AIMode ai_mode;
    protected bool is_alive = true;
    protected int ai_sight_progress = 0;
    protected Vector2 ai_last_spot;
    protected Vector2 ai_move_direction;
    protected float ai_face_degree;   // AI���Ƶ�Ŀ�곯��rotation��
    protected Rigidbody2D rb;
    protected ParticleSystem particle_blood_ref;  // ����ʱ������Ч����Ŀǰͨ������������Ѱ��ParticleSystem��á�
    protected GameObject hide_behind_wall_ref;
    protected EnemyInfo enemy_info;

    protected static List<EnemyBase> enemies = new();
    public static List<EnemyBase> Enemies { get { return enemies; } }

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
            Debug.LogError("�Ҳ����Ӷ���: HideBehindWall");
        }
        else
        {
            hide_behind_wall_ref = t_transform.gameObject;
        }
        // ����UI��Ϣ
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

        // ��ʾ�����Ƿ��ܿ������
        // IsSeePlayer();

        // �ƶ�

        var direction = ai_move_direction;
        if (direction.magnitude > 1) direction = ai_move_direction.normalized;
        //        rb.velocity += direction * acceleration;
        //����ֱ�Ӹı��ٶ�
        rb.velocity = direction * speed;
        rb.velocity *= sticky_rate;
        // ��ת  ��תȦbug�޺���
        // rb.rotation �����ظ�Ȧ����
        var rdelta = ShakeFix(((ai_face_degree - transform.rotation.eulerAngles.z + 900) % 360) - 180); // ����ħ�����֣��㲻��Ҫ֪��Ϊʲô��ֻҪ�����оͺ��ˡ�
        Debug.Log(rdelta);
        /*if (Mathf.Abs(rdelta) <= rotate_speed)
        {
            rb.angularVelocity = 0;
            rb.SetRotation(ai_face_degree);
        }
        else */
        if (rdelta > 0.1f)//����Ҳ���˷�����������д����
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

    protected void AIBehaviorSight()
    {
        if (!IsSeePlayer()) return;

        ai_last_spot = PlayerControl.Instance.transform.position;
        ai_sight_progress = Mathf.Min(ai_sight_progress + 1, ai_sight_stagesize);

        var delta = ai_last_spot - (Vector2)transform.position;
        ai_face_degree = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90;    // ʵ��Ĭ�ϳ��ϣ���Ĭ�Ϸ�������90�����λ�

        if (ai_sight_progress < ai_sight_stagesize) return;
        // ����ȼ�����
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
    }
    // AI��ΪӦ��д������
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
                Debug.LogError("δ�����AIMode��" + ai_mode.ToString());
                throw new System.NotImplementedException(ai_mode.ToString());
                // break;
        }
    }



    private void ProcessSight()
    {
        // ��ʾ״̬
        enemy_info.SightProgress = (float)ai_sight_progress / ai_sight_stagesize;

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
    // ��������Ŀ��λ�õĽǶȣ���Right����Ϊ0�Ƚǣ���ʱ��Ƕ����ӡ�
    public float GetAngleTo(Vector3 target)
    {
        var v = target - transform.position;
        return Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
    }
    // ����Ƿ��ܿ�����ң�������1֡���ӳ�
    // ���������˴�����ҵľ�������Ұ��Χ�ڣ��������Ұ�Ƕ��ڣ����߼��·����û��terrian��ǽ�ڵȣ�
    public bool IsSeePlayer()
    {
        if (!is_alive)
        {
            return false;
        }
        var delta = PlayerControl.Instance.transform.position - transform.position;
        delta.z = 0;
        // ������
        if (delta.magnitude > sight_range)
        {
            return false;
        }
        // �����Ұ�Ƕ�
        // 
        var t_angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        // if (Mathf.Abs(t - 90 - transform.rotation.eulerAngles.z) > sight_angle)
        if (FW.Utilities.GetDeltaAngleCircularDeg(t_angle, 90 + transform.rotation.eulerAngles.z) > sight_angle)
        {
            return false;
        }
        // ���·��
        var t_range = Mathf.Min(sight_range, delta.magnitude);
        var ray_result = Physics2D.Raycast(transform.position, delta, t_range, LayerMask.GetMask("Terrain"));
    
        Debug.DrawLine(transform.position, transform.position + delta.normalized * t_range, Color.cyan);

        if (ray_result.collider is null)
        {
            return true;
        }

        return false;
    }
    // ��������������Ŀ���������˵���ͼ�Ϳ��ӻ����ߣ�
    // ������ҷ��ֵ��˺�����δ���ֵĵ���
    public void SetShowEx(bool visible)
    {
        if (hide_behind_wall_ref is null) return;
        hide_behind_wall_ref.SetActive(visible);

        enemy_info.SetActive(visible);
    }

    public Vector2 ShakeFix(Vector2 rawVec)//����
    {
        if(rawVec.magnitude<0.1f)
        {
            return Vector2.zero;
        }
        return rawVec;
    }
    public float ShakeFix(float rawFloat)//����
    {
        if (Mathf.Abs(rawFloat) < 5)
        {
            return 0;
        }
        return rawFloat;
    }
    // �ӿں�����
    // ͨ��������Ҫ��д�����������������������ԡ�
    public virtual Object Serialize()
    {
        throw new System.NotImplementedException();
    }

    public virtual void Deserialize(Object saved_data)
    {
        throw new System.NotImplementedException();
    }
}

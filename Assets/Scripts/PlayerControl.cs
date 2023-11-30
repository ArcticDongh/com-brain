using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControl : UniqueMono<PlayerControl>
{
    public float acceleration = 0.45f;
    public float sticky_rate = 0.9f;
    public float sight_range = 6.4f;

    private bool debug_show = true;
    public bool DebugShow { get { return debug_show; } }
    private Rigidbody2D rb;

    public UnityEvent trigger_weapon_primary;
    public UnityEvent trigger_weapon_secondary;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // 重开场景，测试用
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    // 物理帧
    private void FixedUpdate()
    {
        // 时间流速控制
        float ts = -1f;
        if (Input.GetKey(KeyCode.Alpha1))
        {
            ts = 0.25f;
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            ts = 0.5f;
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            ts = 1f;
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            ts = 2f;
        }

        if (ts >= 0)
        {
            Time.timeScale = ts;
        }

        var direction = GetInputAxis();
        rb.velocity *= sticky_rate;
        
        if (direction.magnitude > 1)
        {
            direction.Normalize();  // 防止斜向加速移动
        }

        rb.velocity += direction * acceleration;

        // 转向到鼠标位置，朝向以上方向为准（Vector2.UP）
        var delta = FW.Utilities.GetMouseWorldCoordinate() - transform.position;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90));

        // 触发武器
        // 按住时有效，低帧率时可能造成输入问题。
        if (Input.GetMouseButton(FW.Utilities.MOUSE_BUTTON_LEFT))
        {
            trigger_weapon_primary.Invoke();
        }
        if (Input.GetMouseButton(FW.Utilities.MOUSE_BUTTON_RIGHT))
        {
            trigger_weapon_secondary.Invoke();
        }

        // 处理视线
        ProcessSight();
    }
    // 处理视线，检查每一个敌人（敌人使用静态变量Enemies存贮，初始化时自动注册）
    // 若敌人在自身视野范围内并且射线检测路径上没有terrain（墙壁等），则视作玩家发现了该敌人，显示该敌人。
    private void ProcessSight()
    {
        foreach (var enemy in EnemyBase.Enemies)
        {
            var delta = enemy.transform.position - transform.position;
            delta.z = 0;
            if (delta.magnitude > sight_range)
            {
                enemy.SetShowEx(false);
                continue;
            }
            var t_range = Mathf.Min(sight_range, delta.magnitude);
            var ray_result = Physics2D.Raycast(transform.position, delta, t_range, LayerMask.GetMask("Terrain"));

            if (ray_result.collider is not null)
            {
                Debug.DrawLine(transform.position, enemy.transform.position, Color.red);
                Debug.DrawLine(transform.position, ray_result.point, Color.green);
            }

            enemy.SetShowEx(ray_result.collider is null);
        }

        // throw new System.NotImplementedException();
    }

    private Vector2 GetInputAxis()
    {
        Vector2 result = Vector2.zero;
        result.x = Input.GetAxis("Horizontal");
        result.y = Input.GetAxis("Vertical");
        return result;
    }
}

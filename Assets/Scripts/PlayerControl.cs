using FW;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControl : UniqueMono<PlayerControl>, FW.ISoundListener, FW.ISoundSender
{
    public float sound_range = 2.0f;//发出声音范围
    public float sticky_rate = 0.9f;
    public float sight_range = 6.4f;
    public float speed = 0.2f;
    public float sneak_speed = 2f;
    public float normal_speed = 5f;
    private bool debug_show = true;
    public bool DebugShow { get { return debug_show; } }

    public GameObject SoundGameObject => gameObject;

    public SoundType SoundSourceType => SoundType.PLAYER;

    public float SoundRange => sound_range;

    private Rigidbody2D rb;
    private Light2D playerSight;

    private readonly UnityEvent trigger_weapon_primary = new();
    private readonly UnityEvent trigger_weapon_secondary = new();
    private readonly HashSet<Rigidbody2D> ignored_hideouts = new();  // 使用rigidbody2D确定一个对象
    private readonly WeaponBase[] weapons = new WeaponBase[2];

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerSight = transform.Find("Sight").GetComponent<Light2D>();

        TestWeapon();
    }

    // Update is called once per frame
    void Update()
    {
        // 重开场景，测试用
        if (Input.GetKeyDown(KeyCode.R))
        {
            FW.TimelineManager.Reset();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            Application.Quit();
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
        //潜行控制
        if(Input.GetKey(KeyCode.LeftShift))
        {
            speed = sneak_speed;
        }
        else
        {
            speed = normal_speed;
        }

        
        if (direction.magnitude > 1)
        {
            direction.Normalize();  // 防止斜向加速移动
        }

        /*
        rb.velocity *= sticky_rate;
        rb.velocity += direction * acceleration;
        */

        rb.velocity = direction* speed;//尝试放弃加速度，更好控制
        rb.velocity *= sticky_rate;
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
        SendSound();
    }
    // 处理视线，检查每一个敌人（敌人使用静态变量Enemies存贮，初始化时自动注册）
    // 若敌人在自身视野范围内并且射线检测路径上没有terrain（墙壁等），则视作玩家发现了该敌人，显示该敌人。
    private void ProcessSight()
    {
        //修改玩家视野范围
        playerSight.pointLightInnerRadius = sight_range / 2;
        playerSight.pointLightOuterRadius = sight_range;
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
            // 检查玩家到该敌人路径上的所有collider2d，仅包含图层Terrain和Hideout（主要是墙壁和草丛）
            var ray_result = Physics2D.RaycastAll(transform.position, delta, t_range, LayerMask.GetMask("Terrain") | LayerMask.GetMask("Hideout"));
            // 查找第一个有效碰撞，忽略一些hideouts
            int first_collide_index = -1;
            for (int i = 0; i < ray_result.Length; i++)
            {
                // Debug.Log(ray_result[i].collider.name);
                if (ray_result[i].collider is null) continue;   // 跳过空对象（应该不会有）
                if (ignored_hideouts.Contains(ray_result[i].rigidbody)) continue;    // 跳过玩家所在的hideout
                first_collide_index = i;
                break;
            }

            if (first_collide_index != -1)
            {
                Debug.DrawLine(transform.position, enemy.transform.position, Color.red);
                Debug.DrawLine(transform.position, ray_result[first_collide_index].point, Color.green);
            }

            enemy.SetShowEx(first_collide_index == -1);
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

    // 处理玩家进入、离开草丛。
    // 由于视线的碰撞检测和草丛区域在不同图层且分别为collider2d和trigger2d，射线检测获得的是草丛的碰撞体，而玩家进入获得trigger。
    // 这里使用rigidbody2d唯一指定草丛对象。
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log("entering hideout:" + collision.name);
        ignored_hideouts.Add(collision.attachedRigidbody);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Debug.Log("exiting hideout:" + collision.name);
        ignored_hideouts.Remove(collision.attachedRigidbody);
    }
    // 尝试在当前transform下创建武器对象，返回是否成功。
    private bool CreateWeaponByName(string name, int mount)
    {
        if (mount > 1)
        {
            Debug.LogError("暂不支持多于2的武器槽位。");
            return false;
        }
        // 覆盖消除
        if (weapons[mount] is not null)
        {
            weapons[mount].DestroyWeapon();
        }
        // 创建实例
        var prefab = FW.Utilities.LoadPrefab(name);
        if (prefab is null) return false;
        var w = Instantiate(prefab, transform).GetComponent<WeaponMount>().GetWeapon();
        // 绑定UI
        string uiname = mount == 0 ? "WeaponInfoLeft" : "WeaponInfoRight";
        w.BindUI(UniqueName.FindByName(uiname).GetComponent<WeaponInfoUI>());
        // 绑定触发器（按键）
        UnityEvent trig = mount == 0 ? trigger_weapon_primary : trigger_weapon_secondary;
        trig.RemoveAllListeners();
        trig.AddListener(w.OnWeaponTriggered);
        // 登记
        weapons[mount] = w;
        return true;
    }

    private void TestWeapon()
    {
        CreateWeaponByName("WeaponShortBase", 0);
        CreateWeaponByName("WeaponLongrangeBase", 1);
    }

    public void DebugCreateWeaponTo1(string name)
    {
        CreateWeaponByName(name, 1);
    }

    public void OnHearSound(ISoundSender source)
    {
        //播放声音
    //    throw new System.NotImplementedException();
    }

    public void SendSound()
    {
        for(int i=0;i< EnemyBase.Enemies.Count;i++)
        {
            if ((EnemyBase.Enemies[i].transform.position - transform.position).magnitude < sound_range*speed/normal_speed)//如果敌人和玩家的距离小于玩家的发声音范围，则敌人听到声音
            {
                EnemyBase.Enemies[i].OnHearSound(this);
            }
                
        }
        
//        throw new System.NotImplementedException();
    }
}

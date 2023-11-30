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
        // �ؿ�������������
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    // ����֡
    private void FixedUpdate()
    {
        // ʱ�����ٿ���
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
            direction.Normalize();  // ��ֹб������ƶ�
        }

        rb.velocity += direction * acceleration;

        // ת�����λ�ã��������Ϸ���Ϊ׼��Vector2.UP��
        var delta = FW.Utilities.GetMouseWorldCoordinate() - transform.position;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90));

        // ��������
        // ��סʱ��Ч����֡��ʱ��������������⡣
        if (Input.GetMouseButton(FW.Utilities.MOUSE_BUTTON_LEFT))
        {
            trigger_weapon_primary.Invoke();
        }
        if (Input.GetMouseButton(FW.Utilities.MOUSE_BUTTON_RIGHT))
        {
            trigger_weapon_secondary.Invoke();
        }

        // ��������
        ProcessSight();
    }
    // �������ߣ����ÿһ�����ˣ�����ʹ�þ�̬����Enemies��������ʼ��ʱ�Զ�ע�ᣩ
    // ��������������Ұ��Χ�ڲ������߼��·����û��terrain��ǽ�ڵȣ�����������ҷ����˸õ��ˣ���ʾ�õ��ˡ�
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

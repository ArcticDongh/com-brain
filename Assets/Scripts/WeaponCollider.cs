using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollider : MonoBehaviour
{
    public bool do_destroy_on_wall = false;
    public bool do_pierce = false;
    public bool do_kill_enemy = true;
    public bool do_kill_player = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (PlayerControl.Instance.DebugShow)
        {
            Debug.Log("weapon hit" + collision.name);
        }
        // 使用unity的消息机制调用击杀敌人。SendMessage可能造成性能问题和调用问题，但是这个耦合度低且便利。
        // 使用CompareTag过滤Enemy类，同时也能优化性能。
        if (   (do_kill_enemy  && collision.CompareTag("Enemy"))
            || (do_kill_player && collision.CompareTag("Player"))  )
        {
            collision.SendMessage("OnKilled");

            if (!do_pierce)
            {
                Destroy(gameObject);
                return;
            }
        }

        if (do_destroy_on_wall && collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }
    }
}

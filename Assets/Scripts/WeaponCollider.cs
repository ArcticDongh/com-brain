using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (PlayerControl.Instance.DebugShow)
        {
            Debug.Log("weapon hit" + collision.name);
        }
        // 使用unity的消息机制调用击杀敌人。SendMessage可能造成性能问题和调用问题，但是这个耦合度低且便利。
        // 使用CompareTag过滤Enemy类，同时也能优化性能。
        if (collision.CompareTag("Enemy"))
        {
            collision.SendMessage("OnKilled");
        }
    }
}

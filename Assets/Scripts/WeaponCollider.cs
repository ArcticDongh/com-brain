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
        // ʹ��unity����Ϣ���Ƶ��û�ɱ���ˡ�SendMessage���������������͵������⣬���������϶ȵ��ұ�����
        // ʹ��CompareTag����Enemy�࣬ͬʱҲ���Ż����ܡ�
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

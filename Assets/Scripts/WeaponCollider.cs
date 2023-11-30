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
        // ʹ��unity����Ϣ���Ƶ��û�ɱ���ˡ�SendMessage���������������͵������⣬���������϶ȵ��ұ�����
        // ʹ��CompareTag����Enemy�࣬ͬʱҲ���Ż����ܡ�
        if (collision.CompareTag("Enemy"))
        {
            collision.SendMessage("OnKilled");
        }
    }
}

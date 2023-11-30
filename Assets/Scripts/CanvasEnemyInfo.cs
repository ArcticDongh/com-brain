using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasEnemyInfo : UniqueMono<CanvasEnemyInfo>
{
    private static GameObject enemy_info_prefab;    // ʹ��lazyģʽ��ʹ�õ�ʱ�ټ��ء�


    public EnemyInfo CreateEnemyInfoInstance()
    {
        return Instantiate(GetEnemyInfoPrefab(), transform).GetComponent<EnemyInfo>();
    }

    private static GameObject GetEnemyInfoPrefab()
    {
        if (enemy_info_prefab is null)
        {
            enemy_info_prefab = FW.Utilities.LoadPrefab("EnemyInfo");
            if (enemy_info_prefab is null)
            {
                Debug.LogError("������Դʧ�ܣ�EnemyInfo");
            }
        }

        return enemy_info_prefab;
    }
}

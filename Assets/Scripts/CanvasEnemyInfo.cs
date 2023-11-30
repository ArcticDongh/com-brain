using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasEnemyInfo : UniqueMono<CanvasEnemyInfo>
{
    private static GameObject enemy_info_prefab;    // 使用lazy模式，使用到时再加载。


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
                Debug.LogError("加载资源失败：EnemyInfo");
            }
        }

        return enemy_info_prefab;
    }
}

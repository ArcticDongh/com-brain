using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTypeEnemyShoot : WeaponLongrangeBase
{
    [Header("EnemyShoot")]
    public Transform enemyTransform;
    protected override void Shoot()
    {
        ShootTowards(enemyTransform, PlayerControl.Instance.transform.position);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponMount : MonoBehaviour
{
    public WeaponBase weapon;

    public WeaponBase GetWeapon()
    {
        return weapon;
    }
}

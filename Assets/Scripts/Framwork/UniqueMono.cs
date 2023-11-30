using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    protected virtual void Awake()
    {
        if (instance != null)
        {
            throw new System.Exception("这个类至多有一个实例：" + nameof(T));
        }
        instance = this as T;
    }

    public static T Instance { get { return instance; } }

}

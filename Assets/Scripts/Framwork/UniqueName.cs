using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueName : MonoBehaviour
{
    [Header("UniqueName")]
    public string uname;
    private static readonly Dictionary<string, GameObject> name2obj = new();

    protected virtual void Awake()
    {
        if (name2obj.ContainsKey(uname))
        {
            Debug.LogWarning("�Ѵ���UniqueName: " + uname);
            return;
        }
        name2obj[uname] = gameObject;
    }
    /// <summary>
    /// �������Ʋ���ע��Ķ��������������׳��쳣��
    /// </summary>
    /// <param name="name">Ŀ������</param>
    /// <returns></returns>
    public static GameObject IndexByName(string name)
    {
        return name2obj[name];
    }
    /// <summary>
    /// �������Ʋ���ע��Ķ������������򷵻�null��
    /// </summary>
    /// <param name="name">Ŀ������</param>
    /// <returns>GameObject | null</returns>
    public static GameObject FindByName(string name)
    {
        if (name2obj.ContainsKey(name))
        {
            return name2obj[name];
        }
        return null;
    }

    public static bool TryIndexByName(string name, out GameObject result)
    {
        return name2obj.TryGetValue(name, out result);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW
{
    public static class Utilities
    {
        public static Vector3 GetMouseWorldCoordinate()
        {
            return MainCamera.Instance.ScreenToWorldPoint(Input.mousePosition);
        }

        public const int MOUSE_BUTTON_LEFT = 0;
        public const int MOUSE_BUTTON_RIGHT = 1;
        public const int MOUSE_BUTTON_MID = 2;

        // 计算圆周上两个点之间的夹角。取角度（degree）
        public static float GetDeltaAngleCircularDeg(float a1, float a2)
        {
            var t = Mathf.Abs(a1 - a2) % 360;
            return Mathf.Min(t, 360 - t);
        }
        // 计算圆周上两个点之间的夹角。取弧度（radian）
        public static float GetDeltaAngleCircularRad(float a1, float a2)
        {
            const float _2pi = Mathf.PI * 2;
            var t = Mathf.Abs(a1 - a2) % _2pi;
            return Mathf.Min(t, _2pi - t);
        }

        public static Transform GetChildByName(Transform self, string name)
        {
            for (int i = 0; i < self.childCount; i++)
            {
                var t = self.GetChild(i);
                if (t.name == name) return t;
            }

            return null;
        }

        public static GameObject LoadPrefab(string name)
        {
            return Resources.Load("Prefabs/" + name) as GameObject;
        }
    }
}
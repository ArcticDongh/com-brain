using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW
{
    public class Path2D
    {
        private Vector2[] points;   // ·���еĸ�����
        private float[] faceing_directions; // �ڸ�����ĳ��򣬲�һ��ʹ��
        private int size;
        private bool is_circulated; // �Ƿ�ѭ����Ĭ��ѭ��

        public bool IsCirculated { get { return is_circulated; } set { is_circulated = value; } }

        public Path2D(int size)
        {
            this.size = size;
            this.points = new Vector2[size];
            this.faceing_directions = new float[size];
            this.is_circulated = true;
        }

        public Path2D(Vector2[] vecs, float[] vs)
        {
            this.size = Mathf.Min(vecs.Length, vs.Length);
            if (vecs.Length != vs.Length)
            {
                Debug.LogWarning(string.Format("�������Ȳ�һ��: %d, %d", vecs.Length, vs.Length));
            }
            this.points = (Vector2[])vecs.Clone();
            this.faceing_directions = (float[])vs.Clone();
            this.is_circulated = true;
        }

        public Path2D(Path2D source)
        {
            this.size = source.size;
            this.points = (Vector2[])source.points.Clone();
            this.faceing_directions = (float[])source.faceing_directions.Clone();
            this.is_circulated = true;
        }

        public Vector2 GetPoint(int index)
        {
            if (is_circulated)
            {
                return this.points[index % this.size];
            }
            else
            {
                return this.points[index];
            }
        }

        public void SetPoint(int index, Vector2 point)
        {
            if (is_circulated)
            {
                this.points[index % this.size] = point;
            }
            else
            {
                this.points[index] = point;
            }
        }
    }
}
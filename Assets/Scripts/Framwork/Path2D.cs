using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW
{
    public class Path2D
    {
        // path节点的数据结构（未实装！）
        public class Path2DNode
        {
            public Vector2 point;
            public float faceing_direction;
            public float stay_time;
            public bool do_look_around;

            public Path2DNode() { }
            public Path2DNode(Path2DNode another)
            {
                point = another.point;
                faceing_direction = another.faceing_direction;
                stay_time = another.stay_time;
                do_look_around = another.do_look_around;
            }
        }

        private Path2DNode[] nodes; // 路径节点。
        // private Vector2[] points;   // 路径中的各个点
        // private float[] faceing_directions; // 在各个点的朝向，不一定使用
        private int size;
        private bool is_circulated; // 是否循环。默认循环
        private int currentIndex = 0;

        public bool IsCirculated { get { return is_circulated; } set { is_circulated = value; } }
        public int Size { get { return size; } }
        public int CurrentIndex { get => currentIndex; set { currentIndex = value % size; } }
        public Path2DNode CurrentNode { get => nodes[currentIndex]; }

        public Path2D(int size)
        {
            this.size = size;
            nodes = new Path2DNode[size];
            // this.points = new Vector2[size];
            // this.faceing_directions = new float[size];
            this.is_circulated = true;
        }

        public Path2D(Vector2[] points, float[] facings, float[] stay_times, bool[] look_arounds)
        {
            size = points.Length;
            if (size != facings.Length)
            {
                Debug.LogWarning(string.Format("参数长度不一致: %d, %d", points.Length, facings.Length));
            }
            // this.points = (Vector2[])points.Clone();
            // this.faceing_directions = (float[])vs.Clone();

            nodes = new Path2DNode[size];
            for (int i = 0; i < size; i++)
            {
                nodes[i] = new Path2DNode() { point = points[i], faceing_direction = facings[i], stay_time = stay_times[i], do_look_around = look_arounds[i] };
            }

            this.is_circulated = true;
        }

        public Path2D(Path2D source)
        {
            size = source.size;
            // this.points = (Vector2[])source.points.Clone();
            // this.faceing_directions = (float[])source.faceing_directions.Clone();
            nodes = new Path2DNode[size];
            for (int i = 0; i < size; i++)
            {
                nodes[i] = new Path2DNode(source.nodes[i]);
            }

            this.is_circulated = true;
        }

        public Vector2 GetPoint(int index)
        {
            if (is_circulated)
            {
                return this.nodes[index % this.size].point;
            }
            else
            {
                return this.nodes[index].point;
            }
        }

        public void SetPoint(int index, Vector2 point)
        {
            if (is_circulated)
            {
                this.nodes[index % this.size].point = point;
            }
            else
            {
                this.nodes[index].point = point;
            }
        }

        public void DebugDraw()
        {
            if (nodes is null || nodes.Length == 0) return;
            var last = nodes[0].point;
            
            for (int i = 1; i <= nodes.Length; i++)
            {
                var cur = nodes[i % nodes.Length].point;
                Debug.DrawLine(last, cur, Color.magenta);
                last = cur;
            }
        }
    }
}
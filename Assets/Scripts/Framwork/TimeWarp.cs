using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW 
{
    // 可序列化接口，用于保存现场和还原现场。
    public interface ISerializable
    {
        public Object Serialize();  // 保存数据
        public void Deserialize(Object saved_data); // 读取数据
    }

    public static class TimelineManager
    {
        private class TimeNode
        {
            private float time_stamp;
            private List<Object> container; // 作用等同于队列

            public TimeNode(float time_now)
            {
                time_stamp = time_now;
                container = new();
            }

            public void Add(ISerializable obj)
            {
                container.Add(obj.Serialize());
            }

            public void Recover(ISerializable obj)
            {
                var data = container[0];
                container.RemoveAt(0);
                obj.Deserialize(data);
            }
        }

        private static List<TimeNode> nodes = new();
        private static List<ISerializable> managed = new();

        // 注册对象
        public static void Register(ISerializable obj)
        {
            if (nodes.Count != 0)
            {
                Debug.LogWarning(string.Format("已有节点时注册对象可能破坏数据结构。新增对象: %s", obj.ToString()));
            }
            managed.Add(obj);
        }

        public static void Reset()
        {
            nodes = new();
            managed = new();
        }

        public static void Save()
        {
            var node = new TimeNode(Time.time); // 后续要换成自定义时间
            foreach (var item in managed)
            {
                node.Add(item);
            }
        }

        private static void Load(TimeNode node)
        {
            foreach (var item in managed)
            {
                node.Recover(item);
            }
        }

        public static void Load(int index)
        {
            Load(nodes[index]);
            RemoveAllNodesAfter(index); // 不一定要移除
        }

        // 移除从索引index+1开始的node。
        private static void RemoveAllNodesAfter(int index)
        {
            index = Mathf.Clamp(index + 1, 0, nodes.Count - 1);
            var n = nodes.Count - index;
            if (n <= 0) return;

            nodes.RemoveRange(index, n);
        }
    }
}
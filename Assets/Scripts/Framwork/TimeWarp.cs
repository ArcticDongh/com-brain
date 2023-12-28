using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FW 
{
    // �����л��ӿڣ����ڱ����ֳ��ͻ�ԭ�ֳ���
    public interface ISerializable
    {
        public Object Serialize();  // ��������
        public void Deserialize(Object saved_data); // ��ȡ����
    }

    public static class TimelineManager
    {
        private class TimeNode
        {
            private float time_stamp;
            private List<Object> container; // ���õ�ͬ�ڶ���
            private List<Object>.Enumerator iter;

            public TimeNode(float time_now)
            {
                time_stamp = time_now;
                container = new();
            }

            public float TimeStamp { get => time_stamp; }

            public void Add(ISerializable obj)
            {
                container.Add(obj.Serialize());
            }

            public void Recover(ISerializable obj)
            {
                iter.MoveNext();
                var data = iter.Current;
                // var data = container[0];
                // container.RemoveAt(0);
                obj.Deserialize(data);
            }

            public void StartRecover()
            {
                iter = container.GetEnumerator();
            }
        }

        private static List<TimeNode> nodes = new();
        private static List<ISerializable> managed = new();

        // ע�����
        public static void Register(ISerializable obj)
        {
            if (nodes.Count != 0)
            {
                Debug.LogWarning(string.Format("���нڵ�ʱע���������ƻ����ݽṹ����������: %s", obj.ToString()));
            }
            if (managed.Contains(obj))
            {
                Debug.LogWarning(string.Format("�����ظ�ע�ᣬ��ͨ����Ӧ���֡�����: %s", obj.ToString()));
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
            var node = new TimeNode(Time.time); // ����Ҫ�����Զ���ʱ��
            Debug.Log("Saving at:" + node.TimeStamp);
            foreach (var item in managed)
            {
                node.Add(item);
            }
            nodes.Add(node);
        }

        private static void Load(TimeNode node)
        {
            Debug.Log("Load with:" + node.TimeStamp);
            node.StartRecover();
            foreach (var item in managed)
            {
                node.Recover(item);
            }
        }

        public static void Load(int index)
        {
            if (nodes.Count <= 0)
            {
                Debug.LogWarning("No nodes to be loaded.");
                return;
            }
            Debug.Log(string.Format("{0:d} mod {1:d}", index, nodes.Count));
            if (index < 0)
            {
                if (nodes.Count == -index)
                {
                    index = 0;
                }
                else
                {
                    // index %= nodes.Count;
                    index = nodes.Count - (-index % nodes.Count);
                }
            }
            Debug.Log("load from index:" + index.ToString());
            Load(nodes[index]);
            RemoveAllNodesAfter(index); // ��һ��Ҫ�Ƴ�
        }

        // �Ƴ�������index+1��ʼ��node��
        private static void RemoveAllNodesAfter(int index)
        {
            if (index >= nodes.Count - 1) return;
            // index = Mathf.Clamp(index + 1, 0, nodes.Count - 1);
            var n = nodes.Count - index - 1;
            if (n <= 0) return;

            nodes.RemoveRange(index + 1, n);
        }
    }
}
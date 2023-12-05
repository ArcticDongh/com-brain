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

        // ע�����
        public static void Register(ISerializable obj)
        {
            if (nodes.Count != 0)
            {
                Debug.LogWarning(string.Format("���нڵ�ʱע���������ƻ����ݽṹ����������: %s", obj.ToString()));
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
            RemoveAllNodesAfter(index); // ��һ��Ҫ�Ƴ�
        }

        // �Ƴ�������index+1��ʼ��node��
        private static void RemoveAllNodesAfter(int index)
        {
            index = Mathf.Clamp(index + 1, 0, nodes.Count - 1);
            var n = nodes.Count - index;
            if (n <= 0) return;

            nodes.RemoveRange(index, n);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DreamersInc
{
    public class Pool<T> where T : new()
    {
        private List<T> m_free = new List<T>();

        private List<T> m_used = new List<T>();

        public T Get()
        {
            lock (m_free)
            {
                if (m_free.Count > 0)
                {
                    T item = m_free[0];
                    m_used.Add(item);
                    m_free.RemoveAt(0);
                    return item;
                }
                else
                {
                    T item = new T();
                    m_used.Add(item);
                    return item;
                }
            }
        }

        public void Release(T item)
        {
            lock (m_free)
            {
                m_free.Add(item);
                m_used.Remove(item);
            }
        }



    }
}
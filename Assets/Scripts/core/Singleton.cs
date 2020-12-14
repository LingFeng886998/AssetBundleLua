using UnityEngine;
using System.Threading;
using System;

namespace UnityLua
{
    public class Singleton<T> where T : new()
    {
        private static T m_Instance = default(T);
        private static object singletorLock = new object();

        public static T Instance {
            get {
                if (m_Instance == null)
                {
                    //加锁防止多线程创建单例
                    Monitor.Enter(singletorLock);
                    try
                    {
                        if (m_Instance == null)
                        {
                            m_Instance = default(T) == null ? Activator.CreateInstance<T>() : default(T);
                        }
                    }
                    finally {
                        Monitor.Exit(singletorLock);
                    }
                }
                return m_Instance;
            }
        }
    }
}

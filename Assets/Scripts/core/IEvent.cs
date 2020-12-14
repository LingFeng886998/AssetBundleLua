using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityLua
{
    interface IEvent
    {
        void Action();
        void Action<T,T1>();
        void Action<T,T1,T3>();
        void Action<T,T1,T3,T4>();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityLua
{
    class EventManager:Singleton<EventManager>
    {
        private Dictionary<string, List<Delegate>> m_DicEvent = new Dictionary<string, List<Delegate>>();
    }
}

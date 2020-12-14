using UnityEngine;

namespace UnityLua {
    public class UnityLuaGameManager : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
            Debug.Log("游戏开始");
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

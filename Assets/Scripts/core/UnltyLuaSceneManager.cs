using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityLua
{
    public enum SceneEnem {
        Loading = 0,
        MAIN = 1,
    }

    class UnltyLuaSceneManager : Singleton<UnltyLuaSceneManager>
    {

        public void LoadScene(SceneEnem scene) {

            Array array = Enum.GetValues(typeof(SceneEnem));
            Enum.GetNames(typeof(SceneEnem));
            for (int i = 0; i < array.Length; i++)
            {
                var value=  array.GetValue(i);
                Debug.Log(value);
            }

            object obj = Enum.Parse(typeof(SceneEnem),scene.ToString());
            //SceneManager.LoadSceneAsync(,LoadSceneMode.Additive);
        }

    }
}

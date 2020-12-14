
using UnityEngine;
using UnityEngine.UI;

namespace UnityLua { 
    public class Loading : MonoBehaviour {

        private Slider slider;
        private Text text_pro;
        private Text text_tip;

        private void Awake()
        {
            slider = transform.Find("Slider").GetComponent<Slider>();
            text_pro = transform.Find("text_pro").GetComponent<Text>();
            text_tip = transform.Find("text_tip").GetComponent<Text>();
        }



        public void SetActive(bool value) {
            gameObject.SetActive(value);
        }

        public void Quit()
        {
            Debug.Log("退出执行！！！");
            Application.Quit();
        }

        private void OnDestroy()
        {
            
        }
    }
}
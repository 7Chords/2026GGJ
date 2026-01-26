using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// 持久化的泛型单例基类(继承Mono)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonPersistent<T> : _ASCLifeGameObjBase where T : MonoBehaviour
    {
        private static T _g_instance;

        public static T instance => _g_instance;

        protected virtual void Awake()
        {
            if (_g_instance)
                Destroy(gameObject);
            else
                _g_instance = this as T;

            DontDestroyOnLoad(gameObject);
        }

        public override void OnDiscard()
        {
        }

        public override void OnInitialize()
        {
        }

        public override void OnResume()
        {
        }

        public override void OnSuspend()
        {
        }
    }
}

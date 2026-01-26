using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// 普通泛型单例基类(继承mono)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonMono<T> : _ASCLifeGameObjBase where T : MonoBehaviour
    {
        private static T _g_instance;

        public static T instance
        {
            get { return _g_instance; }
            set { _g_instance = value; }
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

        protected virtual void Awake()
        {
            _g_instance = this as T;
        }
    }
}

namespace SCFrame
{
    /// <summary>
    /// 普通泛型单例基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> :_ASCLifeObjBase where T : class, new()
    {
        protected static T _g_instance = new T();

        public static T instance
        {
            get
            {
                if (_g_instance == null)
                    _g_instance = new T();
                return _g_instance;
            }
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
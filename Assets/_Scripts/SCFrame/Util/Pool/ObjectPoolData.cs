using System.Collections.Generic;

namespace SCFrame
{
    /// <summary>
    /// 普通类 对象 对象池数据
    /// </summary>
    public class ObjectPoolData
    {
        public ObjectPoolData(object _obj)
        {
            PushObj(_obj);
        }

        // 对象容器
        public Queue<object> poolQueue = new Queue<object>();

        /// <summary>
        /// 将对象放进对象池
        /// </summary>
        public void PushObj(object _obj)
        {
            poolQueue.Enqueue(_obj);
        }

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <returns></returns>
        public object GetObj()
        {
            return poolQueue.Dequeue();
        }
    }
}
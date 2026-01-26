using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// GameObject对象池数据
    /// </summary>
    public class GameObjectPoolData
    {
        // 对象池中 父节点
        public GameObject _m_fatherObj;

        // 对象容器
        public Queue<GameObject> poolQueue;

        public GameObjectPoolData(GameObject _obj, GameObject _poolRootObj)
        {
            // 创建父节点 并设置到对象池根节点下方
            _m_fatherObj = new GameObject(_obj.name);
            _m_fatherObj.transform.SetParent(_poolRootObj.transform);
            poolQueue = new Queue<GameObject>();
            // 把首次创建时候 需要放入的对象 放进容器
            PushObj(_obj);
        }


        /// <summary>
        /// 将对象放进对象池
        /// </summary>
        public void PushObj(GameObject _obj)
        {
            // 对象进容器
            poolQueue.Enqueue(_obj);
            // 设置父物体
            _obj.transform.SetParent(_m_fatherObj.transform);
            // 设置隐藏
            _obj.SetActive(false);
        }

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <returns></returns>
        public GameObject GetObj(Transform _parent = null)
        {
            GameObject obj = poolQueue.Dequeue();

            // 显示对象
            obj.SetActive(true);
            // 设置父物体
            obj.transform.SetParent(_parent);
            if (_parent == null)
            {
                // 回归默认场景
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj,
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }

            return obj;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    public class SCPoolMgr : SingletonMono<SCPoolMgr>
    {
        // 根节点
        [SerializeField] private GameObject poolRootObj;

        /// <summary>
        /// GameObject对象容器
        /// </summary>
        private Dictionary<string, GameObjectPoolData> _m_gameObjectPoolDic;

        /// <summary>
        /// 普通类 对象容器
        /// </summary>
        private Dictionary<string, ObjectPoolData> _m_objectPoolDic;


        public int initSpawnAmount = 3;

        public override void OnInitialize()
        {
            _m_gameObjectPoolDic = new Dictionary<string, GameObjectPoolData>();
            _m_objectPoolDic = new Dictionary<string, ObjectPoolData>();
        }

        public override void OnDiscard()
        {
            if (_m_gameObjectPoolDic !=null)
            {
                _m_gameObjectPoolDic.Clear();
                _m_gameObjectPoolDic = null;
            }
            if (_m_objectPoolDic != null)
            {
                _m_objectPoolDic.Clear();
                _m_objectPoolDic = null;
            }
        }

        #region GameObject对象相关操作

        /// <summary>
        /// 获取GameObject,但是如果没有则返回Null
        /// </summary>
        public GameObject GetGameObject(string _assetName, GameObject _prefab = null, Transform _parent = null)
        {
            GameObject obj = null;
            if (!_m_gameObjectPoolDic.ContainsKey(_assetName))
            {
                for (int i = 0; i < initSpawnAmount; i++)
                {
                    obj = Instantiate(_prefab);
                    PushGameObject(obj);
                }

                return GetGameObject(_assetName);
            }

            // 检查有没有这一层
            if (_m_gameObjectPoolDic.TryGetValue(_assetName, out GameObjectPoolData poolData) &&
                poolData.poolQueue.Count > 0)
            {
                obj = poolData.GetObj(_parent);
            }

            return obj;
        }

        /// <summary>
        /// GameObject放进对象池
        /// </summary>
        public void PushGameObject(GameObject _obj)
        {
            string name = _obj.name.Replace("(Clone)", "");
            // 现在有没有这一层
            if (_m_gameObjectPoolDic.TryGetValue(name, out GameObjectPoolData poolData))
            {
                poolData.PushObj(_obj);
            }
            else
            {
                _m_gameObjectPoolDic.Add(name, new GameObjectPoolData(_obj, poolRootObj));
            }
        }

        #endregion

        #region 普通对象相关操作

        /// <summary>
        /// 获取普通对象
        /// </summary>
        public T GetObject<T>() where T : class, new()
        {
            T obj;
            if (CheckObjectCache<T>())
            {
                string name = typeof(T).FullName;
                obj = (T)_m_objectPoolDic[name].GetObj();
                return obj;
            }
            else
            {
                for (int i = 0; i < initSpawnAmount; i++)
                {
                    obj = new T();
                    PushObject(obj);
                }

                return GetObject<T>();
            }
        }

        /// <summary>
        /// GameObject放进对象池
        /// </summary>
        /// <param name="_obj"></param>
        public void PushObject(object _obj)
        {
            string name = _obj.GetType().FullName;
            // 现在有没有这一层
            if (_m_objectPoolDic.ContainsKey(name))
            {
                _m_objectPoolDic[name].PushObj(_obj);
            }
            else
            {
                _m_objectPoolDic.Add(name, new ObjectPoolData(_obj));
            }
        }

        private bool CheckObjectCache<T>()
        {
            string name = typeof(T).FullName;
            return _m_objectPoolDic.ContainsKey(name) && _m_objectPoolDic[name].poolQueue.Count > 0;
        }

        #endregion

        #region 删除

        /// <summary>
        /// 删除全部
        /// </summary>
        /// <param name="_clearGameObject">是否删除游戏物体</param>
        /// <param name="_clearCObject">是否删除普通C#对象</param>
        public void Clear(bool _clearGameObject = true, bool _clearCObject = true)
        {
            if (_clearGameObject)
            {
                for (int i = 0; i < poolRootObj.transform.childCount; i++)
                {
                    Destroy(poolRootObj.transform.GetChild(i).gameObject);
                }

                _m_gameObjectPoolDic.Clear();
            }

            if (_clearCObject)
            {
                _m_objectPoolDic.Clear();
            }
        }

        public void ClearAllGameObject()
        {
            Clear(true, false);
        }

        public void ClearGameObject(string _prefabName)
        {
            GameObject go = poolRootObj.transform.Find(_prefabName).gameObject;
            if (go != null)
            {
                Destroy(go);
                _m_gameObjectPoolDic.Remove(_prefabName);
            }
        }

        public void ClearGameObject(GameObject _prefab)
        {
            ClearGameObject(_prefab.name);
        }

        public void ClearAllObject()
        {
            Clear(false, true);
        }

        public void ClearObject<T>()
        {
            _m_objectPoolDic.Remove(typeof(T).FullName);
        }

        public void ClearObject(Type _type)
        {
            _m_objectPoolDic.Remove(_type.FullName);
        }

        #endregion
    }
}

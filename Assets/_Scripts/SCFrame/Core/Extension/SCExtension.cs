using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace SCFrame
{
    /// <summary>
    /// SCFrame框架的拓展方法
    /// </summary>
    public static class SCExtension
    {
        #region 通用
        /// <summary>
        /// 获取特性
        /// </summary>
        public static T GetAttribute<T>(this object _obj) where T : Attribute
        {
            return _obj.GetType().GetCustomAttribute<T>();
        }
        /// <summary>
        /// 获取特性
        /// </summary>
        /// <param name="_type">特性所在的类型</param>
        /// <returns></returns>
        public static T GetAttribute<T>(this object _obj, Type _type) where T : Attribute
        {
            return _type.GetCustomAttribute<T>();
        }

        /// <summary>
        /// 数组相等对比
        /// </summary>
        public static bool ArraryEquals(this object[] _objs, object[] _other)
        {
            if (_other == null || _objs.GetType() != _other.GetType())
            {
                return false;
            }
            if (_objs.Length == _other.Length)
            {
                for (int i = 0; i < _objs.Length; i++)
                {
                    if (!_objs[i].Equals(_other[i]))
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public static string ToPercentString(this float _num)
        {
            return string.Format("{0:0.00}%", _num * 100);
        }
        public static string ToPercentString(this int _num)
        {
            return string.Format("{0:0.00}%", _num * 100);
        }
        public static string ToPercentString(this long _num)
        {
            return string.Format("{0:0.00}%", _num * 100);
        }

        public static string ToPercentString(this double _num)
        {
            return string.Format("{0:0.00}%", _num * 100);
        }
        #endregion

        #region 资源管理
        /// <summary>
        /// GameObject放入对象池
        /// </summary>
        public static void GameObjectPushPool(this GameObject _go)
        {
            SCPoolMgr.instance.PushGameObject(_go);
        }

        /// <summary>
        /// GameObject放入对象池
        /// </summary>
        public static void GameObjectPushPool(this Component _comp)
        {
            GameObjectPushPool(_comp.gameObject);
        }

        /// <summary>
        /// 普通类放进池子
        /// </summary>
        /// <param name="_obj"></param>
        public static void ObjectPushPool(this object _obj)
        {
            SCPoolMgr.instance.PushObject(_obj);
        }
        #endregion

        //#region 本地化
        ///// <summary>
        ///// 从本地化系统中修改内容
        ///// </summary>
        ///// <param name="packName"></param>
        ///// <param name="contentKey"></param>
        //public static void JKLocaSet(this Text text, string packName, string contentKey)
        //{
        //    text.text = LocalizationManager.Instance.GetContent<L_Text>(packName, contentKey).content;
        //}
        ///// <summary>
        ///// 从本地化系统中修改内容
        ///// </summary>
        ///// <param name="packName"></param>
        ///// <param name="contentKey"></param>
        //public static void JKLocaSet(this Image image, string packName, string contentKey)
        //{
        //    image.sprite = LocalizationManager.Instance.GetContent<L_Image>(packName, contentKey).content;
        //}
        ///// <summary>
        ///// 从本地化系统中修改内容
        ///// </summary>
        ///// <param name="packName"></param>
        ///// <param name="contentKey"></param>
        //public static void JKLocaSet(this AudioSource audioSource, string packName, string contentKey)
        //{
        //    audioSource.clip = LocalizationManager.Instance.GetContent<L_Audio>(packName, contentKey).content;
        //}
        ///// <summary>
        ///// 从本地化系统中修改内容
        ///// </summary>
        ///// <param name="packName"></param>
        ///// <param name="contentKey"></param>
        //public static void JKLocaSet(this VideoPlayer videoPlayer, string packName, string contentKey)
        //{
        //    videoPlayer.clip = LocalizationManager.Instance.GetContent<L_Video>(packName, contentKey).content;
        //}
        //#endregion

        #region Mono

        /// <summary>
        /// 添加Update监听
        /// </summary>
        public static void OnUpdate(this object _obj, Action _action)
        {
            SCTaskHelper.instance.AddUpdateListener(_action);
        }
        /// <summary>
        /// 移除Update监听
        /// </summary>
        public static void RemoveUpdate(this object _obj, Action _action)
        {
            SCTaskHelper.instance.RemoveUpdateListener(_action);
        }

        /// <summary>
        /// 添加LateUpdate监听
        /// </summary>
        public static void OnLateUpdate(this object _obj, Action _action)
        {
            SCTaskHelper.instance.AddLateUpdateListener(_action);
        }
        /// <summary>
        /// 移除LateUpdate监听
        /// </summary>
        public static void RemoveLateUpdate(this object _obj, Action _action)
        {
            SCTaskHelper.instance.RemoveLateUpdateListener(_action);
        }

        /// <summary>
        /// 添加FixedUpdate监听
        /// </summary>
        public static void OnFixedUpdate(this object _obj, Action _action)
        {
            SCTaskHelper.instance.AddFixedUpdateListener(_action);
        }
        /// <summary>
        /// 移除Update监听
        /// </summary>
        public static void RemoveFixedUpdate(this object _obj, Action _action)
        {
            SCTaskHelper.instance.RemoveFixedUpdateListener(_action);
        }

        public static Coroutine StartCoroutine(this object _obj, IEnumerator _routine)
        {
            return SCTaskHelper.instance.StartCoroutine(_routine);
        }

        public static void StopCoroutine(this object _obj, Coroutine _routine)
        {
            SCTaskHelper.instance.StopCoroutine(_routine);
        }
        public static void StopAllCoroutines(this object _obj)
        {
            SCTaskHelper.instance.StopAllCoroutines();
        }

        #endregion

        #region GameObject
        public static bool IsNull(this GameObject _obj)
        {
            return ReferenceEquals(_obj, null);
        }
        #endregion



        #region 其他简易写法拓展

        public static RectTransform GetRectTransform(this GameObject _go)
        {
            RectTransform rect =  _go.GetComponent<RectTransform>();
            if (rect != null)
                return rect;
            Debug.LogError(_go + "物体上不存在RectTransform！！！");
            return null;
        }

        public static Image GetImage(this GameObject _go)
        {
            Image img = _go.GetComponent<Image>();
            if (img != null)
                return img;
            Debug.LogError(_go + "物体上不存在Image！！！");
            return null;
        }

        #endregion
    }
}

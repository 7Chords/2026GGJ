using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SCFrame
{
    /// <summary>
    /// SCFrame资源工具类
    /// </summary>
    public static class ResourcesHelper
    {

        /// <summary>
        /// 加载Unity资源  如AudioClip Sprite 预制体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T LoadAsset<T>(string _assetName) where T : UnityEngine.Object
        {
            try
            {
                return Addressables.LoadAssetAsync<T>(_assetName).WaitForCompletion();
            }
            catch(Exception ex)
            {
                Debug.LogError("ResourcesHelper记载资源出错！！！" + ex);
                return null;
            }
        }



        /// <summary>
        /// 加载游戏物体
        /// </summary>
        /// <param name="_assetName">资源名称</param>
        /// <param name="_parent">父物体</param>
        /// <param name="_automaticRelease">物体销毁时，会自动去调用一次Addressables.Release</param>
        /// <returns></returns>
        public static GameObject LoadGameObject(string _assetName, Transform _parent = null, bool _automaticRelease = true)
        {
            try
            {
                GameObject go = null;
                go = Addressables.InstantiateAsync(_assetName, _parent).WaitForCompletion();
                if (_automaticRelease)
                {
                    go.transform.AddReleaseAddressableAsset(AutomaticReleaseAssetAction);
                }
                go.name = _assetName;
                return go;
            }
            catch(Exception ex)
            {
                Debug.LogError("ResourcesHelper 加载游戏物体出错！！！" + ex);
                return null;
            }
        }

        /// <summary>
        /// 加载游戏物体
        /// </summary>
        /// <param name="_assetName">资源名称</param>
        /// <param name="_position">位置</param>
        /// <param name="_quaternion">旋转</param>
        /// <param name="_automaticRelease">物体销毁时，会自动去调用一次Addressables.Release</param>
        /// <returns></returns>
        public static GameObject LoadGameObject(string _assetName,Vector3 _position, Quaternion _quaternion , bool _automaticRelease = true)
        {
            try
            {
                GameObject go = null;
                go = Addressables.InstantiateAsync(_assetName, _position, _quaternion).WaitForCompletion();
                if (_automaticRelease)
                {
                    go.transform.AddReleaseAddressableAsset(AutomaticReleaseAssetAction);
                }
                go.name = _assetName;
                return go;
            }
            catch(Exception ex)
            {
                Debug.LogError("ResourcesHelper记载游戏物体出错！！！" + ex);
                return null;
            }
        }

        /// <summary>
        /// 自动释放资源事件，基于事件工具
        /// </summary>
        private static void AutomaticReleaseAssetAction(GameObject _obj, object[] _arg2)
        {
            Addressables.ReleaseInstance(_obj);
        }


        /// <summary>
        /// 获取实例--组件模式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="_parent"></param>
        /// <returns></returns>
        public static T Load<T>(string _assetName, Transform _parent = null) where T : Component
        {
            return LoadGameObject(_assetName, _parent).GetComponent<T>(); ;
        }

        /// <summary>
        /// 异步加载游戏物体
        /// </summary>
        /// <typeparam name="T">具体的组件</typeparam>
        public static void LoadGameObjectAsync<T>(string _assetName, Action<T> _callBack = null, Transform _parent = null) where T : UnityEngine.Object
        {

            SCTaskHelper.instance.StartCoroutine(DoLoadGameObjectAsync<T>(_assetName, _callBack, _parent));
        }

        public static void LoadGameObjectAsync<T>(string _assetName, Vector3 _pos,Quaternion _rotate, Action<T> _callBack = null, Transform _parent = null) where T : UnityEngine.Object
        {

            SCTaskHelper.instance.StartCoroutine(DoLoadGameObjectAsync<T>(_assetName, _pos, _rotate,_callBack, _parent));

        }
        static IEnumerator DoLoadGameObjectAsync<T>(string _assetName, Action<T> _callBack = null, Transform _parent = null) where T : UnityEngine.Object
        {
            AsyncOperationHandle<GameObject> request = Addressables.InstantiateAsync(_assetName, _parent);
            yield return request;
            _callBack?.Invoke(request.Result.GetComponent<T>());
        }
        static IEnumerator DoLoadGameObjectAsync<T>(string _assetName, Vector3 _pos,Quaternion _rotate, Action<T> _callBack = null, Transform _parent = null) where T : UnityEngine.Object
        {
            AsyncOperationHandle<GameObject> request = Addressables.InstantiateAsync(_assetName, _parent);
            yield return request;
            request.Result.transform.position = _pos;
            request.Result.transform.rotation = _rotate;
            _callBack?.Invoke(request.Result.GetComponent<T>());
        }

        /// <summary>
        /// 异步加载Unity资源 AudioClip Sprite GameObject(预制体)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="_callBack"></param>
        public static void LoadAssetAsync<T>(string _assetName, Action<T> _callBack) where T : UnityEngine.Object
        {
            SCTaskHelper.instance.StartCoroutine(DoLoadAssetAsync<T>(_assetName, _callBack));
        }

        static IEnumerator DoLoadAssetAsync<T>(string _assetName, Action<T> _callBack) where T : UnityEngine.Object
        {
            AsyncOperationHandle<T> request = Addressables.LoadAssetAsync<T>(_assetName);
            yield return request;
            _callBack?.Invoke(request.Result);
        }

        /// <summary>
        /// 加载指定Key的所有资源
        /// </summary>
        public static IList<T> LoadAssets<T>(string _keyName, Action<T> _callBack = null)
        {
            return Addressables.LoadAssetsAsync<T>(_keyName, _callBack).WaitForCompletion();
        }

        /// <summary>
        /// 异步加载指定Key的所有资源
        /// </summary>
        public static void LoadAssetsAsync<T>(string _keyName, Action<IList<T>> _callBack = null, Action<T> _callBackOnEveryOne = null)
        {
            SCTaskHelper.instance.StartCoroutine(DoLoadAssetsAsync<T>(_keyName, _callBack, _callBackOnEveryOne));
        }

        static IEnumerator DoLoadAssetsAsync<T>(string _keyName, Action<IList<T>> _callBack = null, Action<T> _callBackOnEveryOne = null)
        {
            AsyncOperationHandle<IList<T>> request = Addressables.LoadAssetsAsync<T>(_keyName, _callBackOnEveryOne);
            yield return request;
            _callBack?.Invoke(request.Result);
        }
        /// <summary>
        /// 异步加载游戏物体（直接返回 GameObject，不获取组件）
        /// </summary>
        /// <param name="_assetName">资源名称</param>
        /// <param name="_parent">父物体</param>
        /// <param name="_automaticRelease">物体销毁时自动释放</param>
        /// <param name="_callBack">加载完成回调（返回 GameObject）</param>
        public static void LoadGameObjectDirectAsync(string _assetName, Action<GameObject> _callBack, Transform _parent = null, bool _automaticRelease = true)
        {
            SCTaskHelper.instance.StartCoroutine(DoLoadGameObjectDirectAsync(_assetName, _callBack, _parent, _automaticRelease));
        }

        /// <summary>
        /// 异步加载游戏物体（带位置、旋转，直接返回 GameObject）
        /// </summary>
        /// <param name="_assetName">资源名称</param>
        /// <param name="_pos">位置</param>
        /// <param name="_rot">旋转</param>
        /// <param name="_callBack">加载完成回调（返回 GameObject）</param>
        /// <param name="_parent">父物体</param>
        /// <param name="_automaticRelease">物体销毁时自动释放</param>
        public static void LoadGameObjectDirectAsync(string _assetName, Vector3 _pos, Quaternion _rot, Action<GameObject> _callBack, Transform _parent = null, bool _automaticRelease = true)
        {
            SCTaskHelper.instance.StartCoroutine(DoLoadGameObjectDirectAsync(_assetName, _pos, _rot, _callBack, _parent, _automaticRelease));
        }

        /// <summary>
        /// 协程逻辑：直接返回 GameObject
        /// </summary>
        private static IEnumerator DoLoadGameObjectDirectAsync(string _assetName, Action<GameObject> _callBack, Transform _parent = null, bool _automaticRelease = true)
        {
            AsyncOperationHandle<GameObject> request = Addressables.InstantiateAsync(_assetName, _parent);
            yield return request;

            GameObject resultGO = null;
            if (request.Status == AsyncOperationStatus.Succeeded && request.Result != null)
            {
                resultGO = request.Result;
                if (_automaticRelease)
                {
                    resultGO.transform.AddReleaseAddressableAsset(AutomaticReleaseAssetAction);
                }
                resultGO.name = _assetName;
            }
            else
            {
                Debug.LogError($"直接加载 GameObject 失败！资源名称：{_assetName}，错误：{request.OperationException?.Message}");
            }

            _callBack?.Invoke(resultGO);
        }

        /// <summary>
        /// 协程逻辑：带位置、旋转，直接返回 GameObject
        /// </summary>
        private static IEnumerator DoLoadGameObjectDirectAsync(string _assetName, Vector3 _pos, Quaternion _rot, Action<GameObject> _callBack, Transform _parent = null, bool _automaticRelease = true)
        {
            AsyncOperationHandle<GameObject> request = Addressables.InstantiateAsync(_assetName, _pos, _rot, _parent);
            yield return request;

            GameObject resultGO = null;
            if (request.Status == AsyncOperationStatus.Succeeded && request.Result != null)
            {
                resultGO = request.Result;
                if (_automaticRelease)
                {
                    resultGO.transform.AddReleaseAddressableAsset(AutomaticReleaseAssetAction);
                }
                resultGO.name = _assetName;

            }
            else
            {
                Debug.LogError($"直接加载 GameObject 失败！资源名称：{_assetName}，错误：{request.OperationException?.Message}");
            }

            _callBack?.Invoke(resultGO);
        }
        public static void Release<T>(T _obj)
        {
            Addressables.Release<T>(_obj);
        }
        /// <summary>
        /// 释放实例
        /// </summary>
        public static bool ReleaseInstance(GameObject _obj)
        {
            return Addressables.ReleaseInstance(_obj);
        }
    }
}

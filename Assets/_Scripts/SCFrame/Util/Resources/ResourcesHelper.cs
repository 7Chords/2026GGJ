using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SCFrame
{
    /// <summary>
    /// SCFrame��Դ������
    /// </summary>
    public static class ResourcesHelper
    {

        /// <summary>
        /// ����Unity��Դ  ��AudioClip Sprite Ԥ����
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
                Debug.LogError("ResourcesHelper������Դ����������" + ex);
                return null;
            }
        }



        /// <summary>
        /// ������Ϸ����
        /// </summary>
        /// <param name="_assetName">��Դ����</param>
        /// <param name="_parent">������</param>
        /// <param name="_automaticRelease">��������ʱ�����Զ�ȥ����һ��Addressables.Release</param>
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
                Debug.LogError("ResourcesHelper ������Ϸ�������������" + ex);
                return null;
            }
        }

        /// <summary>
        /// ������Ϸ����
        /// </summary>
        /// <param name="_assetName">��Դ����</param>
        /// <param name="_position">λ��</param>
        /// <param name="_quaternion">��ת</param>
        /// <param name="_automaticRelease">��������ʱ�����Զ�ȥ����һ��Addressables.Release</param>
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
                Debug.LogError("ResourcesHelper������Ϸ�������������" + ex);
                return null;
            }
        }

        /// <summary>
        /// �Զ��ͷ���Դ�¼��������¼�����
        /// </summary>
        private static void AutomaticReleaseAssetAction(GameObject _obj, object[] _arg2)
        {
            Addressables.ReleaseInstance(_obj);
        }


        /// <summary>
        /// ��ȡʵ��--���ģʽ
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
        /// �첽������Ϸ����
        /// </summary>
        /// <typeparam name="T">��������</typeparam>
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
        /// �첽����Unity��Դ AudioClip Sprite GameObject(Ԥ����)
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
        /// ����ָ��Key��������Դ
        /// </summary>
        public static IList<T> LoadAssets<T>(string _keyName, Action<T> _callBack = null)
        {
            return Addressables.LoadAssetsAsync<T>(_keyName, _callBack).WaitForCompletion();
        }

        /// <summary>
        /// �첽����ָ��Key��������Դ
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
        /// �첽������Ϸ���壨ֱ�ӷ��� GameObject������ȡ�����
        /// </summary>
        /// <param name="_assetName">��Դ����</param>
        /// <param name="_parent">������</param>
        /// <param name="_automaticRelease">��������ʱ�Զ��ͷ�</param>
        /// <param name="_callBack">������ɻص������� GameObject��</param>
        public static void LoadGameObjectDirectAsync(string _assetName, Action<GameObject> _callBack, Transform _parent = null, bool _automaticRelease = true)
        {
            SCTaskHelper.instance.StartCoroutine(DoLoadGameObjectDirectAsync(_assetName, _callBack, _parent, _automaticRelease));
        }

        /// <summary>
        /// �첽������Ϸ���壨��λ�á���ת��ֱ�ӷ��� GameObject��
        /// </summary>
        /// <param name="_assetName">��Դ����</param>
        /// <param name="_pos">λ��</param>
        /// <param name="_rot">��ת</param>
        /// <param name="_callBack">������ɻص������� GameObject��</param>
        /// <param name="_parent">������</param>
        /// <param name="_automaticRelease">��������ʱ�Զ��ͷ�</param>
        public static void LoadGameObjectDirectAsync(string _assetName, Vector3 _pos, Quaternion _rot, Action<GameObject> _callBack, Transform _parent = null, bool _automaticRelease = true)
        {
            SCTaskHelper.instance.StartCoroutine(DoLoadGameObjectDirectAsync(_assetName, _pos, _rot, _callBack, _parent, _automaticRelease));
        }

        /// <summary>
        /// Э���߼���ֱ�ӷ��� GameObject
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
                Debug.LogError($"ֱ�Ӽ��� GameObject ʧ�ܣ���Դ���ƣ�{_assetName}������{request.OperationException?.Message}");
            }

            _callBack?.Invoke(resultGO);
        }

        /// <summary>
        /// Э���߼�����λ�á���ת��ֱ�ӷ��� GameObject
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
                Debug.LogError($"ֱ�Ӽ��� GameObject ʧ�ܣ���Դ���ƣ�{_assetName}������{request.OperationException?.Message}");
            }

            _callBack?.Invoke(resultGO);
        }
        public static void Release<T>(T _obj)
        {
            Addressables.Release<T>(_obj);
        }
        /// <summary>
        /// �ͷ�ʵ��
        /// </summary>
        public static bool ReleaseInstance(GameObject _obj)
        {
            return Addressables.ReleaseInstance(_obj);
        }
    }
}

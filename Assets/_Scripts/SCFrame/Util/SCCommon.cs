using UnityEngine;
using UnityEditor;
using System.Globalization;
using System.Collections.Generic;
using System;
using GameCore.RefData;
using GameCore;

namespace SCFrame
{
    /// <summary>
    /// һЩ������ȫ�ֵ�ͨ�÷���
    /// </summary>
    public static class SCCommon
    {

        /// <summary>
        /// ������Ϸ�����Ƿ񱻼���
        /// </summary>
        /// <param name="_obj"></param>
        /// <param name="_isEnable"></param>
        public static void SetGameObjectEnable(GameObject _obj, bool _isEnable)
        {
            if (_obj == null)
                return;
            _obj.SetActive(_isEnable);
        }

        /// <summary>
        /// ������Ϸ�����Ƿ񱻼���
        /// </summary>
        /// <param name="_objs"></param>
        /// <param name="_isEnable"></param>
        public static void SetGameObjectEnable(GameObject[] _objs, bool _isEnable)
        {
            if (_objs == null || _objs.Length == 0)
                return;

            foreach(var go in _objs)
                go.SetActive(_isEnable);
        }

        /// <summary>
        /// ������Ϸ�����Ƿ񱻼���
        /// </summary>
        /// <param name="_objs"></param>
        /// <param name="_isEnable"></param>
        public static void SetGameObjectEnable(List<GameObject> _objs, bool _isEnable)
        {
            if (_objs == null || _objs.Count == 0)
                return;

            foreach (var go in _objs)
                go.SetActive(_isEnable);
        }

        /// <summary>
        /// ������Ϸ����
        /// </summary>
        /// <param name="_obj"></param>
        /// <returns></returns>
        public static GameObject InstantiateGameObject(GameObject _obj)
        {
            if (_obj == null)
                return null;
            GameObject go = GameObject.Instantiate(_obj);
            return go;
        }

        /// <summary>
        /// ������Ϸ����
        /// </summary>
        /// <param name="_obj"></param>
        /// <returns></returns>
        public static GameObject InstantiateGameObject(GameObject _obj,Transform _parent)
        {
            if (_obj == null)
                return null;
            GameObject go = GameObject.Instantiate(_obj, _parent);
            return go;
        }

        /// <summary>
        /// ������Ϸ����
        /// </summary>
        /// <param name="_obj"></param>
        public static void DestoryGameObject(GameObject _obj)
        {
            if (_obj == null)
                return;
            GameObject.Destroy(_obj);
        }


#if UNITY_EDITOR
        /// <summary>
        /// �༭����ǰ�Ƿ���Ԥ����༭ģʽ
        /// </summary>
        /// <returns></returns>
        public static bool IsInPrefabStage()
        {
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            return stage != null;
        }

#endif

        /// <summary>
        /// �ַ���������float
        /// </summary>
        /// <param name="_str"></param>
        /// <returns></returns>
        public static float ParseFloat(string _str)
        {
            if(float.TryParse(_str, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }
            Debug.LogError("string������float����������");
            return 0f;
        }

        /// <summary>
        /// �ַ���������int
        /// </summary>
        /// <param name="_str"></param>
        /// <returns></returns>
        public static int ParseInt(string _str)
        {
            if(int.TryParse(_str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }
            Debug.LogError("string������int����������");
            return 0;
        }


        public static object ParseEnum(string _str,Type _enumType)
        {
            if (string.IsNullOrEmpty(_str))
            {
                Debug.LogError("_str����Ч�ģ�����");
                return 0;
            }

            object obj = Enum.Parse(_enumType, _str);
            return obj;
        }

        /// <summary>
        /// �ַ���������long
        /// </summary>
        /// <param name="_str"></param>
        /// <returns></returns>
        public static long ParseLong(string _str)
        {
            if (long.TryParse(_str, NumberStyles.Number, CultureInfo.InvariantCulture, out long result))
            {
                return result;
            }
            Debug.LogError("string������long����������");
            return 0L;
        }


        public static List<T> ParseList<T>(string _name, bool _canNull = true)
        {
            List<T> list = new List<T>();

            string tempValue = _name;
            //���б���ʶ
            if (tempValue == "*")
                return list;

            string[] strs = tempValue.Split(new char[] { ';' });
            for (var i = 0; i < strs.Length; i++)
            {
                string tempStr = strs[i];
                object value = ParseValue(tempStr, typeof(T));
                if (value == null)
                {
                    continue;
                }
                else
                {
                    list.Add((T)value);
                }
            }


            return list;
        }

        // �����ֶ�ֵ
        public static object ParseValue(string _value, Type _type)
        {
            try
            {
                if (_value.Equals(string.Empty))
                {
                    if (_type == typeof(string))
                    {
                        return "";
                    }
                    return Activator.CreateInstance(_type, true);
                }
                else
                {
                    _value = _value.Trim();

                    // ö�� 
                    if (_type.IsEnum)
                    {
                        return Enum.Parse(_type, _value);
                    }

                    // �ַ���
                    else if (_type == typeof(string))
                    {
                        return _value;
                    }

                    // ������
                    else if (_type == typeof(float))
                    {
                        if (_value == "0" || _value == "" || _value == string.Empty)
                            return 0f;

                        return float.Parse(_value, CultureInfo.InvariantCulture);
                    }

                    // ����
                    else if (_type == typeof(int))
                    {
                        if (_value == "")
                            return 0;

                        return int.Parse(_value);
                    }

                    else if (_type == typeof(bool))
                    {
                        return bool.Parse(_value);
                    }

                    else if (_type == typeof(long))
                    {
                        return long.Parse(_value);
                    }
                    else if (_type.IsSubclassOf(typeof(_AEffectObjBase)))
                    {
                        return ParseEffectObj(_value, _type);
                    }
                    else if(_type == typeof(object))
                    {
                        return _value;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseValue type:{_type.ToString()}, value:{_value}, failed: {ex}");
            }
            return null;
        }

        /// <summary>
        /// �������Ч��obj
        /// </summary>
        /// <param name="_str"></param>
        /// <param name="_type"></param>
        /// <returns></returns>
        public static _AEffectObjBase ParseEffectObj(string _str, Type _type)
        {
            if (string.IsNullOrEmpty(_str))
                return null;
            _AEffectObjBase effectObj = null;

            if (_type == typeof(GoodsEffectObj))
            {
                effectObj = new GoodsEffectObj();
                effectObj.Deserialize(_str);
            }
            else if(_type == typeof(EntryEffectObj))
            {
                effectObj = new EntryEffectObj();
                effectObj.Deserialize(_str);
            }
            else if (_type == typeof(PosEffectObj))
            {
                effectObj = new PosEffectObj();
                effectObj.Deserialize(_str);
            }
            return effectObj;

        }
    }

}

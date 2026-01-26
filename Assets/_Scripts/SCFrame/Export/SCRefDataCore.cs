using GameCore;
using GameCore.RefData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// 单条配表类 用于只有key和value一一对应的类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SCRefDataCore
    {
        private string _m_assetPath;//资产路径
        private string _m_sheetName;//
        public SCRefDataCore()
        {

        }

        public SCRefDataCore(string _assetPath, string _sheetName)
        {
            _m_assetPath = _assetPath;
            _m_sheetName = _sheetName;
        }

        /** 获取加载资源对象的路径 */
        protected string _assetPath { get { return _m_assetPath; } }
        protected string _sheetName { get { return _m_sheetName; } }

        private Dictionary<string, string> _m_keyValueMap = new Dictionary<string, string>();

        private const string EXCEL_EMPTY_FLAG = "*";

        public void readFromTxt()
        {
            if (string.IsNullOrEmpty(_m_assetPath) || string.IsNullOrEmpty(_m_sheetName))
            {
                Debug.LogError(_m_assetPath + "或" + _m_sheetName + "没有信息，导出失败！");
                return;
            }
            using (StreamReader sr = new StreamReader("Assets/Resources/RefData/ExportTxt/" + _m_sheetName + ".txt"))
            {
                string str = sr.ReadToEnd();
                parseFromTxt(str);
            }
        }
        protected void parseFromTxt(string _string)
        {
            if (string.IsNullOrEmpty(_string))
            {
                Debug.LogError("txt为空");
                return;
            }

            _m_keyValueMap.Clear();
            string[] lineArray = _string.Split('\n');
            for (int i = 1; i < lineArray.Length; i++)
            {
                string line = lineArray[i];
                if (string.IsNullOrEmpty(line) || line == "\t")
                {
                    continue;
                }
                string[] lineSplit = line.Split('\t');
                if (lineSplit.Length < 2)
                {
                    Debug.LogError($"general表格式错误：表格{_m_sheetName}，第{i}行：\n{line}");
                    continue;
                }
                try
                {
                    _m_keyValueMap.Add(lineSplit[0].Trim(), lineSplit[1].Trim());
                }
                catch(Exception ex)
                {
                    Debug.LogError($"general表格式的" + _m_sheetName + "表添加到字典出现问题，key：" + lineSplit[0] + ",value:" +lineSplit[1]+
                        "----"+ex) ;
                }
            }

            try
            {
                _parseFromString();
            }
            catch (Exception e)
            {
                Debug.LogError($"SCRefDataCore:{e}");
                return;
            }
        }
        public void singleParseFormTxt(string _keys,string _values)
        {
            if (string.IsNullOrEmpty(_keys) || string.IsNullOrEmpty(_values))
                return;

            string[] keySplitArr = _keys.Split("\t");
            string[] valSplitArr = _values.Split("\t");

            if(keySplitArr.Length != valSplitArr.Length)
            {
                Debug.LogError(_sheetName + "要配置的变量数目和实际配置的变量数目不匹配！！！");
                return;
            }
            _m_keyValueMap.Clear();
            for (int i =0;i<keySplitArr.Length;i++)
            {
                _m_keyValueMap.Add(keySplitArr[i], valSplitArr[i]);
            }
            _parseFromString();
            //try
            //{
            //    _parseFromString();
            //}
            //catch (Exception e)
            //{
            //    Debug.LogError($"SCRefDataCore:{e}");
            //    return;
            //}
        }
        protected abstract void _parseFromString();

        #region getXXX

        protected string getString(string _key)
        {
            string result;
            if (!_m_keyValueMap.TryGetValue(_key, out result))
            {
                Debug.LogError($"_key不存在{_key}");
                return null;
            }
            return result;
        }

        protected int getInt(string _name)
        {
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return 0;
            }
            if (!int.TryParse(tempValue, out int result))
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是int");
            }
            return result;
        }

        protected object getEnum(string _name,Type _type)
        {
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return 0;
            }

            object obj = Enum.Parse(_type, tempValue);
            if (obj == null)
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是enum");
            }
            return obj;
        }

        protected long getLong(string _name, bool _canNull = true)
        {

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return 0;
            }
            if (!long.TryParse(tempValue, out long result))
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是long");
            }
            return result;
        }

        protected bool getBool(string _name, bool _canNull = true)
        {

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return false;
            }
            if (!bool.TryParse(tempValue, out bool result))
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是bool");
            }
            return result;
        }

        protected float getFloat(string _name, bool _canNull = true)
        {

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return 0;
            }
            if (!float.TryParse(tempValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是float");
            }
            return result;
        }

        protected Vector2 getVector2(string _name, bool _canNull = true)
        {
            Vector2 v2 = new Vector2();
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return v2;
            }

            string[] strs = tempValue.Split(new char[] { ';', ':' });
            if (strs.Length < 2)
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是Vector2");
                return v2;
            }
            v2.Set(SCCommon.ParseFloat(strs[0]), SCCommon.ParseFloat(strs[1]));
            return v2;
        }

        protected Vector3 getVector3(string _name, bool _canNull = true)
        {
            Vector3 v3 = new Vector3();
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return v3;
            }

            string[] strs = tempValue.Split(new char[] { ';', ':' });
            if (strs.Length < 3)
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是Vector3");
                return v3;
            }

            v3.Set(SCCommon.ParseFloat(strs[0]), SCCommon.ParseFloat(strs[1]), SCCommon.ParseFloat(strs[2]));
            return v3;
        }

        protected Rect getRect(string _name, bool _canNull = true)
        {
            Rect rect = new Rect();
            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return rect;
            }

            string[] strs = tempValue.Split(new char[] { ';', ':' });
            if (strs.Length < 4)
            {
                Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempValue}不是rect");
                return rect;
            }

            rect.Set(SCCommon.ParseFloat(strs[0]), SCCommon.ParseFloat(strs[1]), SCCommon.ParseFloat(strs[2]), SCCommon.ParseFloat(strs[3]));
            return rect;
        }

        protected List<T> getList<T>(string _name, bool _canNull = true)
        {
            List<T> list = new List<T>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return list;
            }
            //空列表标识
            if (tempValue == EXCEL_EMPTY_FLAG)
                return list;

            string[] strs = tempValue.Split(new char[] { ';' });
            for (var i = 0; i < strs.Length; i++)
            {
                string tempStr = strs[i];
                object value = ParseValue(tempStr, typeof(T));
                if (value == null)
                {
                    Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},填的{tempStr}解析失败，完整数据：{value}");
                }
                else
                {
                    list.Add((T)value);
                }
            }


            return list;
        }

        protected List<Vector2> getVector2List(string _name, bool _canNull = true)
        {
            List<Vector2> vector2List = new List<Vector2>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return vector2List;
            }
            string[] strs = tempValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < strs.Length; i++)
            {
                string[] valueStrs = strs[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (valueStrs == null || valueStrs.Length != 2)
                {
                    Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},解析失败，完整数据：{valueStrs}");
                }
                else
                {
                    float x = SCCommon.ParseFloat(valueStrs[0]);
                    float y = SCCommon.ParseFloat(valueStrs[1]);
                    vector2List.Add(new Vector2(x, y));
                }
            }
            return vector2List;
        }

        protected List<Vector2Int> getVector2IntList(string _name, bool _canNull = true)
        {
            List<Vector2Int> Vector2IntList = new List<Vector2Int>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return Vector2IntList;
            }
            string[] strs = tempValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < strs.Length; i++)
            {
                string[] valueStrs = strs[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (valueStrs == null || valueStrs.Length != 2)
                {
                    Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},解析失败，完整数据：{valueStrs}");
                }
                else
                {
                    int x = SCCommon.ParseInt(valueStrs[0]);
                    int y = SCCommon.ParseInt(valueStrs[1]);
                    Vector2IntList.Add(new Vector2Int(x, y));
                }
            }
            return Vector2IntList;
        }

        protected List<Vector3> getVector3List(string _name, bool _canNull = true)
        {
            List<Vector3> vector3List = new List<Vector3>();

            string tempValue = getString(_name);
            if (string.IsNullOrEmpty(tempValue))
            {
                if (!_canNull)
                    Debug.LogError($"{_m_assetPath},{_m_sheetName}的字段{_name}为空");
                return vector3List;
            }
            string[] strs = tempValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < strs.Length; i++)
            {
                string[] valueStrs = strs[i].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                if (valueStrs == null || valueStrs.Length != 3)
                {
                    Debug.LogError($"表\"{_m_assetPath},{_m_sheetName}\"\t数据填写错误: {_name},解析失败，完整数据：{valueStrs}");
                }
                else
                {
                    float x = SCCommon.ParseFloat(valueStrs[0]);
                    float y = SCCommon.ParseFloat(valueStrs[1]);
                    float z = SCCommon.ParseFloat(valueStrs[2]);
                    vector3List.Add(new Vector3(x, y, z));
                }
            }
            return vector3List;
        }

        // 解析字段值
        protected static object ParseValue(string _value, Type _type)
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

                    // 枚举 
                    if (_type.IsEnum)
                    {
                        return Enum.Parse(_type, _value);
                    }

                    // 字符串
                    else if (_type == typeof(string))
                    {
                        return _value;
                    }

                    // 浮点型
                    else if (_type == typeof(float))
                    {
                        if (_value == "0" || _value == "" || _value == string.Empty)
                            return 0f;

                        return float.Parse(_value, CultureInfo.InvariantCulture);
                    }

                    // 整形
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
                        return SCCommon.ParseEffectObj(_value, _type);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseValue type:{_type.ToString()}, value:{_value}, failed: {ex}");
            }
            return null;
        }

        #endregion

    }
}

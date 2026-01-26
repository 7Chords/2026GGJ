using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// 单条配表集合类 用于第一行是标题 下面全是配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SCRefDataList<T> where T : SCRefDataCore,new()
    {
        private List<T> _m_refDataList;
        public List<T> refDataList => _m_refDataList;

        private string _m_assetPath;
        private string _m_sheetName;
        public SCRefDataList(string _assetPath, string _sheetName)
        {
            _m_assetPath = _assetPath;
            _m_sheetName = _sheetName;
            _m_refDataList = new List<T>();

        }

        public void readFromTxt()
        {
            if(string.IsNullOrEmpty(_m_assetPath) || string.IsNullOrEmpty(_m_sheetName))
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
            string[] lineArray = _string.Split('\n');
            //对于list类型的配表来说 第一行是标题行 是key行
            string keyLine = lineArray[0];
            for (int i = 1; i < lineArray.Length; i++)
            {
                string line = lineArray[i];
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }
                T dataCore = new T();
                dataCore.singleParseFormTxt(keyLine, line);
                _m_refDataList.Add(dataCore);
            }
        }
    }


}

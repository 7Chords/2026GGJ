using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class TextLanguageRefObj : SCRefDataCore
    {
        public TextLanguageRefObj()
        {

        }
        public TextLanguageRefObj(string _assetPath, string _sheetName) : base(_assetPath, _sheetName)
        {
        }
        public long id;
        public string translateKey;
        public string zh_CN;
        public string en_US;
        protected override void _parseFromString()
        {
            id = getLong("id");
            translateKey = getString("translateKey");
            zh_CN = getString("zh_CN");
            en_US = getString("en_US");
        }

        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "text_language";
    }
}

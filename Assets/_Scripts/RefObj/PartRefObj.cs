using SCFrame;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class PartRefObj : SCRefDataCore
    {
        public long id;
        public string partName;
        public EPartType partType;
        public int partHealth;
        public string partDesc;
        public string partSpriteObjName;
        public Vector2Int midPos;
        public string logicClassName; // Logic Class Name
        public List<EntryEffectObj> entryList;
        public List<PosEffectObj> posList;

        public PartRefObj()
        {

        }
        public PartRefObj(string _assetPath, string _sheetName) : base(_assetPath, _sheetName)
        {

        }
        protected override void _parseFromString()
        {
            id = getLong("id");
            partName = getString("partName");
            partType = (EPartType)getEnum("partType",typeof(EPartType));
            partHealth = getInt("partHealth");
            partDesc = getString("partDesc");
            partSpriteObjName = getString("partSpriteObjName");
            entryList = getList<EntryEffectObj>("entryList");
            posList = getList<PosEffectObj>("posList");
            
            logicClassName = getString("logicClassName");

            string midPosStr = getString("midPos");
            if (!string.IsNullOrEmpty(midPosStr))
            {
                string[] strArr = midPosStr.Split(':');
                if (strArr != null && strArr.Length >= 2)
                {
                    midPos = new Vector2Int(SCCommon.ParseInt(strArr[0]), SCCommon.ParseInt(strArr[1]));
                }
            }
        }

        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "part";
    }
}

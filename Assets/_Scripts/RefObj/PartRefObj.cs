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
        public EQualityType qualityType;
        public int partHealth;
        public string partDesc;
        public string partSpriteObjName;
        public string partGameObjectName;
        public Vector2Int midPos;
        public string logicClassName; // Logic Class Name
        public List<EntryEffectObj> entryList;
        public List<PosEffectObj> occupyPosList;
        public List<PosEffectObj> effectPosList;

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
            qualityType = (EQualityType)getEnum("qualityType", typeof(EQualityType));
            partHealth = getInt("partHealth");
            partDesc = getString("partDesc");
            partSpriteObjName = getString("partSpriteObjName");
            partGameObjectName = getString("partGameObjectName");
            entryList = getList<EntryEffectObj>("entryList");
            occupyPosList = getList<PosEffectObj>("occupyPosList");
            effectPosList = getList<PosEffectObj>("effectPosList");

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

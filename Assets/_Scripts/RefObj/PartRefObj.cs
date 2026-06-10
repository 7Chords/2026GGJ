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
        public bool isEnemyPart;
        public string partSpriteObjName;
        public string partPlayerGameObjectName;
        public string partEnemyGameObjectName;
        public List<PosEffectObj> occupyPosList;
        public string triggerSuccessTip;
        public string triggerFailTip;
        public string triggerEffectTip;

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
            isEnemyPart = getBool("isEnemyPart");
            partSpriteObjName = getString("partSpriteObjName");
            partPlayerGameObjectName = getString("partPlayerGameObjectName");
            partEnemyGameObjectName = getString("partEnemyGameObjectName");
            occupyPosList = getList<PosEffectObj>("occupyPosList");
            triggerSuccessTip = getString("triggerSuccessTip");
            triggerFailTip = getString("triggerFailTip");
            triggerEffectTip = getString("triggerEffectTip");
        }

        public List<Vector2Int> GetOccupyPosList()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            foreach (PosEffectObj obj in occupyPosList)
            {
                result.Add(new Vector2Int(obj.x, obj.y));
            }
            return result;
        }

        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "part";
    }
}

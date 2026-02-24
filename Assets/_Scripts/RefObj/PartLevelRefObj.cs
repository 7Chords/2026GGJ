using SCFrame;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class PartLevelRefObj : SCRefDataCore
    {
        public long id;
        public long partId;
        public int partLevel;
        public int partHealth;
        public string partDesc;
        public List<PosEffectObj> effectPosList;
        public List<EntryEffectObj> entryList;
        public int levelUpCost;
        protected override void _parseFromString()
        {
            id = getLong("id");
            partId = getLong("partId");
            partLevel = getInt("partLevel");
            partHealth = getInt("partHealth");
            partDesc = getString("partDesc");
            effectPosList = getList<PosEffectObj>("effectPosList");
            entryList = getList<EntryEffectObj>("entryList");
            levelUpCost = getInt("levelUpCost");
        }
        public List<Vector2Int> GetEffectPosList()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            foreach (PosEffectObj obj in effectPosList)
            {
                result.Add(new Vector2Int(obj.x, obj.y));
            }
            return result;
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "part_level";
    }
}

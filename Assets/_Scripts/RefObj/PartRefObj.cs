using SCFrame;
using System.Collections.Generic;

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
        public List<EntryEffectObj> entryList;
        public List<PosEffectObj> posList;
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
        }

        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "part";
    }
}

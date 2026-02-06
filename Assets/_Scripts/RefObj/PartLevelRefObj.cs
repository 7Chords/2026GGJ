using SCFrame;
using System.Collections.Generic;

namespace GameCore.RefData
{
    public class PartLevelRefObj : SCRefDataCore
    {
        public long id;
        public long partId;
        public int partLevel;
        public int partHealth;
        public string partDesc;
        public List<EntryEffectObj> entryList;
        protected override void _parseFromString()
        {
            id = getLong("id");
            partId = getLong("partId");
            partLevel = getInt("partLevel");
            partHealth = getInt("partHealth");
            partDesc = getString("partDesc");
            entryList = getList<EntryEffectObj>("entryList");
        }

        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "part_level";
    }
}

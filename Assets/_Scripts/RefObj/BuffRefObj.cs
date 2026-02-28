using SCFrame;

namespace GameCore.RefData
{
    public class BuffRefObj : SCRefDataCore
    {
        public long id;
        public string buffName;
        public string buffDesc;
        public EBuffType buffType;
        public EAttributeTriggerPointType triggerPointType;
        public string buffIconResName;
        public float buffValue;
        public bool isPositive;
        protected override void _parseFromString()
        {
            id = getLong("id");
            buffName = getString("buffName");
            buffDesc = getString("buffDesc");
            buffType = (EBuffType)getEnum("buffType",typeof(EBuffType));
            triggerPointType = (EAttributeTriggerPointType)getEnum("triggerPointType", typeof(EAttributeTriggerPointType));
            buffIconResName = getString("buffIconResName");
            buffValue = getFloat("buffValue");
            isPositive = getBool("isPositive");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "buff";
    }
}

using System.Collections.Generic;
using GameCore;
using SCFrame;

namespace GameCore.RefData
{
    public class EnemyPassiveRefObj : SCRefDataCore
    {
        public long id;
        public EEnemyPassiveSkillType passiveType;
        public List<float> paramList;
        public string passiveName;
        public string passiveDesc;
        public string passiveIconResName;
        protected override void _parseFromString()
        {
            id = getLong("id");
            passiveType = (EEnemyPassiveSkillType)getEnum("passiveType", typeof(EEnemyPassiveSkillType));
            paramList = getList<float>("paramList");
            passiveName = getString("passiveName");
            passiveDesc = getString("passiveDesc");
            passiveIconResName = getString("passiveIconResName");
        }

        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "enemy_passive";
    }
}

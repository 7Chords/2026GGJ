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

        protected override void _parseFromString()
        {
            id = getLong("id");
            passiveType = (EEnemyPassiveSkillType)getEnum("passiveType", typeof(EEnemyPassiveSkillType));
            paramList = getList<float>("paramList");
        }

        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "enemy_passive";
    }
}

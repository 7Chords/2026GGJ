using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class EnemyRefObj : SCRefDataCore
    {
        public long id;
        public int floor;
        public int column;
        public string enemyName;
        public EBattleType battleType;
        public bool isBoss;
        public EBossType bossType;
        public int enemyHealth;
        public string layoutPresetName;
        public List<PartLevelEffectObj> initPartList;
        public List<BootyEffectObj> bootyList;
        public int winMoney;
        public int winCount;
        protected override void _parseFromString()
        {
            id = getLong("id");
            floor = getInt("floor");
            column = getInt("column");
            enemyName = getString("enemyName");
            battleType = (EBattleType)getEnum("battleType", typeof(EBattleType));
            isBoss = getBool("isBoss");
            bossType = (EBossType)getEnum("bossType", typeof(EBossType));
            enemyHealth = getInt("enemyHealth");
            layoutPresetName = getString("layoutPresetName");
            initPartList = getList<PartLevelEffectObj>("initPartList");
            bootyList = getList<BootyEffectObj>("bootyList");
            winMoney = getInt("winMoney");
            winCount = getInt("winCount");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "enemy";
    }
}


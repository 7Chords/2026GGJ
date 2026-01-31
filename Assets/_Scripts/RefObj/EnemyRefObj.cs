using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class EnemyRefObj : SCRefDataCore
    {
        public long id;
        public string enemyName;
        public int enemyHealth;
        public List<PartEffectObj> initPartList;
        public int winMoney;
        public int winCount;
        protected override void _parseFromString()
        {
            id = getLong("id");
            enemyName = getString("enemyName");
            enemyHealth = getInt("enemyHealth");
            initPartList = getList<PartEffectObj>("initPartList");
            winMoney = getInt("winMoney");
            winCount = getInt("winCount");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "enemy";
    }
}


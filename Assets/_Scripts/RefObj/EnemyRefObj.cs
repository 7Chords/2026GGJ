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
        public List<GoodsEffectObj> initPartList;
        protected override void _parseFromString()
        {
            id = getLong("id");
            enemyName = getString("enemyName");
            enemyHealth = getInt("enemyHealth");
            initPartList = getList<GoodsEffectObj>("initPartList");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "enemy";
    }
}


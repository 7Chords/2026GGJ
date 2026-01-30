using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class PlayerRefObj : SCRefDataCore
    {
        public int playerHealth;
        public List<GoodsEffectObj> initPartList;
        protected override void _parseFromString()
        {
            playerHealth = getInt("playerHealth");
            initPartList = getList<GoodsEffectObj>("initPartList");
        }


        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "player";
    }
}


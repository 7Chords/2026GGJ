using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class PlayerRefObj : SCRefDataCore
    {
        public int playerHealth;
        public List<PartEffectObj> initPartList;
        public int playerMoney;
        public PlayerRefObj()
        {

        }
        public PlayerRefObj(string _assetPath, string _sheetName) : base(_assetPath, _sheetName)
        {

        }
        protected override void _parseFromString()
        {
            playerHealth = getInt("playerHealth");
            initPartList = getList<PartEffectObj>("initPartList");
            playerMoney = getInt("playerMoney");
        }


        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "player";
    }
}


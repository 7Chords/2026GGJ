using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class StoreRefObj : SCRefDataCore
    {
        public long id;
        public int floor;
        public string storeName;
        public string storeDesc;
        public List<GoodsEffectObj> goodsList;

        public StoreRefObj()
        {

        }
        public StoreRefObj(string _assetPath, string _sheetName) : base(_assetPath, _sheetName)
        {

        }
        protected override void _parseFromString()
        {
            id = getLong("id");
            floor = getInt("floor");
            storeName = getString("storeName");
            storeDesc = getString("storeDesc");
            goodsList = getList<GoodsEffectObj>("goodsList");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "store";
    }
}

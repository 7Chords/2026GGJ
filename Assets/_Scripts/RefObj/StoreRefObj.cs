using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class StoreRefObj : SCRefDataCore
    {
        public long id;
        public string storeName;
        public string storeDesc;
        public List<GoodsEffectObj> goodsList;
        protected override void _parseFromString()
        {
            id = getLong("id");
            storeName = getString("storeName");
            storeDesc = getString("storeDesc");
            goodsList = getList<GoodsEffectObj>("goodsList");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "store";
    }
}

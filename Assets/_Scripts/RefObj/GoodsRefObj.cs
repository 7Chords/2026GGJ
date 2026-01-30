using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class GoodsRefObj : SCRefDataCore
    {

        public long id;
        public string goodsName;
        public EGoodsType goodsType;
        public int goodsPrice;
        public long partId;
        public int healthValue;
        public string goodsSpriteObjName;

        protected override void _parseFromString()
        {
            id = getLong("id");
            goodsName = getString("goodsName");
            goodsType = (EGoodsType)getEnum("goodsType", typeof(EGoodsType));
            goodsPrice = getInt("goodsPrice");
            partId = getLong("partId");
            healthValue = getInt("healthValue");
            goodsSpriteObjName = getString("goodsSpriteObjName");
        }
        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "goods";
    }
}


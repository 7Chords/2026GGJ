using GameCore.RefData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class GoodsInfo
    {
        public GoodsRefObj goodsRefObj;
        public int goodsLevel;
        public bool hasBought;

        public GoodsInfo(GoodsRefObj goodsRefObj, int goodsLevel)
        {
            this.goodsRefObj = goodsRefObj;
            this.goodsLevel = goodsLevel;
        }
    }
}

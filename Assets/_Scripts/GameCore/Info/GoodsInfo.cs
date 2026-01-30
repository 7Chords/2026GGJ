using GameCore.RefData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class GoodsInfo
    {
        public GoodsRefObj goodsRefObj;
        public int goodsAmount;

        public GoodsInfo(GoodsRefObj goodsRefObj, int goodsAmount)
        {
            this.goodsRefObj = goodsRefObj;
            this.goodsAmount = goodsAmount;
        }
    }
}

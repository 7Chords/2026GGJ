using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoStoreItem : _ASCUIMonoBase
    {
        [Header("商品图标")]
        public Image imgIcon;
        [Header("商品价格")]
        public Text txtPrice;
        [Header("商品数量")]
        public Text txtAmount;
        [Header("购买按钮")]
        public Button btnPurchase;
        [Header("商品名字")]
        public Text txtName;
    }
}

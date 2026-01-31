using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBattleWin : _ASCUIMonoBase
    {
        [Header("战利品container")]
        public UIMonoCommonContainer monoContainer;
        [Header("金钱文本")]
        public Text txtMoney;
        [Header("goto按钮")]
        public Button btnGoto;
    }

}

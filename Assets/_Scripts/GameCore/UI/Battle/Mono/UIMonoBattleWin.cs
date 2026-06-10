using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBattleWin : _ASCUIMonoBase
    {
        [Header("ս��Ʒcontainer")]
        public UIMonoCommonContainer monoContainer;
        [Header("��Ǯ�ı�")]
        public Text txtMoney;
        [Header("goto��ť")]
        public Button btnGoto;

        [Header("Battle win: money count-up duration (sec)")]
        public float moneyCountUpDuration = 0.55f;
        [Header("Battle win: booty pop interval (sec)")]
        public float bootyPopInterval = 0.08f;
        [Header("Battle win: booty pop duration (sec)")]
        public float bootyPopDuration = 0.22f;
        [Header("Battle win: booty pop ease overshoot (OutBack)")]
        public float bootyPopOvershoot = 1.35f;
    }

}

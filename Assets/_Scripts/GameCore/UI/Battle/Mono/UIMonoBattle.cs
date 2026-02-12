using System.Collections;
using System.Collections.Generic;
using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBattle : _ASCUIMonoBase
    {
        [Header("玩家面具")]
        public UIMonoBattleFace monoPlayerFace;
        [Header("敌方面具")]
        public UIMonoBattleFace monoEnemyFace;
        [Header("血条bar")]
        public Image imgHealthBar_player;
        [Header("血量文本")]
        public Text txtHealth_player;
        [Header("血条bar")]
        public Image imgHealthBar_enemy;
        [Header("血量文本")]
        public Text txtHealth_enemy;
        [Header("玩家部位触发信息")]
        public Text playerPartInfoText;
        [Header("敌人部位触发信息")]
        public Text enemyPartInfoText;

    }
}

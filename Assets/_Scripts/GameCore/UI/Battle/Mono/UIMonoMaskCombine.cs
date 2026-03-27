using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoMaskCombine : _ASCUIAnimMonoBase
    {
        [Header("??¦ËContainer")]
        public UIMonoCommonContainer monoPartContainer;
        [Header("????mono")]
        public UIMonoMaskCombineFace monoFace;
        [Header("??????")]
        public Button btnConfirm;
        [Header("???????mono")]
        public UIMonoEnemyMask monoEnemyMask;
        [Header("?????")]
        public Button btnDeck;
        [Header("???bar")]
        public Image imgHealthBar;
        [Header("??????")]
        public Text txtHealth;
        [Header("?????")]
        public Text txtCoin;
        [Header("??????????")]
        public Text txtBattleOrder;
        [Header("???????bar")]
        public Image imgEnemyHealthBar;
        [Header("??????????")]
        public Text txtEnemyHealth;
        [Header("??????")]
        public Button btnSetting;
        [Header("?????")]
        public Button btnGuide;
        [Header("??????????????")]
        public float scaleMouseEnter = 1.08f;
        [Header("???????????????")]
        public float scaleChgDuration = 0.15f;
        [Header("Entity HP preview colors")]
        public Color previewDamageColor = new Color(0.92f, 0.32f, 0.32f, 1f);
        public Color previewHealColor = new Color(0.32f, 0.82f, 0.45f, 1f);
    }
}

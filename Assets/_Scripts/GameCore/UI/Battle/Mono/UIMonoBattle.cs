using System.Collections;
using System.Collections.Generic;
using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace GameCore.UI
{
    public class UIMonoBattle : _ASCUIAnimMonoBase
    {
        [Header("玩家面具")]
        public UIMonoBattleFace monoPlayerFace;
        [Header("敌方面具")]
        public UIMonoBattleFace monoEnemyFace;
        [Header("玩家血条bar")]
        public Image imgPlayerHealthBar;
        [Header("玩家血量文本")]
        public Text txtPlayerHealth;
        [Header("敌人血条bar")]
        public Image imgEnemyHealthBar;
        [Header("敌人血量文本")]
        public Text txtEnemyHealth;
        [Header("玩家部位触发信息")]
        public Text playerPartInfoText;
        [Header("敌人部位触发信息")]
        public Text enemyPartInfoText;
        [Header("玩家血条物体")]
        public GameObject goPlayerHealth;
        [Header("敌人血条物体")]
        public GameObject goEnemyHealth;
        [Header("血条物体震动强度")]
        public float healthShakeStrength;
        [Header("血条物体震动时间")]
        public float healthShakeDuration;
        [Header("血条缓动时间")]
        public float healthBarFillTweenDuration = 0.35f;
    }
}

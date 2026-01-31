using System.Collections;
using System.Collections.Generic;
using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIMonoBattle : _ASCUIMonoBase
    {
        [Header("玩家面具")]
        public GameCore.UI.UIMonoMaskCombineFace playerFace;
        [Header("敌方面具")]
        public GameCore.UI.UIMonoMaskCombineFace enemyFace;
        
        [Header("玩家血条")]
        public UnityEngine.UI.Slider sliderPlayerHp;
        [Header("敌方血条")]
        [Header("敌方血条")]
        public UnityEngine.UI.Slider sliderEnemyHp;

        public Text playerPartInfoText;
        public Text enemyPartInfoText;
        
        public System.Action onUpdate;
        
        private void Update()
        {
            onUpdate?.Invoke();
        }
    }

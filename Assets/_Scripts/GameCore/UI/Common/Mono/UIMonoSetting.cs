using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoSetting : _ASCUIMonoBase
    {
        [Header("??????????")]
        public Slider sldMusic;
        [Header("??§¹??????")]
        public Slider sldSound;
        [Header("?????")]
        public Button btnClose;
        [Header("????????Ë®?")]
        public Button btnReturnMain;
        [Header("CRT????")]
        public Toggle togCRT;
        [Header("Battle speed 1.5x")]
        public Toggle togBattleSpeed;
        [Header("????????§³")]
        public float btnEnterScale;
        [Header("??????????")]
        public float btnScaleChgTime;
    }
}

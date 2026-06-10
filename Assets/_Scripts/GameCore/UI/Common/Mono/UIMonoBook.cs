using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBook : _ASCUIMonoBase
    {
        [Header("Part list container")]
        public UIMonoCommonContainer monoContainer;
        [Header("Close button")]
        public Button btnClose;
        [Header("Filter: eye")]
        public Button btnEye;
        [Header("Filter: nose")]
        public Button btnNose;
        [Header("Filter: mouth")]
        public Button btnMouth;
        [Header("Filter: skin")]
        public Button btnSkin;
        [Header("Button hover scale")]
        public float btnEnterScale = 1.1f;
        [Header("Button scale duration")]
        public float btnScaleChgTime = 0.1f;
    }
}

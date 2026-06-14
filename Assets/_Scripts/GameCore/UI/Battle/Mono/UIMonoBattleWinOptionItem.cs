using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBattleWinOptionItem : _ASCUIMonoBase
    {
        [Header("Option button")]
        public Button btnSelect;
        [Header("Mouse hover scale")]
        public float scaleMouseEnter = 1.05f;
        [Header("Mouse hover scale duration")]
        public float scaleChgDuration = 0.1f;
    }
}

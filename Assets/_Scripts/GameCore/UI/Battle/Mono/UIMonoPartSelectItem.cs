using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{

    public class UIMonoPartSelectItem : _ASCUIMonoBase
    {
        [Header("Select button")]
        public Button btnSelect;
        [Header("Part icon")]
        public Image imgIcon;
        [Header("Part health text")]
        public Text txtHealth;
        [Header("Mouse hover scale")]
        public float scaleMouseEnter = 1.05f;
        [Header("Mouse hover scale duration")]
        public float scaleChgDuration = 0.1f;
    }
}

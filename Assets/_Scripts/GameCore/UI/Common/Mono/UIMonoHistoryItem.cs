using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoHistoryItem : _ASCUIMonoBase
    {
        [Header("Expand toggle")]
        public Button btnToggle;
        [Header("Win/Lose text")]
        public Text txtResult;
        [Header("Recorded time")]
        public Text txtTime;
        [Header("Lose location (hidden on win)")]
        public Text txtLoseLocation;
        [Header("Expand hint")]
        public Text txtExpandHint;
        [Header("Expanded part library root")]
        public GameObject goPartRoot;
        [Header("End-of-run part container")]
        public UIMonoCommonContainer monoPartContainer;
    }
}

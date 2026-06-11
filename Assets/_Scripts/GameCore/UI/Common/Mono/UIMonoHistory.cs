using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoHistory : _ASCUIMonoBase
    {
        [Header("History list container")]
        public UIMonoCommonContainer monoListContainer;
        [Header("Close button")]
        public Button btnClose;
        [Header("Empty list hint")]
        public GameObject goEmptyHint;
    }
}

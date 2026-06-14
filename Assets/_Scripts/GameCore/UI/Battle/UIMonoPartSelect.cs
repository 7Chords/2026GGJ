using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoPartSelect : _ASCUIMonoBase
    {
        [Header("Part choice container")]
        public UIMonoCommonContainer monoContainer;
        [Header("Skip button")]
        public Button btnSkip;
        [Header("Offer count")]
        public int offerCount = 3;
    }
}

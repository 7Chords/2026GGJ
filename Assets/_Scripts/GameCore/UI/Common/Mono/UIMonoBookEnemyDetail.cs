using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBookEnemyDetail : _ASCUIMonoBase
    {
        [Header("Enemy name")]
        public Text txtEnemyName;
        [Header("Enemy health")]
        public Text txtEnemyHealth;
        [Header("Part reserve container")]
        public UIMonoCommonContainer monoPartReserveContainer;
        [Header("Turn layout container")]
        public UIMonoCommonContainer monoTurnLayoutContainer;
        [Header("Close button")]
        public Button btnClose;
    }
}

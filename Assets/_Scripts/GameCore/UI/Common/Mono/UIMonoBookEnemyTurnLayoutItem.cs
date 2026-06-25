using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBookEnemyTurnLayoutItem : _ASCUIMonoBase
    {
        [Header("Turn label")]
        public Text txtTurnLabel;
        [Header("Enemy mask preview")]
        public UIMonoBookEnemyMask monoEnemyMask;
    }
}

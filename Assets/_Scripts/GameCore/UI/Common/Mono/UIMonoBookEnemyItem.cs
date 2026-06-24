using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBookEnemyItem : _ASCUIMonoBase
    {
        [Header("Select button")]
        public Button btnSelect;
        [Header("Enemy name")]
        public Text txtName;
        [Header("Enemy health")]
        public Text txtHealth;
        [Header("Enemy type text")]
        public Text txtType;
        [Header("Mouse hover scale")]
        public float scaleMouseEnter = 1.05f;
        [Header("Mouse hover scale duration")]
        public float scaleChgDuration = 0.1f;
    }
}

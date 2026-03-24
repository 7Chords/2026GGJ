using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoMap : _ASCUIMonoBase
    {
        [Header("滚动视图")]
        public ScrollRect scrollView;
        [Header("金钱文本")]
        public Text txtCoin;
        [Header("背包按钮")]
        public Button btnBag;
        [Header("教程按钮")]
        public Button btnGuide;
        [Header("设置按钮")]
        public Button btnSetting;
        [Header("血量文本")]
        public Text txtHealth;
        [Header("地图名称文本")]
        public Text txtMapName;
        [Header("血量条")]
        public Image imgHealthBar;
    }
}

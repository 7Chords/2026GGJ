using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoCommonTwoBtn : _ASCUIMonoBase
    {
        [Header("内容文本")]
        public Text txtContent;
        [Header("左边按钮")]
        public Button btnLeft;
        [Header("右边按钮")]
        public Button btnRight;
        [Header("左边文本")]
        public Text txtLeft;
        [Header("右边文本")]
        public Text txtRight;
        [Header("选中的按钮颜色")]
        public Color colorBtnSelect;
        [Header("选中的文本颜色")]
        public Color colorTextSelect;
        [Header("未选中的按钮颜色")]
        public Color colorBtnUnselect;
        [Header("未选中的文本颜色")]
        public Color colorTextUnselect;
    }
}

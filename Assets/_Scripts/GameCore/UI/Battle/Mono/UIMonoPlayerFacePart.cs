using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoPlayerFacePart : _ASCUIMonoBase
    {
        [Header("物体图片")]
        public Image imgGO;
        [Header("部位图片")]
        public Image imgPart;
        [Header("生命文本")]
        public Text txtHealth;
        [Header("顺序文本")]
        public Text txtOrder;
        [Header("生命信息物体")]
        public GameObject goHealthInfo;
        [Header("序号信息物体")]
        public GameObject goOrder;
        [Header("鼠标移入缩放")]
        public float scaleMouseEnter;
        [Header("缩放时间")]
        public float scaleChgDuration;

    }
}

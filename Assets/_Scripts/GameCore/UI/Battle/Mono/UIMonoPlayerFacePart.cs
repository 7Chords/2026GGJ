using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoPlayerFacePart : _ASCUIMonoBase
    {
        [Header("部位物体")]
        public Image imgGO;
        [Header("部位图片")]
        public Image imgPart;
        [Header("生命文本")]
        public Text txtHealth;
        [Header("顺序文本")]
        public Text txtOrder;
        [Header("生命信息物体")]
        public GameObject goHealthInfo;
        [Header("顺序信息物体")]
        public GameObject goOrder;
        [Header("Buff信息物体")]
        public GameObject goBuff;
        [Header("Preview Damage Color")]
        public Color previewDamageColor = new Color(0.92f, 0.32f, 0.32f, 1f);
        [Header("Preview Heal Color")]
        public Color previewHealColor = new Color(0.32f, 0.82f, 0.45f, 1f);
        [Header("生命信息物体锚点")]
        public Vector2 goHealthPosPivot;
        [Header("顺序信息物体锚点")]
        public Vector2 goOrderPosPivot;
        [Header("buff信息物体锚点")]
        public Vector2 goBuffPosPivot;
        [Header("鼠标移入缩放")]
        public float scaleMouseEnter;
        [Header("鼠标移入缩放时间")]
        public float scaleChgDuration;

    }
}

using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBattlePart : _ASCUIMonoBase
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
        [Header("buff信息物体")]
        public GameObject goBuff;
        [Header("生命信息物体所处位置比例")]
        public Vector2 goHealthPosPivot;
        [Header("序号信息物体所处位置比例")]
        public Vector2 goOrderPosPivot;
        [Header("buff信息物体所处位置比例")]
        public Vector2 goBuffPosPivot;
        [Header("受伤震动强度")]
        public float hurtShakeStrength;
        [Header("受伤震动时间")]
        public float hurtShakeDuration;
        [Header("行动缩放")]
        public float activeScale;
        [Header("行动缩放时间")]
        public float scaleChgDuration;
    }
}

using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBattlePart : _ASCUIMonoBase
    {
        [Header("物体")]
        public Image imgGO;
        [Header("部位图片")]
        public Image imgPart;
        [Header("生命值文本")]
        public Text txtHealth;
        [Header("顺序文本")]
        public Text txtOrder;
        [Header("生命值信息物体")]
        public GameObject goHealthInfo;
        [Header("顺序信息物体")]
        public GameObject goOrder;
        [Header("buff信息物体")]
        public GameObject goBuff;
        [Header("生命值信息物体锚点")]
        public Vector2 goHealthPosPivot;
        [Header("顺序信息物体锚点")]
        public Vector2 goOrderPosPivot;
        [Header("buff信息物体锚点")]
        public Vector2 goBuffPosPivot;
        [Header("受伤震动强度")]
        public float hurtShakeStrength;
        [Header("受伤震动持续时间")]
        public float hurtShakeDuration;
        [Header("激活缩放")]
        public float activeScale;
        [Header("激活缩放时间")]
        public float scaleChgDuration;
        [Header("Mouth attack: rotation shake duration")]
        public float mouthAttackShakeDuration = 0.22f;
        [Header("Mouth attack: max rotation shake angle (deg Z)")]
        public float mouthAttackShakeAngle = 18f;
        [Header("Mouth attack: shake vibrato")]
        [Range(1, 30)]
        public int mouthAttackShakeVibrato = 16;
        [Header("Mouth attack: punch scale on hit (0 = off)")]
        public float mouthAttackPunchScale = 0.14f;
        [Header("Mouth attack: impact punch rotation (deg Z)")]
        public float mouthAttackImpactPunchAngle = 9f;
        [Header("Mouth attack: impact punch duration (sec)")]
        public float mouthAttackImpactPunchDuration = 0.07f;
        [Header("Mouth attack: stutter pause after impact (sec, frame-skip feel)")]
        public float mouthAttackImpactStutterPause = 0.032f;
        [Header("Mouth attack: mid-shake stutter pause (sec)")]
        public float mouthAttackMidStutterPause = 0.028f;
        [Header("受伤闪烁颜色")]
        public Color hurtFlashTint = new Color(1f, 0.4f, 0.4f, 1f);
        [Header("受伤闪烁淡入时间")]
        public float hurtFlashInDuration = 0.07f;
        [Header("受伤闪烁淡出时间")]
        public float hurtFlashOutDuration = 0.12f;
        [Header("Body HP hurt: extra part shake vs part-hurt shake strength")]
        [Range(0.05f, 1f)]
        public float bodyHurtPartFollowShakeStrengthMul = 0.36f;
        [Header("Eye/Nose trigger success bounce (distinct from hurt shake)")]
        public float triggerSuccessBounceHeight = 22f;
        public float triggerSuccessBounceDuration = 0.38f;
        [Range(0.15f, 0.55f)]
        public float triggerSuccessBounceUpPortion = 0.32f;
        public float triggerSuccessPunchScale = 0.1f;
        [Header("是玩家部位时的颜色")]
        public Color colorIsPlayerPart;
        [Header("是敌人部位时的颜色")]
        public Color colorIsEnemyPart;
        [Header("生命值信息背景图片")]
        public Image imgHealthBg;
        [Header("顺序信息背景图片")]
        public Image imgOrderBg;
    }
}

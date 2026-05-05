using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBattleFace : _ASCUIMonoBase
    {
        [Header("格子预制体名")]
        public string gridPrefabName;
        [Header("列数")]
        public int columnCount;
        [Header("行数")]
        public int rowCount;
        [Header("失效格子")]
        public List<Vector2Int> disabledGrids;
        [Header("layout")]
        public GridLayoutGroup girdLayoutGroup;
        [Header("格子父物体")]
        public Transform tranParentPart;
        [Header("是否是敌方脸部")]
        public bool isEnemyFace;
        [Header("脸部图片")]
        public Image imgFace;
        [Header("Defeat dissolve duration (seconds)")]
        public float defeatFaceEffectDuration = 1.15f;
        [Header("Defeat: deactivate imgFace when effect ends")]
        public bool defeatFaceHideImageWhenDone = true;
        [Header("Body HP hurt: full face shake strength")]
        public float faceBodyHurtShakeStrength = 14f;
        [Header("Body HP hurt: full face shake duration")]
        public float faceBodyHurtShakeDuration = 0.26f;
        [Header("Body HP hurt: optional full-face Image for red flash (can be transparent when idle)")]
        public Image faceBodyHurtFlashImage;
        [Header("Body HP hurt: flash tint")]
        public Color faceBodyHurtFlashTint = new Color(1f, 0.38f, 0.38f, 0.62f);
        [Header("Body HP hurt: flash fade in time")]
        public float faceBodyHurtFlashInDuration = 0.07f;
        [Header("Body HP hurt: flash fade out time")]
        public float faceBodyHurtFlashOutDuration = 0.14f;
    }
}

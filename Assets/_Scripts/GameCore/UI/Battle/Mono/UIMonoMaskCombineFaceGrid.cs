using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoMaskCombineFaceGrid : _ASCUIMonoBase
    {
        [Header("����")]
        public Vector2 gridPos;
        [Header("Ĭ����ɫ")]
        public Color colorDefault;
        [Header("���Է��õ���ɫ")]
        public Color colorCanPlace;
        [Header("�����Է��õ���ɫ")]
        public Color colorCanNotPlace;

        public Image imgBg;
    }
}

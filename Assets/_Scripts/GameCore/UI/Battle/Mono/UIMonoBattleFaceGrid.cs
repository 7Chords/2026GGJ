using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBattleFaceGrid : _ASCUIMonoBase
    {
        [Header("Ĭ�ϵ���ɫ")]
        public Color colorDefault;
        [Header("���Է��õ���ɫ")]
        public Color colorCanPlace;
        [Header("�����Է��õ���ɫ")]
        public Color colorCanNotPlace;
        [Header("���÷�Χ����ɫ")]
        public Color colorIsEffective;
        [Header("����ͼƬ")]
        public Image imgGrid;
    }
}

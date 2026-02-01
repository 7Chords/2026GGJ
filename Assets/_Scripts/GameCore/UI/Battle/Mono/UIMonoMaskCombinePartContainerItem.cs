using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoMaskCombinePartContainerItem : _ASCUIMonoBase
    {
        [Header("��Ʒͼ��")]
        public Image imgGoods;
        [Header("Ѫ���ı�")]
        public Text txtHealth;
        [Header("�����������")]
        public float scaleMouseEnter;
        [Header("�����������ʱ��")]
        public float scaleChgDuration;
        
        public Canvas imgCanvas;
    }
}

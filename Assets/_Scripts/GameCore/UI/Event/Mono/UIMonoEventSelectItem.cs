using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoEventSelectItem : _ASCUIMonoBase
    {
        [Header("ѡ��ť")]
        public Button btnSelect;

        [Header("�����ı�")]
        public Text txtContent;

        [Header("�����������")]
        public float scaleMouseEnter = 1.05f;

        [Header("���Ŷ���ʱ��")]
        public float scaleChgDuration = 0.12f;
    }

}

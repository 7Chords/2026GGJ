using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoEvent : _ASCUIMonoBase
    {
        [Header("�����ı�")]
        public Text txtName;
        [Header("�����ı�")]
        public Text txtContent;
        [Header("ѡ��������")]
        public UIMonoCommonContainer monoSelectContainer;
        [Header("�Ի��������")]
        public Image imgClickArea;
        [Header("�Ի�����������ʾ������룩")]
        public float dialogueTypewriterInterval = 0.04f;
        [Header("Ѫ����")]
        public Image imgHealthBar;
        [Header("Ѫ���ı�")]
        public Text txtHealth;
        [Header("����ı�")]
        public Text txtCoin;
    }
}

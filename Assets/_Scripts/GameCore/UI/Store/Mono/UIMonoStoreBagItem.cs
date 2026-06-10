using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SCFrame.UI;

namespace GameCore.UI
{
    public class UIMonoStoreBagItem : _ASCUIMonoBase
    {
        [Header("��λicon")]
        public Image imgIcon;
        [Header("����ֵ�ı�")]
        public Text txtHealth;
        [Header("��ֵ�ı�")]
        public Text txtValue;
        [Header("������������")]
        public float scaleMouseEnter;
        [Header("������������ʱ��")]
        public float scaleChgDuration;
        [Header("Sell button hover scale")]
        public float scaleMouseEnterSell;
        [Header("���۰�ť")]
        public Button btnSell;
        [Header("��������")]
        public GameObject goContent;

    }
}

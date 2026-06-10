using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoStrengthen : _ASCUIMonoBase
    {
        [Header("ǿ��ǰԤ��mono")]
        public UIMonoStrengthenPreview monoPreviewBefore;
        [Header("ǿ����Ԥ��mono")]
        public UIMonoStrengthenPreview monoPreviewAfter;
        [Header("����mono")]
        public UIMonoCommonContainer monoBagContainer;
        [Header("ȷ��ǿ����ť")]
        public Button btnConfirm;
        [Header("ǿ�����Ľ�Ǯ�ı�")]
        public Text txtStrengthenCoin;
        [Header("��ҽ�Ǯ�ı�")]
        public Text txtPlayerCoin;
        [Header("�뿪��ť")]
        public Button btnExit;
        [Header("���ð�ť")]
        public Button btnSetting;
        [Header("�̳̰�ť")]
        public Button btnGuide;
        [Header("ѡ����ǿ������Ҫ��ʾ������")]
        public List<GameObject> goHasSelectPart;
        [Header("������밴ť������")]
        public float scaleMouseEnter = 1.08f;
        [Header("������������ʱ��")]
        public float scaleChgDuration = 0.15f;
    }
}

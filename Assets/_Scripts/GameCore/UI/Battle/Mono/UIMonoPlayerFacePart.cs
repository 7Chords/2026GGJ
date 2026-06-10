using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoPlayerFacePart : _ASCUIMonoBase
    {
        [Header("��λ����")]
        public Image imgGO;
        [Header("��λͼƬ")]
        public Image imgPart;
        [Header("�����ı�")]
        public Text txtHealth;
        [Header("˳���ı�")]
        public Text txtOrder;
        [Header("������Ϣ����")]
        public GameObject goHealthInfo;
        [Header("˳����Ϣ����")]
        public GameObject goOrder;
        [Header("Buff��Ϣ����")]
        public GameObject goBuff;
        [Header("Preview Damage Color")]
        public Color previewDamageColor = new Color(0.92f, 0.32f, 0.32f, 1f);
        [Header("Preview Heal Color")]
        public Color previewHealColor = new Color(0.32f, 0.82f, 0.45f, 1f);
        [Header("������Ϣ����ê��")]
        public Vector2 goHealthPosPivot;
        [Header("˳����Ϣ����ê��")]
        public Vector2 goOrderPosPivot;
        [Header("buff��Ϣ����ê��")]
        public Vector2 goBuffPosPivot;
        [Header("�����������")]
        public float scaleMouseEnter;
        [Header("�����������ʱ��")]
        public float scaleChgDuration;

    }
}

using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBookEnemyFacePart : _ASCUIMonoBase
    {
        [Header("Part image (hover target)")]
        public Image imgGO;
        [Header("Health text")]
        public Text txtHealth;
        [Header("Battle order text")]
        public Text txtOrder;
        [Header("Health info root")]
        public GameObject goHealthInfo;
        [Header("Order info root")]
        public GameObject goOrder;
        [Header("Health info anchor pivot")]
        public Vector2 goHealthPosPivot;
        [Header("Order info anchor pivot")]
        public Vector2 goOrderPosPivot;
        [Header("Hover scale")]
        public float scaleMouseEnter = 0.44f;
        [Header("Scale tween duration")]
        public float scaleChgDuration = 0.1f;
        [Header("Default scale")]
        public float scaleGO = 0.4f;
    }
}

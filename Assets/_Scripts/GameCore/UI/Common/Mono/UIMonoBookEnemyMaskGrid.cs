using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBookEnemyMaskGrid : _ASCUIMonoBase
    {
        [Header("Grid image")]
        public Image imgGrid;
        [Header("Occupy color")]
        public Color colorOccupy;
        [Header("Effect range color")]
        public Color colorEffect;
        [Header("Occupy and effect overlap color")]
        public Color colorOverlap;
        [Header("Default color")]
        public Color colorDefault;
    }
}

using GameCore;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class TooltipGrid : MonoBehaviour
    {
        [Header("是位置格子的颜色")]
        public Color colorIsOccupyGrid;
        [Header("是效果格子的颜色")]
        public Color colorIsEffectGrid;
        [Header("是二者重叠的颜色")]
        public Color colorIsBothGrid;
        [Header("格子img")]
        public Image imgGrid;
        public void SetGridTShow(EGridPosType _gridPosType)
        {
            if (_gridPosType == EGridPosType.OCCUPY)
                imgGrid.color = colorIsOccupyGrid;
            else if (_gridPosType == EGridPosType.EFFECT)
                imgGrid.color = colorIsEffectGrid;
            else if(_gridPosType == EGridPosType.BOTH)
                imgGrid.color = colorIsBothGrid;
        }
    }
}

using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoPassiveItem : _ASCUIMonoBase
    {
        public Image imgPassiveIcon;
        [Header("Raycast target for hover (Image on same GO or child)")]
        public Graphic hoverTarget;
    }
}

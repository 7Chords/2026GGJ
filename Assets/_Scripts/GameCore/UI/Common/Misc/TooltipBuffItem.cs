using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class TooltipBuffItem : MonoBehaviour
    {
        [Header("buff图标")]
        public Image imgBuffIcon;
        [Header("buff层数文本")]
        public Text txtBuffLayer;

        public void SetBuffInfo(BuffInfo _buffInfo)
        {
            if (_buffInfo == null)
                return;
            imgBuffIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_buffInfo.buffRefObj.buffIconResName);
            txtBuffLayer.text = _buffInfo.buffLayer.ToString();
        }
    }
}

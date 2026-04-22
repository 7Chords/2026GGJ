using GameCore.RefData;
using SCFrame;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class CommonBuffSideItem : MonoBehaviour
    {
        [Header("buff图标")]
        public Image imgBuffIcon;
        [Header("buff名文本")]
        public Text txtBuffName;
        [Header("描述文本")]
        public Text txtBuffDesc;

        public void Initialize(EBuffType _buffType)
        {
            BuffRefObj buffRefObj = SCRefDataMgr.instance.buffRefList.refDataList.Find(x => x.buffType == _buffType);
            if (buffRefObj == null)
                return;
            imgBuffIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(buffRefObj.buffIconResName);
            if (txtBuffName != null)
            {
                txtBuffName.text = buffRefObj.buffName;
                if (txtBuffDesc != null)
                    txtBuffDesc.text = buffRefObj.buffDesc;
            }
            else if (txtBuffDesc != null)
            {
                txtBuffDesc.text = buffRefObj.buffName + ":" + buffRefObj.buffDesc;
            }

            var rt = transform as RectTransform;
            if (rt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            var parentRt = transform.parent as RectTransform;
            if (parentRt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRt);
        }
    }
}

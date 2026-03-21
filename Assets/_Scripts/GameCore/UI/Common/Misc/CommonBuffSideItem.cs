using GameCore.RefData;
using SCFrame;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class CommonBuffSideItem : MonoBehaviour
    {
        [Header("buffÍ¼±ê")]
        public Image imgBuffIcon;
        [Header("ÃèÊöÎÄ±¾")]
        public Text txtBuffDesc;

        public void Initialize(EBuffType _buffType)
        {
            BuffRefObj buffRefObj = SCRefDataMgr.instance.buffRefList.refDataList.Find(x => x.buffType == _buffType);
            if (buffRefObj == null)
                return;
            imgBuffIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(buffRefObj.buffIconResName);
            txtBuffDesc.text = buffRefObj.buffName + ":" + buffRefObj.buffDesc;
        }
    }
}

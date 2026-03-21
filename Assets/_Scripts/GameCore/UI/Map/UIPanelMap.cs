using SCFrame;
using SCFrame.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelMap : _ASCUIPanelBase<UIMonoMap>
    {
        public UIPanelMap(UIMonoMap _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        private GameObject _m_playerIconGO;

        public override void AfterInitialize()
        {
            
        }
        public override void BeforeDiscard()
        {

        }
        public override void OnHidePanel()
        {
            mono.btnBag.RemoveClickDown(onBtnBagClickDown);
            mono.btnSetting.RemoveClickDown(onBtnSettingClickDown);
            mono.btnGuide.RemoveClickDown(onBtnGuideClickDown);
        }

        public override void OnShowPanel()
        {
            mono.btnBag.AddMouseLeftClickDown(onBtnBagClickDown);
            mono.btnSetting.AddMouseLeftClickDown(onBtnSettingClickDown);
            mono.btnGuide.AddMouseLeftClickDown(onBtnGuideClickDown);

            refreshShow();
        }

        private void refreshShow()
        {
            updatePlayerIcon();
            setPlayerInfo();
        }
        private void updatePlayerIcon()
        {
            var pos = GameModel.instance.playerInfo.playerMapPosition;
            if (pos.x == -1 || MapManager.instance.currentMapNodes == null) return; // Not started or invalid

            var targetNode = MapManager.instance.GetNode(pos.x, pos.y);
            if (targetNode != null)
            {
                if (_m_playerIconGO == null)
                {
                    if (_m_playerIconGO == null)
                    {
                        //todo:Ìæ»»Íæ¼Òicon
                        _m_playerIconGO = new GameObject("PlayerIcon");
                        var img = _m_playerIconGO.AddComponent<UnityEngine.UI.Image>();
                        img.color = Color.green;
                    }
                }
                
                // Parent to the Node so it moves with it
                _m_playerIconGO.transform.SetParent(targetNode.transform);
                _m_playerIconGO.transform.localPosition = Vector3.zero;
                _m_playerIconGO.transform.localScale = Vector3.one * 0.5f; // Small icon
                _m_playerIconGO.SetActive(true);
                
                // Ensure it draws on top
                _m_playerIconGO.transform.SetAsLastSibling();
            }
        }
        
        private void setPlayerInfo()
        {
            mono.txtCoin.text = GameModel.instance.playerInfo.playerMoney.ToString();
            mono.txtHealth.text = GameModel.instance.playerInfo.currentHealth + "/" + GameModel.instance.playerInfo.maxHealth;
            mono.imgHealthBar.fillAmount = GameModel.instance.playerInfo.currentHealth / (float)GameModel.instance.playerInfo.maxHealth;
        }

        private void onBtnBagClickDown(PointerEventData _data, object[] _objs)
        {
            UICoreMgr.instance.AddNode(new UINodeDeck(SCUIShowType.ADDITION, GameModel.instance.playerInfo.bagPartInfoList));
        }
        private void onBtnSettingClickDown(PointerEventData _data, object[] _objs)
        {
            UICoreMgr.instance.AddNode(new UINodeSetting(SCUIShowType.ADDITION));
        }
        private void onBtnGuideClickDown(PointerEventData _data, object[] _objs)
        {
            UICoreMgr.instance.AddNode(new UINodeGuideMap(SCUIShowType.ADDITION));
        }
    }
}

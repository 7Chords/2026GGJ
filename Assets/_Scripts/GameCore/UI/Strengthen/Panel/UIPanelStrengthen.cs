using DG.Tweening;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelStrengthen : _ASCUIPanelBase<UIMonoStrengthen>
    {
        private UIPanelStrengthenBagContainer _m_strengthenContainer;
        private UIPanelStrengthenPreview _m_strengthenBeforePreview;
        private UIPanelStrengthenPreview _m_strengthenAfterPreview;

        private PartInfo _m_curSelectPart;
        private TweenContainer _m_tweenContainer;

        public UIPanelStrengthen(UIMonoStrengthen _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
            _m_strengthenContainer = new UIPanelStrengthenBagContainer(mono.monoBagContainer,SCUIShowType.INTERNAL);
            _m_strengthenBeforePreview = new UIPanelStrengthenPreview(mono.monoPreviewBefore,SCUIShowType.INTERNAL,false);
            _m_strengthenAfterPreview = new UIPanelStrengthenPreview(mono.monoPreviewAfter, SCUIShowType.INTERNAL, true);

        }

        public override void BeforeDiscard()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
            _m_strengthenContainer?.Discard();
            _m_strengthenContainer = null;
            _m_strengthenBeforePreview?.Discard();
            _m_strengthenBeforePreview = null;
            _m_strengthenAfterPreview?.Discard();
            _m_strengthenAfterPreview = null;
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.CHEAT_DEBUG_UI_REFRESH, onCheatDebugUiRefresh);
            SCMsgCenter.UnregisterMsg(SCMsgConst.SELECT_STRENGTHEN_PART, onSelectStrengthenPart);
            mono.btnExit.RemoveClickDown(onBtnExitClickDown);
            mono.btnConfirm.RemoveClickDown(onBtnConfirmClickDown);
            mono.btnConfirm.RemoveMouseEnter(onBtnConfirmMouseEnter);
            mono.btnConfirm.RemoveMouseExit(onBtnConfirmMouseExit);
            if (mono.btnSetting != null)
            {
                mono.btnSetting.RemoveClickDown(onBtnSettingClickDown);
                mono.btnSetting.RemoveMouseEnter(onBtnSettingMouseEnter);
                mono.btnSetting.RemoveMouseExit(onBtnSettingMouseExit);
            }

            _m_strengthenContainer?.HidePanel();
            _m_strengthenBeforePreview?.HidePanel();
            _m_strengthenAfterPreview?.HidePanel();
        }

        public override void OnShowPanel()
        {
            _m_curSelectPart = null;

            SCMsgCenter.RegisterMsgAct(SCMsgConst.CHEAT_DEBUG_UI_REFRESH, onCheatDebugUiRefresh);
            SCMsgCenter.RegisterMsg(SCMsgConst.SELECT_STRENGTHEN_PART, onSelectStrengthenPart);
            mono.btnExit.AddMouseLeftClickDown(onBtnExitClickDown);
            mono.btnConfirm.AddMouseLeftClickDown(onBtnConfirmClickDown);
            mono.btnConfirm.AddMouseEnter(onBtnConfirmMouseEnter);
            mono.btnConfirm.AddMouseExit(onBtnConfirmMouseExit);
            if (mono.btnSetting != null)
            {
                mono.btnSetting.AddMouseLeftClickDown(onBtnSettingClickDown);
                mono.btnSetting.AddMouseEnter(onBtnSettingMouseEnter);
                mono.btnSetting.AddMouseExit(onBtnSettingMouseExit);
            }

            _m_strengthenContainer?.ShowPanel();
            _m_strengthenBeforePreview?.ShowPanel();
            _m_strengthenAfterPreview?.ShowPanel();
            
            _m_strengthenContainer?.SetListInfo(GameModel.instance.playerInfo.bagPartInfoList);

            refreshShow();
        }

        private void onCheatDebugUiRefresh()
        {
            refreshShow();
        }

        private void refreshShow()
        {

            mono.txtPlayerCoin.text = GameModel.instance.playerInfo.playerMoney.ToString();

           if (_m_curSelectPart != null)
           {
                PartLevelRefObj partLevelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x=>x.partId == _m_curSelectPart.partRefObj.id 
                    && x.partLevel == _m_curSelectPart.partLevel);
                if(partLevelRefObj != null)
                    mono.txtStrengthenCoin.text = partLevelRefObj.levelUpCost.ToString();

               _m_strengthenContainer?.RefreshShow(GameModel.instance.playerInfo.bagPartInfoList, _m_curSelectPart);

                PartLevelRefObj levelUpBeforeRefObj = _m_curSelectPart.GetLevelRefObj();
                PartLevelRefObj levelUpAfterRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.partId == _m_curSelectPart.partRefObj.id
                    && x.partLevel == _m_curSelectPart.partLevel + 1);

                bool atMaxStrengthen = levelUpBeforeRefObj != null && levelUpAfterRefObj == null;
                _m_strengthenBeforePreview.SetInfo(levelUpBeforeRefObj, atMaxStrengthen);
                _m_strengthenAfterPreview.SetInfo(levelUpAfterRefObj, atMaxStrengthen);

                SCCommon.SetGameObjectEnable(mono.goHasSelectPart, levelUpBeforeRefObj != null && levelUpAfterRefObj != null);
            }
            else
            {
                _m_strengthenBeforePreview.SetInfo(null, false);
                _m_strengthenAfterPreview.SetInfo(null, false);
                SCCommon.SetGameObjectEnable(mono.goHasSelectPart, false);

            }

        }
        private void onSelectStrengthenPart(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo partInfo = _objs[0] as PartInfo;
            _m_curSelectPart = partInfo;
            refreshShow();
        }
        private void onBtnExitClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            TVSwitchTransition.Run(() =>
            {
                GameModel.instance.playerInfo.ApplyPendingMapMove();
                UICoreMgr.instance.CloseTopNode();
                UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
            });
        }
        private void onBtnConfirmClickDown(PointerEventData _data, object[] _objs)
        {
            if (_m_curSelectPart == null)
                return;
            PartLevelRefObj levelRefObj = _m_curSelectPart.GetLevelRefObj();
            if (GameModel.instance.playerInfo.playerMoney < levelRefObj.levelUpCost)
            {
                GameCommon.ShowPopTip("金币不足", Vector2.zero);
                return;
            }
            if (!_m_curSelectPart.HasNextLevel())
            {
                GameCommon.ShowPopTip("等级已达上限", Vector2.zero);
                return;
            }
            
            GameModel.instance.playerInfo.playerMoney = Mathf.Max(GameModel.instance.playerInfo.playerMoney - _m_curSelectPart.GetLevelRefObj().levelUpCost, 0);
            _m_curSelectPart.LevelUp();
            refreshShow();
            GameCommon.ShowPopTip("强化成功", Vector2.zero);
        }

        private void onBtnConfirmMouseEnter(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnConfirm == null) return;
            _m_tweenContainer.RegDoTween(mono.btnConfirm.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnConfirmMouseExit(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnConfirm == null) return;
            _m_tweenContainer.RegDoTween(mono.btnConfirm.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onBtnSettingClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.AddNode(new UINodeSetting(SCUIShowType.ADDITION,true));
        }

        private void onBtnSettingMouseEnter(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnSetting == null) return;
            _m_tweenContainer.RegDoTween(mono.btnSetting.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnSettingMouseExit(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnSetting == null) return;
            _m_tweenContainer.RegDoTween(mono.btnSetting.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }
    }
}

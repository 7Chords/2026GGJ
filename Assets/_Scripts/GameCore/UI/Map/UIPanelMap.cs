using DG.Tweening;
using GameCore;
using GameCore.RefData;
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

        private TweenContainer _m_tweenContainer;

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
        }

        public override void BeforeDiscard()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }
        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsgAct(SCMsgConst.CHEAT_DEBUG_UI_REFRESH, onCheatDebugUiRefresh);
            mono.btnBag.RemoveClickDown(onBtnBagClickDown);
            mono.btnBag.RemoveMouseEnter(onBtnBagMouseEnter);
            mono.btnBag.RemoveMouseExit(onBtnBagMouseExit);
            mono.btnSetting.RemoveClickDown(onBtnSettingClickDown);
            mono.btnSetting.RemoveMouseEnter(onBtnSettingMouseEnter);
            mono.btnSetting.RemoveMouseExit(onBtnSettingMouseExit);
            mono.btnGuide.RemoveClickDown(onBtnGuideClickDown);
            mono.btnGuide.RemoveMouseEnter(onBtnGuideMouseEnter);
            mono.btnGuide.RemoveMouseExit(onBtnGuideMouseExit);
        }

        public override void OnShowPanel()
        {
            var gen = MapGenerator.GetOrFind();
            RectTransform mapContent = mono.scrollView != null ? mono.scrollView.content : null;
            if (gen != null)
                gen.EnsureMapGeneratedIfNeeded(mapContent);

            mono.btnBag.AddMouseLeftClickDown(onBtnBagClickDown);
            mono.btnBag.AddMouseEnter(onBtnBagMouseEnter);
            mono.btnBag.AddMouseExit(onBtnBagMouseExit);
            mono.btnSetting.AddMouseLeftClickDown(onBtnSettingClickDown);
            mono.btnSetting.AddMouseEnter(onBtnSettingMouseEnter);
            mono.btnSetting.AddMouseExit(onBtnSettingMouseExit);
            mono.btnGuide.AddMouseLeftClickDown(onBtnGuideClickDown);
            mono.btnGuide.AddMouseEnter(onBtnGuideMouseEnter);
            mono.btnGuide.AddMouseExit(onBtnGuideMouseExit);

            SCMsgCenter.RegisterMsgAct(SCMsgConst.CHEAT_DEBUG_UI_REFRESH, onCheatDebugUiRefresh);
            refreshShow();
            if (!GameModel.instance.RunTutorialMapAutoShown)
            {
                GameModel.instance.MarkRunTutorialMapAutoShown();
                UICoreMgr.instance.AddNode(new UINodeGuideMap(SCUIShowType.ADDITION));
            }
            GameRunSave.NotifyEnteredMapOnce();
            GameRunSave.SaveFromGameModel();
        }

        private void onCheatDebugUiRefresh()
        {
            refreshShow();
        }

        private void refreshShow()
        {
            setPlayerInfo();
            refreshMapName();
            RefreshAllMapNodeIcons();
            RefreshAllMapNodesCanWalk();
        }

        private void RefreshAllMapNodeIcons()
        {
            var grid = MapManager.instance?.currentMapNodes;
            if (grid == null)
                return;
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    var node = grid[i, j];
                    if (node == null)
                        continue;
                    node.RefreshMapIconDisplay();
                }
            }
        }

        /// <summary> ???????????????????????? Update?? </summary>
        private void RefreshAllMapNodesCanWalk()
        {
            var grid = MapManager.instance?.currentMapNodes;
            if (grid == null)
                return;
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    var node = grid[i, j];
                    if (node == null)
                        continue;
                    node.RefreshCanWalkDisplay();
                }
            }
        }

        /// <summary> ? map ?????????????????mapName?? </summary>
        private void refreshMapName()
        {
            if (mono.txtMapName == null) return;
            if (GameModel.instance?.playerInfo == null || SCRefDataMgr.instance?.mapRefList?.refDataList == null)
            {
                mono.txtMapName.text = string.Empty;
                return;
            }
            int floor = GameModel.instance.playerInfo.playerFloor;
            MapRefObj mapRow = SCRefDataMgr.instance.mapRefList.refDataList.Find(m => m.floor == floor);
            mono.txtMapName.text = mapRow != null && !string.IsNullOrEmpty(mapRow.mapName)
                ? mapRow.mapName
                : string.Empty;
        }

        private void setPlayerInfo()
        {
            mono.txtCoin.text = GameModel.instance.playerInfo.playerMoney.ToString();
            mono.txtHealth.text = GameModel.instance.playerInfo.currentHealth + "/" + GameModel.instance.playerInfo.maxHealth;
            mono.imgHealthBar.fillAmount = GameModel.instance.playerInfo.currentHealth / (float)GameModel.instance.playerInfo.maxHealth;
        }

        private void onBtnBagClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.AddNode(new UINodeDeck(SCUIShowType.ADDITION, GameModel.instance.playerInfo.bagPartInfoList));
        }
        private void onBtnSettingClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.AddNode(new UINodeSetting(SCUIShowType.ADDITION,true));
        }
        private void onBtnGuideClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.AddNode(new UINodeGuideMap(SCUIShowType.ADDITION));
        }

        private void onBtnBagMouseEnter(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnBag == null) return;
            _m_tweenContainer.RegDoTween(mono.btnBag.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnBagMouseExit(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnBag == null) return;
            _m_tweenContainer.RegDoTween(mono.btnBag.transform.DOScale(Vector3.one, mono.scaleChgDuration));
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

        private void onBtnGuideMouseEnter(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnGuide == null) return;
            _m_tweenContainer.RegDoTween(mono.btnGuide.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnGuideMouseExit(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnGuide == null) return;
            _m_tweenContainer.RegDoTween(mono.btnGuide.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }
    }
}

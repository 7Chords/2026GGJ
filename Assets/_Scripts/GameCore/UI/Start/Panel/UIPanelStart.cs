using DG.Tweening;
using GameCore;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelStart : _ASCUIPanelBase<UIMonoStart>
    {
        private TweenContainer _m_tweenContainer;
        public UIPanelStart(UIMonoStart _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

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
            mono.btnStart.RemoveClickDown(onBtnStartClickDown);
            mono.btnExit.RemoveClickDown(onBtnExitClickDown);
            mono.btnStart.RemoveMouseEnter(onBtnStartMouseEnter);
            mono.btnStart.RemoveMouseExit(onBtnStartMouseExit);
            mono.btnExit.RemoveMouseEnter(onBtnExitMouseEnter);
            mono.btnExit.RemoveMouseExit(onBtnExitMouseExit);

            if (mono.btnContinue != null)
            {
                mono.btnContinue.RemoveClickDown(onBtnContinueClickDown);
                mono.btnContinue.RemoveMouseEnter(onBtnContinueMouseEnter);
                mono.btnContinue.RemoveMouseExit(onBtnContinueMouseExit);
            }
            if (mono.btnSetting != null)
            {
                mono.btnSetting.RemoveClickDown(onBtnSettingClickDown);
                mono.btnSetting.RemoveMouseEnter(onBtnSettingMouseEnter);
                mono.btnSetting.RemoveMouseExit(onBtnSettingMouseExit);
            }
        }

        public override void OnShowPanel()
        {
            mono.btnStart.AddMouseLeftClickDown(onBtnStartClickDown);
            mono.btnExit.AddMouseLeftClickDown(onBtnExitClickDown);
            mono.btnStart.AddMouseEnter(onBtnStartMouseEnter);
            mono.btnStart.AddMouseExit(onBtnStartMouseExit);
            mono.btnExit.AddMouseEnter(onBtnExitMouseEnter);
            mono.btnExit.AddMouseExit(onBtnExitMouseExit);

            if (mono.btnContinue != null)
            {
                mono.btnContinue.AddMouseLeftClickDown(onBtnContinueClickDown);
                mono.btnContinue.AddMouseEnter(onBtnContinueMouseEnter);
                mono.btnContinue.AddMouseExit(onBtnContinueMouseExit);
            }
            if (mono.btnSetting != null)
            {
                mono.btnSetting.AddMouseLeftClickDown(onBtnSettingClickDown);
                mono.btnSetting.AddMouseEnter(onBtnSettingMouseEnter);
                mono.btnSetting.AddMouseExit(onBtnSettingMouseExit);
            }

            refreshContinueButtonVisibility();

            if (AudioMgr.instance.bgmAudioInfo == null
                || AudioMgr.instance.bgmAudioInfo.audioName != "bgm_main_music")
                AudioMgr.instance.PlayBgm("bgm_main_music");
        }

        private void refreshContinueButtonVisibility()
        {
            if (mono.btnContinue != null)
                mono.btnContinue.gameObject.SetActive(GameRunSave.HasSavedRun());
        }

        private void onBtnExitClickDown(PointerEventData arg1, object[] arg2)
        {
            Application.Quit();
        }

        /// <summary> 完全新开局：重置数据、删档、重随机地图。 </summary>
        private void onBtnStartClickDown(PointerEventData arg1, object[] arg2)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            GameModel.instance.ResetRunForNewGame();
            GameRunSave.DeleteSave();
            MapManager.instance.ClearCurrentMapNodes();
            MapManager.instance.ClearPendingLayout();
            MapGenerator.GetOrFind()?.GenerateMapDataOnly();

            TVSwitchTransition.Run(() =>
            {
                UICoreMgr.instance.CloseTopNode();
                UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
            });
        }

        private void onBtnContinueClickDown(PointerEventData arg1, object[] arg2)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            MapManager.instance.ClearCurrentMapNodes();
            MapManager.instance.ClearPendingLayout();
            if (!GameRunSave.TryLoadIntoGameModel())
            {
                refreshContinueButtonVisibility();
                return;
            }

            var genPre = MapGenerator.GetOrFind();
            var gmPre = GameModel.instance;
            if (genPre != null && gmPre != null)
            {
                // Prefer pending seed (from save) but fall back to run seed to keep layout stable.
                int? fixedSeed = gmPre.PendingRunMapLayoutSeed;
                if (!fixedSeed.HasValue && gmPre.RunMapLayoutSeed >= 0)
                    fixedSeed = gmPre.RunMapLayoutSeed;
                if (fixedSeed.HasValue)
                    genPre.GenerateMapDataOnly(fixedSeed);
            }

            TVSwitchTransition.Run(() =>
            {
                UICoreMgr.instance.CloseTopNode();
                UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
            });
        }

        private void onBtnSettingClickDown(PointerEventData arg1, object[] arg2)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.AddNode(new UINodeSetting(SCUIShowType.ADDITION,false));
        }

        private void onBtnStartMouseEnter(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnStart.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnStartMouseExit(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnStart.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onBtnExitMouseEnter(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnExit.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnExitMouseExit(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnExit.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onBtnContinueMouseEnter(PointerEventData arg1, object[] arg2)
        {
            if (mono.btnContinue == null) return;
            _m_tweenContainer.RegDoTween(mono.btnContinue.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnContinueMouseExit(PointerEventData arg1, object[] arg2)
        {
            if (mono.btnContinue == null) return;
            _m_tweenContainer.RegDoTween(mono.btnContinue.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onBtnSettingMouseEnter(PointerEventData arg1, object[] arg2)
        {
            if (mono.btnSetting == null) return;
            _m_tweenContainer.RegDoTween(mono.btnSetting.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnSettingMouseExit(PointerEventData arg1, object[] arg2)
        {
            if (mono.btnSetting == null) return;
            _m_tweenContainer.RegDoTween(mono.btnSetting.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }
    }
}

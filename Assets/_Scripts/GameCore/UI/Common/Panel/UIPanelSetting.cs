using SCFrame;
using SCFrame.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelSetting : _ASCUIPanelBase<UIMonoSetting>
    {
        private TweenContainer _m_tweenContainer;
        public UIPanelSetting(UIMonoSetting _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            mono.sldMusic.onValueChanged.RemoveAllListeners();
            mono.sldSound.onValueChanged.RemoveAllListeners();
            mono.btnClose.RemoveClickDown(onBtnCloseClickDown);
            mono.btnReturnMain.RemoveClickDown(onBtnReturnMainClickDonw);
            mono.btnReturnMain.RemoveMouseEnter(onBtnReturnMainMouseEnter);
            mono.btnReturnMain.RemoveMouseExit(onBtnReturnMainMouseExit);
            mono.togCRT.onValueChanged.RemoveAllListeners();
        }

        private void onTogCRTClickDown(bool _isOn)
        {
            SCGame.instance.rendererData.rendererFeatures.Find(x => x.name == "TintRenderFeature").SetActive(_isOn);
        }

        public override void OnShowPanel()
        {
            mono.sldMusic.onValueChanged.AddListener((value) =>
            {
                AudioMgr.instance.ChangeBgmVolume(value);
            });
            mono.sldSound.onValueChanged.AddListener((value) =>
            {
                AudioMgr.instance.ChangeSfxVolume(value);
            });
            mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDown);
            mono.btnReturnMain.AddMouseLeftClickDown(onBtnReturnMainClickDonw);
            mono.btnReturnMain.AddMouseEnter(onBtnReturnMainMouseEnter);
            mono.btnReturnMain.AddMouseExit(onBtnReturnMainMouseExit);
            mono.togCRT.onValueChanged.AddListener(onTogCRTClickDown);

            mono.sldMusic.value = AudioMgr.instance.bgmVolumeFactor;
            mono.sldSound.value = AudioMgr.instance.sfxVolumeFactor;
        }

        private void onBtnReturnMainMouseExit(PointerEventData _data, object[] _objs)
        {
            _m_tweenContainer?.RegDoTween(mono.btnReturnMain.transform.DOScale(Vector3.one, mono.btnScaleChgTime));

        }

        private void onBtnReturnMainMouseEnter(PointerEventData _data, object[] _objs)
        {
            _m_tweenContainer?.RegDoTween(mono.btnReturnMain.transform.DOScale(mono.btnEnterScale, mono.btnScaleChgTime));
        }

        private void onBtnReturnMainClickDonw(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            TVSwitchTransition.Run(() =>
            {
                GameModel.instance.playerInfo.ClearPendingMapMove();
                GameModel.instance.SetAllPlayerPart2Bag();
                GameModel.instance.SetEnemyEmpty();
                // Save after state is normalized (parts returned to bag, enemy cleared), otherwise continue-game
                // can load with empty bag / parts stuck in battle lists.
                GameRunSave.SaveFromGameModel();
                UICoreMgr.instance.CloseTopNode();
                UICoreMgr.instance.RemoveAllNodes(SCUINodeFuncType.BATTLE);
                UICoreMgr.instance.AddNode(new UINodeStart(SCUIShowType.FULL));
            });
        }

        private void onBtnCloseClickDown(PointerEventData _data, object[] _objs)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.CloseTopNode();
        }

        public void SetReturnBtnShowState(bool _needShow)
        {
            SCCommon.SetGameObjectEnable(mono.btnReturnMain.gameObject, _needShow);
        }
    }
}

using DG.Tweening;
using GameCore;
using GameCore.Helpers;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelBookEnemyFacePart : _ASCUIPanelBase<UIMonoBookEnemyFacePart>
    {
        private PartInfo _m_partInfo;
        private List<PartInfo> _m_battleOrderList;
        private Vector2 _m_tooltipScreenRatio = new Vector2(
            GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_X_IN_BOOK,
            GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_Y_IN_BOOK);
        private Action<PartInfo> _m_onHoverEnter;
        private Action _m_onHoverExit;
        private TweenContainer _m_tweenContainer;

        public UIPanelBookEnemyFacePart(UIMonoBookEnemyFacePart mono, SCUIShowType showType) : base(mono, showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
        }

        public override void BeforeDiscard()
        {
            GameCommon.DiscardToolTip();
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
            _m_onHoverEnter = null;
            _m_onHoverExit = null;
        }

        public override void OnHidePanel()
        {
            if (mono.imgGO != null)
            {
                mono.imgGO.RemoveMouseEnter(onMouseEnter);
                mono.imgGO.RemoveMouseExit(onMouseExit);
            }

            GameCommon.DiscardToolTip();
            _m_onHoverExit?.Invoke();
        }

        public override void OnShowPanel()
        {
            if (mono.imgGO != null)
            {
                mono.imgGO.AddMouseEnter(onMouseEnter);
                mono.imgGO.AddMouseExit(onMouseExit);
            }
        }

        public void BindHoverCallbacks(Action<PartInfo> onEnter, Action onExit)
        {
            _m_onHoverEnter = onEnter;
            _m_onHoverExit = onExit;
        }

        public void SetTooltipScreenRatio(Vector2 screenRatio)
        {
            _m_tooltipScreenRatio = screenRatio;
        }

        public void SetLocalPos(Vector2 pos)
        {
            GetGameObject().transform.localPosition = pos;
        }

        public void SetInfo(PartInfo info, List<PartInfo> battleOrderList)
        {
            _m_partInfo = info;
            _m_battleOrderList = battleOrderList;
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_partInfo == null || mono.imgGO == null)
                return;

            mono.imgGO.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partEnemyGameObjectName);
            mono.imgGO.SetNativeSize();
            PartSpriteRaycastHelper.ApplyToPartImages(mono.imgGO, null);

            if (mono.txtOrder != null)
                mono.txtOrder.text = getBattleOrderDisplay().ToString();

            mono.imgGO.transform.rotation = Quaternion.Euler(0f, 0f, _m_partInfo.rotateStep * 90f);
            mono.imgGO.transform.localScale = mono.scaleGO * Vector3.one;
        }

        private int getBattleOrderDisplay()
        {
            if (_m_battleOrderList != null)
                return BattleOrderHelper.GetBattleOrderByPartInfo(_m_battleOrderList, _m_partInfo);

            return 0;
        }

        private void onMouseEnter(PointerEventData data, object[] objs)
        {
            if (_m_partInfo == null)
                return;

            AudioMgr.instance.PlaySfx("sfx_mouse_enter");
            GetGameObject().transform.SetAsLastSibling();
            _m_tweenContainer.RegDoTween(mono.imgGO.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
            GameCommon.ShowTooltip(_m_partInfo, _m_tooltipScreenRatio, false);
            _m_onHoverEnter?.Invoke(_m_partInfo);
        }

        private void onMouseExit(PointerEventData data, object[] objs)
        {
            _m_tweenContainer.RegDoTween(mono.imgGO.transform.DOScale(mono.scaleGO, mono.scaleChgDuration));
            GameCommon.DiscardToolTip();
            _m_onHoverExit?.Invoke();
        }
    }
}

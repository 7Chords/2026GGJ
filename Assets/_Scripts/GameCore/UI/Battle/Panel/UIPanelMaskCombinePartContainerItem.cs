using DG.Tweening;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameCore.UI
{
    /// <summary>
    /// 面具合成界面的部位栏目item
    /// </summary>
    public class UIPanelMaskCombinePartContainerItem : _ASCUIPanelBase<UIMonoMaskCombinePartContainerItem>
    {
        private PartInfo _m_partInfo;

        private GameObject _m_dragPartGO;

        private bool _m_isDraging;

        private TweenContainer _m_tweenContainer;

        public UIPanelMaskCombinePartContainerItem(UIMonoMaskCombinePartContainerItem _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }


        public PartInfo GetPartInfo()
        {
            return _m_partInfo;
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
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLACE_PART_FAIL, onPlacePartFail);

            GetGameObject().transform.RemoveBeginDrag(onBeginDrag);
            GetGameObject().transform.RemoveDrag(onDrag);
            GetGameObject().transform.RemoveEndDrag(onEndDrag);
            GetGameObject().transform.RemoveMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.RemoveMouseExit(onGameObjMouseExit);
        }


        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.PLACE_PART_FAIL, onPlacePartFail);

            GetGameObject().transform.AddBeginDrag(onBeginDrag);
            GetGameObject().transform.AddDrag(onDrag);
            GetGameObject().transform.AddEndDrag(onEndDrag);
            GetGameObject().transform.AddMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.AddMouseExit(onGameObjMouseExit);
        }

        public void AddMouseEvent()
        {
            GetGameObject().transform.AddMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.AddMouseExit(onGameObjMouseExit);
        }
        private void onPlacePartFail(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            PartInfo partInfo = _objs[0] as PartInfo;

            if (partInfo != _m_partInfo)
                return;
            // 恢复物体的显示和交互
            if (mono.canvasGroup != null)
            {
                mono.canvasGroup.alpha = 1f;
                mono.canvasGroup.blocksRaycasts = true;
                mono.canvasGroup.interactable = true;

            }
        }
        public void SetInfo(PartInfo _info)
        {
            _m_partInfo = _info;
            refreshShow();
            
            // 初次设置位置需要根据 gridPos 更新 (这里先不处理，假设初始Layout负责)
            // 如果需要初始化位置：
            // UpdatePositionByGrid(_m_partInfo.gridPos); 
            // 但需要找到对应的Grid Transform，比较复杂，暂时只处理拖拽后的位置更新
        }


        private void refreshShow()
        {
            if (_m_partInfo == null)
            {
                return;
            }

            mono.imgGoods.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);

            string hpStr = $"{_m_partInfo.currentHealth}/{_m_partInfo.partRefObj.partHealth}";

            mono.txtHealth.text = hpStr;

        }

        private void onBeginDrag(PointerEventData _arg, object[] _objs)
        {
            if (_m_dragPartGO != null) return;
            _m_isDraging = true;
            //创建拖拽出来的部位
            _m_dragPartGO = ResourcesHelper.LoadGameObject("prefab_face_part_preview", SCGame.instance.topLayerRoot.transform);
            _m_dragPartGO.GetComponent<FacePartPreview>().Initialize(_m_partInfo);

            //隐藏原物体的交互和显示
            if (mono.canvasGroup != null)
            {
                mono.canvasGroup.alpha = 0f;
                mono.canvasGroup.blocksRaycasts = false;
                mono.canvasGroup.interactable = false;
            }


        }
        private void onEndDrag(PointerEventData _arg, object[] _objs)
        {
            if (_m_dragPartGO == null || !_m_isDraging)
                return;
            _m_isDraging = false;
            _m_dragPartGO.GetComponent<FacePartPreview>().EndDrag(_arg);
            _m_dragPartGO = null;
        }

        private void onDrag(PointerEventData _arg, object[] _objs)
        {
            if (_m_dragPartGO == null || !_m_isDraging)
                return;

            _m_dragPartGO.GetComponent<FacePartPreview>().Drag(_arg);
        }


        private void onGameObjMouseExit(PointerEventData arg1, object[] arg2)
        {
            
            GameCommon.DiscardToolTip();
            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(Vector3.one, mono.scaleChgDuration));

        }

        private void onGameObjMouseEnter(PointerEventData arg1, object[] arg2)
        {
            if (_m_partInfo == null)
                return;

            GameCommon.ShowTooltip(_m_partInfo.partRefObj.partName, _m_partInfo.partRefObj.partDesc, GetGameObject().transform.position);

            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }
    }
}

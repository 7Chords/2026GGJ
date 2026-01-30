using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelMaskCombinePartContainerItem : _ASCUIPanelBase<UIMonoMaskCombinePartContainerItem>
    {
        private PartInfo _m_partInfo;

        private GameObject _m_dragCloneGO;

        private bool _m_isDraging;
        public UIPanelMaskCombinePartContainerItem(UIMonoMaskCombinePartContainerItem _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {

        }

        public override void BeforeDiscard()
        {

        }

        public override void OnHidePanel()
        {
            mono.imgGoods.RemoveBeginDrag(onBeginDrag);
            mono.imgGoods.RemoveDrag(onDrag);
            mono.imgGoods.RemoveEndDrag(onEndDrag);
        }

        public override void OnShowPanel()
        {
            mono.imgGoods.AddBeginDrag(onBeginDrag);
            mono.imgGoods.AddDrag(onDrag);
            mono.imgGoods.AddEndDrag(onEndDrag);

        }

        public void SetInfo(PartInfo _info)
        {
            _m_partInfo = _info;
            refreshShow();
        }
        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            mono.imgGoods.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);
            mono.txtHealth.text = _m_partInfo.currentHealth + "/" + _m_partInfo.partRefObj.partHealth;
        }

        private void onBeginDrag(PointerEventData _arg, object[] _objs)
        {
            _m_isDraging = true;

            // 隐藏原物体的交互
            if (mono.canvasGroup != null)
            {
                mono.canvasGroup.alpha = 0f;
                mono.canvasGroup.blocksRaycasts = false;
            }
            createDragClone();

        }
        private void onEndDrag(PointerEventData _arg, object[] _objs)
        {
            _m_isDraging = false;

            if(checkIsValidInstrument(_arg))
            {

            }
            else
            {
                if (_m_dragCloneGO != null)
                {
                    SCCommon.DestoryGameObject(_m_dragCloneGO);
                    _m_dragCloneGO = null;
                }
                // 恢复原物体的显示和交互
                if (mono.canvasGroup != null)
                {
                    mono.canvasGroup.alpha = 1f;
                    mono.canvasGroup.blocksRaycasts = true;
                }
            }
        }

        private void onDrag(PointerEventData _arg, object[] _objs)
        {
            if (!_m_isDraging)
                return;
            updateDragClonePosition(_arg);
        }

        /// <summary>
        /// 创建拖拽克隆体
        /// </summary>
        private void createDragClone()
        {
            if (_m_dragCloneGO != null) return;

            // 创建克隆体
            _m_dragCloneGO = SCCommon.InstantiateGameObject(GetGameObject(), SCGame.instance.fullLayerRoot.transform);
            _m_dragCloneGO.GetComponent<RectTransform>().sizeDelta = new Vector2
                (GetGameObject().GetComponent<RectTransform>().rect.width,
                GetGameObject().GetComponent<RectTransform>().rect.height);


            // 移除克隆体上不需要的组件
            var cloneInstrumentItem = _m_dragCloneGO.GetComponent<UIMonoMaskCombinePartContainerItem>();
            if (cloneInstrumentItem != null)
                cloneInstrumentItem.enabled = false;

        }


        /// <summary>
        /// 更新拖拽克隆体位置
        /// </summary>
        private void updateDragClonePosition(PointerEventData eventData)
        {
            if (_m_dragCloneGO == null) return;

            Vector2 localPointerPosition = SCUICommon.ScreenPointToUIPoint(SCGame.instance.fullLayerRoot.transform as RectTransform, eventData.position);
            _m_dragCloneGO.transform.localPosition = localPointerPosition;
        }


        /// <summary>
        /// 检查是否放置在有效区域
        /// </summary>
        private bool checkIsValidInstrument(PointerEventData _eventData)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_eventData, results);
            GameObject go = results.Find(x => x.gameObject.GetComponent<UIMonoMaskCombineFaceGrid>() != null).gameObject;
            return go != null;
        }
    }
}


using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelFacePart : _ASCUIPanelBase<UIMonoFacePart>
    {

        private PartInfo _m_partInfo;
        public PartInfo partInfo => _m_partInfo;

        private string _m_dragLoopCoroutineId;

        private bool _m_isDraging = false;
        public UIPanelFacePart(UIMonoFacePart _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            mono.imgGO.RemoveBeginDrag(onBeginDrag);
            mono.imgGO.RemoveDrag(onDrag);
            mono.imgGO.RemoveEndDrag(onEndDrag);
        }

        public override void OnShowPanel()
        {
            mono.imgGO.AddBeginDrag(onBeginDrag);
            mono.imgGO.AddDrag(onDrag);
            mono.imgGO.AddEndDrag(onEndDrag);

        }

        public void SetInfo(PartInfo _info)
        {
            _m_partInfo = _info;
            refreshShow();
        }
        public void SetLocalPos(Vector2 _pos)
        {
            GetGameObject().transform.localPosition = _pos;
        }
        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            mono.imgGO.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partGameObjectName);
            mono.imgGO.SetNativeSize();
            mono.imgPart.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partGameObjectName);
            mono.imgPart.SetNativeSize();
            mono.txtHealth.text = _m_partInfo.currentHealth +"/" + _m_partInfo.maxHealth;
            //todo:order

            //信息不要跟着旋转
            mono.imgGO.transform.rotation = Quaternion.Euler(0, 0, _m_partInfo.rotateStep * 90);
            mono.goHealthInfo.transform.eulerAngles = Vector3.zero;
            mono.goOrder.transform.eulerAngles = Vector3.zero;
        }






        public void onBeginDrag(PointerEventData _data, object[] _objs)
        {
            _m_isDraging = true;
            if (!string.IsNullOrEmpty(_m_dragLoopCoroutineId)) SCTaskHelper.instance.KillAllCoroutines(this);
            _m_dragLoopCoroutineId = SCTaskHelper.instance.CreateCoroutine(this,dragLoop());

            //设置占据的格子的状态
            GameModel.instance.SetGridsEmpty(_m_partInfo.curOccupyFacePosList);
            //取消信息的脸部状态
            partInfo.ClearOnFaceState();

        }
        private void onEndDrag(PointerEventData _data, object[] _objs)
        {
            if (!_m_isDraging)
                return;
            if (!string.IsNullOrEmpty(_m_dragLoopCoroutineId))
                SCTaskHelper.instance.KillAllCoroutines(this);



            //当前鼠标指向的脸部的格子物体
            GameObject gridGO = GameCommon.GetHitGridGameObj(_data);
            bool placementSuccess = false;//是否放置成功

            if (gridGO != null)
            {
                if (GameModel.instance.CanPlacePart(gridGO, _data.position, _m_partInfo.localOccupyPosList))
                {
                    placementSuccess = true;
                    SCMsgCenter.SendMsg(SCMsgConst.REPLACE_PART_POS_SUCCESS, this, GameModel.instance.GetPlaceFacePosList(gridGO, _data.position, _m_partInfo.localOccupyPosList));

                }
            }

            if (!placementSuccess)
            {
                SCMsgCenter.SendMsg(SCMsgConst.REPLACE_PART_POS_FAIL,_m_partInfo);
                SCCommon.DestoryGameObject(GetGameObject());
            }
        }

        private void onDrag(PointerEventData _data, object[] _objs)
        {
            if (!_m_isDraging)
                return;
            RectTransform parentRect = GetGameObject().transform.parent as RectTransform;
            Vector2 localPoint;
            Camera uiCam = SCGame.instance.gameCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, _data.position, uiCam, out localPoint))
            {
                GetGameObject().transform.localPosition = localPoint;

            }
        }

        private IEnumerator dragLoop()
        {
            while (_m_isDraging)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    _m_partInfo.RotateOnce();
                    refreshShow();
                }
                yield return null;
            }
        }


    }
}

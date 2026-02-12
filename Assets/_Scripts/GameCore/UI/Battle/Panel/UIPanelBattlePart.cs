using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelBattlePart : _ASCUIPanelBase<UIMonoBattlePart>
    {
        private PartInfo _m_partInfo;
        public PartInfo partInfo => _m_partInfo;
        public UIPanelBattlePart(UIMonoBattlePart _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            mono.imgGO.RemoveMouseEnter(onMouseEnter);
            mono.imgGO.RemoveMouseExit(onMouseExit);
        }

        public override void OnShowPanel()
        {
            mono.imgGO.AddMouseEnter(onMouseEnter);
            mono.imgGO.AddMouseExit(onMouseExit);
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
            mono.txtHealth.text = _m_partInfo.currentHealth + "/" + _m_partInfo.maxHealth;

            if(_m_partInfo.isEnemyPart)
                mono.txtOrder.text = GameModel.instance.GetEnemyBattleOrderByPartInfo(_m_partInfo).ToString();
            else
                mono.txtOrder.text = GameModel.instance.GetPlayerBattleOrderByPartInfo(_m_partInfo).ToString();

            mono.imgGO.transform.rotation = Quaternion.Euler(0, 0, _m_partInfo.rotateStep * 90);
            //信息子物体自动适配旋转和rect大小
            autoAdjustPosAndRotate(mono.imgGO.gameObject, mono.goHealthInfo, mono.goHealthPosPivot);
            autoAdjustPosAndRotate(mono.imgGO.gameObject, mono.goOrder, mono.goOrderPosPivot);
        }
        private void autoAdjustPosAndRotate(GameObject _parent, GameObject _child, Vector2 _pivotPos)
        {
            RectTransform parentRT = _parent.GetComponent<RectTransform>();
            RectTransform childRT = _child.GetComponent<RectTransform>();

            float scale = parentRT.lossyScale.y;

            // 是否旋转了 90/270 度
            int rotateMod = _m_partInfo.rotateStep % 2;
            bool isRotated90 = rotateMod != 0;

            // 父物体「视觉上」的宽高（旋转后自动互换）
            float parentVisualW = isRotated90 ? parentRT.rect.height : parentRT.rect.width;
            float parentVisualH = isRotated90 ? parentRT.rect.width : parentRT.rect.height;

            // 世界空间下的真实半宽高
            float parentHalfW = parentVisualW * scale * 0.5f;
            float parentHalfH = parentVisualH * scale * 0.5f;

            // 子物体自身半宽高（让子物体自身也居中对齐）
            float childHalfW = childRT.rect.width * scale * 0.5f;
            float childHalfH = childRT.rect.height * scale * 0.5f;

            // ==========================
            // 核心：子物体放在父物体「内部」
            // ==========================
            float x = parentRT.position.x + _pivotPos.x * parentHalfW;
            float y = parentRT.position.y + _pivotPos.y * parentHalfH;

            Vector3 targetPos = new Vector3(x, y, parentRT.position.z);

            // 应用位置
            _child.transform.position = targetPos;
            // 永远不旋转
            _child.transform.rotation = Quaternion.identity;
        }

        private void onMouseExit(PointerEventData arg1, object[] arg2)
        {
            GameCommon.DiscardToolTip();

        }

        private void onMouseEnter(PointerEventData arg1, object[] arg2)
        {
            //放到最下面 显示在最前面
            GetGameObject().transform.SetAsLastSibling();
            GameCommon.ShowTooltip(_m_partInfo.partRefObj.partName,
                _m_partInfo.partRefObj.partDesc,
                new Vector2(GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_X_IN_BATTLE, GameConst.SHOW_FACE_PART_TIP_SCREEN_RATIO_Y_IN_BATTLE),
                _m_partInfo.partRefObj.qualityType,
                false);
        }
    }
}

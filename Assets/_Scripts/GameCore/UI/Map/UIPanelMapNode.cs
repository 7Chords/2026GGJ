using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMapNode : _ASCUIPanelBase<UIMonoMapNode>
    {
        private MapNode _m_mapNode;
        private ERoomType _m_roomType;

        public UIPanelMapNode(UIMonoMapNode _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            mono.btnEnter.onClick.AddListener(OnClickEnter);
        }

        public override void BeforeDiscard()
        {
            mono.btnEnter.onClick.RemoveListener(OnClickEnter);
        }
        public override void OnHidePanel()
        {
        }

        public override void OnShowPanel()
        {
        }
        public void SetNodeInfo(MapNode mapNode)
        {
            _m_mapNode = mapNode;
            _m_roomType = mapNode.NodeType;
            RefreshShow();
        }

        /// <summary>
        /// ?????????¦Ë????????????????????????????? SetNodeInfo/RefreshShow ??????????????????????? Update??
        /// </summary>
        public void RefreshCanWalkState()
        {
            if (_m_mapNode == null || mono == null)
                return;

            bool canMove = ComputeCanMoveToNode();
            SetCanWalkVisual(canMove);
        }

        bool ComputeCanMoveToNode()
        {
            if (_m_mapNode == null || GameModel.instance?.playerInfo == null)
                return false;

            var playerPos = GameModel.instance.playerInfo.playerMapPosition;
            var targetPos = _m_mapNode.GridPosition;
            if (targetPos.x != playerPos.x + 1)
                return false;

            var prevNode = MapManager.instance != null
                ? MapManager.instance.GetNode(playerPos.x, playerPos.y)
                : null;
            if (prevNode != null)
            {
                if (!prevNode.nextLayerConnectedNodes.Contains(targetPos.y))
                    return false;
            }

            return true;
        }

        void SetCanWalkVisual(bool canMove)
        {
            if (mono.goCanWalk == null || mono.goCanWalk.Count == 0)
                return;
            foreach (var go in mono.goCanWalk)
            {
                if (go == null)
                    continue;
                go.SetActive(canMove);
            }
        }

        private void RefreshShow()
        {
            if (_m_mapNode == null)
                return;
            switch (_m_roomType)
            {
                case ERoomType.NONE:
                    break;
                case ERoomType.ENEMY:
                    {
                        mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>("spr_icon_node_enemy");
                    }
                    break;
                case ERoomType.TRIAL:
                    break;
                case ERoomType.SHOP:
                    {
                        mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>("spr_icon_node_shop");
                    }
                    break;
                case ERoomType.EVENT:
                    break;
                case ERoomType.BOSS:
                    {
                        mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>("spr_icon_node_boss");
                    }
                    break;
                case ERoomType.STRENGTHEN:
                    {
                        mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>("spr_icon_node_strengthen");
                    }
                    break;
            }


            RefreshCanWalkState();
        }

        private void OnClickEnter()
        {
            // 1. Validation Logic
            var playerPos = GameModel.instance.playerInfo.playerMapPosition;
            var targetPos = _m_mapNode.GridPosition;

            //// Case A: First Move (Player not on map yet)
            //if (playerPos.x == -1)
            //{
            //    if (targetPos.x != 0)
            //    {
            //        SCDebugHelper.Log("Must start at Layer 0!");
            //        return;
            //    }
            //}
            //// Case B: Normal Move
            //else
            //{
            //    // Check Layer (Must be Next Layer)
            //    if (targetPos.x != playerPos.x + 1)
            //    {
            //        SCDebugHelper.Log($"Can only move to Next Layer! Current: {playerPos.x}, Target: {targetPos.x}");
            //        return;
            //    }

            //    // Check Connection
            //    // Find Logic: Get Previous Node and check if it connects to Target Index
            //    var prevNode = MapManager.instance.GetNode(playerPos.x, playerPos.y);
            //    if (prevNode != null)
            //    {
            //        if (!prevNode.nextLayerConnectedNodes.Contains(targetPos.y))
            //        {
            //            SCDebugHelper.Log("Not connected!");
            //            return;
            //        }
            //    }
            //}

            // 2. ??????????????????????????????????????????? ApplyPendingMapMove??
            GameModel.instance.playerInfo.SetPendingMapTarget(targetPos);

            switch (_m_roomType)
            {
                case ERoomType.ENEMY:
                    EnterEnemyLevel();
                    break;
                case ERoomType.BOSS:
                    EnterBossLevel();
                    break;
                case ERoomType.SHOP:
                    EnterShop();
                    break;
                case ERoomType.TRIAL:
                    EnterTrial();
                    break;
                case ERoomType.EVENT:
                    EnterEvent();
                    break;
                case ERoomType.STRENGTHEN:
                    EnterStrengthen();
                    break;
                default:
                    break;
            }
        }

        #region ??????

        private void EnterEnemyLevel()
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            GameModel.instance.RollBattleOrder();
            UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL));
            GameModel.instance.GenerateNewBattle();
            //UICoreMgr.instance.AddNode(new UINodeBattleOrder(SCUIShowType.ADDITION));
            UICoreMgr.instance.AddNode(new UINodeGuideBattle(SCUIShowType.ADDITION));
        }

        private void EnterBossLevel()
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            GameModel.instance.RollBattleOrder();
            UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL));
            GameModel.instance.GenerateNewBattle(true,999991);
            UICoreMgr.instance.AddNode(new UINodeBattleOrder(SCUIShowType.ADDITION));
        }

        private void EnterShop()
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            GameModel.instance.RollRandomShop();
            UICoreMgr.instance.AddNode(new UINodeStore(SCUIShowType.FULL));
        }

        private void EnterTrial()
        {
            // todo: Enter Trial Logic
        }

        private void EnterEvent()
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            GameModel.instance.RollEventId();
            UICoreMgr.instance.AddNode(new UINodeEvent(SCUIShowType.FULL));
        }
        private void EnterStrengthen()
        {
            UICoreMgr.instance.AddNode(new UINodeStrengthen(SCUIShowType.FULL));

        }

        #endregion

    }
}

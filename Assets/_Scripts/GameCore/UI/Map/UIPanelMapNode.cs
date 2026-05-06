using DG.Tweening;
using GameCore;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelMapNode : _ASCUIPanelBase<UIMonoMapNode>
    {
        private MapNode _m_mapNode;
        private ERoomType _m_roomType;
        private TweenContainer _m_tweenContainer;

        public UIPanelMapNode(UIMonoMapNode _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
            if (mono.btnEnter != null)
            {
                mono.btnEnter.onClick.AddListener(OnClickEnter);
                mono.btnEnter.AddMouseEnter(onBtnEnterMouseEnter);
                mono.btnEnter.AddMouseExit(onBtnEnterMouseExit);
            }
        }

        public override void BeforeDiscard()
        {
            if (mono.btnEnter != null)
            {
                mono.btnEnter.onClick.RemoveListener(OnClickEnter);
                mono.btnEnter.RemoveMouseEnter(onBtnEnterMouseEnter);
                mono.btnEnter.RemoveMouseExit(onBtnEnterMouseExit);
            }
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }
        public override void OnHidePanel()
        {
        }

        public override void OnShowPanel()
        {
        }

        private void onBtnEnterMouseEnter(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnEnter == null) return;
            _m_tweenContainer.RegDoTween(mono.btnEnter.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnEnterMouseExit(PointerEventData _arg1, object[] _arg2)
        {
            if (mono.btnEnter == null) return;
            _m_tweenContainer.RegDoTween(mono.btnEnter.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }
        public void SetNodeInfo(MapNode mapNode)
        {
            _m_mapNode = mapNode;
            _m_roomType = mapNode.NodeType;
            RefreshShow();
        }

        /// <summary>
        /// ???????????????????????????????????????? SetNodeInfo/RefreshShow ??????????????????????? Update??
        /// </summary>
        public void RefreshCanWalkState()
        {
            if (_m_mapNode == null || mono == null)
                return;

            bool canMove = ComputeCanMoveToNode();
            SetCanWalkVisual(canMove);
        }

        /// <summary> Re-run room icon + player-cell override (called when map opens or refreshes). </summary>
        public void RefreshMapVisual()
        {
            RefreshShow();
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
                    {
                        mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>("spr_icon_node_event");
                    }
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

            if (MapManager.instance != null && mono.imgIcon != null && _m_mapNode != null)
            {
                Vector2Int pg = MapManager.instance.GetDisplayedPlayerMapGrid(out bool hasP);
                if (hasP && _m_mapNode.GridPosition == pg)
                {
                    var spPlayer = ResourcesHelper.LoadAsset<Sprite>(GameConst.SPR_ICON_NODE_PLAYER);
                    if (spPlayer != null)
                        mono.imgIcon.sprite = spPlayer;
                }
            }

            RefreshCanWalkState();
        }

        private void OnClickEnter()
        {
            // 1. Validation Logic
            var playerPos = GameModel.instance.playerInfo.playerMapPosition;
            var targetPos = _m_mapNode.GridPosition;

            // Case A: First Move (Player not on map yet)
            if (playerPos.x == -1)
            {
                if (targetPos.x != 0)
                {
                    SCDebugHelper.Log("Must start at Layer 0!");
                    return;
                }
            }
            // Case B: Normal Move
            else
            {
                // Check Layer (Must be Next Layer)
                if (targetPos.x != playerPos.x + 1)
                {
                    SCDebugHelper.Log($"Can only move to Next Layer! Current: {playerPos.x}, Target: {targetPos.x}");
                    return;
                }

                // Check Connection
                // Find Logic: Get Previous Node and check if it connects to Target Index
                var prevNode = MapManager.instance.GetNode(playerPos.x, playerPos.y);
                if (prevNode != null)
                {
                    if (!prevNode.nextLayerConnectedNodes.Contains(targetPos.y))
                    {
                        SCDebugHelper.Log("Not connected!");
                        return;
                    }
                }
            }

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
            TVSwitchTransition.Run(() =>
            {
                GameModel.instance.RollBattleOrder();
                UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL));
                GameModel.instance.GenerateNewBattle();
                RunTutorialAuto.TryShowBattleGuideFirstTimeInRun();
            });
        }

        private void EnterBossLevel()
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            TVSwitchTransition.Run(() =>
            {
                AudioMgr.instance.PlayBgm("bgm_boss");

                int floor = GameModel.instance.playerInfo.playerFloor;
                EnemyRefObj bossRef = SCRefDataMgr.instance.enemyRefList.refDataList
                    .Find(e => e.isBoss && e.floor == floor);
                if (bossRef == null)
                {
                    SCDebugHelper.LogError($"[Map] No boss row in enemy sheet for floor={floor}.");
                    return;
                }
                GameModel.instance.RollBattleOrder();
                UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL));
                GameModel.instance.GenerateNewBattle(true, bossRef.id);
                RunTutorialAuto.TryShowBattleGuideFirstTimeInRun();
                UICoreMgr.instance.AddNode(new UINodeBattleOrder(SCUIShowType.ADDITION));
            });
        }

        private void EnterShop()
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            TVSwitchTransition.Run(() =>
            {
                GameModel.instance.RollRandomShop();
                UICoreMgr.instance.AddNode(new UINodeStore(SCUIShowType.FULL));
            });
        }

        private void EnterTrial()
        {
            // todo: Enter Trial Logic
        }

        private void EnterEvent()
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            TVSwitchTransition.Run(() =>
            {
                GameModel.instance.RollEventId();
                UICoreMgr.instance.AddNode(new UINodeEvent(SCUIShowType.FULL));
            });
        }
        private void EnterStrengthen()
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            TVSwitchTransition.Run(() =>
            {
                UICoreMgr.instance.AddNode(new UINodeStrengthen(SCUIShowType.FULL));
            });
        }

        #endregion

    }
}

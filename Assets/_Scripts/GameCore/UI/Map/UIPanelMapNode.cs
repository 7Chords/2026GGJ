using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
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

        public void SetNodeInfo(MapNode mapNode)
        {
            _m_mapNode = mapNode;
            _m_roomType = mapNode.NodeType;
            RefreshShow();
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
            }
        }

        private void OnClickEnter()
        {
            // 1. Validation Logic
            var playerPos = GameModel.instance.playerMapPosition;
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

            // 2. Update Position
            GameModel.instance.playerMapPosition = targetPos;
            
            // 3. Enter Logic
            Debug.Log($"Enter Node Type: {_m_roomType} at {_m_mapNode.GridPosition}");

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
                default:
                    break;
            }
        }

        #region Level Entry Logic (Placeholders)

        private void EnterEnemyLevel()
        {
            AudioMgr.instance.PlaySfx("sfx_click");

            UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL));
            GameModel.instance.GenerateRandomEnemy();
            BattleManager.instance.EnterBattle();
        }

        private void EnterBossLevel()
        {
            UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL));
            AudioMgr.instance.PlaySfx("sfx_click");
            GameModel.instance.GenerateRandomEnemy();
            BattleManager.instance.EnterBattle();
        }

        private void EnterShop()
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            List<StoreRefObj> storeRefList = SCRefDataMgr.instance.storeRefList.refDataList;
            long id = storeRefList[Random.Range(0, storeRefList.Count)].id;
            GameModel.instance.rollStoreId = id;
            UICoreMgr.instance.AddNode(new UINodeStore(SCUIShowType.FULL));
        }

        private void EnterTrial()
        {
            // TODO: Enter Treasure Logic
        }

        private void EnterEvent()
        {
            // TODO: Enter Event Logic
        }

        #endregion

        public override void OnHidePanel()
        {
        }

        public override void OnShowPanel()
        {
        }
    }
}

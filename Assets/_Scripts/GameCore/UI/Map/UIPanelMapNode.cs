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
        private RoomType _m_roomType;

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
            // TODO: 根据 RoomType 加载对应的图标
            // string iconPath = "Path/To/Icon_" + _m_roomType.ToString();
            // mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(iconPath);
            
            // 暂时为空或默认逻辑
            // Debug.Log($"Node {_m_mapNode.GridPosition} Type: {_m_roomType}");
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
                    Debug.Log("Must start at Layer 0!");
                    return;
                }
            }
            // Case B: Normal Move
            else
            {
                // Check Layer (Must be Next Layer)
                if (targetPos.x != playerPos.x + 1)
                {
                    Debug.Log($"Can only move to Next Layer! Current: {playerPos.x}, Target: {targetPos.x}");
                    return;
                }

                // Check Connection
                // Find Logic: Get Previous Node and check if it connects to Target Index
                var prevNode = MapManager.instance.GetNode(playerPos.x, playerPos.y);
                if (prevNode != null)
                {
                    if (!prevNode.nextLayerConnectedNodes.Contains(targetPos.y))
                    {
                         Debug.Log("Not connected!");
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
                case RoomType.Enemy:
                    EnterEnemyLevel();
                    break;
                case RoomType.Elite:
                    EnterEliteLevel();
                    break;
                case RoomType.Boss:
                    EnterBossLevel();
                    break;
                case RoomType.Shop:
                    EnterShop();
                    break;
                case RoomType.Rest:
                    EnterRest();
                    break;
                case RoomType.Treasure:
                    EnterTreasure();
                    break;
                case RoomType.Event:
                    EnterEvent();
                    break;
                default:
                    break;
            }
        }

        #region Level Entry Logic (Placeholders)

        private void EnterEnemyLevel()
        {
            List<EnemyRefObj> enemyRefList = SCRefDataMgr.instance.enemyRefList.refDataList;
            long id = enemyRefList[Random.Range(0, enemyRefList.Count)].id;
            GameModel.instance.rollEnemyId = id;
            UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL));
        }

        private void EnterEliteLevel()
        {
            // TODO: Enter Elite Level Logic
        }

        private void EnterBossLevel()
        {
            // TODO: Enter Boss Level Logic
            UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCUIShowType.FULL));
        }

        private void EnterShop()
        {
            List<StoreRefObj> storeRefList = SCRefDataMgr.instance.storeRefList.refDataList;
            long id = storeRefList[Random.Range(0, storeRefList.Count)].id;
            GameModel.instance.rollStoreId = id;
            UICoreMgr.instance.AddNode(new UINodeStore(SCUIShowType.FULL));
        }

        private void EnterRest()
        {
            // TODO: Enter Rest Logic
        }

        private void EnterTreasure()
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

using SCFrame;
using SCFrame.UI;
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
            // 点击进入对应情景或关卡
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
            // TODO: Enter Enemy Level Logic
        }

        private void EnterEliteLevel()
        {
            // TODO: Enter Elite Level Logic
        }

        private void EnterBossLevel()
        {
            // TODO: Enter Boss Level Logic
        }

        private void EnterShop()
        {
            // TODO: Enter Shop Logic
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

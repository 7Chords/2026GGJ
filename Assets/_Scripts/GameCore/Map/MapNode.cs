using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameCore.UI;
using SCFrame;
using SCFrame.UI;
using GameCore;

namespace GameCore
{
    public class MapNode : MonoBehaviour
    {

        // UI Reference
        private UIMonoMapNode _uiMono;
        private UIPanelMapNode _uiPanel;

        public bool isActive;
        public List<int> nextLayerConnectedNodes = new List<int>();

        public ERoomType NodeType { get; private set; }
        public Vector2Int GridPosition { get; private set; }

        private void Awake()
        {
            EnsurePanelCreated();
        }

        /// <summary> 延迟创建面板包装（部分预制体可能尚未挂上 UIMonoMapNode，或执行顺序导致首次调用时尚未 Awake）。 </summary>
        private void EnsurePanelCreated()
        {
            if (_uiPanel != null)
                return;
            if (_uiMono == null)
                _uiMono = GetComponent<UIMonoMapNode>();
            if (_uiMono != null)
                _uiPanel = new UIPanelMapNode(_uiMono, SCUIShowType.INTERNAL);
        }

        public void SetMapNodeIndex(int x, int y)
        {
            GridPosition = new Vector2Int(x, y);
            name = $"Node_{x}_{y}";
        }

        public void SetMapNodeType(ERoomType type)
        {
            NodeType = type;
            EnsurePanelCreated();
            if (_uiPanel != null)
                _uiPanel.SetNodeInfo(this);
            else
                Debug.LogWarning($"[MapNode] {name} 上未找到 UIMonoMapNode，无法刷新节点 UI。请检查 MapGenerator 的 nodePrefab 是否包含 UIMonoMapNode。");
        }

        /// <summary> 玩家位置或地图数据变化后，由 UIPanelMap 统一刷新各格「可行走」显示。 </summary>
        public void RefreshCanWalkDisplay()
        {
            EnsurePanelCreated();
            _uiPanel?.RefreshCanWalkState();
        }

        /// <summary> 根据预生成的纯数据设置变换与路线（打开地图 UI 时调用）。 </summary>
        public void ApplyFromLayout(MapCellLayoutData data)
        {
            if (data == null)
                return;
            SetMapNodeIndex(data.gridX, data.gridY);
            nextLayerConnectedNodes.Clear();
            if (data.nextLayerConnectedNodes != null)
                nextLayerConnectedNodes.AddRange(data.nextLayerConnectedNodes);
            transform.localPosition = data.localPosition;
            transform.localRotation = Quaternion.Euler(0, 0, data.angleZ);
            gameObject.SetActive(true);
            SetMapNodeType(data.roomType);
            gameObject.SetActive(data.isActive);
        }
    }
}

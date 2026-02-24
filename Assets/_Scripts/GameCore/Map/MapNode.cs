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
            _uiMono = GetComponent<UIMonoMapNode>();
            if (_uiMono != null)
            {
                _uiPanel = new UIPanelMapNode(_uiMono, SCUIShowType.INTERNAL);
            }
        }

        public void SetMapNodeIndex(int x, int y)
        {
            GridPosition = new Vector2Int(x, y);
            name = $"Node_{x}_{y}";
        }

        public void SetMapNodeType(ERoomType type)
        {
            NodeType = type;
            _uiPanel.SetNodeInfo(this);
        }


    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameCore.UI;
using SCFrame.UI;

public class MapNode : MonoBehaviour
{
    
    // UI Reference
    private UIMonoMapNode _uiMono;
    private UIPanelMapNode _uiPanel;

    private void Awake()
    {
        _uiMono = GetComponent<UIMonoMapNode>();
        if (_uiMono != null)
        {
            // Initialize UI Panel (SCUIShowType.INTERNAL usually for sub-items)
            _uiPanel = new UIPanelMapNode(_uiMono, SCUIShowType.INTERNAL);
            // Note: _ASCUIPanelBase constructor call Initialize() for INTERNAL, so AfterInitialize used for listener binding is good.
        }
    }

    private void Start()
    {
        if (_uiPanel != null)
        {
            _uiPanel.SetNodeInfo(this);
        }
    }

    public bool isActive;
    public List<int> nextLayerConnectedNodes = new List<int>();
    
    public RoomType NodeType { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    [SerializeField]
    private Image nodeImage; // Assuming UI based since MapGenerate uses RectTransform

    [Header("Debug/Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color eliteColor = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] private Color restColor = Color.green;
    [SerializeField] private Color shopColor = Color.yellow;
    [SerializeField] private Color treasureColor = Color.cyan;
    [SerializeField] private Color bossColor = Color.magenta;

    public void SetMapNodeIndex(int x, int y)
    {
        GridPosition = new Vector2Int(x, y);
        name = $"Node_{x}_{y}";
    }

    public void SetMapNodeType(RoomType type)
    {
        NodeType = type;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (nodeImage == null) nodeImage = GetComponent<Image>();
        if (nodeImage == null) return;

        switch (NodeType)
        {
            case RoomType.Enemy:
                nodeImage.color = enemyColor;
                break;
            case RoomType.Elite:
                nodeImage.color = eliteColor;
                break;
            case RoomType.Rest:
                nodeImage.color = restColor;
                break;
            case RoomType.Shop:
                nodeImage.color = shopColor;
                break;
            case RoomType.Treasure:
                nodeImage.color = treasureColor;
                break;
            case RoomType.Boss:
                nodeImage.color = bossColor;
                break;
            default:
                nodeImage.color = normalColor;
                break;
        }
    }
}

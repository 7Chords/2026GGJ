using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameCore.UI;
using SCFrame;
using SCFrame.UI;
using GameCore;

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
    
    public ERoomType NodeType { get; private set; }
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

    public void SetMapNodeType(ERoomType type)
    {
        NodeType = type;
        UpdateVisuals();
    }

    [Header("Visuals")]
    [SerializeField]
    public string nodeImageName; // Image path/name for loading sprite

    private void UpdateVisuals()
    {
        if (nodeImage == null) nodeImage = GetComponent<Image>();
        if (nodeImage == null) return;
        
        switch (NodeType)
        {
            case ERoomType.ENEMY:
                //nodeImage.color = enemyColor;
                nodeImageName = "spr_icon_node_enemy";
                break;
            case ERoomType.ELITE:
                nodeImage.color = eliteColor;
                break;
            case ERoomType.REST:
                nodeImage.color = restColor;
                break;
            case ERoomType.SHOP:
                //nodeImage.color = shopColor;
                nodeImageName = "spr_icon_node_shop";
                break;
            case ERoomType.TREASURE:
                nodeImage.color = treasureColor;
                break;
            case ERoomType.BOSS:
                //nodeImage.color = bossColor;
                nodeImageName = "spr_icon_node_boss";
                break;
            default:
                nodeImage.color = normalColor;
                break;
        }
        
        // Try to load sprite if name is provided
        if (!string.IsNullOrEmpty(nodeImageName))
        {
             Sprite sp = ResourcesHelper.LoadAsset<Sprite>(nodeImageName);
             if (sp != null)
             {
                 nodeImage.sprite = sp;
                 nodeImage.color = Color.white; // Reset color if using sprite
                 return;
             }
        }

        
    }
}

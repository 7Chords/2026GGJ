using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum RoomType
{
    Enemy,
    Elite,
    Rest,
    Treasure,
    Shop,
    Event,
    Boss,
    None
}


public class MapGenerate : MonoBehaviour
{
    [Header("Reference")]
    //地图房间预制体
    [SerializeField]
    private GameObject nodePrefab;
    //房间连接线预制体
    [SerializeField]
    private GameObject linePrefab;
    //地图节点父容器 -- 调整位置大小以契合scroll view
    [SerializeField]
    private RectTransform mapNodeParentRect;
    //地图节点Size
    [SerializeField]
    private Vector2Int nodeSize;
    //地图节点随机偏移量
    [SerializeField]
    private Vector2Int nodeOffset;
    //地图节点角度随机偏移量
    [SerializeField]
    private int nodeAnglesOffset;
    [SerializeField]
    private Vector2Int padding;
    
    [Header("Map Node Database")]
    [SerializeField]
    private MapData mapData;
    
    private Vector2Int _layerCount;
    private MapNode[,] _mapNodeArray;

    private System.Random _mapRandom;
    
    private void Initialize()
    {
        //从种子管理器获取随机数生成器
        _mapRandom = RandomUtility.GetRandomGenerator(ModuleType.Map);
        
        //从地图数据中获取层数
        if (mapData == null)
        {
            Debug.LogError("MapData is missing! Please assign it in the inspector.");
            return;
        }
        _layerCount = mapData.layerCount;
        
        //初始化地图节点数组
        _mapNodeArray = new MapNode[_layerCount.x, _layerCount.y];
    }

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        // 初始化地图生成器
        Initialize();
        
        // 创建地图房间
        CreateMap();
        
        // 随机生成路线
        GenerateRouteLoop();
        
        // 删除未相连的房间
        DeleteInactiveNode();
        
        // 设置房间连接线可视化
        SetNodeLineVisual();
        
        MapManager.instance.SetMapData(_mapNodeArray);
    }

    #region 设置地图视图

    private void CreateMap()
    {
        // 1. 调整 Content 大小以适应地图
        // 高度 = padding上下 + (层数-1) * 间距
        float totalHeight =  padding.y + (_layerCount.x-1) * nodeSize.y;
        // 宽度 = padding左右 + (宽度-1) * 间距 (预估最大宽度)
        float totalWidth =  padding.x + (_layerCount.y- 1) * nodeSize.x;
        
        mapNodeParentRect.sizeDelta = new Vector2(totalWidth, totalHeight);

        // 2. 生成节点
        // 计算起始Y (底部)
        float startY = -totalHeight / 2 + padding.y;
        if (mapNodeParentRect.pivot.y == 0) startY = padding.y; // Pivot在底部
        
        for (var i = 0; i < _mapNodeArray.GetLength(0); i++) // Layers (Vertical)
        {
            // X轴居中计算
            // 这一层的总宽度
            float layerWidth = (_layerCount.y - 1) * nodeSize.x;
            float startX = -layerWidth / 2; // 从左侧开始

            for (var j = 0; j < _mapNodeArray.GetLength(1); j++) // Nodes in Layer (Horizontal)
            {
                var node = Instantiate(nodePrefab, mapNodeParentRect).GetComponent<MapNode>();
                node.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-nodeAnglesOffset, nodeAnglesOffset));
                _mapNodeArray[i, j] = node;

                node.SetMapNodeIndex(i, j);

                // 计算随机偏移
                // X轴偏移 (左右微调)
                float offsetX = Random.Range(-nodeOffset.x, nodeOffset.x);
                // Y轴偏移 (上下微调)
                float offsetY = Random.Range(-nodeOffset.y, nodeOffset.y);

                if (i == 0 || i == _layerCount.x - 1)
                {
                   // 首尾层不偏移Y，保证对齐
                   offsetY = 0;
                }

                // 设置位置
                float finalX = startX + j * nodeSize.x + offsetX;
                float finalY = startY + i * nodeSize.y + offsetY;
                
                node.transform.localPosition = new Vector3(finalX, finalY, 0);
                node.gameObject.SetActive(true);
            }
        }
    }

    #endregion

    #region 设置房间连接

    private void GenerateRouteLoop()
    {
        var originRoomList = new List<int>();
        var repetitionCount = mapData.repetitionCount;
        
        // 强制起点为中间节点
        int centerIndex = _layerCount.y / 2;
        
        for (var i = 0; i < repetitionCount; i++)
        {
            // 之前的随机起点逻辑屏蔽，改为固定起点
            /*
            var originRoom = _mapRandom.Next(0, repetitionCount);
            if (i == 1)
            {
                var safetyCounter = 0;
                while (originRoom == originRoomList[0] && safetyCounter < 100)
                {
                    originRoom = _mapRandom.Next(0, repetitionCount);
                    safetyCounter++;
                }

                if (safetyCounter >= 100) originRoom = originRoomList[0];
            }
            */
            
            var originRoom = centerIndex;

            originRoomList.Add(originRoom);
            SetRoute(originRoom);
        }
    }

    private void SetRoute(int originRoom)
    {
        var currentRoom = originRoom;
        
        var currentRoomType = RoomType.None;
        
        int centerIndex = _layerCount.y / 2;

        for (var i = 0; i < _mapNodeArray.GetLength(0); i++)
        {
            var currentNode = _mapNodeArray[i, currentRoom];
            currentNode.isActive = true;

            var previousRoomType = currentRoomType;
            currentRoomType = SetRoomType(i, previousRoomType);

            // 设置新的房间类型
            currentNode.SetMapNodeType(currentRoomType);

            // 如果已经是最后一层，不需要设置下一个连接点
            if (i == _mapNodeArray.Length - 1)
                break;
                
            // === 特殊处理：如果是倒数第二层，强制指向最后一层的中间节点 ===
            if (i == _mapNodeArray.GetLength(0) - 2)
            {
                var nextRoomIndex = centerIndex;
                var nextLayerNodes = currentNode.nextLayerConnectedNodes;
                if (!nextLayerNodes.Contains(nextRoomIndex)) nextLayerNodes.Add(nextRoomIndex);
                currentRoom = nextRoomIndex;
                continue; // 跳过常规随机逻辑
            }

            var minIndex = 0;
            var maxIndex = _layerCount.y - 1;

            // 检查前一层节点约束 (只在i>0时有效)
            if (currentRoom > 0)
            {
                var previousNode = _mapNodeArray[i, currentRoom - 1];
                if (previousNode.nextLayerConnectedNodes.Count > 0)
                    minIndex = previousNode.nextLayerConnectedNodes.Max();
            }

            // 检查下一层节点约束
            if (currentRoom < _layerCount.y - 1) // 确保下一层和下下层都存在
            {
                var nextNode = _mapNodeArray[i, currentRoom + 1];
                if (nextNode.nextLayerConnectedNodes.Count > 0) maxIndex = nextNode.nextLayerConnectedNodes.Min();
            }

            minIndex = Mathf.Max(minIndex, currentRoom - 1);
            maxIndex = Mathf.Min(maxIndex, currentRoom + 1);

            var nextRoomIndexRnd = _mapRandom.Next(minIndex, maxIndex + 1);

            var nextLayerConnectedNodes = currentNode.nextLayerConnectedNodes;
            if (!nextLayerConnectedNodes.Contains(nextRoomIndexRnd)) nextLayerConnectedNodes.Add(nextRoomIndexRnd);

            currentRoom = nextRoomIndexRnd;
        }
    }

    private void DeleteInactiveNode()
    {
        foreach (var node in _mapNodeArray)
        {
            if (!node) continue;
            node.gameObject.SetActive(node.isActive);
        }
    }

    private void SetNodeLineVisual()
    {
        for (var i = 0; i < _mapNodeArray.GetLength(0) - 1; i++)
        {
            for (var j = 0; j < _mapNodeArray.GetLength(1); j++)
            {
                var node = _mapNodeArray[i, j];
                Vector2 startPosition = node.transform.localPosition;
                if (node.nextLayerConnectedNodes.Count <= 0) continue;

                var connectedNodes = node.nextLayerConnectedNodes;
                foreach (var t in connectedNodes)
                {
                    var connectedNode = _mapNodeArray[i + 1, t];
                    Vector2 endPosition = connectedNode.transform.localPosition;

                    var lineInstance = Instantiate(linePrefab, mapNodeParentRect);
                    lineInstance.SetActive(true);

                    SetNodeLinePosition(lineInstance, startPosition, endPosition);
                }
            }
        }
    }

    private void SetNodeLinePosition(GameObject lineInstance, Vector2 startPosition, Vector2 endPosition)
    {
        var rectTransform = lineInstance.GetComponent<RectTransform>();
        rectTransform.localPosition = (startPosition + endPosition) / 2;

        var size = rectTransform.sizeDelta;
        size.x = Vector2.Distance(startPosition, endPosition) - 70;
        rectTransform.sizeDelta = size;

        var direction = endPosition - startPosition;
        rectTransform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    #endregion

    #region 随机房间类型

    private RoomType SetRoomType(int layerIndex, RoomType previousRoomType)
    {
        switch (layerIndex)
        {
            // 1. 处理固定类型层级
            case 0:
                return RoomType.Enemy; // 首位是战斗节点
            //case 7:
            //    return RoomType.Treasure;
        }

        if (layerIndex == _layerCount.x - 1) return RoomType.Boss; // 末位是战斗节点 (原为Rest)

        // 2. 确定当前层级的限制条件
        var excludedTypes = new List<RoomType> { RoomType.None };
        
        // 屏蔽精英和休息点
        excludedTypes.Add(RoomType.Elite);
        excludedTypes.Add(RoomType.Rest);

        var avoidDuplicates = previousRoomType != RoomType.Enemy && previousRoomType != RoomType.Event;

        /*
        switch (layerIndex)
        {
            case 1:
                excludedTypes.AddRange(new[] { RoomType.Shop, RoomType.Elite, RoomType.Rest });
                break;
            // 2-5层不要精英、休息点
            case > 1 and < 6:
                excludedTypes.AddRange(new[] { RoomType.Elite, RoomType.Rest });
                break;
            default:
            {
                if (layerIndex == _layerCount.x - 2)
                    excludedTypes.Add(RoomType.Rest);
                break;
            }
        }
        */

        // 3. 生成符合条件的节点类型
        return GetValidNodeType(previousRoomType, excludedTypes, avoidDuplicates);
    }

    private RoomType GetValidNodeType(RoomType previousType, List<RoomType> excludedTypes,
        bool avoidDuplicates)
    {
        const int maxAttempts = 100;
        var attempts = 0;
        RoomType nodeType;

        do
        {
            nodeType = mapData.GetRandomMapNodeType(_mapRandom).NodeType;
            attempts++;

            // 如果尝试次数过多，返回Enemy作为默认类型
            if (attempts >= maxAttempts) return RoomType.Enemy;

            // 检查是否符合条件：不在排除列表中，且不重复（如需要）
        } while (excludedTypes.Contains(nodeType) ||
                 (avoidDuplicates && nodeType == previousType));

        return nodeType;
    }

    #endregion
}

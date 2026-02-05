using SCFrame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    public class MapGenerator : MonoBehaviour
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
        private Vector2Int nodeSpacing;
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

        private void Start()
        {
            GenerateMap();
        }
        private void Initialize()
        {
            //从种子管理器获取随机数生成器
            _mapRandom = RandomUtility.GetRandomGenerator(EModuleType.MAP);

            //从地图数据中获取层数
            if (mapData == null)
            {
                SCDebugHelper.LogError("MapData is missing! Please assign it in the inspector.");
                return;
            }
            _layerCount = mapData.layerCount;

            //初始化地图节点数组
            _mapNodeArray = new MapNode[_layerCount.x, _layerCount.y];
        }


        public void GenerateMap()
        {
            // 初始化地图生成器
            Initialize();

            // 创建地图房间
            createMap();

            // 随机生成路线
            generateRouteLoop();

            // 删除未相连的房间
            DeleteInactiveNode();

            // 设置房间连接线可视化
            SetNodeLineVisual();

            MapManager.instance.SetMapData(_mapNodeArray);
        }

        #region 设置地图视图

        /// <summary>
        /// 创建地图 根据data里的层数生成所有节点 此时是一个矩形节点阵
        /// </summary>
        private void createMap()
        {
            //调整 Content 大小以适应地图 (Horizontal Layout)

            //rectTransform的sizedelta和锚点有关 只有设置为中央模式时以下代码才正确
            //参考：https://blog.csdn.net/zcaixzy5211314/article/details/86839636
            // Width = padding左右 + (LayerCount - 1) * NodeSpacingX
            float totalWidth = padding.x + (_layerCount.x - 1) * nodeSpacing.x;
            // Height = padding上下 + (MaxNodesPerLayer - 1) * NodeSpacingY
            float totalHeight = padding.y + (_layerCount.y - 1) * nodeSpacing.y;

            mapNodeParentRect.sizeDelta = new Vector2(totalWidth, totalHeight);

            //生成节点
            //第一个节点x坐标
            float startX = -totalWidth / 2 + padding.x;

            for (var i = 0; i < _mapNodeArray.GetLength(0); i++) //有多少层
            {
                // Y Axis Centering for this layer
                // This layer's height logic (assuming full width for calculation)
                float layerHeight = (_layerCount.y - 1) * nodeSpacing.y;
                float startY = -layerHeight / 2; // Center Vertically

                for (var j = 0; j < _mapNodeArray.GetLength(1); j++) //每层有多少个
                {
                    var node = SCCommon.InstantiateGameObject(nodePrefab, mapNodeParentRect).GetComponent<MapNode>();
                    node.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-nodeAnglesOffset, nodeAnglesOffset));
                    _mapNodeArray[i, j] = node;

                    node.SetMapNodeIndex(i, j);

                    // Calculate Random Offset
                    // X axis offset (Depth jitter)
                    float offsetX = Random.Range(-nodeOffset.x, nodeOffset.x);
                    // Y axis offset (Height jitter)
                    float offsetY = Random.Range(-nodeOffset.y, nodeOffset.y);

                    if (i == 0 || i == _layerCount.x - 1)
                    {
                        // Align start/end layers perfectly on X
                        offsetX = 0;
                    }

                    // Set Position (Horizontal Layout)
                    // X depends on Layer (i)
                    // Y depends on Row (j)
                    float finalX = startX + i * nodeSpacing.x + offsetX;
                    float finalY = startY + j * nodeSpacing.y + offsetY;

                    node.transform.localPosition = new Vector3(finalX, finalY, 0);
                    SCCommon.SetGameObjectEnable(node.gameObject, true);
                }
            }
        }

        #endregion

        #region 设置房间连接

        private void generateRouteLoop()
        {
            List<int> originRoomList = new List<int>();
            int repetitionCount = mapData.repetitionCount;

            // 强制起点为中间节点
            int centerIndex = _layerCount.y / 2;

            for (var i = 0; i < repetitionCount; i++)
            {
                int originRoom = centerIndex;

                originRoomList.Add(originRoom);
                SetRoute(originRoom);
            }
        }

        private void SetRoute(int originRoom)
        {
            int currentRoom = originRoom;

            ERoomType currentRoomType = ERoomType.NONE;

            int centerIndex = _layerCount.y / 2;

            for (var i = 0; i < _mapNodeArray.GetLength(0); i++)//有多少层就循环多少次
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
                if (currentRoom < _layerCount.y - 1)
                {
                    var nextNode = _mapNodeArray[i, currentRoom + 1];
                    if (nextNode.nextLayerConnectedNodes.Count > 0) maxIndex = nextNode.nextLayerConnectedNodes.Min();
                }

                //路线只能 “逐步移动”（下一个房间只能在当前房间 ±1 范围内），避免路线跳变
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

                        GameObject lineInstance = SCCommon.InstantiateGameObject(linePrefab, mapNodeParentRect);
                        SCCommon.SetGameObjectEnable(lineInstance,true);
                        lineInstance.transform.SetAsFirstSibling();//设置为第一个元素 防止图层在node前面
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

        private ERoomType SetRoomType(int layerIndex, ERoomType previousRoomType)
        {

            // 第一层的节点固定是战斗节点 且只有一个节点
            if (layerIndex == 0) return ERoomType.ENEMY;
            // 最后一层的节点固定是BOSS节点 且只有一个节点
            if (layerIndex == _layerCount.x - 1) return ERoomType.BOSS;

            // 确定当前层级的限制条件
            // 被排除的节点类型
            var excludedTypes = new List<ERoomType> { ERoomType.NONE };

            // 屏蔽精英和休息点
            excludedTypes.Add(ERoomType.ELITE);
            excludedTypes.Add(ERoomType.REST);

            //如果上一个节点是战斗节点或者事件节点，则不允许重复
            //表示不会有同一层里不会有相邻的战斗节点或相邻的事件节点
            var avoidDuplicates = previousRoomType != ERoomType.ENEMY && previousRoomType != ERoomType.EVENT;


            //可以根据需求添加逻辑
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

            // 生成符合条件的节点类型
            return GetValidNodeType(previousRoomType, excludedTypes, avoidDuplicates);
        }

        private ERoomType GetValidNodeType(ERoomType previousType, List<ERoomType> excludedTypes,
            bool avoidDuplicates)
        {
            const int maxAttempts = 100;
            int attempts = 0;
            ERoomType nodeType;

            do
            {
                nodeType = mapData.GetRandomMapNodeType(_mapRandom).nodeType;
                attempts++;

                // 如果尝试次数过多，返回Enemy作为默认类型
                if (attempts >= maxAttempts) return ERoomType.ENEMY;

                // 检查是否符合条件：不在排除列表中，且不重复（如需要）
            } while (excludedTypes.Contains(nodeType) ||
                     (avoidDuplicates && nodeType == previousType));

            return nodeType;
        }

        #endregion
    }
}

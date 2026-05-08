using SCFrame;
using GameCore.RefData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameCore
{
    public class MapGenerator : SingletonMono<MapGenerator>
    {
        [Header("Reference")]
        //地图房间预制体
        [SerializeField]
        private GameObject nodePrefab;
        //房间连接线预制体
        [SerializeField]
        private GameObject linePrefab;
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

        [Header("地图配置（Addressables）")]
        [Tooltip("为 true 时：优先用「地址覆盖」，否则从 map 配表按当前楼层取 mapCfgName 作为 Addressables 地址加载 MapData")]
        [SerializeField]
        private bool loadMapDataFromTable = true;
        [Tooltip("可选：直接填 Addressables 地址/标签，将跳过配表（用于测试或单图固定配置）")]
        [SerializeField]
        private string mapDataAddressOverride;

        /// <summary> 运行时从 Addressables 加载，不再在 Inspector 拖拽 MapData。 </summary>
        private MapData _mapData;

        private Vector2Int _layerCount;
        private MapCellLayoutData[,] _layoutData;
        private Vector2 _pendingContentSize;

        private System.Random _mapRandom;


        private void OnDestroy()
        {
            if (ReferenceEquals(instance, this))
                instance = null;
        }

        /// <summary>
        /// 取得场景中的 MapGenerator（含未激活物体上的组件）。Start 只会在物体激活后执行一次，不能依赖 Start 做新游戏重随机。
        /// </summary>
        public static MapGenerator GetOrFind()
        {
            if (instance != null)
                return instance;

            // FindObjectOfType 默认不包含未激活物体；Resources.FindObjectsOfTypeAll 可找到（过滤掉非场景里的资源）
            var all = Resources.FindObjectsOfTypeAll<MapGenerator>();
            foreach (var m in all)
            {
                if (m == null)
                    continue;
                var go = m.gameObject;
                if (go != null && go.scene.IsValid() && go.scene.isLoaded)
                {
                    instance = m;
                    return instance;
                }
            }

            return null;
        }

        /// <summary> 打开地图 UI 时：若有待实例化数据则生成物体；否则先生成纯数据再实例化。 </summary>
        public void EnsureMapGeneratedIfNeeded(RectTransform mapContentParent)
        {
            if (MapManager.instance == null)
                return;

            var grid = MapManager.instance.currentMapNodes;
            if (grid != null && grid.GetLength(0) > 0 && grid.GetLength(1) > 0)
            {
                var probe = grid[0, 0];
                if (probe != null)
                    return;
            }

            if (grid != null)
                MapManager.instance.ClearCurrentMapNodes();

            if (mapContentParent == null)
            {
                SCDebugHelper.LogError("[MapGenerator] 地图 ScrollRect Content 为空，无法生成节点。");
                return;
            }

            if (!MapManager.instance.HasPendingLayout)
            {
                var gm = GameModel.instance;
                int? fixedSeed = gm != null ? gm.PendingRunMapLayoutSeed : (int?)null;
                if (!fixedSeed.HasValue && MapManager.instance.LastMapLayoutSeed >= 0)
                    fixedSeed = MapManager.instance.LastMapLayoutSeed;
                // Continue safety: Pending seed can be cleared after data-gen; MapManager seed can be lost if lifecycle resets.
                // Fall back to the run seed persisted on GameModel so layout remains stable after returning to main menu.
                if (!fixedSeed.HasValue && gm != null && gm.RunMapLayoutSeed >= 0)
                    fixedSeed = gm.RunMapLayoutSeed;
                GenerateMapDataOnly(fixedSeed);
            }

            SpawnMapVisuals(mapContentParent);
        }

        /// <summary> 仅生成格子与路线的纯数据，不依赖任何 UI 父节点；结果保存在 MapManager，供之后 SpawnMapVisuals。 </summary>
        /// <param name="fixedMapSeed"> 非空时使用该种子复现布局（继续游戏）；为空时新随机一局。 </param>
        public void GenerateMapDataOnly(int? fixedMapSeed = null)
        {
            int usedSeed;
            if (fixedMapSeed.HasValue)
            {
                usedSeed = fixedMapSeed.Value;
                RandomUtility.ReseedModule(EModuleType.MAP, usedSeed);
            }
            else
                usedSeed = ReseedMapRandomAndReturnSeed();

            MapManager.instance?.SetLastMapLayoutSeed(usedSeed);
            GameModel.instance?.SetRunMapLayoutSeed(usedSeed);

            InitializeData();
            if (_mapData == null || _layoutData == null)
                return;

            createMapLayoutData();
            generateRouteLoop();
            StackActiveNodesVerticallyPerColumn();
            RecenterLayoutOnActiveCluster();
            if (!_mapData.useLegacyInteriorRoomRandom)
                ApplyInteriorRoomTypeQuotasFromConfiguredWeights();

            var lines = CollectLineSegments();
            if (MapManager.instance == null)
                return;
            MapManager.instance.SetPendingMapLayout(_layoutData, _pendingContentSize, lines);
            _layoutData = null;

            // Persist seed as soon as layout data exists so continue matches even if map UI never finishes
            // (e.g. quit during transition) or a later save path is skipped.
            if (GameModel.instance != null && GameModel.instance.playerInfo != null)
            {
                GameRunSave.NotifyEnteredMapOnce();
                GameRunSave.SaveFromGameModel();
            }

            if (fixedMapSeed.HasValue)
                GameModel.instance?.ClearPendingRunMapLayoutSeed();
        }

        /// <summary> 根据 MapManager 中暂存的纯数据，在地图 Content 下实例化节点与连线。 </summary>
        public void SpawnMapVisuals(RectTransform mapContentParent)
        {
            if (mapContentParent == null || MapManager.instance == null)
                return;
            if (!MapManager.instance.HasPendingLayout)
                return;

            var pending = MapManager.instance.PendingLayout;
            var lines = MapManager.instance.PendingLines;
            mapContentParent.sizeDelta = MapManager.instance.PendingContentSize;

            for (int c = mapContentParent.childCount - 1; c >= 0; c--)
                SCCommon.DestoryGameObject(mapContentParent.GetChild(c).gameObject);

            int w = pending.GetLength(0);
            int h = pending.GetLength(1);
            var mapNodes = new MapNode[w, h];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var cell = pending[i, j];
                    var node = SCCommon.InstantiateGameObject(nodePrefab, mapContentParent).GetComponent<MapNode>();
                    node.ApplyFromLayout(cell);
                    mapNodes[i, j] = node;
                }
            }

            if (lines != null && linePrefab != null)
            {
                foreach (var seg in lines)
                {
                    GameObject lineInstance = SCCommon.InstantiateGameObject(linePrefab, mapContentParent);
                    SCCommon.SetGameObjectEnable(lineInstance, true);
                    lineInstance.transform.SetAsFirstSibling();
                    SetNodeLinePosition(lineInstance, seg.start, seg.end);
                }
            }

            MapManager.instance.SetMapData(mapNodes);
            MapManager.instance.ClearPendingLayout();
        }

        /// <summary>
        /// 地图使用固定全局种子时每次开局路线相同；新开局前重新播种，使路线与节点抖动都不同。
        /// </summary>
        static int ReseedMapRandomAndReturnSeed()
        {
            unchecked
            {
                int seed = (int)(DateTime.UtcNow.Ticks ^ Guid.NewGuid().GetHashCode());
                RandomUtility.ReseedModule(EModuleType.MAP, seed);
                return seed;
            }
        }

        /// <summary>
        /// 按配表 mapCfgName 或覆盖地址，通过 Addressables 加载 MapData。
        /// </summary>
        private MapData LoadMapDataAsset()
        {
            string address = mapDataAddressOverride;
            if (string.IsNullOrEmpty(address) && loadMapDataFromTable)
            {
                int floor = 1;
                if (GameModel.instance != null && GameModel.instance.playerInfo != null)
                    floor = GameModel.instance.playerInfo.playerFloor;

                var list = SCRefDataMgr.instance?.mapRefList?.refDataList;
                if (list == null || list.Count == 0)
                {
                    SCDebugHelper.LogError("[MapGenerator] map 配表未加载或为空。");
                    return null;
                }

                MapRefObj mapRow = list.Find(m => m.floor == floor);
                if (mapRow == null)
                {
                    SCDebugHelper.LogError($"[MapGenerator] map 配表中找不到 floor={floor} 的行。");
                    return null;
                }

                if (string.IsNullOrEmpty(mapRow.mapCfgName))
                {
                    SCDebugHelper.LogError($"[MapGenerator] map 配表 floor={floor} 的 mapCfgName 为空。");
                    return null;
                }

                address = mapRow.mapCfgName;
            }

            if (string.IsNullOrEmpty(address))
            {
                SCDebugHelper.LogError("[MapGenerator] MapData 的 Addressables 地址为空。请填 mapDataAddressOverride 或配表 mapCfgName。");
                return null;
            }

            var data = ResourcesHelper.LoadAsset<MapData>(address);
            if (data == null)
                SCDebugHelper.LogError($"[MapGenerator] Addressables 加载 MapData 失败，请检查地址与分组：\"{address}\"");
            return data;
        }

        private void InitializeData()
        {
            //从种子管理器获取随机数生成器
            _mapRandom = RandomUtility.GetRandomGenerator(EModuleType.MAP);

            _mapData = LoadMapDataAsset();
            if (_mapData == null)
            {
                SCDebugHelper.LogError("[MapGenerator] MapData 加载失败，无法生成地图。");
                _layoutData = null;
                return;
            }
            _layerCount = _mapData.layerCount;

            _layoutData = new MapCellLayoutData[_layerCount.x, _layerCount.y];
        }

        #region 设置地图视图

        /// <summary>
        /// 仅计算格子布局与随机偏移，不实例化物体。
        /// </summary>
        private void createMapLayoutData()
        {
            float totalWidth = padding.x + (_layerCount.x - 1) * nodeSpacing.x;
            float totalHeight = padding.y + (_layerCount.y - 1) * nodeSpacing.y;
            _pendingContentSize = new Vector2(totalWidth, totalHeight);

            float startX = -totalWidth / 2 + padding.x;

            for (var i = 0; i < _layoutData.GetLength(0); i++)
            {
                float layerHeight = (_layerCount.y - 1) * nodeSpacing.y;
                float startY = -layerHeight / 2;

                for (var j = 0; j < _layoutData.GetLength(1); j++)
                {
                    var cell = new MapCellLayoutData
                    {
                        gridX = i,
                        gridY = j,
                        isActive = false
                    };

                    float angleZ = _mapRandom.Next(-nodeAnglesOffset, nodeAnglesOffset + 1);
                    cell.angleZ = angleZ;

                    float offsetX = NextFloatInclusive(-nodeOffset.x, nodeOffset.x);
                    float offsetY = NextFloatInclusive(-nodeOffset.y, nodeOffset.y);

                    if (i == 0 || i == _layerCount.x - 1)
                        offsetX = 0;

                    float finalX = startX + i * nodeSpacing.x + offsetX;
                    float finalY = startY + j * nodeSpacing.y + offsetY;
                    cell.localPosition = new Vector3(finalX, finalY, 0);

                    _layoutData[i, j] = cell;
                }
            }
        }

        /// <summary>
        /// After routes are chosen, snap each column's active nodes to a vertical stack centered on Y=0 (before global
        /// recenter), so edges are not all biased to one screen direction while grid indices and connections stay the same.
        /// </summary>
        private void StackActiveNodesVerticallyPerColumn()
        {
            if (_layoutData == null)
                return;

            int w = _layoutData.GetLength(0);
            int h = _layoutData.GetLength(1);
            float layerHeight = (_layerCount.y - 1) * nodeSpacing.y;
            float gridBaseY = -layerHeight * 0.5f;

            for (int i = 0; i < w; i++)
            {
                var activeRows = new List<int>();
                for (int j = 0; j < h; j++)
                {
                    var c = _layoutData[i, j];
                    if (c != null && c.isActive)
                        activeRows.Add(j);
                }

                activeRows.Sort();
                int k = activeRows.Count;
                if (k == 0)
                    continue;

                for (int r = 0; r < k; r++)
                {
                    int j = activeRows[r];
                    var cell = _layoutData[i, j];
                    float slotY = (r - (k - 1) * 0.5f) * nodeSpacing.y;
                    float jitterY = cell.localPosition.y - (gridBaseY + j * nodeSpacing.y);
                    float x = cell.localPosition.x;
                    float z = cell.localPosition.z;
                    cell.localPosition = new Vector3(x, slotY + jitterY, z);
                }
            }
        }

        /// <summary>
        /// Inactive nodes are hidden but the grid still spans all rows; ScrollRect centers the full rect so the path
        /// looks shifted. Shift all cells so the active-route bounding box is centered and tighten content size to it.
        /// </summary>
        private void RecenterLayoutOnActiveCluster()
        {
            if (_layoutData == null)
                return;

            int w = _layoutData.GetLength(0);
            int h = _layoutData.GetLength(1);
            bool any = false;
            float minX = 0f, maxX = 0f, minY = 0f, maxY = 0f;

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var cell = _layoutData[i, j];
                    if (cell == null || !cell.isActive)
                        continue;
                    Vector3 p = cell.localPosition;
                    if (!any)
                    {
                        minX = maxX = p.x;
                        minY = maxY = p.y;
                        any = true;
                    }
                    else
                    {
                        if (p.x < minX) minX = p.x;
                        if (p.x > maxX) maxX = p.x;
                        if (p.y < minY) minY = p.y;
                        if (p.y > maxY) maxY = p.y;
                    }
                }
            }

            if (!any)
                return;

            float cx = (minX + maxX) * 0.5f;
            float cy = (minY + maxY) * 0.5f;
            var offset = new Vector3(cx, cy, 0f);

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var cell = _layoutData[i, j];
                    if (cell != null)
                        cell.localPosition -= offset;
                }
            }

            float spanX = maxX - minX;
            float spanY = maxY - minY;
            const float minSpan = 1f;
            if (spanX < minSpan) spanX = minSpan;
            if (spanY < minSpan) spanY = minSpan;

            _pendingContentSize = new Vector2(spanX + padding.x, spanY + padding.y);
        }

        private float NextFloatInclusive(float min, float max)
        {
            return Mathf.Lerp(min, max, (float)_mapRandom.NextDouble());
        }

        #endregion

        #region 设置房间连接

        private void generateRouteLoop()
        {
            List<int> originRoomList = new List<int>();
            int repetitionCount = _mapData.repetitionCount;

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

            int layerCountX = _layoutData.GetLength(0);

            for (var i = 0; i < layerCountX; i++)
            {
                var currentCell = _layoutData[i, currentRoom];
                currentCell.isActive = true;

                var previousRoomType = currentRoomType;
                currentRoomType = SetRoomType(i, previousRoomType);

                currentCell.roomType = currentRoomType;

                if (i == layerCountX - 1)
                    break;

                if (i == layerCountX - 2)
                {
                    var nextRoomIndex = centerIndex;
                    var nextLayerNodes = currentCell.nextLayerConnectedNodes;
                    if (!nextLayerNodes.Contains(nextRoomIndex)) nextLayerNodes.Add(nextRoomIndex);
                    currentRoom = nextRoomIndex;
                    continue;
                }

                var minIndex = 0;
                var maxIndex = _layerCount.y - 1;

                if (currentRoom > 0)
                {
                    var previousCell = _layoutData[i, currentRoom - 1];
                    if (previousCell.nextLayerConnectedNodes.Count > 0)
                        minIndex = previousCell.nextLayerConnectedNodes.Max();
                }

                if (currentRoom < _layerCount.y - 1)
                {
                    var nextCell = _layoutData[i, currentRoom + 1];
                    if (nextCell.nextLayerConnectedNodes.Count > 0) maxIndex = nextCell.nextLayerConnectedNodes.Min();
                }

                minIndex = Mathf.Max(minIndex, currentRoom - 1);
                maxIndex = Mathf.Min(maxIndex, currentRoom + 1);

                var nextRoomIndexRnd = _mapRandom.Next(minIndex, maxIndex + 1);

                var nextLayerConnectedNodes = currentCell.nextLayerConnectedNodes;
                if (!nextLayerConnectedNodes.Contains(nextRoomIndexRnd)) nextLayerConnectedNodes.Add(nextRoomIndexRnd);

                currentRoom = nextRoomIndexRnd;
            }
        }

        private List<MapLineSegment> CollectLineSegments()
        {
            var list = new List<MapLineSegment>();
            for (var i = 0; i < _layoutData.GetLength(0) - 1; i++)
            {
                for (var j = 0; j < _layoutData.GetLength(1); j++)
                {
                    var cell = _layoutData[i, j];
                    Vector2 startPosition = new Vector2(cell.localPosition.x, cell.localPosition.y);
                    if (cell.nextLayerConnectedNodes.Count <= 0) continue;

                    foreach (var t in cell.nextLayerConnectedNodes)
                    {
                        var endCell = _layoutData[i + 1, t];
                        Vector2 endPosition = new Vector2(endCell.localPosition.x, endCell.localPosition.y);
                        list.Add(new MapLineSegment { start = startPosition, end = endPosition });
                    }
                }
            }
            return list;
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

        /// <summary>
        /// Reassign interior layer room types so counts match <see cref="MapData.roomProbabilities"/> weights
        /// among <b>visible</b> nodes only (isActive, excluding first/last column which stay fixed types).
        /// </summary>
        private void ApplyInteriorRoomTypeQuotasFromConfiguredWeights()
        {
            if (_layoutData == null || _mapData == null || _mapRandom == null)
                return;

            var probs = _mapData.roomProbabilities;
            if (probs == null || probs.Count == 0)
                return;

            // Important: keep iteration deterministic across runs/platforms.
            // Dictionary iteration order is not stable, which can change room type assignment even with same seed.
            var mergedWeight = new Dictionary<ERoomType, int>();
            for (int i = 0; i < probs.Count; i++)
            {
                var rp = probs[i];
                if (rp == null || rp.weight <= 0)
                    continue;
                if (!mergedWeight.ContainsKey(rp.type))
                    mergedWeight[rp.type] = 0;
                mergedWeight[rp.type] += rp.weight;
            }

            if (mergedWeight.Count == 0)
                return;

            int totalW = 0;
            foreach (var w in mergedWeight.Values)
                totalW += w;
            if (totalW <= 0)
                return;

            var interiorCells = new List<MapCellLayoutData>();
            for (int i = 1; i < _layerCount.x - 1; i++)
            {
                if (_mapData.usePenultimateNodeRoomType && _layerCount.x >= 3 && i == _layerCount.x - 2)
                    continue;
                for (int j = 0; j < _layerCount.y; j++)
                {
                    var cell = _layoutData[i, j];
                    if (cell != null && cell.isActive)
                        interiorCells.Add(cell);
                }
            }

            int n = interiorCells.Count;
            if (n <= 0)
                return;

            var quota = new Dictionary<ERoomType, int>();
            var fracOrder = new List<(ERoomType type, float frac)>();
            int sumFloor = 0;

            var mergedKeys = new List<ERoomType>(mergedWeight.Keys);
            mergedKeys.Sort((a, b) => ((int)a).CompareTo((int)b));
            for (int idx = 0; idx < mergedKeys.Count; idx++)
            {
                var key = mergedKeys[idx];
                int w = mergedWeight[key];
                float exact = (float)n * w / totalW;
                int fl = Mathf.FloorToInt(exact);
                quota[key] = fl;
                sumFloor += fl;
                fracOrder.Add((key, exact - fl));
            }

            int deficit = n - sumFloor;
            // Deterministic tie-break on type when fractional parts match.
            fracOrder.Sort((a, b) =>
            {
                int c = b.frac.CompareTo(a.frac);
                return c != 0 ? c : ((int)a.type).CompareTo((int)b.type);
            });
            for (int k = 0; k < deficit; k++)
            {
                var t = fracOrder[k % fracOrder.Count].type;
                quota[t] = quota[t] + 1;
            }

            var pool = new List<ERoomType>(n);
            var quotaKeys = new List<ERoomType>(quota.Keys);
            quotaKeys.Sort((a, b) => ((int)a).CompareTo((int)b));
            for (int q = 0; q < quotaKeys.Count; q++)
            {
                var type = quotaKeys[q];
                int count = quota[type];
                for (int c = 0; c < count; c++)
                    pool.Add(type);
            }

            while (pool.Count < n)
                pool.Add(fracOrder[0].type);
            while (pool.Count > n)
                pool.RemoveAt(pool.Count - 1);

            for (int k = pool.Count - 1; k > 0; k--)
            {
                int j = _mapRandom.Next(k + 1);
                var tmp = pool[k];
                pool[k] = pool[j];
                pool[j] = tmp;
            }

            for (int k = interiorCells.Count - 1; k > 0; k--)
            {
                int j = _mapRandom.Next(k + 1);
                var tmp = interiorCells[k];
                interiorCells[k] = interiorCells[j];
                interiorCells[j] = tmp;
            }

            for (int i = 0; i < n; i++)
                interiorCells[i].roomType = pool[i];
        }

        private ERoomType SetRoomType(int layerIndex, ERoomType previousRoomType)
        {

            // 第一层 / 最后一层类型由 MapData 配置（默认战斗 / BOSS）
            if (layerIndex == 0) return _mapData.firstNodeRoomType;
            if (layerIndex == _layerCount.x - 1) return _mapData.lastNodeRoomType;
            if (_mapData.usePenultimateNodeRoomType && _layerCount.x >= 3 && layerIndex == _layerCount.x - 2)
                return _mapData.penultimateNodeRoomType;

            // 确定当前层级的限制条件
            // 被排除的节点类型
            var excludedTypes = new List<ERoomType> { ERoomType.NONE };

            // 屏蔽精英和休息点
            //excludedTypes.Add(ERoomType.ELITE);
            //excludedTypes.Add(ERoomType.REST);

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
                nodeType = _mapData.GetRandomMapNodeType(_mapRandom);
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

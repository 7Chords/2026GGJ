using UnityEngine;

namespace GameCore.Data
{
    /// <summary>
    /// 单格部位在敌人脸上的摆放数据（对应 part_level 表的一条记录）。
    /// </summary>
    [System.Serializable]
    public class EnemyLayoutSlot
    {
        [Tooltip("part_level 表 id，需与生成敌人部位时使用的 PartLevelRefObj.id 一致")]
        public long partLevelRefId;

        [Tooltip("该部位锚点（形状归一化后的原点）落在脸部网格上的坐标")]
        public Vector2Int originFacePosition;

        [Tooltip("顺时针旋转次数 0~3")]
        [Range(0, 3)]
        public int rotationSteps;
    }
}

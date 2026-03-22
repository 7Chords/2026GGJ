using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Data
{
    /// <summary>
    /// 某一「大回合」结束后敌人脸部应使用的布局（与 DealNextTurn 中一次生成对应）。
    /// 第一场战斗开战时使用第 0 个元素。
    /// </summary>
    [System.Serializable]
    public class EnemyTurnFaceLayout
    {
        [Tooltip("本回合要摆到脸上的部位（顺序影响匹配同 partLevelRefId 的多张牌时）")]
        public List<EnemyLayoutSlot> slots = new List<EnemyLayoutSlot>();
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Data
{
    /// <summary>
    /// 某一敌人的脸部布局预设：按「大回合」索引配置每回合脸上的部位与摆放。
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyEncounterLayout", menuName = "Game/Enemy Layout Preset", order = 0)]
    public class EnemyLayoutPreset : ScriptableObject
    {
        [Tooltip("索引 0 = 本场战斗开始；索引 1 = 第一次 DealNextTurn 后；以此类推。超出长度时沿用最后一项")]
        public List<EnemyTurnFaceLayout> turnLayouts = new List<EnemyTurnFaceLayout>();
    }
}

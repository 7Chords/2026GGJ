using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIMonoBattleOrder : _ASCUIMonoBase
    {
        [Header("先手顺序文本")]
        public Text txtOrder;
        [Header("是玩家先手展示的物体")]
        public List<GameObject> goIsPlayerFirstShow;
        [Header("是敌人先手展示的物体")]
        public List<GameObject> goIsEnemyFirstShow;
        [Header("展示时长")]
        public float showDuration;
    }
}

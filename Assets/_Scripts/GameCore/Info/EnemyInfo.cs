using GameCore.RefData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class EnemyInfo
    {
        public EnemyRefObj enemyRefObj;
        public List<PartInfo> battleParts;
        public List<PartInfo> deckParts;
        public int maxHealth;
        public int currentHealth;

        public EnemyInfo(EnemyRefObj _enemyRefObj)
        {
            if (_enemyRefObj == null)
                return;
            enemyRefObj = _enemyRefObj;
            battleParts = new List<PartInfo>();
            deckParts = new List<PartInfo>();
            maxHealth = _enemyRefObj.enemyHealth;
            currentHealth = maxHealth;
        }
    }
}

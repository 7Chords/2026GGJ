using GameCore.RefData;
using System.Collections.Generic;

namespace GameCore
{
    public class EnemyInfo
    {
        public EnemyRefObj enemyRefObj;
        public List<PartInfo> battlePartInfoList;
        public List<PartInfo> busyPartInfoList;
        public List<PartInfo> deckPartInfoList;
        public int maxHealth;
        public int currentHealth;

        public EnemyInfo(EnemyRefObj _enemyRefObj)
        {
            if (_enemyRefObj == null)
                return;
            enemyRefObj = _enemyRefObj;
            battlePartInfoList = new List<PartInfo>();
            busyPartInfoList = new List<PartInfo>();
            deckPartInfoList = new List<PartInfo>();
            maxHealth = _enemyRefObj.enemyHealth;
            currentHealth = maxHealth;
        }
    }
}

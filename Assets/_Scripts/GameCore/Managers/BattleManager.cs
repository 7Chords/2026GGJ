using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameCore
{
    public class BattleManager : Singleton<BattleManager>
    {

        public void StartBattle()
        {
            PartInfo partInfo = null;
            for(int i =0;i<GameModel.instance.battlePartInfoList.Count;i++)
            {
                partInfo = GameModel.instance.battlePartInfoList[i];
            }
        }

        public void EnterNextTurn()
        {
            GameModel.instance.GenerateNewBattle();
        }
    }
}

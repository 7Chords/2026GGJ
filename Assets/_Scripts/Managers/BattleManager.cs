using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameCore
{
    public class BattleManager : Singleton<BattleManager>
    {
        public void EnterBattle()
        {
            EnterNextTurn();
        }

        public void ExitBattle()
        {

        }
        public void EnterNextTurn()
        {
            GameModel.instance.PrepareNextBattleRound();
            SCMsgCenter.SendMsg(SCMsgConst.SET_ENEMY_FACE);
        }
    }
}

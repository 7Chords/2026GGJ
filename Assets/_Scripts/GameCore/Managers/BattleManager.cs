using DG.Tweening;
using GameCore.UI;
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
            DOVirtual.DelayedCall(2f, () =>
            {
                GameModel.instance.curTurnOwner = ETurnOwnerType.PLAYER;
                TriggerParts(true);
                ChangeTurnOwner();
                TriggerParts(false);
                FinishBattle();
            });
        }
        public void TriggerParts(bool _isPlayer)
        {

            PartInfo partInfo = null;
            if (_isPlayer)
            {
                for (int i = 0; i < GameModel.instance.playerInfo.battlePartInfoList.Count; i++)
                {
                    partInfo = GameModel.instance.playerInfo.battlePartInfoList[i];
                    if (partInfo == null)
                        continue;
                    partInfo.TriggerActiveLogic(EAttributeTriggerPointType.ACTIVE);
                }
            }
            else
            {
                for (int i = 0; i < GameModel.instance.curEnemyInfo.battlePartInfoList.Count; i++)
                {
                    partInfo = GameModel.instance.curEnemyInfo.battlePartInfoList[i];
                    if (partInfo == null)
                        continue;
                    partInfo.TriggerActiveLogic(EAttributeTriggerPointType.ACTIVE);
                }
            }
        }
        public void ChangeTurnOwner()
        {
            GameModel.instance.curTurnOwner = GameModel.instance.curTurnOwner == ETurnOwnerType.PLAYER 
                ? ETurnOwnerType.ENEMY 
                : ETurnOwnerType.PLAYER;
        }

        public void FinishBattle()
        {
            GameModel.instance.DealNextTurn();
            UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCFrame.UI.SCUIShowType.FULL));
        }
    }
}

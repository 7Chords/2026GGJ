using SCFrame;

namespace GameCore.Battle.Effects
{
    /// <summary>
    /// CLEAR_BAD_SKIN: remove debuff skin parts (quality NONE, type SKIN) among allies in effect range.
    /// </summary>
    public class ClearBadSkinEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            for (int i = 0; i < partInfoList.Count; i++)
            {
                var part = partInfoList[i];
                if (part == null || part.partRefObj == null)
                    continue;
                if (part.partRefObj.partType != EPartType.SKIN)
                    continue;
                if (part.partRefObj.qualityType != EQualityType.NONE)
                    continue;

                SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, part, _caster);
                RemovePartFromBattleFace(part);
            }
        }

        private static void RemovePartFromBattleFace(PartInfo _part)
        {
            if (_part == null)
                return;

            SCMsgCenter.SendMsg(SCMsgConst.PART_DIE, _part);

            if (_part.isEnemyPart)
            {
                GameModel.instance.curEnemyInfo?.battlePartInfoList.Remove(_part);
                ClearEnemyFaceGrids(_part.curOccupyFacePosList);
                BattleManager.instance.RemovePartFromList(false, _part);
                SCMsgCenter.SendMsg(SCMsgConst.BATTLE_ENEMY_PART_ORDER_CHG);
            }
            else
            {
                GameModel.instance.playerInfo?.battlePartInfoList.Remove(_part);
                GameModel.instance.SetGridsEmpty(_part.curOccupyFacePosList);
                BattleManager.instance.RemovePartFromList(true, _part);
                SCMsgCenter.SendMsg(SCMsgConst.BATTLE_PLAYER_PART_ORDER_CHG);
            }

            _part.ClearOnFaceState();
        }

        private static void ClearEnemyFaceGrids(System.Collections.Generic.List<UnityEngine.Vector2Int> _posList)
        {
            if (_posList == null || GameModel.instance.enemyFaceGridInfoList == null)
                return;
            for (int i = 0; i < _posList.Count; i++)
            {
                var g = GameModel.instance.enemyFaceGridInfoList.Find(x => x.pos == _posList[i]);
                g?.SetEmpty();
            }
        }
    }
}

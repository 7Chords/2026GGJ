using GameCore;
using SCFrame;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class ChangeBreedingMass2OtherEffectHandler : IPartEffectHandler
    {
        const int BreedingCap = 20;

        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            if (_caster == null || !_caster.isOnFace) return;
            var breeding = _caster.GetBuff(EBuffType.BREEDING_MASS);
            if (breeding == null || breeding.buffLayer <= BreedingCap) return;
            int excess = breeding.buffLayer - BreedingCap;
            if (excess <= 0) return;

            var allyGrid = _caster.isEnemyPart
                ? GameModel.instance.enemyFaceGridInfoList
                : GameModel.instance.playerFaceGridInfoList;
            var inArea = GameModel.CollectPartsInEffectArea(_caster, allyGrid);
            bool hasLowBreedingAlly = false;
            for (int i = 0; i < inArea.Count; i++)
            {
                var p = inArea[i];
                if (p == null) continue;
                var b = p.GetBuff(EBuffType.BREEDING_MASS);
                int bl = b != null ? b.buffLayer : 0;
                if (bl <= BreedingCap)
                {
                    hasLowBreedingAlly = true;
                    break;
                }
            }

            breeding.ReduceBuffLayer(excess);
            if (breeding.buffLayer == 0)
                _caster.buffLogic.RemoveBuff(breeding);
            else
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, breeding);

            EBuffType addType = hasLowBreedingAlly ? EBuffType.HEAL_MASS : EBuffType.ATTACK_MASS;
            var addBuff = BuffFactory.CreateBuffInfoByType(addType, excess, _caster, _caster);
            if (addBuff != null)
                _caster.AddBuff(addBuff);
        }
    }
}

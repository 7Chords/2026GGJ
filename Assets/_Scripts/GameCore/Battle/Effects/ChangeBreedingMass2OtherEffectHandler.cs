using GameCore;
using SCFrame;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class ChangeBreedingMass2OtherEffectHandler : IPartEffectHandler
    {
        const int DefaultBreedingCap = 20;

        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            if (_caster == null || !_caster.isOnFace) return;
            int cap = ResolveCap(_entry);
            var breeding = _caster.GetBuff(EBuffType.BREEDING_MASS);
            if (breeding == null || breeding.buffLayer <= cap) return;
            int toTransfer = breeding.buffLayer - cap;
            if (toTransfer <= 0) return;

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
                if (bl <= cap)
                {
                    hasLowBreedingAlly = true;
                    break;
                }
            }

            // Remove from breeding first so layer count always matches what we add to the other colony.
            breeding.ReduceBuffLayer(toTransfer);
            if (breeding.buffLayer == 0)
                _caster.buffLogic.RemoveBuff(breeding);
            else
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, breeding);

            EBuffType addType = hasLowBreedingAlly ? EBuffType.HEAL_MASS : EBuffType.ATTACK_MASS;
            var addBuff = BuffFactory.CreateBuffInfoByType(addType, toTransfer, _caster, _caster);
            if (addBuff != null)
                _caster.AddBuff(addBuff);
        }

        static int ResolveCap(EntryInfo _entry)
        {
            if (_entry?.attributeValueList == null || _entry.attributeValueList.Count < 1)
                return DefaultBreedingCap;
            int c = Mathf.RoundToInt(_entry.attributeValueList[0]);
            return Mathf.Max(1, c);
        }
    }
}

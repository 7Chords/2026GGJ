using SCFrame;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Battle
{
    public class BuffLogic
    {
        public List<BuffInfo> buffList = new List<BuffInfo>();

        /// <summary>
        /// buff的效果周期和生命周期计时(回合制）
        /// </summary>
        public void BuffTurnTick()
        {
            List<BuffInfo> deleteBuffList = new List<BuffInfo>();
            foreach (var buffInfo in buffList)
            {
                buffInfo.buffLayer--;

                if (buffInfo.buffLayer == 0)
                {
                    deleteBuffList.Add(buffInfo);
                }
                else
                {
                    SCDebugHelper.LogWarning(buffInfo.buffRefObj.buffName + ":" + buffInfo.buffLayer);
                }
            }

            foreach (var buffInfo in deleteBuffList)
            {
                RemoveBuff(buffInfo);
            }
        }

        /// <summary>
        /// 添加buff
        /// </summary>
        /// <param name="_buffInfo"></param>
        public void AddBuff(BuffInfo _buffInfo)
        {
            if (_buffInfo == null) return;
            BuffInfo findBuffInfo = FindBuff(_buffInfo.buffRefObj.id);

            if (findBuffInfo != null)
            {
                findBuffInfo.AddBuffLayer(_buffInfo.buffLayer);
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, findBuffInfo);
            }
            else
            {
                buffList.Add(_buffInfo);
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_ADD, _buffInfo);
            }

            // 添加后做一次 buff 之间的交互处理（如油脂->燃烧）
            PostProcessAfterBuffAdded(_buffInfo);
        }

        private void PostProcessAfterBuffAdded(BuffInfo _addedBuff)
        {
            if (_addedBuff == null) return;

            // 规则：部位存在燃烧时，添加油脂会自动转化：每 2 层油脂 => 1 层燃烧
            // 这里按“总油脂层数”计算，保证多次添加也能连续转化。
            if (_addedBuff.buffType != EBuffType.FAT) return;
            if (_addedBuff.owner == null) return;

            var burn = FindBuff(EBuffType.BURN);
            if (burn == null || burn.buffLayer <= 0) return;

            var fat = FindBuff(EBuffType.FAT);
            if (fat == null || fat.buffLayer <= 0) return;

            int convert = fat.buffLayer / 2;
            if (convert <= 0) return;

            // 先扣油脂
            fat.ReduceBuffLayer(convert * 2);
            if (fat.buffLayer <= 0)
                RemoveBuff(fat);
            else
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, fat);

            // 再加燃烧（燃烧来源/创建者沿用“加油脂的人/部位”更合理；这里用 _addedBuff.creator）
            burn.AddBuffLayer(convert);
            SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, burn);
        }

        /// <summary>
        /// 移除buff
        /// </summary>
        /// <param name="_buffInfo"></param>
        public void RemoveBuff(BuffInfo _buffInfo)
        {
            if (!buffList.Contains(_buffInfo))
                return;

            buffList.Remove(_buffInfo);

            SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_REMOVE, _buffInfo);

        }

        public void ReduceBuffLayer(long _id,int _reduceLayer)
        {
            if (_id < 0 || _reduceLayer <= 0)
                return;
            BuffInfo buffInfo = buffList.Find(x => x.buffRefObj.id == _id);
            if (buffInfo == null)
                return;
            buffInfo.buffLayer = Mathf.Max(0, buffInfo.buffLayer - _reduceLayer);
            if(buffInfo.buffLayer == 0)
            {
                RemoveBuff(buffInfo);
            }
            else
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, buffInfo);
            }

        }

        public void ReduceAllBuffLayer(int _reduceLayer)
        {
            if (_reduceLayer <= 0)
                return;

            for(int i =0;i<buffList.Count;i++)
            {
                buffList[i].buffLayer = Mathf.Max(0, buffList[i].buffLayer - _reduceLayer);
                if (buffList[i].buffLayer == 0)
                {
                    RemoveBuff(buffList[i]);
                }
                else
                {
                    SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, buffList[i]);
                }
            }

        }

        public void ClearAllBuffs()
        {
            if (buffList == null)
                return;
            List<BuffInfo> deleteInfoList = new List<BuffInfo>();
            foreach (BuffInfo buffInfo in buffList)
            {
                deleteInfoList.Add(buffInfo);
            }
            buffList.Clear();
            foreach (BuffInfo buffInfo in deleteInfoList)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_REMOVE, buffInfo);
            }
        }

        public void ClearBuff(EBuffType _buffType)
        {
            BuffInfo info = buffList.Find(x => x.buffType == _buffType);
            if (info == null)
                return;
            buffList.Remove(info);
            SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_REMOVE, info);
        }

        /// <summary>
        /// 查找列表中的buff
        /// </summary>
        /// <param name="_buffDataID"></param>
        /// <returns></returns>
        public BuffInfo FindBuff(long _buffDataID)
        {
            foreach (var buffInfo in buffList)
            {
                if (buffInfo.buffRefObj.id == _buffDataID)
                {
                    return buffInfo;
                }
            }

            return default;
        }

        public BuffInfo FindBuff(EBuffType _buffType)
        {
            foreach (var buffInfo in buffList)
            {
                if (buffInfo.buffType == _buffType)
                {
                    return buffInfo;
                }
            }

            return default;
        }

        public bool HasFindByTriggerPointType(EAttributeTriggerPointType _triggerPointType)
        {
            return buffList.Find(x=>x.buffRefObj.triggerPointType == _triggerPointType) != null;
        }
        public void TriggerPartBuff(EAttributeTriggerPointType _triggerPointType)
        {
            List<BuffInfo> removeBuffList = new List<BuffInfo>();
            foreach (var buffInfo in buffList)
            {
                if (buffInfo == null || buffInfo.buffRefObj == null)
                    continue;
                if (buffInfo.buffRefObj.triggerPointType != _triggerPointType)
                    continue;

                switch (_triggerPointType)
                {
                    case EAttributeTriggerPointType.ACTIVE:
                        buffInfo.onPartActive?.Invoke();
                        break;
                    case EAttributeTriggerPointType.GET_HIT:
                        buffInfo.onPartGetHit?.Invoke();
                        break;
                    case EAttributeTriggerPointType.DIE:
                        buffInfo.onPartDie?.Invoke();
                        break;
                    case EAttributeTriggerPointType.GET_EFFECT:
                        buffInfo.onPartGetEffect?.Invoke();
                        break;
                    case EAttributeTriggerPointType.TURN_OVER:
                        buffInfo.onTurnOver?.Invoke();
                        break;
                    case EAttributeTriggerPointType.ACTION_OVER:
                        buffInfo.onPartActionOver?.Invoke();
                        break;
                }

                bool canConsume = buffInfo.owner != null && buffInfo.owner.isOnFace;
                if (!canConsume)
                    continue;

                //触发层数
                int triggerLayer = 1;
                switch(buffInfo.buffType)
                {
                    case EBuffType.BURN:
                        {
                            triggerLayer = buffInfo.buffLayer;
                        }
                        break;
                    default:
                        break;
                }
                buffInfo.ReduceBuffLayer(triggerLayer);
                if (buffInfo.buffLayer == 0)
                    removeBuffList.Add(buffInfo);
                else
                    SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, buffInfo);
            }
            for(int i =0;i<removeBuffList.Count;i++)
            {
                RemoveBuff(removeBuffList[i]);
            }
        }
    }
}

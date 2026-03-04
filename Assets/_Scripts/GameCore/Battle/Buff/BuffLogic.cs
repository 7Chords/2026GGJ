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
                findBuffInfo.buffLayer += findBuffInfo.buffLayer;
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, _buffInfo);
            }
            else
            {
                buffList.Add(_buffInfo);
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_ADD, _buffInfo);
            }
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
            switch(_triggerPointType)
            {
                case EAttributeTriggerPointType.ACTIVE:
                    {
                        
                        foreach (var buffInfo in buffList)
                        {
                            if (buffInfo == null)
                                continue;
                            if (buffInfo.buffRefObj.triggerPointType != EAttributeTriggerPointType.ACTIVE)
                                return;
                            buffInfo.onPartActive?.Invoke();
                            buffInfo.ReduceBuffLayer();
                            if(buffInfo.buffLayer == 0)
                            {
                                removeBuffList.Add(buffInfo);
                            }
                            else
                                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, buffInfo);
                        }
                    }
                    break;
                case EAttributeTriggerPointType.GET_HIT:
                    {
                        foreach (var buffInfo in buffList)
                        {
                            if (buffInfo == null)
                                continue;
                            buffInfo.onPartGetHit?.Invoke();
                        }
                    }
                    break;
                case EAttributeTriggerPointType.DIE:
                    {
                        foreach (var buffInfo in buffList)
                        {
                            if (buffInfo == null)
                                continue;
                            buffInfo.onPartDie?.Invoke();
                        }
                    }
                    break;
                case EAttributeTriggerPointType.GET_EFFECT:
                    {
                        foreach (var buffInfo in buffList)
                        {
                            if (buffInfo == null)
                                continue;
                            buffInfo.onPartGetEffect?.Invoke();
                        }
                    }
                    break;
                case EAttributeTriggerPointType.TURN_OVER:
                    {
                        foreach (var buffInfo in buffList)
                        {
                            if (buffInfo == null)
                                continue;
                            buffInfo.onTurnOver?.Invoke();
                        }
                    }
                    break;
                case EAttributeTriggerPointType.ACTION_OVER:
                    {

                    }
                    break;
                default:
                    break;
            }
            for(int i =0;i<removeBuffList.Count;i++)
            {
                RemoveBuff(removeBuffList[i]);
            }
        }
    }
}

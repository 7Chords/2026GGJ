using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// ��Ϸ��ʼ��
    /// </summary>
    public class GameInit : SingletonPersistent<GameInit>
    {
        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Discard();
        }

        public override void OnInitialize()
        {
            SCRefDataMgr.instance.Initialize();
            SCTaskHelper.instance.Initialize();
            SCMsgCenter.instance.Initialize();
            SCPoolMgr.instance.Initialize();
            SCInputListener.instance.Initialize();
            UICoreMgr.instance.Initialize();
            GameModel.instance.Initialize();
            MapManager.instance.Initialize();
        }

        public override void OnDiscard()
        {
            
            UICoreMgr.instance.Discard();
            SCInputListener.instance.Discard();
            SCPoolMgr.instance.Discard();
            SCMsgCenter.instance.Discard();
            SCTaskHelper.instance.Discard();
            SCRefDataMgr.instance.Discard();
            GameModel.instance.Discard();
            MapManager.instance.Discard();
        }
    }
}

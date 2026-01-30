using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// ”Œœ∑≥ı ºªØ
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

            GameModel.instance.Initialize();
            SCRefDataMgr.instance.Initialize();
            SCTaskHelper.instance.Initialize();
            SCMsgCenter.instance.Initialize();
            SCPoolMgr.instance.Initialize();
            SCInputListener.instance.Initialize();
            UICoreMgr.instance.Initialize();
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


        }
    }
}

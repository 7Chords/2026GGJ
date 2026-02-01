using GameCore.UI;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            AudioMgr.instance.Initialize();
            startGame();
        }

        public override void OnDiscard()
        {
            AudioMgr.instance.Discard();
            UICoreMgr.instance.Discard();
            SCInputListener.instance.Discard();
            SCPoolMgr.instance.Discard();
            SCMsgCenter.instance.Discard();
            SCTaskHelper.instance.Discard();
            SCRefDataMgr.instance.Discard();
            GameModel.instance.Discard();
            MapManager.instance.Discard();
        }

        private void startGame()
        {
            if(SceneManager.GetActiveScene().name == "Release")
            {
                UICoreMgr.instance.AddNode(new UINodeStart(SCFrame.UI.SCUIShowType.FULL));
                AudioMgr.instance.PlayBgm("game_music");
            }
        }
    }
}

using GameCore.RuntimeDebug;
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

        private void OnApplicationPause(bool _pause)
        {
            if (_pause)
                GameRunSave.SaveFromGameModel();
        }

        public override void OnInitialize()
        {
            SCRefDataMgr.instance.Initialize();
            LanguageHelper.instance.Initialize();
            SCTimeCaller.instance.Initialize();
            SCTaskHelper.instance.Initialize();
            SCMsgCenter.instance.Initialize();
            SCPoolMgr.instance.Initialize();
            SCInputListener.instance.Initialize();
            SCSettingMgr.instance.Initialize();
            UICoreMgr.instance.Initialize();
            GameModel.instance.Initialize();
            MapManager.instance.Initialize();
            AudioMgr.instance.Initialize();
            BattleManager.instance.Initialize();
            RuntimeDebug.CheatDebugRuntimePanel.AttachIfNeeded(gameObject);
            startGame();
        }

        public override void OnDiscard()
        {
            BattleManager.instance.Discard();
            AudioMgr.instance.Discard();
            MapManager.instance.Discard();
            UICoreMgr.instance.Discard();
            SCInputListener.instance.Discard();
            SCPoolMgr.instance.Discard();
            SCMsgCenter.instance.Discard();
            SCTaskHelper.instance.Discard();
            LanguageHelper.instance.Discard();

            SCRefDataMgr.instance.Discard();
            SCTimeCaller.instance.Discard();

            GameModel.instance.Discard();
        }

        private void startGame()
        {
            if(SceneManager.GetActiveScene().name == "Release")
            {
                UICoreMgr.instance.AddNode(new UINodeStart(SCFrame.UI.SCUIShowType.FULL));
                AudioMgr.instance.PlayBgm("bgm_main_music");
            }
        }
    }
}

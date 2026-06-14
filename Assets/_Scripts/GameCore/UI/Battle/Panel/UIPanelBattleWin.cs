using DG.Tweening;
using GameCore;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class UIPanelBattleWin : _ASCUIPanelBase<UIMonoBattleWin>
    {
        private EnemyRefObj _m_enemyRefObj;
        private Tween _m_moneyTween;

        public UIPanelBattleWin(UIMonoBattleWin _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
        }

        public override void BeforeDiscard()
        {
            _m_moneyTween?.Kill(false);
            _m_moneyTween = null;
        }

        public override void OnHidePanel()
        {
            mono.btnGoto.onClick.RemoveAllListeners();
            _m_moneyTween?.Kill(false);
            _m_moneyTween = null;
        }

        public override void OnShowPanel()
        {
            mono.btnGoto.onClick.AddListener(() =>
            {
                AudioMgr.instance.PlaySfx("sfx_click");
                TVSwitchTransition.Run(() =>
                {
                    GameModel.instance.playerInfo.ApplyPendingMapMove();

                    bool bossWin = GameModel.instance.LastWinEnemyWasBoss;
                    int winFloor = GameModel.instance.LastWinPlayerFloor;
                    bool finalBoss = bossWin && winFloor >= GameConst.RUN_TOTAL_FLOORS;

                    GameModel.instance.ClearEnemyWinSnapshot();

                    UICoreMgr.instance.RemoveAllNodes(SCUINodeFuncType.BATTLE);

                    if (finalBoss)
                    {
                        GameRunSave.SaveFromGameModel();
                        UICoreMgr.instance.AddNode(new UINodeWin(SCUIShowType.FULL));
                    }
                    else if (bossWin)
                    {
                        AudioMgr.instance.PlayBgm("bgm_main_music");
                        GameModel.instance.AdvanceToNextRunFloorAndResetMap();
                        GameRunSave.SaveFromGameModel();
                        UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
                    }
                    else
                    {
                        AudioMgr.instance.PlayBgm("bgm_main_music");
                        UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
                    }
                });
            });

            long winEnemyId = GameModel.instance.LastWinEnemyRefId;
            _m_enemyRefObj = winEnemyId != 0
                ? SCRefDataMgr.instance.enemyRefList.refDataList.Find(x => x.id == winEnemyId)
                : null;

            refreshMoneyReward();
        }

        private void refreshMoneyReward()
        {
            if (_m_enemyRefObj == null)
            {
                Debug.LogWarning("UIPanelBattleWin: missing enemy ref for win reward.");
                if (mono.txtMoney != null)
                    mono.txtMoney.text = "0";
                return;
            }

            int targetMoney = _m_enemyRefObj.winMoney;
            GameModel.instance.playerInfo.playerMoney += targetMoney;

            _m_moneyTween?.Kill(false);
            _m_moneyTween = null;
            if (mono.txtMoney == null)
                return;

            mono.txtMoney.text = "0";
            float dur = Mathf.Max(0f, mono.moneyCountUpDuration);
            if (dur <= 0.0001f)
            {
                mono.txtMoney.text = targetMoney.ToString();
                return;
            }

            int cur = 0;
            _m_moneyTween = DOTween.To(() => cur, v =>
            {
                cur = v;
                mono.txtMoney.text = cur.ToString();
            }, targetMoney, dur).SetEase(Ease.OutQuad);
        }
    }
}

using GameCore;
using GameCore.RefData;
using SCFrame;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RuntimeDebug
{
    /// <summary>
    /// Runtime cheat window (IMGUI). Editor: always. Player: Development Build only.
    /// Toggle with F9.
    /// </summary>
    public sealed class CheatDebugRuntimePanel : MonoBehaviour
    {
        private const int WindowId = 92001;
        private static readonly KeyCode ToggleKey = KeyCode.F9;

        private bool _visible;
        private Rect _winRect = new Rect(24f, 24f, 320f, 520f);
        private Vector2 _scroll;
        private string _cheatAddPartIdText = "101001";
        private string _cheatAddPartLevelText = "1";

        public static void AttachIfNeeded(GameObject host)
        {
            if (host == null)
                return;
            if (host.GetComponent<CheatDebugRuntimePanel>() != null)
                return;
            host.AddComponent<CheatDebugRuntimePanel>();
        }

        private static bool IsCheatRuntimeAllowed =>
#if UNITY_EDITOR
            true;
#else
            Debug.isDebugBuild;
#endif

        private void Awake()
        {
            if (!IsCheatRuntimeAllowed)
            {
                Destroy(this);
                return;
            }
        }

        private void Update()
        {
            if (!IsCheatRuntimeAllowed)
                return;
            if (Input.GetKeyDown(ToggleKey))
                _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!IsCheatRuntimeAllowed || !_visible)
                return;

            _winRect = GUILayout.Window(WindowId, _winRect, DrawWindow, "Cheat / 调试");
        }

        private void DrawWindow(int id)
        {
            _scroll = GUILayout.BeginScrollView(_scroll);
            GUILayout.Label($"Toggle: {ToggleKey}");

            if (GUILayout.Button("回满角色血量"))
            {
                CheatFullPlayerBodyHp();
                NotifyUiRefresh();
            }

            if (GUILayout.Button("获得所有部位（缺啥补啥进背包）"))
            {
                CheatGrantAllMissingPartsToBag();
                NotifyUiRefresh();
            }

            if (GUILayout.Button("+9999 金币"))
            {
                CheatAddGold(9999);
                NotifyUiRefresh();
            }

            if (GUILayout.Button("回满所有己方部位血量"))
            {
                CheatFullAllPlayerPartsHp();
                NotifyUiRefresh();
            }

            if (GameModel.instance != null && GameModel.instance.curEnemyInfo != null)
            {
                if (GUILayout.Button("回满当前敌人本体血量"))
                {
                    CheatFullEnemyBodyHp();
                    NotifyUiRefresh();
                }

                if (GUILayout.Button("当前敌人本体血量变为 1"))
                {
                    CheatEnemyBodyHpToOne();
                    NotifyUiRefresh();
                }
            }

            GUILayout.Space(6f);
            GUILayout.Label("部位 id / 等级 → 手牌");
            GUILayout.BeginHorizontal();
            GUILayout.Label("id", GUILayout.Width(22f));
            _cheatAddPartIdText = GUILayout.TextField(_cheatAddPartIdText, GUILayout.MinWidth(80f));
            GUILayout.Label("lv", GUILayout.Width(18f));
            _cheatAddPartLevelText = GUILayout.TextField(_cheatAddPartLevelText, GUILayout.Width(28f));
            GUILayout.EndHorizontal();
            if (GUILayout.Button("添加部位到手牌"))
            {
                CheatAddPartToPlayerHand(_cheatAddPartIdText, _cheatAddPartLevelText);
                NotifyUiRefresh();
            }

            GUILayout.EndScrollView();
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 24f));
        }

        private static void NotifyUiRefresh()
        {
            SCMsgCenter.SendMsg(SCMsgConst.CHEAT_DEBUG_UI_REFRESH);
        }

        private static void CheatFullPlayerBodyHp()
        {
            var gm = GameModel.instance;
            if (gm?.playerInfo == null)
                return;
            gm.playerInfo.currentHealth = gm.playerInfo.maxHealth;
            SCMsgCenter.SendMsg(SCMsgConst.PLAYER_HEAL);
        }

        private static void CheatFullEnemyBodyHp()
        {
            var gm = GameModel.instance;
            if (gm?.curEnemyInfo == null)
                return;
            gm.curEnemyInfo.currentHealth = gm.curEnemyInfo.maxHealth;
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_HEAL);
        }

        private static void CheatEnemyBodyHpToOne()
        {
            var gm = GameModel.instance;
            if (gm?.curEnemyInfo == null)
                return;
            if (gm.curEnemyInfo.maxHealth <= 0)
                return;
            gm.curEnemyInfo.currentHealth = 1;
            SCMsgCenter.SendMsg(SCMsgConst.ENEMY_HEAL);
        }

        private static void CheatAddPartToPlayerHand(string partIdText, string levelText)
        {
            var gm = GameModel.instance;
            if (gm?.playerInfo?.busyPartInfoList == null)
                return;

            string idTrim = partIdText != null ? partIdText.Trim() : string.Empty;
            if (!long.TryParse(idTrim, out long partId))
            {
                Debug.LogWarning("[Cheat] Invalid part id.");
                return;
            }

            int level = 1;
            if (levelText != null && int.TryParse(levelText.Trim(), out int parsed) && parsed >= 1)
                level = parsed;

            PartRefObj partRef = FindPartRefById(partId);
            if (partRef == null)
            {
                Debug.LogWarning($"[Cheat] partRef not found: {partId}");
                return;
            }

            PartLevelRefObj levelRow = FindPartLevelRefObj(partId, level) ?? FindLowestLevelRowForPart(partId);
            if (levelRow == null)
            {
                Debug.LogWarning($"[Cheat] no part_level row for partId={partId}");
                return;
            }

            if (gm.playerInfo.busyPartInfoList.Count >= GameConst.BUSY_CARD_MAX_COUNT)
            {
                Debug.LogWarning($"[Cheat] hand full (max {GameConst.BUSY_CARD_MAX_COUNT}).");
                return;
            }

            var info = new PartInfo(partRef, false, levelRow.partLevel);
            if (info.levelRefObj == null)
                return;
            gm.playerInfo.busyPartInfoList.Add(info);
        }

        private static PartRefObj FindPartRefById(long partId)
        {
            var list = SCRefDataMgr.instance?.partRefList?.refDataList;
            if (list == null)
                return null;
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                if (p != null && p.id == partId)
                    return p;
            }
            return null;
        }

        private static PartLevelRefObj FindPartLevelRefObj(long partId, int partLevel)
        {
            var rows = SCRefDataMgr.instance?.partLevelRefList?.refDataList;
            if (rows == null)
                return null;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row != null && row.partId == partId && row.partLevel == partLevel)
                    return row;
            }
            return null;
        }

        private static void CheatAddGold(int amount)
        {
            var gm = GameModel.instance;
            if (gm?.playerInfo == null || amount <= 0)
                return;
            gm.playerInfo.playerMoney += amount;
        }

        private static void CheatFullAllPlayerPartsHp()
        {
            var gm = GameModel.instance;
            if (gm?.playerInfo == null)
                return;
            var p = gm.playerInfo;
            var lists = new List<PartInfo>[]
            {
                p.bagPartInfoList,
                p.deckPartInfoList,
                p.busyPartInfoList,
                p.battlePartInfoList
            };
            for (int i = 0; i < lists.Length; i++)
            {
                var list = lists[i];
                if (list == null)
                    continue;
                for (int j = 0; j < list.Count; j++)
                {
                    var part = list[j];
                    if (part == null)
                        continue;
                    int need = part.maxHealth - part.currentHealth;
                    if (need > 0)
                        gm.PartHeal(part, need);
                }
            }
        }

        private static PartLevelRefObj FindLowestLevelRowForPart(long partId)
        {
            var rows = SCRefDataMgr.instance?.partLevelRefList?.refDataList;
            if (rows == null)
                return null;
            PartLevelRefObj best = null;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row == null || row.partId != partId)
                    continue;
                if (best == null || row.partLevel < best.partLevel)
                    best = row;
            }
            return best;
        }

        private static bool PlayerOwnsPartId(long partId)
        {
            var p = GameModel.instance?.playerInfo;
            if (p == null)
                return false;
            var lists = new List<PartInfo>[]
            {
                p.bagPartInfoList,
                p.deckPartInfoList,
                p.busyPartInfoList,
                p.battlePartInfoList
            };
            for (int i = 0; i < lists.Length; i++)
            {
                var list = lists[i];
                if (list == null)
                    continue;
                for (int j = 0; j < list.Count; j++)
                {
                    var part = list[j];
                    if (part?.partRefObj != null && part.partRefObj.id == partId)
                        return true;
                }
            }
            return false;
        }

        private static void CheatGrantAllMissingPartsToBag()
        {
            var gm = GameModel.instance;
            var parts = SCRefDataMgr.instance?.partRefList?.refDataList;
            if (gm?.playerInfo?.bagPartInfoList == null || parts == null)
                return;

            for (int i = 0; i < parts.Count; i++)
            {
                var partRef = parts[i];
                if (partRef == null)
                    continue;
                if (PlayerOwnsPartId(partRef.id))
                    continue;
                var levelRow = FindLowestLevelRowForPart(partRef.id);
                if (levelRow == null)
                    continue;
                var info = new PartInfo(partRef, false, levelRow.partLevel);
                if (info.levelRefObj == null)
                    continue;
                gm.playerInfo.bagPartInfoList.Add(info);
            }
        }
    }
}

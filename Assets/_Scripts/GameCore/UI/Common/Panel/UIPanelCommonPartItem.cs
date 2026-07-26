using DG.Tweening;
using GameCore;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    /// <summary>
    /// Common part item panel.
    /// </summary>
    public class UIPanelCommonPartItem : _ASCUIPanelBase<UIMonoCommonPartItem>
    {
        private PartInfo _m_sourcePartInfo;
        private PartInfo _m_partInfo;
        private TweenContainer _m_tweenContainer;
        private bool _m_levelPreviewCycleEnabled;
        private bool _m_isHovering;
        private readonly List<int> _m_availableLevels = new List<int>();
        private int _m_previewLevelIndex;

        public UIPanelCommonPartItem(UIMonoCommonPartItem _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
        }

        public override void BeforeDiscard()
        {
            stopHoverLoop();
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        public override void OnHidePanel()
        {
            stopHoverLoop();
            resetPreviewToSource();
            GetGameObject().transform.RemoveMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.RemoveMouseExit(onGameObjMouseExit);
        }

        public override void OnShowPanel()
        {
            GetGameObject().transform.AddMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.AddMouseExit(onGameObjMouseExit);
        }

        public void SetEnableLevelPreviewCycle(bool enabled)
        {
            _m_levelPreviewCycleEnabled = enabled;
        }

        public void SetInfo(PartInfo _partInfo)
        {
            stopHoverLoop();
            _m_sourcePartInfo = _partInfo;
            _m_partInfo = _partInfo;
            rebuildAvailableLevels();
            _m_previewLevelIndex = findLevelIndex(_partInfo != null ? _partInfo.partLevel : 0);
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);
            if (!mono.isTxtHealthIsRunningInfo)
                mono.txtHealth.text = PartHealthDisplay.FormatMaxOnly(_m_partInfo.maxHealth);
            else
                mono.txtHealth.text = PartHealthDisplay.FormatSlashLine(_m_partInfo.currentHealth, _m_partInfo.maxHealth);
        }

        private void onGameObjMouseExit(PointerEventData arg1, object[] arg2)
        {
            stopHoverLoop();
            resetPreviewToSource();
            GameCommon.DiscardToolTip();
            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onGameObjMouseEnter(PointerEventData arg1, object[] arg2)
        {
            if (_m_partInfo == null)
                return;

            _m_isHovering = true;
            showCurrentTooltip();
            startHoverLoop();
            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void showCurrentTooltip()
        {
            if (_m_partInfo == null)
                return;
            GameCommon.ShowTooltip(_m_partInfo, GetGameObject().transform.position);
        }

        private void startHoverLoop()
        {
            if (!_m_levelPreviewCycleEnabled || _m_availableLevels.Count <= 1)
                return;
            if (SCTaskHelper.instance == null)
                return;

            SCTaskHelper.instance.KillAllCoroutines(this);
            SCTaskHelper.instance.CreateCoroutine(this, hoverRightClickLoop());
        }

        private void stopHoverLoop()
        {
            _m_isHovering = false;
            if (SCTaskHelper.instance != null)
                SCTaskHelper.instance.KillAllCoroutines(this);
        }

        private IEnumerator hoverRightClickLoop()
        {
            while (_m_isHovering)
            {
                if (Input.GetMouseButtonDown(1))
                    cyclePreviewLevel();
                yield return null;
            }
        }

        private void cyclePreviewLevel()
        {
            if (_m_sourcePartInfo?.partRefObj == null || _m_availableLevels.Count <= 1)
                return;

            _m_previewLevelIndex = (_m_previewLevelIndex + 1) % _m_availableLevels.Count;
            int nextLevel = _m_availableLevels[_m_previewLevelIndex];
            if (_m_sourcePartInfo.partLevel == nextLevel)
            {
                _m_partInfo = _m_sourcePartInfo;
            }
            else
            {
                var preview = new PartInfo(
                    _m_sourcePartInfo.partRefObj,
                    _m_sourcePartInfo.isEnemyPart,
                    nextLevel);
                if (preview.levelRefObj == null)
                    return;
                _m_partInfo = preview;
            }

            AudioMgr.instance.PlaySfx("sfx_click");
            refreshShow();
            showCurrentTooltip();
        }

        private void resetPreviewToSource()
        {
            if (_m_partInfo == _m_sourcePartInfo)
                return;
            _m_partInfo = _m_sourcePartInfo;
            _m_previewLevelIndex = findLevelIndex(_m_sourcePartInfo != null ? _m_sourcePartInfo.partLevel : 0);
            refreshShow();
        }

        private void rebuildAvailableLevels()
        {
            _m_availableLevels.Clear();
            if (_m_sourcePartInfo?.partRefObj == null)
                return;

            var rows = SCRefDataMgr.instance?.partLevelRefList?.refDataList;
            if (rows == null)
                return;

            long partId = _m_sourcePartInfo.partRefObj.id;
            for (int i = 0; i < rows.Count; i++)
            {
                PartLevelRefObj row = rows[i];
                if (row == null || row.partId != partId)
                    continue;
                if (!_m_availableLevels.Contains(row.partLevel))
                    _m_availableLevels.Add(row.partLevel);
            }
            _m_availableLevels.Sort();
        }

        private int findLevelIndex(int partLevel)
        {
            for (int i = 0; i < _m_availableLevels.Count; i++)
            {
                if (_m_availableLevels[i] == partLevel)
                    return i;
            }
            return 0;
        }
    }
}

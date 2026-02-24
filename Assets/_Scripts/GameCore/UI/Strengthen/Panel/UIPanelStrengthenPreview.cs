using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelStrengthenPreview : _ASCUIPanelBase<UIMonoStrengthenPreview>
    {
        private PartLevelRefObj _m_levelRefObj;
        private bool _m_isAfter;
        public UIPanelStrengthenPreview(UIMonoStrengthenPreview _mono, SCUIShowType _showType,bool _isAfter) : base(_mono, _showType)
        {
            _m_isAfter = _isAfter;
        }

        public override void AfterInitialize()
        {
        }

        public override void BeforeDiscard()
        {
        }

        public override void OnHidePanel()
        {
        }

        public override void OnShowPanel()
        {
        }

        public void SetInfo(PartLevelRefObj _partLevelRefObj)
        {
            _m_levelRefObj = _partLevelRefObj;
            refreshShow();
        }

        private void refreshShow()
        {
            SCCommon.SetGameObjectEnable(mono.goHasSelectPartShowList, _m_levelRefObj != null);
            SCCommon.SetGameObjectEnable(mono.goNoSelectPartShowList, _m_levelRefObj == null);
            if (_m_levelRefObj == null)
                return;

            PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == _m_levelRefObj.partId);
            if (partRefObj == null)
                return;
            setBaseInfo(partRefObj.partName, _m_levelRefObj.partDesc, partRefObj.qualityType, _m_levelRefObj.partHealth, _m_levelRefObj.partLevel);
            setGridInfo(partRefObj.GetOccupyPosList(), _m_levelRefObj.GetEffectPosList());
        }

        private void setBaseInfo(string _name, string _desc, EQualityType _quality,int _health,int _level)
        {
            SCCommon.SetGameObjectEnable(mono.txtLevel.gameObject, _level > 1);

            if (mono.txtName != null)
                mono.txtName.text = string.IsNullOrEmpty(_name) ? "默认部位" : _name;

            if (mono.txtDesc != null)
                mono.txtDesc.text = string.IsNullOrEmpty(_desc) ? "默认部位描述" : _desc;

            if (mono.txtQuality != null)
            {
                switch (_quality)
                {
                    case EQualityType.NONE:
                        mono.txtQuality.text = "无效品质";
                        break;
                    case EQualityType.NORMAL:
                        mono.txtQuality.text = "普通";
                        break;
                    case EQualityType.RARE:
                        mono.txtQuality.text = "稀有";
                        break;
                    case EQualityType.PRECIOUS:
                        mono.txtQuality.text = "史诗";
                        break;
                }
            }

            if(mono.txtHealth!=null)
                mono.txtHealth.text = _health.ToString();

            if(mono.txtLevel!=null)
                mono.txtLevel.text = "+" + (_level - 1).ToString();
        }
        private void setGridInfo(List<Vector2Int> _occupyPosList, List<Vector2Int> _effectPosList)
        {
            for(int i = mono.tranParentGrid.transform.childCount - 1; i>=0 ; i--)
            {
                SCCommon.DestoryGameObject(mono.tranParentGrid.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < _occupyPosList.Count; i++)
            {
                createOneGrid(_occupyPosList[i], EGridPosType.OCCUPY);
            }
            if (!_occupyPosList.Vector2IntListEquals(_effectPosList))
            {
                for (int i = 0; i < _effectPosList.Count; i++)
                {
                    createOneGrid(_effectPosList[i], EGridPosType.EFFECT);
                }
            }
            else
            {
                //todo:现在设计的有重叠都是完全重叠的暂时这样写
                for (int i = 0; i < _effectPosList.Count; i++)
                {
                    createOneGrid(_effectPosList[i], EGridPosType.BOTH);
                }
            }
        }

        /// <summary>
        /// 生成单个格子并设置位置
        /// </summary>
        private void createOneGrid(Vector2Int gridPos, EGridPosType posType)
        {
            if (mono.tranParentGrid == null)
                return;
            GameObject grid = ResourcesHelper.LoadGameObject(GameConst.PREFAB_TOOLTIP_GIRD, mono.tranParentGrid.transform);
            RectTransform rt = grid.GetComponent<RectTransform>();
            float x = gridPos.x * rt.rect.width;
            float y = -gridPos.y * rt.rect.height;
            if (rt != null)
                rt.anchoredPosition = new Vector2(x, y);
            rt.GetComponent<TooltipGrid>().SetGridTShow(posType);
        }
    }
}

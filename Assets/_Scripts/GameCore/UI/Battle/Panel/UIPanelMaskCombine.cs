using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using SCFrame;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMaskCombine : _ASCUIPanelBase<UIMonoMaskCombine>
    {
        private UIPanelMaskCombinePartContainer _m_partContainer;
        private UIPanelMaskCombineFace _m_faceGrid;
        public UIPanelMaskCombine(UIMonoMaskCombine _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_partContainer = new UIPanelMaskCombinePartContainer(mono.monoPartContainer);
            _m_faceGrid = new UIPanelMaskCombineFace(mono.monoFace, SCUIShowType.INTERNAL);

        }

        public override void BeforeDiscard()
        {
            _m_partContainer?.Discard();
            _m_partContainer = null;
            _m_faceGrid?.Discard();
            _m_faceGrid = null;
        }

        public override void OnHidePanel()
        {

            mono.btnConfirm.onClick.RemoveAllListeners();
            mono.btnDeck.onClick.RemoveAllListeners();
            _m_partContainer?.HidePanel();
            _m_faceGrid?.HidePanel();
        }

        public override void OnShowPanel()
        {

            _m_faceGrid?.ShowPanel();
            _m_partContainer?.ShowPanel();

            mono.btnConfirm.onClick.AddListener(OnConfirmClick);
            mono.btnDeck.onClick.AddListener(() =>
            {
                AudioMgr.instance.PlaySfx("sfx_click");
                UICoreMgr.instance.AddNode(new UINodeDeck(SCFrame.UI.SCUIShowType.ADDITION));
            });
            
            refreshShow();
        }

        private void OnConfirmClick()
        {
            AudioMgr.instance.PlaySfx("sfx_click");            
            UICoreMgr.instance.AddNode(new UINodeBattle(SCUIShowType.FULL)); 
        }
        
        // Helper to get Max Y (Top-most cell Y)
        private int GetPartMaxY(PartInfo info)
        {
            int maxY = info.gridPos.y; // logical origin y
            
            if (info.partRefObj != null && info.partRefObj.occupyPosList != null)
            {
                foreach(var p in info.partRefObj.occupyPosList)
                {
                     // Apply rotation to shape offset
                     Vector2Int rotatedP = GameCommon.RotateVector(new Vector2Int(p.x, p.y), 0);
                     int currentY = info.gridPos.y + rotatedP.y;
                     if (currentY > maxY) maxY = currentY;
                }
            }
            return maxY;
        }

        // Helper to get Min X (Left-most cell X)
        private int GetPartMinX(PartInfo info)
        {
            int minX = info.gridPos.x; // logical origin x
            
            if (info.partRefObj != null && info.partRefObj.occupyPosList != null)
            {
                foreach(var p in info.partRefObj.occupyPosList)
                {
                     // Apply rotation to shape offset
                     Vector2Int rotatedP = GameCommon.RotateVector(new Vector2Int(p.x, p.y), 0);
                     int currentX = info.gridPos.x + rotatedP.x;
                     if (currentX < minX) minX = currentX;
                }
            }
            return minX;
        }
       

        private void refreshShow()
        {
            _m_partContainer?.ReloadParts();

            mono.imgHealthBar.fillAmount = (float)GameModel.instance.playerHealth / GameModel.instance.playerMaxHealth;
            mono.txtHealth.text = GameModel.instance.playerHealth +"/" + GameModel.instance.playerMaxHealth;
        }
    }
}

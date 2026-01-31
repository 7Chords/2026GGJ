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
        public UIPanelMaskCombine(UIMonoMaskCombine _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_partContainer = new UIPanelMaskCombinePartContainer(mono.monoPartContainer);
            
            if (mono.monoFace != null)
            {
                var faceGrid = new UIPanelMaskCombineFaceGrid(mono.monoFace, SCUIShowType.INTERNAL);
                faceGrid.ShowPanel();
            }
        }

        public override void BeforeDiscard()
        {
            _m_partContainer?.Discard();
            _m_partContainer = null;
        }

        public override void OnHidePanel()
        {
        }

        public override void OnShowPanel()
        {
            mono.btnConfirm.onClick.RemoveAllListeners();
            mono.btnConfirm.onClick.AddListener(OnConfirmClick);

            mono.btnCheckEnemyMask.onClick.RemoveAllListeners();
            
            mono.btnCheckEnemyMask.onClick.AddListener(() =>
            {
                UICoreMgr.instance.AddNode(new UINodeEnemyMask(SCFrame.UI.SCUIShowType.ADDITION));
            });

            mono.btnDeck.onClick.RemoveAllListeners();

            mono.btnDeck.onClick.AddListener(() =>
            {
                UICoreMgr.instance.AddNode(new UINodeDeck(SCFrame.UI.SCUIShowType.ADDITION));
            });


            // Generate Enemy when entering the mask combine (preparation phase)
            GameModel.instance.GenerateRandomEnemy();
            
            refreshShow();
        }

        private void OnConfirmClick()
        {
            if (_m_partContainer == null) return;
            
            // 1. 获取所有放置的部位
            List<PartInfo> placedParts = _m_partContainer.GetPlacedParts();
            if (placedParts == null || placedParts.Count == 0)
            {
                Debug.Log("没有放置任何部位！");
                return;
            }
            
            // 2. 排序
            // 规则：Top-to-Bottom (MaxY desc), then Left-to-Right (MinX asc)
            placedParts.Sort((a, b) =>
            {
                int aMaxY = GetPartMaxY(a);
                int bMaxY = GetPartMaxY(b);
                
                if (aMaxY != bMaxY)
                {
                    return aMaxY.CompareTo(bMaxY); // Descending Y
                }
                
                int aMinX = GetPartMinX(a);
                int bMinX = GetPartMinX(b);
                
                return bMinX.CompareTo(aMinX); // Ascending X
            });
            
            // 3. Save to GameModel for Battle
            GameModel.instance.playerBattleParts = placedParts;
            
            // 4. Open Battle Node
            UICoreMgr.instance.AddNode(new UINodeBattle(SCUIShowType.FULL)); 
        }
        
        // Helper to get Max Y (Top-most cell Y)
        private int GetPartMaxY(PartInfo info)
        {
            int maxY = info.gridPos.y; // logical origin y
            
            if (info.partRefObj != null && info.partRefObj.posList != null)
            {
                foreach(var p in info.partRefObj.posList)
                {
                     // Apply rotation to shape offset
                     Vector2Int rotatedP = RotateVector(new Vector2Int(p.x, p.y), info.rotation);
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
            
            if (info.partRefObj != null && info.partRefObj.posList != null)
            {
                foreach(var p in info.partRefObj.posList)
                {
                     // Apply rotation to shape offset
                     Vector2Int rotatedP = RotateVector(new Vector2Int(p.x, p.y), info.rotation);
                     int currentX = info.gridPos.x + rotatedP.x;
                     if (currentX < minX) minX = currentX;
                }
            }
            return minX;
        }
        
        // Duplicated helper, could be in Utility
        private Vector2Int RotateVector(Vector2Int v, int rotationSteps)
        {
            Vector2Int ret = v;
            for(int i=0; i<rotationSteps; i++)
            {
                ret = new Vector2Int(-ret.y, ret.x);
            }
            return ret;
        }

        private void refreshShow()
        {
            _m_partContainer?.ShowPanel();
            _m_partContainer?.ReloadParts();

            mono.imgHealthBar.fillAmount = (float)GameModel.instance.playerHealth / GameModel.instance.playerMaxHealth;
            mono.txtHealth.text = GameModel.instance.playerHealth +"/" + GameModel.instance.playerMaxHealth;
        }
    }
}

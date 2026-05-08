using System.Collections.Generic;
using System.Text;
using SCFrame.UI;

namespace SCFrame
{
    /// <summary>
    /// 
    /// </summary>
    public class UICoreMgr : Singleton<UICoreMgr>
    {

        private List<_ASCUINodeBase> _m_nodeList;

        public override void OnInitialize()
        {
            _m_nodeList = new List<_ASCUINodeBase>();
        }
        public override void OnDiscard()
        {
            _m_nodeList.Clear();
            _m_nodeList = null;
        }

        public override void OnResume()
        {
        }

        public override void OnSuspend()
        {
        }

        #region ����

        /// <summary>
        /// 添加节点（已经存在就show）
        /// </summary>
        /// <param name="_node"></param>
        /// <param name="_needShow"></param>
        public void AddNode(_ASCUINodeBase _node,bool _needShow = true)
        {

            if (_m_nodeList == null)
                return;
            _ASCUINodeBase lastTopNode = GetTopNode(false);

            _ASCUINodeBase node = _m_nodeList.Find(x => x.GetNodeName() == _node.GetNodeName());
            if (node != null)
            {
                node.CopyData(_node);
                if (node.hasHideNode)
                {
                    node.ShowNode();
                    _m_nodeList.Remove(node);
                    _m_nodeList.Add(node);
                }
                else
                    return;
            }
            else
            {
                if (_node.EnterNode())
                    _m_nodeList.Add(_node);
            }

            SCMsgCenter.SendMsg(SCMsgConst.UI_NODE_CHG, lastTopNode, _node);

            _ASCUINodeBase lastSameTypeNode = null;
            for (int i = _m_nodeList.Count - 2; i > -1; i--)
            {
                lastSameTypeNode = _m_nodeList[i];
                if (lastSameTypeNode == null || lastSameTypeNode.ignoreOnUIList)
                    continue;
                if (lastSameTypeNode.showType == _node.showType)
                {
                    if (lastSameTypeNode.needHideWhenEnterNewSameTypeNode)
                    {
                        lastSameTypeNode.HideNode();
                        if(lastSameTypeNode.needMoveToBottomWhenHide)
                        {
                            _m_nodeList.Remove(lastSameTypeNode);
                            _m_nodeList.Insert(0, lastSameTypeNode);
                        }
                        break;
                    }
                }
            }


            if (node == null && _needShow)
                _node.ShowNode();

        }

        /// <summary>
        /// 关闭顶部节点
        /// </summary>
        public void CloseTopNode()
        {
            if (_m_nodeList == null || _m_nodeList.Count == 0)
                return;

            int nodeIdx = _m_nodeList.Count - 1;
            _ASCUINodeBase topNode = _m_nodeList[nodeIdx];
            while(topNode.ignoreOnUIList)
            {
                nodeIdx--;
                if (nodeIdx < 0)
                    return;
                topNode = _m_nodeList[nodeIdx];
            }

            if(topNode != null)
            {
                topNode.HideNode();

                if(topNode.needMoveToBottomWhenHide)
                {
                    _m_nodeList.Remove(topNode);
                    _m_nodeList.Insert(0, topNode);
                }
            }

            _ASCUINodeBase lastSameTypeNode = null;

            for(int i = _m_nodeList.Count - 2;i>-1;i--)
            {
                lastSameTypeNode = _m_nodeList[i];
                if (lastSameTypeNode == null)
                    continue;
                if (lastSameTypeNode.showType == topNode.showType)
                {
                    if (lastSameTypeNode.needShowWhenQuitNewSameTypeNode)
                    {
                        lastSameTypeNode.ShowNode();
                        _m_nodeList.Remove(lastSameTypeNode);
                        _m_nodeList.Add(lastSameTypeNode);
                        break;
                    }
                }
            }

            _ASCUINodeBase nextTopNode = GetTopNode(true);
            SCMsgCenter.SendMsg(SCMsgConst.UI_NODE_CHG, topNode, nextTopNode);
        }

        /// <summary>
        /// Close the topmost ADDITION-layer node (scan from list end). Use when list order does not match
        /// visual stack after needMoveToBottomWhenHide (e.g. tutorial at index 0 while FULL node is last).
        /// </summary>
        public void CloseTopAdditionNode()
        {
            if (_m_nodeList == null || _m_nodeList.Count == 0)
                return;

            _ASCUINodeBase topNode = null;
            for (int i = _m_nodeList.Count - 1; i >= 0; i--)
            {
                var node = _m_nodeList[i];
                if (node == null || node.ignoreOnUIList)
                    continue;
                if (node.showType == SCUIShowType.ADDITION)
                {
                    topNode = node;
                    break;
                }
            }

            if (topNode == null)
            {
                CloseTopNode();
                return;
            }

            topNode.HideNode();

            if (topNode.needMoveToBottomWhenHide)
            {
                _m_nodeList.Remove(topNode);
                _m_nodeList.Insert(0, topNode);
            }

            _ASCUINodeBase lastSameTypeNode = null;

            for (int i = _m_nodeList.Count - 2; i > -1; i--)
            {
                lastSameTypeNode = _m_nodeList[i];
                if (lastSameTypeNode == null)
                    continue;
                if (lastSameTypeNode.showType == topNode.showType)
                {
                    if (lastSameTypeNode.needShowWhenQuitNewSameTypeNode)
                    {
                        lastSameTypeNode.ShowNode();
                        _m_nodeList.Remove(lastSameTypeNode);
                        _m_nodeList.Add(lastSameTypeNode);
                        break;
                    }
                }
            }

            _ASCUINodeBase nextTopNode = GetTopNode(true);
            SCMsgCenter.SendMsg(SCMsgConst.UI_NODE_CHG, topNode, nextTopNode);
        }

        /// <summary>
        /// ͨ通过esc关闭节点
        /// </summary>
        public void CloseNodeByEsc()
        {
            if (_m_nodeList == null || _m_nodeList.Count == 0)
                return;
            _ASCUINodeBase topNode = GetTopNode(false);
            if (!topNode.canQuitByEsc)
                return;
            CloseTopNode();
        }

        /// <summary>
        /// ͨ通过右键关闭节点
        /// </summary>
        public void CloseNodeByMouseRight()
        {
            if (_m_nodeList == null || _m_nodeList.Count == 0)
                return;
            _ASCUINodeBase topNode = GetTopNode(false);
            if (!topNode.canQuitByMouseRight)
                return;
            CloseTopNode();
        }


        public void HideNode(string _nodeName)
        {
            _ASCUINodeBase node = null;

            for (int i = _m_nodeList.Count - 1; i>-1; i--)
            {
                node = _m_nodeList[i];
                if (node == null)
                    continue;
                if (node.GetNodeName()==_nodeName)
                {
                    node.HideNode();
                    if(node.needMoveToBottomWhenHide)
                    {
                        _m_nodeList.Remove(node);
                        _m_nodeList.Insert(0, node);
                    }
                    return;
                }
            }
        }

        public void ShowNode(string _nodeName)
        {
            _ASCUINodeBase node = null;
            for (int i = _m_nodeList.Count - 1; i > -1; i--)
            {
                node = _m_nodeList[i];
                if (node == null)
                    continue;

                if (node.GetNodeName() == _nodeName)
                {
                    node.ShowNode();
                    _m_nodeList.Remove(node);
                    _m_nodeList.Add(node);
                    return;
                }
            }
        }

        /// <summary>
        /// 展示节点但不移动到顶部
        /// </summary>
        /// <param name="_nodeName"></param>
        public void ShowNodeButNotMove2Top(string _nodeName)
        {
            for (int i = _m_nodeList.Count - 1; i > -1; i--)
            {
                if (_m_nodeList[i].GetNodeName() == _nodeName)
                {
                    _m_nodeList[i].ShowNode();
                    return;
                }
            }
        }
        public void RemoveNode(string _nodeName)
        {
            _ASCUINodeBase node = GetNodeByName(_nodeName);
            if (node == null)
                return;
            node.HideNode();
            node.QuitNode();
            _m_nodeList.Remove(node);
        }
        public void RemoveAllNodes(SCUINodeFuncType _nodeFuncType)
        {
            _ASCUINodeBase node = null;
            for (int i = _m_nodeList.Count - 1; i > -1; i--)
            {
                node = _m_nodeList[i];
                if (node == null)
                    continue;
                if (node.nodeFuncType == _nodeFuncType)
                {
                    node.HideNode();
                    node.QuitNode();
                    _m_nodeList.RemoveAt(i);
                }
            }
        }

        /// <summary> Remove and discard every node (all func types). </summary>
        public void RemoveAllNodes()
        {
            if (_m_nodeList == null || _m_nodeList.Count == 0)
                return;
            for (int i = _m_nodeList.Count - 1; i > -1; i--)
            {
                var node = _m_nodeList[i];
                if (node == null)
                    continue;
                node.HideNode();
                node.QuitNode();
                _m_nodeList.RemoveAt(i);
            }
        }
        public _ASCUINodeBase GetNodeByName(string _nodeName)
        {
            foreach(var node in _m_nodeList)
            {
                if (node.GetNodeName() == _nodeName)
                    return node;
            }
            return null;
        }

        public _ASCUINodeBase GetTopNode(bool _includeIgnore = true)
        {
            if (_m_nodeList == null || _m_nodeList.Count == 0)
                return null;
            _ASCUINodeBase topNode = null;
            if (_includeIgnore)
                 topNode = _m_nodeList[_m_nodeList.Count - 1];
            else
            {
                int nodeIdx = _m_nodeList.Count - 1;
                topNode = _m_nodeList[nodeIdx];
                while (topNode.ignoreOnUIList)
                {
                    nodeIdx--;
                    if (nodeIdx < 0)
                        return null;
                    topNode = _m_nodeList[nodeIdx];
                }
            }
            return topNode;
        }

        public _ASCUINodeBase GetTopNode(SCUIShowType _showType, bool _includeIgnore = true)
        {
            if (_m_nodeList == null)
                return null;

            if(_includeIgnore)
            {
                for (int i = _m_nodeList.Count - 1; i >= 0; i--)
                {
                    if (_m_nodeList[i].showType == _showType)
                        return _m_nodeList[i];
                }
            }
            else
            {
                for (int i = _m_nodeList.Count - 1; i >= 0; i--)
                {
                    if (_m_nodeList[i].showType == _showType && !_m_nodeList[i].ignoreOnUIList)
                        return _m_nodeList[i];
                }
            }
            return null;

        }


        public _ASCUINodeBase GetTopShowNode(SCUIShowType _showType, bool _includeIgnore = true)
        {
            if (_m_nodeList == null)
                return null;

            if (_includeIgnore)
            {
                for (int i = _m_nodeList.Count - 1; i >= 0; i--)
                {
                    if (_m_nodeList[i].showType == _showType && !_m_nodeList[i].hasHideNode)
                        return _m_nodeList[i];
                }
            }
            else
            {
                for (int i = _m_nodeList.Count - 1; i >= 0; i--)
                {
                    if (_m_nodeList[i].showType == _showType && !_m_nodeList[i].ignoreOnUIList)
                        return _m_nodeList[i];
                }
            }
            return null;
        }
        #endregion


        public void PrintNodeList()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _m_nodeList.Count; i++)
            {
                sb.Append(_m_nodeList[i].GetNodeName());
                if(_m_nodeList[i].ignoreOnUIList)
                    sb.Append("(ignore)");
                sb.Append("---");
            }
            SCDebugHelper.Log(sb.ToString());
        }
    }
}

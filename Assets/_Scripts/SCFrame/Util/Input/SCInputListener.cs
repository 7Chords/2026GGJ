using GameCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    public class SCInputListener : Singleton<SCInputListener>
    {

        private int _m_tbsFrameChecker;

        private int _m_tbsFrameInterval;

        public bool _m_canInput;
        public override void OnInitialize()
        {
            SCTaskHelper.instance.AddUpdateListener(updateInput);
            _m_canInput = true;
        }

        public override void OnDiscard()
        {
            SCTaskHelper.instance.RemoveUpdateListener(updateInput);
        }


        private void updateInput()
        {
            if (!_m_canInput)
                return;

            if (_m_tbsFrameChecker < _m_tbsFrameInterval)
            {
                _m_tbsFrameChecker += 1;
                return;
            }
            if (Input.anyKeyDown)
                _m_tbsFrameChecker = 0;

            if (Input.GetKeyDown(KeyCode.Escape))
                UICoreMgr.instance.CloseNodeByEsc();
            if (Input.GetMouseButtonDown(1))
                UICoreMgr.instance.CloseNodeByMouseRight();
        }

        public float GetHorizontalInput()
        {
            if (!_m_canInput)
                return 0;
            return Input.GetAxis("Horizontal");
        }
        public float GetVerticalInput()
        {
            if (!_m_canInput)
                return 0;
            return Input.GetAxis("Vertical");
        }

        public bool GetKeyCodeDown(KeyCode _code)
        {
            if (!_m_canInput)
                return false;
            return Input.GetKeyDown(_code);
        }
        public bool GetKeyCode(KeyCode _code)
        {
            if (!_m_canInput)
                return false;
            return Input.GetKey(_code);
        }

        public void SetCanInput(bool _canInput)
        {
            _m_canInput = _canInput;
        }
    }
}

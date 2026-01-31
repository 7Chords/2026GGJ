using GameCore;
using GameCore.UI;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            UICoreMgr.instance.AddNode(new UINodeMaskCombine(SCFrame.UI.SCUIShowType.FULL));
        }
        else if(Input.GetKeyDown(KeyCode.K))
        {
            GameModel.instance.rollStoreId = 103001;
            UICoreMgr.instance.AddNode(new UINodeStore(SCFrame.UI.SCUIShowType.FULL));
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            UICoreMgr.instance.AddNode(new UINodeMap(SCFrame.UI.SCUIShowType.FULL));
        }
    }
}

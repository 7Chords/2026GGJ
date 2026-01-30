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
    }
}

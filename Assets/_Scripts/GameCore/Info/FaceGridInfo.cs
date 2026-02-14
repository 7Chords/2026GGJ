using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class FaceGridInfo
    {
        public Vector2Int pos;
        public bool hasPart;
        public PartInfo ownerPart;
        public FaceGridInfo(Vector2Int _pos, bool _hasPart)
        {
            pos = _pos;
            hasPart = _hasPart;
        }

        public void SetOwnerPart(PartInfo _info)
        {
            hasPart = true;
            ownerPart = _info;
        }
        public void SetEmpty()
        {
            hasPart = false;
            ownerPart = null;
        }
    }
}

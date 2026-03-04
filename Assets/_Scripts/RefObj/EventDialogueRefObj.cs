using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class EventDialogueRefObj : SCRefDataCore
    {
        public long id;
        public string content;
        public EEventDialogueType dialogueType;
        public EEventDialogueFlagType flagType;
        public List<long> nextList;
        public EEventType eventType;

        protected override void _parseFromString()
        {
            id = getLong("id");
            content = getString("content");
            dialogueType = (EEventDialogueType)getEnum("dialogueType", typeof(EEventDialogueType));
            flagType = (EEventDialogueFlagType)getEnum("flagType", typeof(EEventDialogueFlagType));
            nextList = getList<long>("nextList");
        }

        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "event_dialogue";
    }
}

using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.RefData
{
    public class EventDialogueRefObj : SCRefDataCore
    {
        public long id;
        public string name;
        public string content;
        public EEventDialogueType dialogueType;
        public EEventDialogueFlagType flagType;
        public List<long> nextList;
        public EEventType eventType;

        protected override void _parseFromString()
        {
            id = getLong("id");
            name = getString("name");
            content = getString("content");
            dialogueType = (EEventDialogueType)getEnum("dialogueType", typeof(EEventDialogueType));
            flagType = (EEventDialogueFlagType)getEnum("flagType", typeof(EEventDialogueFlagType));
            nextList = getList<long>("nextList");
            eventType = (EEventType)getEnum("eventType", typeof(EEventType));
        }

        public static string assetPath => "RefData/ExportTxt";
        public static string sheetName => "event_dialogue";
    }
}

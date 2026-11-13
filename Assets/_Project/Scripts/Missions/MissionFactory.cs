using ExtensaoZero.Core;
using ExtensaoZero.Environment;
using UnityEngine;

namespace ExtensaoZero.Missions
{
    public static class MissionFactory
    {
        public static MissionBase Create(int chapter, Transform parent, GameManager manager, WorldContext world)
        {
            var go = new GameObject("Mission_" + chapter);
            if (parent != null) go.transform.SetParent(parent, false);
            MissionBase mission;
            switch (chapter)
            {
                case 1: mission = go.AddComponent<CentralMission>(); break;
                case 2: mission = go.AddComponent<VoiceArchiveMission>(); break;
                case 3: mission = go.AddComponent<DeadLineMission>(); break;
                case 4: mission = go.AddComponent<OtherSideMission>(); break;
                case 5: mission = go.AddComponent<ExtensionZeroMission>(); break;
                default: mission = go.AddComponent<FirstCallMission>(); break;
            }

            mission.Bind(manager, world);
            return mission;
        }
    }
}

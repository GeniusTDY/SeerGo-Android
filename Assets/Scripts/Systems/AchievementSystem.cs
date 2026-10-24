using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Systems
{
    public class AchievementSystem : AbstractSystem, IAchievementSystem
    {
        private IStorage storage;
        private Dictionary<int, string> mapAchievements = new Dictionary<int, string>();
        private Dictionary<string, string> npcAchievements = new Dictionary<string, string>();

        protected override void OnInit()
        {
            storage = this.GetUtility<IStorage>();
            InitializeAchievements();

        }

        private void InitializeAchievements()
        {
            // 地图探索成就
            mapAchievements[10001] = "First_Transport_Room";
            mapAchievements[10015] = "First_Volcano_Star";
            mapAchievements[10081] = "First_SPT_Challenge";

            // NPC交互成就
            npcAchievements["1000101"] = "Talk_To_Space_Station";
            npcAchievements["1000103"] = "First_SPT_Visit";
        }

        public void CheckMapExplorationAchievement(int mapId)
        {
            if (mapAchievements.TryGetValue(mapId, out string achievementId))
            {
                string key = $"Achievement_{achievementId}";
                if (!storage.ContainsKey(key))
                {
                    storage.Save(key, true);
                    UnlockAchievement(achievementId, GetAchievementName(achievementId));
                }
            }
        }

        public void CheckNpcInteractionAchievement(string npcId)
        {
            if (npcAchievements.TryGetValue(npcId, out string achievementId))
            {
                string key = $"Achievement_{achievementId}";
                if (!storage.ContainsKey(key))
                {
                    storage.Save(key, true);
                    UnlockAchievement(achievementId, GetAchievementName(achievementId));
                }
            }
        }

        public void CheckMapChangeAchievement(int fromMapId, int toMapId)
        {
            // 检查特定地图切换组合的成就
            if (fromMapId == 10001 && toMapId == 10081)
            {
                string achievementId = "First_Teleport";
                string key = $"Achievement_{achievementId}";
                if (!storage.ContainsKey(key))
                {
                    storage.Save(key, true);
                    UnlockAchievement(achievementId, "第一次传送");
                }
            }
        }

        private void UnlockAchievement(string achievementId, string achievementName)
        {
            //SeerGoLogger.Log($"🎉 成就解锁: {achievementName}");

            this.SendEvent(new AchievementUnlockedEvent
            {
                AchievementId = achievementId,
                AchievementName = achievementName
            });
        }

        private string GetAchievementName(string achievementId)
        {
            var names = new Dictionary<string, string>
            {
                ["First_Transport_Room"] = "首次探索传送仓",
                ["First_Volcano_Star"] = "首次探索火山星",
                ["First_SPT_Challenge"] = "首次挑战SPT",
                ["Talk_To_Space_Station"] = "与太空站对话",
                ["First_SPT_Visit"] = "首次访问SPT",
                ["First_Teleport"] = "第一次传送"
            };

            return names.GetValueOrDefault(achievementId, achievementId);
        }
    }
}
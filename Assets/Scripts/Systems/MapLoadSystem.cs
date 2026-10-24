using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Systems
{
    public class MapLoadSystem : AbstractSystem, IMapLoadSystem
    {
        private IMapModel mapModel;
        private IAchievementSystem achievementSystem;
        private ICreateSpriteEntitySystem createSpriteEntitySystem;

        protected override void OnInit()
        {
            mapModel = this.GetModel<IMapModel>();
            achievementSystem = this.GetSystem<IAchievementSystem>();
            createSpriteEntitySystem = this.GetSystem<ICreateSpriteEntitySystem>();
            // 监听地图加载和npc点击交互请求
            this.RegisterEvent<MapLoadRequestEvent>(OnMapLoadRequest);
            this.RegisterEvent<NpcClickRequestEvent>(OnNpcClickRequest);
        }


        public void RequestMapLoad(int mapId)
        {
            // System层处理地图加载逻辑
            // 地图相同不重复加载
            if (mapModel.CurrentMapId.Value == mapId) return;

            mapModel.CurrentMapId.Value = mapId;
        }

        private void OnMapLoadRequest(MapLoadRequestEvent e)
        {
            RequestMapLoad(e.TargetMapId);
        }

        private void OnNpcClickRequest(NpcClickRequestEvent e)
        {
            HandleNpcClick(e.NpcId, e.ActionType, e.Parameters);
        }
        private void SendNpcClickResult(string npcId, bool success, string message)
        {
            // System层有权限发送事件
            this.SendEvent(new NpcClickResultEvent
            {
                NpcId = npcId,
                Success = success,
                Message = success ? message : "",
                ErrorMessage = success ? "" : message
            });
        }

        public void HandleNpcClick(string npcId, string action, string parameters, Transform parent = null, List<GameObject> resultList = null)
        {
            achievementSystem.CheckNpcInteractionAchievement(npcId);

            switch (action)
            {
                case "teleport":
                    if (int.TryParse(parameters, out int targetMapId))
                    {
                        RequestMapLoad(targetMapId);
                        SendNpcClickResult(npcId, true, $"传送到地图 {targetMapId}");
                    }
                    break;
                case "BraveTower":
                    {
                        var config = BraveTowerConfigLoader.LoadConfig();
                        if (config == null) { SendNpcClickResult(npcId, false, "配置数据为空"); break; }
                        int floor = GetCurrentTowerFloor("BRAVETOWER");
                        var data = config.GetFloorData(floor);
                        HandleTowerNpcClick(npcId, parent, resultList, "勇者之塔", "BRAVETOWER", floor, data?.monsters);
                    }
                    break;
                case "TrialTower":
                    {
                        var config = TrialTowerConfigLoader.LoadConfig();
                        if (config == null) { SendNpcClickResult(npcId, false, "配置数据为空"); break; }
                        int floor = GetCurrentTowerFloor("TRIALTOWER");
                        var data = config.GetFloorData(floor);
                        HandleTowerNpcClick(npcId, parent, resultList, "试炼之塔", "TRIALTOWER", floor, data?.monsters);
                    }
                    break;
                case "Battle":

                    break;
                default:
                    SendNpcClickResult(npcId, true, $"执行动作: {action}");
                    break;
            }
        }


        /// <summary>
        /// 勇者/试炼之塔统一NPC点击处理：创建怪物精灵实体
        /// </summary>
        private void HandleTowerNpcClick(string npcId, Transform parent, List<GameObject> resultList,
            string towerName, string enemyType, int currentFloor, ITowerMonsterData[] monsters)
        {
            if (monsters == null || monsters.Length == 0)
            {
                SeerGoLogger.LogWarning($"[{towerName}] 第{currentFloor}层未找到怪物配置");
                SendNpcClickResult(npcId, false, $"第{currentFloor}层无怪物数据");
                return;
            }

            var monsterDict = LoadMonsterDictSync();
            int monsterCount = monsters.Length;

            SeerGoLogger.Log($"=== [{towerName}] 当前层数: {currentFloor}, 怪物数量: {monsterCount} ===");

            for (int i = 0; i < monsterCount; i++)
            {
                var monster = monsters[i];

                string monsterName = monsterDict.TryGetValue(monster.id, out var monsterData)
                    ? monsterData.DefName
                    : $"未知精灵{monster.id}";

                string position = GetBraveTowerMonsterPosition(i, monsterCount);

                var mapNpc = new MapNpc();
                mapNpc.Id = (-5000000 - i).ToString();
                mapNpc.ResId = "-5000000";
                mapNpc.Type = "sprite";
                mapNpc.Name = monsterName;
                mapNpc.Sort = 3;
                mapNpc.Size = "1,1";
                mapNpc.Position = position;
                mapNpc.battle.branch.enemy.Type = enemyType;

                for (int j = 0; j < monsterCount; j++)
                {
                    var m = monsters[j];
                    var pet = new Pet();
                    pet.Id = m.id.ToString();
                    pet.Level = m.level;
                    pet.LoopSkill = string.Join(",", m.skills);
                    mapNpc.battle.branch.enemy.pet.Add(pet);
                }

                mapNpc.eventHandler.button.Type = "click";
                mapNpc.eventHandler.button.action = "battle";
                mapNpc.eventHandler.button.param = "default";

                createSpriteEntitySystem.CreateSpriteAsync(mapNpc, parent, resultList);

                // 从外部 Resources/ 目录加载精灵图片
                string petImagePath = ResourcePathBuilder.GetPetImageResPath(monster.id);
                var petSprite = ExternalResourcesManager.Instance.LoadSprite(petImagePath);
                if (petSprite != null)
                {
                    var sr = resultList[resultList.Count - 1].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = petSprite;

                        // preserveAspect: 在固定尺寸内按宽高比自适应
                        Vector2 bounds = petSprite.bounds.size;
                        if (bounds.x > 0 && bounds.y > 0)
                        {
                            float maxSize = 1.8f;
                            float scale = Mathf.Min(maxSize / bounds.x, maxSize / bounds.y);
                            sr.transform.localScale = new Vector3(scale, scale, 1f);
                        }

                        SeerGoLogger.Log($"  实例[{i}] 精灵图片设置成功: Pet_{monster.id}/1");
                    }
                }
                else
                {
                    SeerGoLogger.LogWarning($"  实例[{i}] 未找到精灵图片: Pet_{monster.id}/1");
                }
                SeerGoLogger.Log($"  实例[{i}] mapNpcId={mapNpc.Id}, resId={mapNpc.ResId}, name={monsterName}, pos={position}, petCount={mapNpc.battle.branch.enemy.pet.Count}");
            }
        }

        /// <summary>
        /// 从存档读取当前塔楼层（勇者之塔 BRAVETOWER / 试炼之塔 TRIALTOWER）
        /// </summary>
        private int GetCurrentTowerFloor(string enemyType)
        {
            var archiveSystem = this.GetSystem<IArchiveSystem>();
            var savedData = archiveSystem.LoadGame("autosave0");
            var yiteData = savedData != null
                ? savedData.YiteRogueData
                : this.GetModel<IArchiveModel>().CurrentData.YiteRogueData;
            return enemyType == "BRAVETOWER"
                ? yiteData.CurrentBraveTowerFloor
                : yiteData.CurrentTrialTowerFloor;
        }

        /// <summary>
        /// 同步从JSON文件加载精灵配置字典，解决异步Po加载PetInfoModel的时机问题
        /// </summary>
        private static Dictionary<int, MonsterData> LoadMonsterDictSync()
        {
            var dict = new Dictionary<int, MonsterData>();
            try
            {
                string path = ResourcePathBuilder.GetPetsInfoDataPath() + "/monsters";
                var textAsset = Resources.Load<TextAsset>(path);
                if (textAsset != null)
                {
                    var config = JsonUtility.FromJson<MonsterConfig>(textAsset.text);
                    if (config?.Monsters != null)
                    {
                        foreach (var m in config.Monsters)
                        {
                            dict[m.ID] = m;
                        }
                    }
                    Resources.UnloadAsset(textAsset);
                }
            }
            catch (System.Exception e)
            {
                SeerGoLogger.LogWarning($"[勇者/试炼之塔] 同步加载精灵配置失败: {e.Message}");
            }
            return dict;
        }

        /// <summary>
        /// 勇者之塔怪物站位：根据怪物数量和索引返回世界坐标
        /// 1只→(0.08,-0.5); 2只→左(-0.5,-0.5) 右(0.08,-0.5); 3只→左(-1.5,-0.5) 中(0.08,-0.5) 右(2,-0.5)
        /// </summary>
        private static string GetBraveTowerMonsterPosition(int index, int totalCount)
        {
            switch (totalCount)
            {
                case 1:
                    return "0.08,-0.5";
                case 2:
                    return index == 0 ? "-0.5,-0.5" : "0.5,-0.5";
                case 3:
                    return index switch
                    {
                        0 => "-1.5,-0.5",
                        1 => "0.08,-0.5",
                        2 => "2,-0.5",
                        _ => "0.08,-0.5"
                    };
                default:
                    return "0.08,-0.5";
            }
        }

        public void PrintCurrentMapInfo()
        {
            mapModel.PrintMapInfoToConsole();
        }

        public void Deinit()
        {
            this.UnRegisterEvent<MapLoadRequestEvent>(OnMapLoadRequest);
            this.UnRegisterEvent<NpcClickRequestEvent>(OnNpcClickRequest);
        }
    }
}
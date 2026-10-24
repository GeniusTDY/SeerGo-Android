using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SeerGoGame.Services
{
    /// <summary>
    /// 战斗奖励服务接口
    /// 负责处理战斗胜利后的经验值、努力值(EV)分配和升级计算
    /// </summary>
    public interface IBattleRewardService
    {
        /// <summary>
        /// 处理Normal战斗奖励
        /// </summary>
        /// <param name="petData">我方宠物数据</param>
        /// <param name="petIndex">宠物在背包中的索引</param>
        /// <param name="enemyPets">敌方宠物列表</param>
        /// <param name="uiTransform">UI画布变换，用于显示对话框</param>
        void ProcessNormalBattleReward(PetData petData, int petIndex, List<Pet> enemyPets);

        /// <summary>
        /// 处理CatchPet或者获得宠物战斗奖励
        /// </summary>
        /// <param name="petData">我方宠物数据</param>
        /// <param name="petIndex">宠物在背包中的索引</param>
        /// <param name="enemyPets">敌方宠物列表</param>
        /// <param name="uiTransform">UI画布变换，用于显示对话框</param>
        void ProcessCatchOrGetPetBattleReward(int petId, bool isCatch, bool isShowDialog = true);

        /// <summary>
        /// 处理勇者之塔战斗奖励
        /// </summary>
        void ProcessBraveTowerReward();

        /// <summary>
        /// 处理试炼之塔战斗奖励
        /// </summary>
        void ProcessTrialTowerReward();
    }

    /// <summary>
    /// 战斗奖励服务实现类
    /// 处理战斗胜利后的经验值、努力值(EV)分配和升级逻辑
    /// </summary>
    public class BattleRewardService : IBattleRewardService
    {
        /// <summary>
        /// 金豆、赛尔豆、NoNo经验分配池上限：5亿
        /// </summary>
        private const int MaxRewardLimit = 500_000_000;

        private IPetInfoSystem petInfoSystem;
        private IExpConfigSystem expConfigSystem;
        private IPetAttributeCalcSystem petAttributeCalcSystem;
        private IArchiveModel archiveModel;
        private ISkillSystem skillSystem;
        private IArchiveSystem archiveSystem;
        private IItemModel itemModel;
        private Transform uiTransform;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="petInfoSystem">宠物信息系统</param>
        /// <param name="expConfigSystem">经验配置系统</param>
        /// <param name="petAttributeCalcSystem">宠物属性计算系统</param>
        /// <param name="archiveModel">存档数据模型</param>
        public BattleRewardService(IPetInfoSystem petInfoSystem,
                                  IExpConfigSystem expConfigSystem,
                                  IPetAttributeCalcSystem petAttributeCalcSystem,
                                  IArchiveModel archiveModel,
                                  ISkillSystem skillSystem,
                                  IArchiveSystem archiveSystem,
                                  IItemModel itemModel,
                                  Transform uiTransform)
        {
            this.petInfoSystem = petInfoSystem;
            this.expConfigSystem = expConfigSystem;
            this.petAttributeCalcSystem = petAttributeCalcSystem;
            this.archiveModel = archiveModel;
            this.skillSystem = skillSystem;
            this.archiveSystem = archiveSystem;
            this.itemModel = itemModel;
            expConfigSystem.InitializeData();
            this.uiTransform = uiTransform;
        }

        #region 处理普通战斗奖励
        /// <summary>
        /// 处理战斗奖励的主要入口方法
        /// </summary>
        public void ProcessNormalBattleReward(PetData petData, int petIndex, List<Pet> enemyPets)
        {
            if (petData == null || enemyPets == null || enemyPets.Count == 0)
            {
                SeerGoLogger.LogWarning("战斗奖励处理失败：宠物数据或敌方宠物列表为空");
                return;
            }
            List<WinButton> winButton = BattleDataManager.SelectedNpc.battle.branch.winHandler.button;
            winButton.ForEach(button =>
            {
                if (button.action == "getPet")
                {
                    ProcessCatchOrGetPetBattleReward(button.param[0].ToInt(), false);
                }
                else if (button.action == "getMolecule")
                {
                    ProcessGetgetMoleculeBattleReward(button.param[0].ToInt());
                }

            });

            // 获取当前激活的翻倍倍率
            int expMultiplier = GetActiveMultiplier("exp");
            int seerBeanMultiplier = GetActiveMultiplier("seerBean");
            int dropRateMultiplier = GetActiveMultiplier("dropRate");

            // 1. 计算总经验值（应用经验倍率）
            int totalExpGained = CalculateTotalExpGained(enemyPets, expMultiplier);

            // 2. 分配努力值(EV)（应用经验倍率）
            ApplyEVGain(petData, enemyPets, expMultiplier);

            // 3. 显示经验获得对话框并处理升级
            ShowExpDistributionDialog(petData, totalExpGained);

            // 4.赠送赛尔豆（应用赛尔豆倍率）
            int seerBeanAdd = (BattleDataManager.CurrentBattleType == BattleDataManager.BattleType.NORMAL ? 1000 : 5000) * seerBeanMultiplier;
            int goldBeanAdd = (BattleDataManager.CurrentBattleType == BattleDataManager.BattleType.NORMAL ? 100 : 500) * seerBeanMultiplier;

            // 统一添加赛尔豆和金豆，自动受上限约束（ID1=赛尔豆，ID2=金豆）
            AddItemToStorage(1, seerBeanAdd);
            AddItemToStorage(2, goldBeanAdd);
            // 处理eventHandler中的reward道具掉落（应用掉落率倍率）
            ProcessRewardItems(dropRateMultiplier);

            // 消耗本次使用的buff次数
            ConsumeAllRewardBuffs();

            archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);

            // 弹窗提示
            string extraInfo = BuildMultiplierInfo(expMultiplier, seerBeanMultiplier);
            Dialog1Manager.Instance.ShowPopupOnTransform(
                uiTransform,
                "",
                $"战斗胜利，获得{seerBeanAdd}赛尔豆，{goldBeanAdd}金豆！{extraInfo}"
            );

        }
        #endregion

        #region 处理捉宠战斗奖励
        public void ProcessCatchOrGetPetBattleReward(int petId, bool isCatch, bool isShowDialog = true)
        {
            MonsterData monsterBaseData = petInfoSystem.GetMonsterById(petId);

            if (monsterBaseData == null)
            {
                SeerGoLogger.LogWarning("无法获取怪物数据");
                return;
            }

            PetData newMonsterData = new PetData();

            newMonsterData.Id = petId;

            newMonsterData.Basic.Id = petId;
            newMonsterData.Basic.Name = monsterBaseData.DefName;
            newMonsterData.Basic.Gender = monsterBaseData.Gender;
            newMonsterData.Basic.Personality = Random.Range(1, 25);
            newMonsterData.Basic.Attributes = monsterBaseData.Type;
            newMonsterData.Basic.GetPetDate = DateTime.Now.ToString("yyyy-MM-dd");

            newMonsterData.Feature.Id = petId;

            newMonsterData.Exp.Id = petId;
            newMonsterData.Exp.Level = 1;
            newMonsterData.Exp.CurrentExp = expConfigSystem.GetExpValue(1, monsterBaseData.GrowthType);

            newMonsterData.Talent.Id = petId;
            newMonsterData.Talent.IV = Random.Range(1, 32);

            newMonsterData.Skills.Id = petId;
            List<LearnableMove> learnableMoves = petInfoSystem.GetLearnableMoves(petId);
            for (int i = 0; i < learnableMoves.Count; i++)
            {
                if (learnableMoves[i].LearningLv == 1)
                {
                    newMonsterData.Skills.OwnSkillId.Add(learnableMoves[i].ID);
                    EquippedSkill equippedSkill = new EquippedSkill();
                    equippedSkill.id = learnableMoves[i].ID;
                    equippedSkill.maxPP = skillSystem.GetSkillById(learnableMoves[i].ID.ToString()).MaxPP;
                    equippedSkill.currentPP = equippedSkill.maxPP;
                    newMonsterData.Skills.NormalSkill.Add(equippedSkill);
                }
            }
            // 计算宠物属性
            petAttributeCalcSystem.CalculateAllStats(newMonsterData);

            bool isBagFull = archiveModel.CurrentData.PetBag.Count >= 6;

            if (archiveModel.CurrentData.PetBag.Count < 6)
            {
                archiveModel.CurrentData.PetBag.Add(newMonsterData);
            }
            else
            {
                archiveModel.CurrentData.PetStorage.Add(newMonsterData);
            }
            if (isShowDialog)
            {
                // 保存存档
                archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);
                // 弹窗提示
                ShowCatchPetDialog(isBagFull, isCatch, newMonsterData);
            }

        }
        #endregion
        #region 处理勇者之塔战斗奖励
        /// <summary>
        /// 处理勇者之塔战斗奖励
        /// 从brave_tower.json读取当前楼层对应的奖励并发放
        /// 首次通关发放物品奖励（赛尔豆、道具、金豆、精灵），重复挑战仅发放经验奖励
        /// </summary>
        public void ProcessBraveTowerReward()
        {
            int currentFloor = archiveModel.CurrentData.YiteRogueData.CurrentBraveTowerFloor;
            // 回推刚刚通关的楼层
            // 正常情况：BattleManageController已把current+1，beatenFloor = current - 1
            // 上限层数：current未+1，由TowerBeatenFloorOverride指定实际通关楼层
            int beatenFloor;
            if (BattleDataManager.TowerBeatenFloorOverride > 0)
            {
                beatenFloor = BattleDataManager.TowerBeatenFloorOverride;
                BattleDataManager.TowerBeatenFloorOverride = 0; // 使用后重置
            }
            else
            {
                beatenFloor = currentFloor > 1 ? currentFloor - 1 : 1;
            }

            // 通过BattleDataManager的标志判断是否首次通关（战斗发起时已检查）
            if (!BattleDataManager.BraveTowerIsFirstClear)
            {
                SeerGoLogger.Log($"[勇者之塔奖励] 第{beatenFloor}层非首次通关，仅发放经验奖励");
                return;
            }

            var config = BraveTowerConfigLoader.LoadConfig();
            var rewardEntry = config?.GetRewardData(beatenFloor);

            if (rewardEntry == null)
            {
                SeerGoLogger.LogWarning($"[勇者之塔奖励] 第{beatenFloor}层无奖励配置");
                return;
            }

            var rewardParts = new List<string>();

            // 1. 发放赛尔豆 (item id 1)
            if (rewardEntry.coins > 0)
            {
                AddItemToStorage(1, rewardEntry.coins);
                rewardParts.Add($"{rewardEntry.coins}赛尔豆");
            }

            // 2. 发放items中的道具
            if (rewardEntry.items != null)
            {
                foreach (var item in rewardEntry.items)
                {
                    int itemId = int.TryParse(item.itemId, out int parsed) ? parsed : 0;
                    if (itemId > 0 && item.count > 0)
                    {
                        AddItemToStorage(itemId, item.count);
                        var itemConfig = itemModel.GetById(itemId);
                        string itemName = itemConfig != null ? itemConfig.name : $"物品{itemId}";
                        rewardParts.Add($"{itemName} x{item.count}");
                    }
                }
            }

            // 3. 发放金豆 (goldBeans + milestoneGoldBeans, item id 2)
            int totalGoldBeans = rewardEntry.goldBeans + rewardEntry.milestoneGoldBeans;
            if (totalGoldBeans > 0)
            {
                AddItemToStorage(2, totalGoldBeans);
                rewardParts.Add($"{totalGoldBeans}金豆");
            }
            ProcessBraveTowerPetReward(rewardEntry.petRewardId);
            // 4. 保存存档
            archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);



            SeerGoLogger.Log($"[勇者之塔奖励] 第{beatenFloor}层首次通关，奖励已发放: coins={rewardEntry.coins}, goldBeans={totalGoldBeans}, items={rewardEntry.items?.Length ?? 0}, petRewardId={rewardEntry.petRewardId}");
        }

        /// <summary>
        /// 发放勇者之塔精灵奖励（在确认弹窗后执行）
        /// </summary>
        private void ProcessBraveTowerPetReward(int petRewardId)
        {
            if (petRewardId > 0)
            {
                ProcessCatchOrGetPetBattleReward(petRewardId, false);
            }
        }

        /// <summary>
        /// 向存档ItemStorage添加物品（已存在则叠加，不存在则新增，上限5亿）
        /// </summary>
        private void AddItemToStorage(int itemId, int addNum)
        {
            var existingItem = archiveModel.CurrentData.ItemStorage.Find(item => item.Id == itemId);
            if (existingItem != null)
            {
                existingItem.Num = Mathf.Min(existingItem.Num + addNum, MaxRewardLimit);
            }
            else
            {
                archiveModel.CurrentData.ItemStorage.Add(new ItemData
                {
                    Id = itemId,
                    Num = Mathf.Min(addNum, MaxRewardLimit)
                });
            }
        }
        #endregion
        #region 处理精元奖励
        public void ProcessGetgetMoleculeBattleReward(int itemId)
        {
            var targetItem = archiveModel.CurrentData.ItemStorage.FirstOrDefault(item => item.Id == itemId);
            if (targetItem == null)
            {
                archiveModel.CurrentData.ItemStorage.Add(new ItemData
                {
                    Id = itemId,
                    Num = 1
                });
            }
            else
            {
                targetItem.Num += 1;
            }

            archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);
            var itemConfig = itemModel.GetById(itemId);
            Dialog1Manager.Instance.ShowPopupOnTransform(uiTransform, "", $"恭喜你获得了{itemConfig.name}！");
        }
        #endregion

        #region 处理事件奖励掉落
        /// <summary>
        /// 处理地图NPC eventHandler中button的reward道具掉落
        /// 根据rate概率判断是否掉落，掉落则添加到存档ItemStorage
        /// </summary>
        private void ProcessRewardItems(int rateMultiplier = 1)
        {
            var npc = BattleDataManager.SelectedNpc;
            var rewards = npc?.eventHandler?.button?.reward;
            if (rewards == null || rewards.Count == 0)
                return;

            var droppedItems = new List<string>();

            foreach (var reward in rewards)
            {
                // 概率判断：应用掉落率倍率，上限100%
                int boostedRate = Mathf.Min(reward.Rate * rateMultiplier, 100);
                int roll = Random.Range(0, 100);
                if (roll >= boostedRate)
                {
                    SeerGoLogger.Log($"[战斗奖励] 道具掉落未触发: Id={reward.Id}, Rate={reward.Rate}%, BoostedRate={boostedRate}%, Roll={roll}");
                    continue;
                }

                // 使用AddItemToStorage添加奖励物品，自动受5亿上限约束
                AddItemToStorage(reward.Id, reward.Num);

                // 记录掉落物品名称
                var itemConfig = itemModel.GetById(reward.Id);
                string itemName = itemConfig != null ? itemConfig.name : $"物品{reward.Id}";
                droppedItems.Add($"{itemName} x{reward.Num}");
            }

            // 保存存档
            archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);

            // 统一弹窗提示
            if (droppedItems.Count > 0)
            {
                string msg = droppedItems.Count == 1
                    ? $"恭喜你获得了{droppedItems[0]}！"
                    : $"恭喜你获得了{string.Join("、", droppedItems)}！";
                Dialog1Manager.Instance.ShowPopupOnTransform(uiTransform, "", msg);
            }
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 计算从敌方宠物获得的总经验值
        /// 公式：每只宠物经验 = 基础经验值 × 对手等级 ÷ 11
        /// </summary>
        /// <param name="enemyPets">敌方宠物列表</param>
        /// <returns>总经验值</returns>
        private int CalculateTotalExpGained(List<Pet> enemyPets, int expMultiplier = 1)
        {
            int totalExp = 0;

            for (int i = 0; i < enemyPets.Count; i++)
            {
                // 获取敌方宠物的基础数据
                MonsterData monsterData = petInfoSystem.GetMonsterById(enemyPets[i].Id.ToInt());
                if (monsterData != null)
                {
                    // 经验基础公式：基础经验值 × 对手等级 ÷ 11   5倍经验值 × 翻倍倍率
                    totalExp += monsterData.YieldingExp * enemyPets[i].Level / 11 * 5 * expMultiplier;
                }
            }

            return totalExp;
        }

        /// <summary>
        /// 为宠物分配从战斗中获得的努力值(EV)
        /// 总EV限制：每项能力最大255，总计最大510
        /// </summary>
        /// <param name="petData">我方宠物数据</param>
        /// <param name="enemyPets">敌方宠物列表</param>
        private void ApplyEVGain(PetData petData, List<Pet> enemyPets, int evMultiplier = 1)
        {
            for (int i = 0; i < enemyPets.Count; i++)
            {
                MonsterData monsterData = petInfoSystem.GetMonsterById(enemyPets[i].Id.ToInt());
                if (monsterData == null) continue;

                // 为每只宠物分配EV
                ApplyMonsterEV(petData, monsterData, evMultiplier);
            }
        }

        /// <summary>
        /// 应用单只怪物提供的EV值
        /// 按顺序分配：HP → 攻击 → 防御 → 特攻 → 特防 → 速度
        /// </summary>
        /// <param name="petData">我方宠物数据</param>
        /// <param name="monsterData">怪物基础数据</param>
        private void ApplyMonsterEV(PetData petData, MonsterData monsterData, int evMultiplier = 1)
        {
            // 计算当前总EV值
            int currentTotalEV = CalculateTotalEV(petData.Talent.EV);
            int remainingEVCapacity = 510 - currentTotalEV; // 剩余可分配的EV总量

            if (remainingEVCapacity <= 0)
                return; // 如果已超过或等于510，则不进行任何增加

            // EV分配顺序数组（应用翻倍倍率）
            var evStatOrder = new[]
            {
                ("HP", EVHelper.GetEVByStat(monsterData.YieldingEV, "HP") * evMultiplier),
                ("ATK", EVHelper.GetEVByStat(monsterData.YieldingEV, "ATK") * evMultiplier),
                ("DEF", EVHelper.GetEVByStat(monsterData.YieldingEV, "DEF") * evMultiplier),
                ("SPATK", EVHelper.GetEVByStat(monsterData.YieldingEV, "SPATK") * evMultiplier),
                ("SPDEF", EVHelper.GetEVByStat(monsterData.YieldingEV, "SPDEF") * evMultiplier),
                ("SPD", EVHelper.GetEVByStat(monsterData.YieldingEV, "SPD") * evMultiplier)
            };

            // 按顺序分配各项EV
            foreach (var (statName, evValue) in evStatOrder)
            {
                if (remainingEVCapacity <= 0)
                    break; // 如果没有剩余容量，停止分配

                int evToAdd = Mathf.Min(evValue, remainingEVCapacity);
                AddEVToStat(petData.Talent.EV, statName, evToAdd);
                remainingEVCapacity -= evToAdd;
            }
        }

        /// <summary>
        /// 计算宠物当前总EV值
        /// </summary>
        /// <param name="evData">宠物的EV数据</param>
        /// <returns>总EV值</returns>
        private int CalculateTotalEV(PetEV evData)
        {
            return evData.Hp + evData.Atk + evData.Def +
                   evData.SpAtk + evData.SpDef + evData.Spd;
        }

        /// <summary>
        /// 为特定能力增加EV值
        /// 每项能力最大值为255
        /// </summary>
        /// <param name="evData">宠物的EV数据</param>
        /// <param name="statName">能力名称</param>
        /// <param name="evToAdd">要增加的EV值</param>
        private void AddEVToStat(PetEV evData, string statName, int evToAdd)
        {
            switch (statName)
            {
                case "HP":
                    evData.Hp = Mathf.Min(255, evData.Hp + evToAdd);
                    break;
                case "ATK":
                    evData.Atk = Mathf.Min(255, evData.Atk + evToAdd);
                    break;
                case "DEF":
                    evData.Def = Mathf.Min(255, evData.Def + evToAdd);
                    break;
                case "SPATK":
                    evData.SpAtk = Mathf.Min(255, evData.SpAtk + evToAdd);
                    break;
                case "SPDEF":
                    evData.SpDef = Mathf.Min(255, evData.SpDef + evToAdd);
                    break;
                case "SPD":
                    evData.Spd = Mathf.Min(255, evData.Spd + evToAdd);
                    break;
            }
        }

        /// <summary>
        /// 显示经验获得对话框
        /// 对话框显示后将触发经验计算和升级流程
        /// </summary>
        /// <param name="petData">宠物数据</param>
        /// <param name="expGained">获得的总经验值</param>
        /// <param name="uiTransform">UI父级变换</param>
        private void ShowExpDistributionDialog(PetData petData, int expGained)
        {
            if (uiTransform == null)
            {
                SeerGoLogger.LogError("无法显示经验对话框：UI变换为空");
                return;
            }

            // 格式化显示文本
            string finalText = string.Format("你的<color=#FF0000>{0}</color>获得了<color=#0066FF>{1}</color>点经验，更喜欢你了呢！",
                petData.Basic.Name, expGained);

            // 显示经验获得对话框
            Dialog1Manager.Instance.ShowPopupOnTransform(
                uiTransform,
                null,
                finalText,
                onConfirm: () => ProcessExpAndLevelUp(petData, expGained, uiTransform)
            );
        }

        /// <summary>
        /// 处理经验计算和升级逻辑
        /// </summary>
        /// <param name="petData">宠物数据</param>
        /// <param name="expGained">获得的总经验值</param>
        /// <param name="uiTransform">UI父级变换</param>
        private void ProcessExpAndLevelUp(PetData petData, int expGained, Transform uiTransform)
        {
            // 记录升级前的等级
            int originLevel = petData.Exp.Level;

            // 计算经验值（包含可能的多级升级）
            expConfigSystem.CalcExp(petData, expGained, uiTransform);

            // 重新计算所有属性
            List<int> addNums = petAttributeCalcSystem.CalculateAllStats(petData);

            // 检查是否升级
            int newLevel = petData.Exp.Level - originLevel;
            if (newLevel > 0)
            {
                // 添加升级级数到显示列表
                addNums.Add(newLevel);

                // 显示升级对话框
                ShowLevelUpDialog(addNums, petData, expGained);
            }
        }

        /// <summary>
        /// 显示升级对话框
        /// 展示升级后的属性变化
        /// </summary>
        /// <param name="uiTransform">UI父级变换</param>
        /// <param name="addNums">各项能力增加值列表</param>
        /// <param name="petData">宠物数据</param>
        /// <param name="expGained">获得的总经验值</param>
        private void ShowLevelUpDialog(List<int> addNums, PetData petData, int expGained)
        {
            Dialog1LevelUpManager.Instance.ShowPopupImmediatelyOnTransform(
                uiTransform,
                addNums,
                petData,
                expGained
            );
        }

        /// <summary>
        /// 显示捉宠对话框
        /// </summary>
        private void ShowCatchPetDialog(bool isBagFull, bool isCatch, PetData petData)
        {
            if (isCatch)
            {
                Dialog1Manager.Instance.ShowPopupOnTransform(
                    uiTransform,
                    "",
                    isBagFull ? "恭喜！捕捉成功，你可以在精灵仓库中找到它哦！" : "恭喜你！捕捉成功！"
               );
            }
            else
            {
                Dialog1GetPetManager.Instance.ShowPopupOnTransform(
                    uiTransform,
                    isBagFull ? $"恭喜你获得了<color=green>{petData.Basic.Name}</color>，背包已满，已放入仓库" : $"恭喜你获得了<color=green>{petData.Basic.Name}</color>，你可以点击右下方的精灵按钮",
                    petData,
                    isBagFull
                );
            }
        }

        #endregion

        #region 处理试炼之塔战斗奖励
        /// <summary>
        /// 处理试炼之塔战斗奖励
        /// 首次通关：发放完整奖励（赛尔豆、道具、金豆、精灵、经验池）
        /// 非首次通关：仅发放赛尔豆、金豆、经验池
        /// </summary>
        public void ProcessTrialTowerReward()
        {
            int currentFloor = archiveModel.CurrentData.YiteRogueData.CurrentTrialTowerFloor;
            // 回推刚刚通关的楼层
            // 正常情况：BattleManageController已把current+1，beatenFloor = current - 1
            // 上限层数：current未+1，由TowerBeatenFloorOverride指定实际通关楼层
            int beatenFloor;
            if (BattleDataManager.TowerBeatenFloorOverride > 0)
            {
                beatenFloor = BattleDataManager.TowerBeatenFloorOverride;
                BattleDataManager.TowerBeatenFloorOverride = 0; // 使用后重置
            }
            else
            {
                beatenFloor = currentFloor > 1 ? currentFloor - 1 : 1;
            }

            var config = TrialTowerConfigLoader.LoadConfig();
            var rewardEntry = config?.GetRewardData(beatenFloor);

            if (rewardEntry == null)
            {
                SeerGoLogger.LogWarning($"[试炼之塔奖励] 第{beatenFloor}层无奖励配置");
                return;
            }

            var rewardParts = new List<string>();

            // 不管是否首次通关，经验池都累加到 PetTotalExp（上限5亿）
            if (rewardEntry.expPool > 0)
            {
                archiveModel.CurrentData.PetTotalExp = Mathf.Min(
                    archiveModel.CurrentData.PetTotalExp + rewardEntry.expPool,
                    MaxRewardLimit
                );
                SeerGoLogger.Log($"[试炼之塔奖励] 经验池 +{rewardEntry.expPool}，当前PetTotalExp={archiveModel.CurrentData.PetTotalExp}");
            }

            // ---- 非首次通关：仅发放货币 + 经验池 ----
            if (!BattleDataManager.TrialTowerIsFirstClear)
            {
                if (rewardEntry.coins > 0)
                {
                    AddItemToStorage(1, rewardEntry.coins);
                    rewardParts.Add($"{rewardEntry.coins}赛尔豆");
                }
                if (rewardEntry.goldBeans > 0)
                {
                    AddItemToStorage(2, rewardEntry.goldBeans);
                    rewardParts.Add($"{rewardEntry.goldBeans}金豆");
                }
                if (rewardEntry.expPool > 0)
                {
                    rewardParts.Add($"经验池{rewardEntry.expPool}点");
                }

                archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);

                string nonFirstMsg = rewardParts.Count > 0
                    ? $"第{beatenFloor}层非首次通关，获得{string.Join("、", rewardParts)}！"
                    : $"第{beatenFloor}层非首次通关。";
                Dialog1Manager.Instance.ShowPopupOnTransform(uiTransform, "", nonFirstMsg);

                SeerGoLogger.Log($"[试炼之塔奖励] 第{beatenFloor}层非首次通关，奖励已发放: coins={rewardEntry.coins}, goldBeans={rewardEntry.goldBeans}, expPool={rewardEntry.expPool}");
                return;
            }

            // ---- 首次通关：完整奖励 ----
            // 1. 发放赛尔豆
            if (rewardEntry.coins > 0)
            {
                AddItemToStorage(1, rewardEntry.coins);
                rewardParts.Add($"{rewardEntry.coins}赛尔豆");
            }

            // 2. 发放道具
            if (rewardEntry.items != null)
            {
                foreach (var item in rewardEntry.items)
                {
                    int itemId = int.TryParse(item.itemId, out int parsed) ? parsed : 0;
                    if (itemId > 0 && item.count > 0)
                    {
                        AddItemToStorage(itemId, item.count);
                        var itemConfig = itemModel.GetById(itemId);
                        string itemName = itemConfig != null ? itemConfig.name : $"物品{itemId}";
                        rewardParts.Add($"{itemName} x{item.count}");
                    }
                }
            }

            // 3. 发放金豆
            int totalGoldBeans = rewardEntry.goldBeans + rewardEntry.milestoneGoldBeans;
            if (totalGoldBeans > 0)
            {
                AddItemToStorage(2, totalGoldBeans);
                rewardParts.Add($"{totalGoldBeans}金豆");
            }

            // 4. 经验池提示
            if (rewardEntry.expPool > 0)
            {
                rewardParts.Add($"经验池{rewardEntry.expPool}点");
            }
            ProcessTrialTowerPetReward(rewardEntry.petRewardId);
            // 5. 保存存档
            archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);

            SeerGoLogger.Log($"[试炼之塔奖励] 第{beatenFloor}层首次通关，奖励已发放: coins={rewardEntry.coins}, goldBeans={totalGoldBeans}, items={rewardEntry.items?.Length ?? 0}, petRewardId={rewardEntry.petRewardId}, expPool={rewardEntry.expPool}");
        }

        /// <summary>
        /// 发放试炼之塔精灵奖励（在确认弹窗后执行）
        /// </summary>
        private void ProcessTrialTowerPetReward(int petRewardId)
        {
            if (petRewardId > 0)
            {
                ProcessCatchOrGetPetBattleReward(petRewardId, false);
            }
        }
        #endregion

        #region 奖励翻倍Buff辅助方法
        /// <summary>
        /// 获取指定buffType的当前激活倍率（同类型只存在一个，不同倍率使用时会替换）
        /// </summary>
        private int GetActiveMultiplier(string buffType)
        {
            var buff = archiveModel.CurrentData.RewardBuffs
                .FirstOrDefault(b => b.BuffType == buffType && b.RemainCount > 0);
            return buff != null ? buff.Multiplier : 1;
        }

        /// <summary>
        /// 消耗指定buffType的所有激活buff次数
        /// </summary>
        private void ConsumeRewardBuffs(string buffType)
        {
            for (int i = archiveModel.CurrentData.RewardBuffs.Count - 1; i >= 0; i--)
            {
                var buff = archiveModel.CurrentData.RewardBuffs[i];
                if (buff.BuffType == buffType && buff.RemainCount > 0)
                {
                    buff.RemainCount--;
                    if (buff.RemainCount <= 0)
                    {
                        archiveModel.CurrentData.RewardBuffs.RemoveAt(i);
                        SeerGoLogger.Log($"<color=cyan>[RewardBuff] buff已用完: type={buffType}, id={buff.BuffId}</color>");
                    }
                }
            }
        }

        /// <summary>
        /// 消耗所有类型的激活buff次数（每次战斗消耗1次）
        /// </summary>
        private void ConsumeAllRewardBuffs()
        {
            ConsumeRewardBuffs("exp");
            ConsumeRewardBuffs("seerBean");
            ConsumeRewardBuffs("dropRate");
        }

        /// <summary>
        /// 构建倍率提示信息
        /// </summary>
        private string BuildMultiplierInfo(int expMultiplier, int seerBeanMultiplier)
        {
            var parts = new List<string>();
            if (expMultiplier > 1) parts.Add($"经验&学习力×{expMultiplier}");
            if (seerBeanMultiplier > 1) parts.Add($"赛尔豆×{seerBeanMultiplier}");

            if (parts.Count > 0)
                return $"（{string.Join("，", parts)}）";

            return "";
        }
        #endregion
    }
}
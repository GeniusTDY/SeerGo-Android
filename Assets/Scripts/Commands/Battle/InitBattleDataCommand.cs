using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace SeerGoGame.Commands
{
    public class InitBattleDataCommand : AbstractCommand
    {
        protected override async void OnExecute()
        {
            // 从BattleDataManager恢复存档数据到BattleArchitecture
            RestoreArchiveData();

            await LoadAllConfigs();

            // 加载AssetBundles
            LoadAssetBundles();
        }

        private void RestoreArchiveData()
        {
            // 从BattleDataManager获取缓存的存档数据
            var cachedData = BattleDataManager.CachedArchiveData;

            if (cachedData != null)
            {
                // 恢复到BattleArchitecture的ArchiveModel
                var battleArchiveModel = this.GetModel<IArchiveModel>();
                battleArchiveModel.CurrentData = cachedData;
                SeerGoLogger.Log($"已从缓存恢复存档数据到BattleArchitecture，精灵数量: {cachedData.PetBag.Count}");
            }
            else
            {
                SeerGoLogger.LogWarning("BattleDataManager中没有缓存的存档数据，尝试从文件加载");
                LoadLatestSave();
            }
        }

        private void LoadLatestSave()
        {
            var archiveSystem = this.GetSystem<IArchiveSystem>();
            string[] saves = archiveSystem.GetAllSaves();

            if (saves.Length > 0)
            {
                this.SendCommand(new LoadGameCommand(saves[0]));
            }
            else
            {
                SeerGoLogger.LogWarning("存档错误");
            }
        }

        private async Task LoadAllConfigs()
        {
            var tasks = new List<Task>
            {
                LoadConfigAsync(() => this.GetModel<IPetInfoModel>().LoadMonsterConfigAsync()),
                LoadConfigAsync(() => this.GetModel<IPersonalityModel>().LoadNatureInfoConfigAsync()),
                LoadConfigAsync(() => this.GetModel<IAttributeModel>().LoadAttributeConfigAsync()),
                LoadConfigAsync(() => this.GetModel<ISkillModel>().LoadSkillData()),
                LoadConfigAsync(() => this.GetModel<ISkillModel>().LoadSkillEffectInfoConfigAsync()),
                LoadConfigAsync(() => this.GetModel<ITypeRelationModel>().LoaTypeRelationConfigAsync()),
                LoadConfigAsync(() => this.GetModel<IAnimationFrameModel>().LoadAnimationConfigAsync()),
                LoadConfigAsync(() => this.GetModel<IBattleEffectModel>().LoadBattleEffectInfoConfigAsync()),
            };

            foreach (var task in tasks)
            {
                await task;
            }
        }

        private async Task LoadConfigAsync(Func<Task> loadFunc)
        {
            await loadFunc();
            BattleLoadProgressController.Instance.CompleteTask("battleData");
        }

        private void LoadAssetBundles()
        {
            var manager = ExternalAssetBundleManager.Instance;
            //manager.SetCustomPath("D:\\Dev\\FFDec\\Test\\Assets\\PC");
            //manager.SetCustomPath(Path.Combine(Application.persistentDataPath, "Android/"));
            manager.SetCustomPath(GetBundleDirectory());
            var assets = GetAssetBundleList();

            // 使用协程而不是await
            manager.StartCoroutine(LoadBundlesCoroutine(assets));
        }

        private string GetBundleDirectory()
        {
            #if UNITY_EDITOR
                return Path.Combine(Application.dataPath, "../GameAnim/PetFightAnim/");
            #elif UNITY_ANDROID
                return Path.Combine(Application.persistentDataPath, "GameAnim/PetFightAnim/");
            #else
                return Path.Combine(Application.dataPath, "../GameAnim/PetFightAnim/");
            #endif
        }

        private IEnumerator LoadBundlesCoroutine(List<string> assets)
        {
            yield return ExternalAssetBundleManager.Instance.LoadAssetBundlesAsync(
                assets,
                success =>
                {
                    if (success)
                    {
                        SeerGoLogger.Log("AssetBundles 加载成功");
                        BattleLoadProgressController.Instance.CompleteTask("battleData");
                        this.SendEvent<FightDataLoadSuccess>();
                    }
                    else
                    {
                        SeerGoLogger.LogError("AssetBundles 加载失败");
                    }
                }
            );
        }

        private List<string> GetAssetBundleList()
        {
            var assets = new List<string>();
            var archiveModel = this.GetModel<IArchiveModel>();

            foreach (var pet in archiveModel.CurrentData.PetBag)
            {
                assets.Add($"pfa_{pet.Id}");
            }

            var monsters = BattleDataManager.SelectedNpc.battle.branch.enemy.pet;
            foreach (var monster in monsters)
            {
                assets.Add($"pfa_{monster.Id}");
            }

            return assets;
        }
    }
}
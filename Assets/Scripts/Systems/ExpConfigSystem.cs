using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using Transform = UnityEngine.Transform;

/// <summary>
/// 经验值配置系统实现
/// </summary>
namespace SeerGoGame.Systems
{
    public class ExpConfigSystem : AbstractSystem, IExpConfigSystem
    {
        private IExpConfigModel _expConfigModel;
        private Dictionary<int, ExpConfig> _levelConfigDict;

        private IArchiveModel _archiveModel;
        private IExpConfigSystem _expConfigSystem;
        private IPetInfoSystem _petInfoSystem;
        private ISkillSystem _skillSystem;
        private IArchiveSystem archiveSystem;

        // 新增：用于管理技能学习队列
        private Queue<SkillLearnRequest> _skillLearnQueue;
        private bool _isProcessingSkillLearn;

        protected override void OnInit()
        {
            _expConfigModel = this.GetModel<IExpConfigModel>();
            // 初始化系统
            _archiveModel = this.GetModel<IArchiveModel>();
            _expConfigSystem = this.GetSystem<IExpConfigSystem>();
            _petInfoSystem = this.GetSystem<IPetInfoSystem>();
            _skillSystem = this.GetSystem<ISkillSystem>();
            archiveSystem = this.GetSystem<IArchiveSystem>();

            // 初始化技能学习队列
            _skillLearnQueue = new Queue<SkillLearnRequest>();
            _isProcessingSkillLearn = false;
        }

        public void InitializeData()
        {
            // 安全地创建字典
            _levelConfigDict = new Dictionary<int, ExpConfig>();
            foreach (var config in _expConfigModel.ExpConfigs)
            {
                if (config != null)
                {
                    if (!_levelConfigDict.ContainsKey(config.Level))
                    {
                        _levelConfigDict[config.Level] = config;
                    }
                    else
                    {
                        SeerGoLogger.LogWarning($"发现重复的等级配置: {config.Level}");
                    }
                }
            }
        }

        /// <summary>
        /// 根据等级获取经验值配置
        /// </summary>
        /// <param name="level">等级</param>
        /// <returns>经验值配置</returns>
        public ExpConfig GetByLevel(int level)
        {
            if (_levelConfigDict == null || !_levelConfigDict.ContainsKey(level))
            {
                SeerGoLogger.LogWarning($"未找到等级{level}的经验值配置");
                return null;
            }

            return _levelConfigDict[level];
        }

        /// <summary>
        /// 获取指定等级和类型的经验值
        /// </summary>
        /// <param name="level">等级</param>
        /// <param name="type">类型索引</param>
        /// <returns>经验值</returns>
        public int GetExpValue(int level, int type)
        {
            var config = GetByLevel(level);
            if (config == null) return 0;

            return type switch
            {
                0 => config.Type_0,
                2 => config.Type_2,
                3 => config.Type_3,
                4 => config.Type_4,
                5 => config.Type_5,
                _ => 0
            };
        }

        public void CalcExp(PetData petData, int expGained, Transform transform = null)
        {
            // 提取宠物经验数据的局部引用
            var petExp = petData.Exp;

            // 语义化命名变量，提升可读性
            int currentExp = petExp.CurrentExp;
            int currentPetLevel = petExp.Level;
            int petGrowthType = petData.Exp.GrowthType;

            // 循环升级逻辑：从当前等级+1到100级
            for (int targetLevel = currentPetLevel + 1; targetLevel <= 100; targetLevel++)
            {
                // 获取升级到目标等级所需的经验值
                int expRequiredForLevelUp = _expConfigSystem.GetExpValue(targetLevel, petGrowthType);

                if (currentExp > expGained)
                {
                    UpdatePetExp(currentExp - expGained, currentPetLevel, petData, transform);
                    break;
                }

                if (currentExp == expGained)
                {
                    UpdatePetExp(expRequiredForLevelUp, targetLevel, petData, transform);
                    break;
                }

                if (currentExp + expRequiredForLevelUp > expGained)
                {
                    int remainingExp = currentExp + expRequiredForLevelUp - expGained;
                    UpdatePetExp(remainingExp, targetLevel, petData, transform);
                    break;
                }

                // 满级处理（100级）
                if (targetLevel == 100)
                {
                    UpdatePetExp(0, targetLevel, petData, transform);
                    break;
                }

                // 经验未满足升级，累加经验继续循环
                currentExp += expRequiredForLevelUp;
            }
        }

        /// <summary>
        /// 封装宠物经验/等级更新、存档保存、资源加载的通用逻辑
        /// </summary>
        /// <param name="newCurrentExp">新的当前经验值</param>
        /// <param name="newLevel">新的等级</param>
        private void UpdatePetExp(int newCurrentExp, int newLevel, PetData petData, Transform transform = null)
        {
            // 提取当前选中宠物的局部变量
            PetData currentPet = petData;
            var petExp = currentPet.Exp;

            // 更新经验和等级
            petExp.CurrentExp = newCurrentExp;
            petExp.Level = newLevel;

            // 获取进化链数据 & 判断是否可进化
            List<MonsterData> evolutionChain = _petInfoSystem.GetEvolutionChain(currentPet.Id);
            bool canEvolve = _petInfoSystem.CanMonsterEvolve(currentPet.Id, petExp.Level);

            if (canEvolve)
            {
                // 进化精灵id
                int currentEvolutionId = _petInfoSystem.GetMonsterById(currentPet.Id).EvolvesTo;
                foreach (var monsterData in evolutionChain)
                {
                    bool canEvolveToThisMonster = _petInfoSystem.CanMonsterEvolve(monsterData.ID, petExp.Level);

                    if (canEvolveToThisMonster)
                    {
                        currentEvolutionId = monsterData.EvolvesTo;
                    }

                    MonsterData currentMonsterData = _petInfoSystem.GetMonsterById(currentEvolutionId);
                    UpdatePetIdentityInfo(currentPet, currentMonsterData);
                }
            }
            // 处理技能
            MonsterData newMonsterData = _petInfoSystem.GetMonsterById(currentPet.Id);

            List<LearnableMove> learnableMoves = newMonsterData.LearnableMoves;
            // 临时技能列表
            List<LearnableMove> newTempMoves = new List<LearnableMove>();

            for (int j = 0; j < learnableMoves.Count; j++)
            {
                // 可学习技能
                LearnableMove currentLearnableMove = learnableMoves[j];
                // 判断存档精灵技能是否已有，没有则添加
                if (newLevel >= currentLearnableMove.LearningLv && !currentPet.Skills.OwnSkillId.Contains(currentLearnableMove.ID))
                {
                    currentPet.Skills.OwnSkillId.Add(currentLearnableMove.ID);
                    newTempMoves.Add(currentLearnableMove);
                }
            }
            // 保存升级后数据存档
            archiveSystem.SaveGame("autosave0", _archiveModel.CurrentData);

            ProcessPetLearnableSkills(currentPet, newMonsterData, newLevel, newTempMoves, transform);
        }

        /// <summary>
        /// 更新宠物的标识信息（各模块ID、基础名称等）
        /// </summary>
        private void UpdatePetIdentityInfo(PetData pet, MonsterData monsterData)
        {
            pet.Id = monsterData.ID;
            pet.Basic.Id = monsterData.ID;
            pet.Basic.Name = monsterData.DefName;
            pet.Feature.Id = monsterData.ID;
            pet.Exp.Id = monsterData.ID;
            pet.Talent.Id = monsterData.ID;
            pet.Skills.Id = monsterData.ID;
            pet.UI.Id = monsterData.ID;

            // 更新技能PP
            for (int i = 0; i < pet.Skills.NormalSkill.Count; i++)
            {
                pet.Skills.NormalSkill[i].currentPP = pet.Skills.NormalSkill[i].maxPP;
            }
        }

        /// <summary>
        /// 处理宠物可学习技能列表
        /// </summary>
        private void ProcessPetLearnableSkills(PetData pet, MonsterData monsterData, int currentLevel, List<LearnableMove> newTempMoves, Transform transform = null)
        {

            for (int j = 0; j < newTempMoves.Count; j++)
            {
                // 可学习技能
                LearnableMove currentLearnableMove = newTempMoves[j];
                // 可学习技能信息
                SkillData skillData = _skillSystem.GetSkillById(currentLearnableMove.ID.ToString());

                // 判断精灵上场技能是否不到4个
                if (pet.Skills.NormalSkill.Count < 4)
                {
                    // 不到4个则添加
                    EquippedSkill equippedSkill = new EquippedSkill(
                        currentLearnableMove.ID,
                        skillData.MaxPP,
                        skillData.MaxPP);
                    pet.Skills.NormalSkill.Add(equippedSkill);

                    // 将技能学习请求加入队列，而不是立即弹窗
                    var request = new SkillLearnRequest
                    {
                        Pet = pet,
                        SkillData = skillData,
                        Transform = transform,
                        LearnableMove = currentLearnableMove,
                        IsReplaceSkill = false
                    };
                    _skillLearnQueue.Enqueue(request);
                }
                else
                {
                    // 将技能替换请求加入队列
                    var request = new SkillLearnRequest
                    {
                        Pet = pet,
                        Transform = transform,
                        LearnableMove = currentLearnableMove,
                        IsReplaceSkill = true
                    };
                    _skillLearnQueue.Enqueue(request);
                }
            }

            // 开始处理技能学习队列（如果不在处理中且队列不为空）
            if (!_isProcessingSkillLearn && _skillLearnQueue.Count > 0 && transform != null)
            {
                ProcessNextSkillLearn();
            }
        }

        /// <summary>
        /// 处理队列中的下一个技能学习请求
        /// </summary>
        private void ProcessNextSkillLearn()
        {
            if (_skillLearnQueue.Count == 0)
            {
                _isProcessingSkillLearn = false;
                archiveSystem.SaveGame("autosave0", _archiveModel.CurrentData);

                return;
            }

            _isProcessingSkillLearn = true;
            SkillLearnRequest request = _skillLearnQueue.Dequeue();

            if (request.IsReplaceSkill)
            {
                // 技能替换逻辑
                Dialog2SkillReplaceManager.Instance.ShowPopupOnTransform(
                    request.Transform,
                    request.Pet,
                    _skillSystem.GetSkillById(request.LearnableMove.ID.ToString()),
                    (selectedIndex) =>  // 确认回调，接收选中的技能下标
                    {
                        // 替换技能弹窗
                        Dialog2SkillReplaceConfirmManager.Instance.ShowConfirmImmediatelyOnTransform(
                            request.Transform,
                            request.Pet.Skills.NormalSkill[selectedIndex].id,
                            request.LearnableMove.ID,
                            onConfirm: () =>
                            {
                                // 安全地执行替换
                                if (selectedIndex >= 0 && selectedIndex < request.Pet.Skills.NormalSkill.Count)
                                {
                                    SkillData skillData = _skillSystem.GetSkillById(request.LearnableMove.ID.ToString());
                                    EquippedSkill equippedSkill = new EquippedSkill(
                                        request.LearnableMove.ID,
                                        skillData.MaxPP,
                                        skillData.MaxPP);

                                    request.Pet.Skills.NormalSkill[selectedIndex] = equippedSkill;
                                }
                                else
                                {
                                    SeerGoLogger.LogError($"替换失败: 无效索引{selectedIndex}");
                                }

                                // 显示成功提示
                                Dialog1Manager.Instance.ShowPopupImmediatelyOnTransform(
                                    request.Transform,
                                    "",
                                    "恭喜你！宠物技能学习成功！",
                                    onConfirm: () => ProcessNextSkillLearn() // 处理下一个技能
                                );
                            },
                            onCancel: () =>
                            {
                                // 用户取消替换，但技能已经添加到拥有技能列表，只是不替换上场技能
                                ProcessNextSkillLearn(); // 继续处理下一个技能
                            }
                        );
                    },
                    onCancel: () =>
                    {
                        // 用户取消选择替换位置，但技能已经添加到拥有技能列表
                        ProcessNextSkillLearn(); // 继续处理下一个技能
                    }
                );
            }
            else
            {
                // 直接学习新技能逻辑
                ShowDialog(request.Pet, request.Transform, request.LearnableMove);
            }
        }

        /// <summary>
        /// 显示技能学习对话框
        /// </summary>
        private void ShowDialog(PetData pet, Transform transform, LearnableMove learnableMove)
        {
            Dialog1SkillManager.Instance.ShowPopupOnTransform(
                transform,
                _skillSystem.GetSkillById(learnableMove.ID.ToString()),
                pet,
                onConfirm: () => ProcessNextSkillLearn() // 对话框关闭后处理下一个技能
            );
        }

        /// <summary>
        /// 技能学习请求数据结构
        /// </summary>
        private struct SkillLearnRequest
        {
            public PetData Pet;
            public Transform Transform;
            public SkillData SkillData;
            public LearnableMove LearnableMove;
            public bool IsReplaceSkill;
        }
    }
}
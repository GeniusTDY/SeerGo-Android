using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;

namespace SeerGoGame
{
    public class BattleArchitecture : Architecture<BattleArchitecture>
    {
        protected override void Init()
        {
            // 注册工具类到IOC容器
            RegisterModel<IBuffModel>(new BuffModel());
            RegisterModel<IItemModel>(new ItemModel());

            // 注册System
            RegisterSystem<BuffSystem>(new BuffSystem());
            RegisterSystem<BattleCalcUtilsHelper>(new BattleCalcUtilsHelper());
            RegisterSystem<ItemEffectManager>(new ItemEffectManager());

            // 注册模型到IOC容器
            RegisterModel<IArchiveModel>(new ArchiveModel());
            RegisterModel<IAttributeModel>(new AttributeModel());
            RegisterModel<ISkillModel>(new SkillModel());
            RegisterModel<ITypeRelationModel>(new TypeRelationModel());
            RegisterModel<IAnimationFrameModel>(new AnimationFrameModel());
            RegisterModel<IExpConfigModel>(new ExpConfigModel());
            RegisterModel<IBattleEffectModel>(new BattleEffectModel());
            RegisterModel<IPetInfoModel>(new PetInfoModel());
            RegisterModel<IPersonalityModel>(new PersonalityModel());


            // 注册系统
            RegisterSystem<IArchiveSystem>(new ArchiveSystem());
            RegisterSystem<IAttributeSystem>(new AttributeSystem());
            RegisterSystem<ISkillSystem>(new SkillSystem());
            RegisterSystem<ITypeRelationSystem>(new TypeRelationSystem());
            RegisterSystem<IAnimationFrameSystem>(new AnimationFrameSystem());
            RegisterSystem<IExpConfigSystem>(new ExpConfigSystem());
            RegisterSystem<IBattleEffectSystem>(new BattleEffectSystem());
            RegisterSystem<IPersonalitySystem>(new PersonalitySystem());
            RegisterSystem<IPetInfoSystem>(new PetInfoSystem());
            RegisterSystem<IPetAttributeCalcSystem>(new PetAttributeCalcSystem());
            RegisterSystem<IUISystem>(new UISystem());

        }


        // 全局访问点
        public static T GetModel<T>() where T : class, IModel
        {
            return Interface.GetModel<T>();
        }

        public static T GetUtility<T>() where T : class, IUtility
        {
            return Interface.GetUtility<T>();
        }
        void OnDestroy()
        {
            // 场景销毁时自动清理模块
            this.OnDeinit();
        }
    }
}


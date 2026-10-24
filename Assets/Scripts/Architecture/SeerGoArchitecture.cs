using QFramework;
using SeerGoGame;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame
{
    public class SeerGoArchitecture : Architecture<SeerGoArchitecture>
    {
        protected override void Init()
        {
            // 注册工具类到IOC容器
            RegisterUtility<IStorage>(new XmlStorage());
            RegisterUtility<IMapParser>(new MapXmlParser());
            RegisterUtility<IWorldMapXmlParser>(new WorldMapXmlParser());

            // 注册模型到IOC容器
            RegisterModel<IMapModel>(new MapModel());
            RegisterModel<IPlayerModel>(new PlayerModel());
            RegisterModel<IMapEffectModel>(new MapEffectModel());
            RegisterModel<IWorldMapModel>(new WorldMapModel());
            RegisterModel<IArchiveModel>(new ArchiveModel());
            RegisterModel<IAttributeModel>(new AttributeModel());
            RegisterModel<ISkillModel>(new SkillModel());
            RegisterModel<ITypeRelationModel>(new TypeRelationModel());
            RegisterModel<IAnimationFrameModel>(new AnimationFrameModel());
            RegisterModel<IExpConfigModel>(new ExpConfigModel());
            RegisterModel<IBattleEffectModel>(new BattleEffectModel());
            RegisterModel<IPetInfoModel>(new PetInfoModel());
            RegisterModel<IPersonalityModel>(new PersonalityModel());
            RegisterModel<IItemModel>(new ItemModel());

            // 注册系统
            RegisterSystem<IMapLoadSystem>(new MapLoadSystem());
            RegisterSystem<IUISystem>(new UISystem());
            RegisterSystem<ICreateSpriteEntitySystem>(new CreateSpriteEntitySystem());
            RegisterSystem<IAchievementSystem>(new AchievementSystem());
            RegisterSystem<IMapEffectSystem>(new MapEffectSystem());
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
            RegisterSystem<ItemEffectManager>(new ItemEffectManager());


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


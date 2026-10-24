using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;

// 性格系统实现
namespace SeerGoGame.Systems
{
    public class PersonalitySystem : AbstractSystem, IPersonalitySystem
    {
        private IPersonalityModel m_model;

        protected override void OnInit()
        {
            // 通过IOC容器获取Model实例
            m_model = this.GetModel<IPersonalityModel>();
        }

        public PersonalityData GetPersonalityById(int id)
        {
            var personality = m_model.GetById(id);
            if (personality == null)
            {
                SeerGoLogger.LogWarning($"未找到ID为 {id} 的性格数据");
            }
            return personality;
        }
    }
}
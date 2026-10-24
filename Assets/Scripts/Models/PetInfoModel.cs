using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SeerGoGame.Models
{
    /// <summary>
    /// 精灵数据模型实现 - 只负责数据加载
    /// </summary>
    public class PetInfoModel : AbstractModel, IPetInfoModel
    {
        public Dictionary<int, MonsterData> MonsterDict { get; private set; }
        public Dictionary<string, MonsterData> NameToMonsterDict { get; private set; }

        protected override void OnInit()
        {
            MonsterDict = new Dictionary<int, MonsterData>();
            NameToMonsterDict = new Dictionary<string, MonsterData>();
        }


        /// <summary>
        /// 加载精灵配置 - Model只负责数据加载
        /// </summary>
        /// 
        public async Task LoadMonsterConfigAsync()
        {
            string path = ResourcePathBuilder.GetPetsInfoDataPath();
            string fullPath = $"{path}/monsters";

            var result = await JsonResourceLoadManager.LoadConfigAsync<MonsterConfig>(fullPath);

            if (result.Success && result.Data?.Monsters != null)
            {
                foreach (var monster in result.Data.Monsters)
                {
                    MonsterDict[monster.ID] = monster;
                    NameToMonsterDict[monster.DefName] = monster;
                }

               // SeerGoLogger.Log($"Model: 成功加载 {MonsterDict.Count} 个精灵配置");
            }
        }
    
    }
}
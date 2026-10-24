using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;

// 性格数据模型实现
namespace SeerGoGame.Models
{
    public class PersonalityModel : AbstractModel, IPersonalityModel
    {
        private Dictionary<int, PersonalityData> m_personalityDict;

        public Dictionary<int, PersonalityData> PersonalityDict => m_personalityDict;
        public int PersonalityCount => m_personalityDict?.Count ?? 0;
        protected override void OnInit()
        {
            m_personalityDict = new Dictionary<int, PersonalityData>();
        }

        public async Task LoadNatureInfoConfigAsync()
        {
            string path = ResourcePathBuilder.GetPetsInfoDataPath();
            string fullPath = $"{path}/NatureInfo";

            var result = await JsonResourceLoadManager.LoadConfigAsync<PersonalityDataRoot>(fullPath);

            if (result.Success && result.Data?.personalities != null)
            {
                foreach (var personalities in result.Data.personalities)
                {
                    if (!m_personalityDict.ContainsKey(personalities.id))
                    {
                        m_personalityDict[personalities.id] = personalities;
                    }
                    else
                    {
                        SeerGoLogger.LogWarning($"重复的性格ID: {personalities.id}");
                    }
                }
               // SeerGoLogger.Log("性格数据加载成功。");
            }
        }

        public PersonalityData GetById(int id)
        {
            return m_personalityDict.TryGetValue(id, out var data) ? data : null;
        }

    }
}
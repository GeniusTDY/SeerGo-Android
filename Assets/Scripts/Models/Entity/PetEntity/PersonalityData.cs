// 性格修正修饰符数据结构
using System.Collections.Generic;

namespace SeerGoGame.Models
{
    [System.Serializable]
    public class PersonalityModifiers
    {
        public float m_attack;
        public float m_defence;
        public float m_SA;
        public float m_SD;
        public float m_speed;
    }

    // 单个性格数据
    [System.Serializable]
    public class PersonalityData
    {
        public int id;
        public string name;
        public PersonalityModifiers modifiers;
        public string desc;
    }

    // JSON文件根结构
    [System.Serializable]
    public class PersonalityDataRoot
    {
        public List<PersonalityData> personalities;
    }
}
using System;
using System.Collections.Generic;

namespace SeerGoGame.Models
{
    [Serializable]
    public class MoveSideEffectRoot
    {
        public List<MoveSideEffect> sideeffects;
    }
    
    [Serializable]
    public class MoveSideEffect
    {
        public int ID;
        public string des;
        public string help;
    }

    [Serializable]
    public class EffectInfoRoot
    {
        public EffectInfoData root;
    }
    
    [Serializable]
    public class EffectInfoData
    {
        public List<EffectInfo> Effect;
    }
    
    [Serializable]
    public class EffectInfo
    {
        public int id;
        public int argsNum;
        public string info;
    }
}
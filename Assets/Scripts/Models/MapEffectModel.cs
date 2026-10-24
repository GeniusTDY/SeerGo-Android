using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Models
{
    public class MapEffectModel : AbstractModel, IMapEffectModel
    {
        public int CurrentMapId { get; set; }
        public Dictionary<string, List<GameObject>> ActiveEffects { get; } = new Dictionary<string, List<GameObject>>();

        protected override void OnInit()
        {
            
        }
    }
}
using System.Collections.Generic;
using QFramework;
using SeerGoGame.Models;
using UnityEngine;

namespace SeerGoGame.Interfaces
{
    public interface IMapEffectModel: IModel
    {
        int CurrentMapId { get; set; }
        Dictionary<string, List<GameObject>> ActiveEffects { get; }
    }

}
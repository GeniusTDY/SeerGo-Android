using SeerGoGame.Models;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Events
{
    // 
    public struct CreateSpriteEventSuccessEvent
    {
        public List<GameObject> _resultList;

        public CreateSpriteEventSuccessEvent(List<GameObject> resultList)
        {
            _resultList = resultList;
        }
    }

 
}
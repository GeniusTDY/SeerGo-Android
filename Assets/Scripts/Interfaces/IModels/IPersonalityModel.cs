using QFramework;
using SeerGoGame.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

// Model²ã½Ó¿Ú

namespace SeerGoGame.Interfaces
{
    public interface IPersonalityModel : IModel
    {
        Dictionary<int, PersonalityData> PersonalityDict { get; }
        int PersonalityCount { get; }

        Task LoadNatureInfoConfigAsync();
        PersonalityData GetById(int id);
    }
}
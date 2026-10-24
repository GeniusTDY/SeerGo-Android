using SeerGoGame.Helper;
using UnityEngine;

public class PlayerAvatarPosHelper : MonoBehaviour
{
    // 静态实例，用于全局访问
    public static PlayerAvatarPosHelper Instance { get; private set; }

    private void Awake()
    {
        // 实现单例模式，确保只有一个实例存在
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); // 跨场景时不销毁
        }
    }

    // 通过地图分类id判断地图人物头像
    public void ChangePlanetAvatarById(int id, bool isActived)
    {
        GameObject planet = GameObjectDictionary.GetGameObject(id);
        if (planet != null)
        {
            PlanetItemUI planetItemUI = planet.GetComponent<PlanetItemUI>();
            planetItemUI.image.gameObject.SetActive(isActived);
        }
    }
}

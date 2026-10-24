using SeerGoGame.Controller;
using SeerGoGame.Models;
using UnityEngine;
using UnityEngine.UI;

public class PlanetItemUI : BaseController
{
    [Header("UI References")]
    public UnityEngine.UI.Button button;
    public Image image;

    public void SetPlanetData(PlanetData planetData)
    {
        RectTransform planetRect = button.GetComponent<RectTransform>();
        button.enabled = true;
        button.image.enabled = true;
        Sprite sprite = ResourcesManager.Instance.Load<Sprite>(planetData.ResId);
        button.image.sprite = sprite;
        //button.image.SetNativeSize();
        button.image.rectTransform.sizeDelta = new Vector2(80f, 80f);

        //设置星系背景位置
        planetRect.anchoredPosition = new Vector2(planetData.Position.x, planetData.Position.y);
        //缩放
        button.image.rectTransform.localScale = new Vector3(planetData.Size.x, planetData.Size.y, 1f);
    }

}
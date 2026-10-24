using SeerGoGame.Models;
using UnityEngine;

public class GalaxyItemUI : MonoBehaviour
{
    [Header("UI References")]
    public UnityEngine.UI.Button button;

    public void SetGalaxyData(GalaxyData galaxy)
    {
        RectTransform galaxyRect = button.GetComponent<RectTransform>();
        button.enabled = true;
        button.image.enabled = true;
        Sprite sprite = Resources.Load<Sprite>(galaxy.ResId);
        button.image.sprite = sprite;
        //galaxyRect.sizeDelta = new Vector2(300f,300f);
        // button.image.SetNativeSize();
        button.image.rectTransform.sizeDelta = new Vector2(300f, 300f);

        //设置星系背景位置
        galaxyRect.anchoredPosition = new Vector2(galaxy.Position.x, galaxy.Position.y);
        //缩放
        button.image.rectTransform.localScale = new Vector3(galaxy.Size.x, galaxy.Size.y, 1f);

    }


}
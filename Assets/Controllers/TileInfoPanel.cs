using UnityEngine;
using TMPro;

public class TileInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI tileInfoText;

    // Singleton for easy access
    public static TileInfoPanel Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string text)
    {
        if (tileInfoText != null)
        {
            tileInfoText.text = text;
        }
    }

    public void Clear()
    {
        if (tileInfoText != null)
        {
            tileInfoText.text = "Click a tile to see info";
        }
    }
}

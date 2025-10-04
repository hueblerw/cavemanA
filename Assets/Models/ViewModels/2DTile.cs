using UnityEngine;
using System.Collections.Generic;


namespace CavemanLand.Models.ViewModels
{
    public class 2DTile : MonoBehaviour
    {
        public SpriteRenderer groundLayer;
        public SpriteRenderer vegetationLayer;
        public SpriteRenderer terrainSymbol;
        public SpriteRenderer iconTopLeft;
        public SpriteRenderer iconTopRight;
        public SpriteRenderer iconBottomLeft;
        public SpriteRenderer iconBottomRight;

        public void Initialize(TileData data)
        {
            // Update ground layer
            groundLayer.sprite = GetGroundSprite(data.groundType);

            // Update vegetation layer
            // vegetationLayer.sprite = GetVegetationSprite(data.vegetationType);

            // Update terrain symbol
            // terrainSymbol.sprite = GetTerrainSymbolSprite(data.terrainSymbol);

            // Update icons
            /*
            iconTopLeft.sprite = data.icons.ContainsKey("topLeft") ? data.icons["topLeft"] : null;
            iconTopRight.sprite = data.icons.ContainsKey("topRight") ? data.icons["topRight"] : null;
            iconBottomLeft.sprite = data.icons.ContainsKey("bottomLeft") ? data.icons["bottomLeft"] : null;
            iconBottomRight.sprite = data.icons.ContainsKey("bottomRight") ? data.icons["bottomRight"] : null;
            */
        }

        private Sprite GetGroundSprite(string groundType)
        {
            // Load and return the appropriate sprite for the ground layer
            return Resources.Load<Sprite>($"Sprites/Ground/{groundType}");
        }

        private Sprite GetVegetationSprite(string vegetationType)
        {
            // Load and return the appropriate sprite for the vegetation layer
            return Resources.Load<Sprite>($"Sprites/Vegetation/{vegetationType}");
        }

        private Sprite GetTerrainSymbolSprite(string terrainSymbol)
        {
            // Load and return the appropriate sprite for the terrain symbol
            return Resources.Load<Sprite>($"Sprites/Terrain/{terrainSymbol}");
        }
    }
}

using UnityEngine;
using System.Collections.Generic;

namespace CavemanLand.Models.ViewModels
{
    // Class names cannot start with a digit. Renamed to Tile2D.
    public class Tile2D : MonoBehaviour
    {
        public SpriteRenderer groundLayer;
        public SpriteRenderer vegetationLayer;
        public SpriteRenderer terrainSymbol;
        public SpriteRenderer iconTopLeft;
        public SpriteRenderer iconTopRight;
        public SpriteRenderer iconBottomLeft;
        public SpriteRenderer iconBottomRight;

        public void Initialize(TileData2D data)
        {
            // Update ground layer with color coding (until sprites are ready)
            SetGroundColor(data.groundType);

            // Use elevation for tile positioning (visual depth)
            SetElevation(data.elevation);

            // Show vegetation (habitat) information
            SetVegetationDisplay(data.vegetationType, data.vegetationAmount);

            // Show terrain symbols (hills/mountains)
            SetTerrainSymbol(data.terrainSymbol);

            ClearIcons();
        }

        private void SetGroundColor(string groundType)
        {
            if (groundLayer == null) return;

            Color groundColor;
            switch (groundType)
            {
                case "water":
                    groundColor = new Color(0.2f, 0.6f, 1.0f, 1.0f); // Blue
                    break;
                case "sand":
                    groundColor = new Color(1.0f, 0.9f, 0.4f, 1.0f); // Yellow/Sandy
                    break;
                case "ice":
                    groundColor = new Color(0.9f, 0.95f, 1.0f, 1.0f); // Light Blue/White
                    break;
                case "swamp":
                    groundColor = new Color(0.4f, 0.3f, 0.2f, 1.0f); // Brown
                    break;
                case "grass":
                default:
                    groundColor = new Color(0.3f, 0.7f, 0.2f, 1.0f); // Green
                    break;
            }

            groundLayer.color = groundColor;
            
            // Use a simple white square sprite, or create one if needed
            if (groundLayer.sprite == null)
            {
                groundLayer.sprite = CreateSimpleSquareSprite();
            }
        }

        private void SetElevation(double elevation)
        {
            // Use elevation to adjust the tile's visual position/scale for depth perception
            // Higher elevation = slight z-offset and maybe larger scale
            Vector3 pos = transform.position;
            pos.z = (float)elevation * -0.01f; // Negative Z moves it forward (higher)
            transform.position = pos;

            // Optional: Slightly scale higher elevations to show prominence
            float elevationScale = 1.0f + (float)elevation * 0.005f;
            elevationScale = Mathf.Clamp(elevationScale, 0.8f, 1.3f);
            transform.localScale = Vector3.one * elevationScale;
        }

        private void SetVegetationDisplay(string vegetationType, int vegetationAmount)
        {
            if (vegetationLayer == null) return;

            // Show vegetation layer only if there's significant vegetation
            if (vegetationType == "none" || vegetationAmount < 5)
            {
                vegetationLayer.sprite = null;
                return;
            }

            // Create a simple overlay to show vegetation presence
            vegetationLayer.sprite = CreateSimpleSquareSprite();
            
            // Use different colors/transparency based on habitat type and amount
            Color vegColor = GetVegetationColor(vegetationType);
            
            // Adjust alpha based on vegetation amount (0-100%)
            vegColor.a = Mathf.Clamp01(vegetationAmount / 100.0f) * 0.6f; // Max 60% opacity
            
            vegetationLayer.color = vegColor;
        }

        private Color GetVegetationColor(string vegetationType)
        {
            // Color coding for different habitat types
            switch (vegetationType)
            {
                case "Artic Desert":
                    return new Color(0.8f, 0.8f, 0.9f, 1.0f); // Light grayish
                case "Tundra":
                    return new Color(0.6f, 0.7f, 0.5f, 1.0f); // Pale green
                case "Boreal":
                    return new Color(0.2f, 0.5f, 0.3f, 1.0f); // Dark green
                case "Artic Marsh":
                    return new Color(0.4f, 0.6f, 0.7f, 1.0f); // Blue-green
                case "Desert":
                    return new Color(0.9f, 0.8f, 0.6f, 1.0f); // Sandy
                case "Plains":
                    return new Color(0.6f, 0.8f, 0.4f, 1.0f); // Light green
                case "Forest":
                    return new Color(0.3f, 0.6f, 0.2f, 1.0f); // Forest green
                case "Swamp":
                    return new Color(0.4f, 0.5f, 0.3f, 1.0f); // Murky green
                case "Hot Desert":
                    return new Color(1.0f, 0.7f, 0.3f, 1.0f); // Orange-yellow
                case "Savannah":
                    return new Color(0.7f, 0.7f, 0.3f, 1.0f); // Yellow-green
                case "Monsoon Forest":
                    return new Color(0.2f, 0.7f, 0.4f, 1.0f); // Rich green
                case "Rainforest":
                    return new Color(0.1f, 0.6f, 0.2f, 1.0f); // Deep green
                case "Ice Sheet":
                    return new Color(0.9f, 0.95f, 1.0f, 1.0f); // Icy white
                default:
                    return new Color(0.5f, 0.7f, 0.3f, 1.0f); // Default green
            }
        }

        private void SetTerrainSymbol(string terrainSymbolType)
        {
            if (terrainSymbol == null) return;

            switch (terrainSymbolType)
            {
                case "hills":
                    terrainSymbol.sprite = CreateTextSprite("H");
                    terrainSymbol.color = new Color(0.4f, 0.3f, 0.2f, 0.8f); // Dark brown, semi-transparent
                    break;
                case "mountains":
                    terrainSymbol.sprite = CreateTextSprite("M");
                    terrainSymbol.color = new Color(0.3f, 0.3f, 0.3f, 0.9f); // Dark gray, mostly opaque
                    break;
                case "none":
                default:
                    terrainSymbol.sprite = null;
                    break;
            }
        }

        private Sprite CreateTextSprite(string text)
        {
            // Create a simple texture with text
            // For now, we'll create a basic representation
            // In a real implementation, you'd want to use TextMeshPro or actual font rendering
            
            int textureSize = 64;
            Texture2D texture = new Texture2D(textureSize, textureSize);
            
            // Fill with transparent pixels
            Color[] colors = new Color[textureSize * textureSize];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.clear;
            }
            
            // Create a simple pattern for the letter
            if (text == "H")
            {
                CreateHPattern(colors, textureSize);
            }
            else if (text == "M")
            {
                CreateMPattern(colors, textureSize);
            }
            
            texture.SetPixels(colors);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f));
        }

        private void CreateHPattern(Color[] colors, int size)
        {
            Color pixelColor = Color.white;
            int center = size / 2;
            int thickness = 3;
            int height = size - 10;
            int startY = 5;
            
            // Left vertical line
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = center - 15; x < center - 15 + thickness; x++)
                {
                    if (x >= 0 && x < size && y >= 0 && y < size)
                        colors[y * size + x] = pixelColor;
                }
            }
            
            // Right vertical line
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = center + 12; x < center + 12 + thickness; x++)
                {
                    if (x >= 0 && x < size && y >= 0 && y < size)
                        colors[y * size + x] = pixelColor;
                }
            }
            
            // Horizontal crossbar
            for (int x = center - 15; x <= center + 15; x++)
            {
                for (int y = center - 1; y < center + 2; y++)
                {
                    if (x >= 0 && x < size && y >= 0 && y < size)
                        colors[y * size + x] = pixelColor;
                }
            }
        }

        private void CreateMPattern(Color[] colors, int size)
        {
            Color pixelColor = Color.white;
            int center = size / 2;
            int thickness = 3;
            int height = size - 10;
            int startY = 5;
            
            // Left vertical line
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = center - 18; x < center - 18 + thickness; x++)
                {
                    if (x >= 0 && x < size && y >= 0 && y < size)
                        colors[y * size + x] = pixelColor;
                }
            }
            
            // Right vertical line
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = center + 15; x < center + 15 + thickness; x++)
                {
                    if (x >= 0 && x < size && y >= 0 && y < size)
                        colors[y * size + x] = pixelColor;
                }
            }
            
            // Left diagonal
            for (int i = 0; i < height / 2; i++)
            {
                int x = center - 18 + i / 2;
                int y = startY + i;
                for (int dx = 0; dx < thickness; dx++)
                {
                    for (int dy = 0; dy < thickness; dy++)
                    {
                        if (x + dx >= 0 && x + dx < size && y + dy >= 0 && y + dy < size)
                            colors[(y + dy) * size + (x + dx)] = pixelColor;
                    }
                }
            }
            
            // Right diagonal
            for (int i = 0; i < height / 2; i++)
            {
                int x = center + 15 - i / 2;
                int y = startY + i;
                for (int dx = 0; dx < thickness; dx++)
                {
                    for (int dy = 0; dy < thickness; dy++)
                    {
                        if (x + dx >= 0 && x + dx < size && y + dy >= 0 && y + dy < size)
                            colors[(y + dy) * size + (x + dx)] = pixelColor;
                    }
                }
            }
        }

        private Sprite CreateSimpleSquareSprite()
        {
            // Create a simple 1x1 white texture for coloring
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        }

        private void ClearIcons()
        {
            if (iconTopLeft != null) iconTopLeft.sprite = null;
            if (iconTopRight != null) iconTopRight.sprite = null;
            if (iconBottomLeft != null) iconBottomLeft.sprite = null;
            if (iconBottomRight != null) iconBottomRight.sprite = null;
        }

        // TODO: Replace color coding with actual sprites later
        // private Sprite GetGroundSprite(string groundType)
        // {
        //     return Resources.Load<Sprite>($"Sprites/Ground/{groundType}");
        // }
    }
}
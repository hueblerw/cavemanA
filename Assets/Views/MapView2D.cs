using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CavemanLand.Models.ViewModels;
using CavemanLand.Models.GenericModels;


public class MapView2D : MonoBehaviour
{
    public GameObject tilePrefab;
    public int mapWidth = 80;
    public int mapLength = 60;
    public float tileSize = 1.0f;

    private GameObject[,] tileGrid;

    void Start()
    {
        tileGrid = new GameObject[mapWidth, mapLength];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapLength; y++)
            {
                Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);
                GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity, transform);

                // Assign initial data to the tile
                Tile2D tile = newTile.GetComponent<Tile2D>();
                tile.Initialize(GetTileData(x, y)); // GetTileData fetches TileData2D from your 2D array

                tileGrid[x, y] = newTile;
            }
        }
    }

    public void UpdateTile(int x, int y, TileData2D newData)
    {
        Tile2D tile = tileGrid[x, y].GetComponent<Tile2D>();
        tile.Initialize(newData); // Reinitialize the tile with new data
    }

    private TileData2D GetTileData(int x, int y)
    {
        // Return TileData from your data source
        return new TileData2D
        {
            groundType = "grass",
            vegetationType = "tree",
            vegetationAmount = 20,
            terrainSymbol = "hills",
            riverSystem = "none"
        };
    }
}

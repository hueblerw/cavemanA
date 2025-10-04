using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CavemanLand.Models.ViewModels;


public class 2DMapView : MonoBehaviour
{
    public GameObject tilePrefab;
    public int mapWidth = 80;
    public int mapLength = 60;
    public float tileSize = 1.0f;

    private GameObject[,] tileGrid;

    void Start()
    {
        tileGrid = new GameObject[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3 position = new Vector3(x * tileSize, y * tileSize, 0);
                GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity, transform);

                // Assign initial data to the tile
                Tile tile = newTile.GetComponent<Tile>();
                tile.Initialize(GetTileData(x, y)); // GetTileData fetches TileData from your 2D array

                tileGrid[x, y] = newTile;
            }
        }
    }

    public void UpdateTile(int x, int y, TileData newData)
    {
        Tile tile = tileGrid[x, y].GetComponent<Tile>();
        tile.Initialize(newData); // Reinitialize the tile with new data
    }

    private TileData GetTileData(int x, int y)
    {
        // Return TileData from your data source
        return new TileData
        {
            groundType = "grass",
            vegetationType = "tree",
            vegetationAmount = 20,
            terrainSymbol = "hills",
            riverSystem = "none"
        };
    }
}

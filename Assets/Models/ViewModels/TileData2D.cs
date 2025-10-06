using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CavemanLand.Models;


namespace CavemanLand.Models.ViewModels
{
    public class TileData2D
    {
        public string groundType; // "water", "dry", "grass", "swamp"
        public string vegetationType; // "scrub", "tree", "cypress", "none"
        public int vegetationAmount; // a number representing the tree density (0-100)
        public string terrainSymbol; // "none", "hills", "mountains"
        public string riverSystem; // "none", "lake", "river", etc.
        
        // Additional data for herd movement testing
        public double elevation; // Raw elevation for pathfinding
        public double oceanPercent; // How much water coverage (0.0-1.0)

        public TileData2D() 
        {
        }

        public TileData2D(float elevation, float hillPer)
        {
            this.groundType = "grass";
            this.vegetationType = "tree";
            this.vegetationAmount = 20;
            this.terrainSymbol = "none";
            this.riverSystem = "none";
            this.elevation = elevation;
        }
    }
}

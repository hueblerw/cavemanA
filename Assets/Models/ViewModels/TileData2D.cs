using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CavemanLand.Models;


namespace CavemanLand.Models.ViewModels
{
    public class TileData2D
    {
        public string groundType; // "dry", "grass", "swamp"
        public string vegetationType; // "scrub", "tree", "cypress"
        public int vegetationAmount; // a number representing the tree density
        public string terrainSymbol; // "none", "hills", "mountains"
        public string riverSystem; // "none", "lake", etc.

        public TileData2D() { }

        public TileData2D(float elevation, float hillPer)
        {
            this.groundType = "grass";
            this.vegetationType = "tree";
            this.vegetationAmount = 20;
            this.terrainSymbol = "none";
            this.riverSystem = "none";
        }
    }
}

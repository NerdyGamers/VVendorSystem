using System;

namespace Server.Custom.VVendorSystem
{
    /// <summary>
    /// Defines broad categories of resources that influence economic multipliers.
    /// These values can be tuned on a per‑region basis via VendorRegionController.
    /// </summary>
    public enum ResourceType
    {
        Stone,
        PreciousStones,
        Wood,
        Metal,
        Cloth,
        Animals,
        Fish,
        Alcohol,
        Water,
        Cereals,
        FruitsAndVegetables,
        Other0,
        Other1,
        Other2,
        Other3
    }
}
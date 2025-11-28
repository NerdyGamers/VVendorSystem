using System;
using System.Collections.Generic;
using Server;

namespace Server.Custom.VVendorSystem
{
    /// <summary>
    /// A global manager for regional economic multipliers.  Regions are
    /// represented by VendorRegionController objects which register themselves
    /// with this manager.  When vendors compute prices they consult the
    /// economy manager to determine the appropriate multiplier based on
    /// RegionTag and ResourceType.  A three‑tier fallback is provided:
    ///
    /// 1. Exact region name (e.g. "Britain_MageGuild")
    /// 2. Region category (e.g. "Town", "Dungeon", "Wilderness")
    /// 3. Vendor defaults captured at creation
    /// </summary>
    public static class EconomyManager
    {
        private static readonly Dictionary<string, VendorRegionController> m_Regions = new Dictionary<string, VendorRegionController>();

        /// <summary>
        /// Registers a region controller with the given name.  If another
        /// controller with the same name exists it will be replaced.
        /// </summary>
        public static void RegisterRegion(string name, VendorRegionController controller)
        {
            if (string.IsNullOrWhiteSpace(name) || controller == null)
                return;
            m_Regions[name.ToLowerInvariant()] = controller;
        }

        /// <summary>
        /// Deregisters a region controller.
        /// </summary>
        public static void DeregisterRegion(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;
            m_Regions.Remove(name.ToLowerInvariant());
        }

        /// <summary>
        /// Retrieves a region controller by exact name, or null if none
        /// registered.
        /// </summary>
        public static VendorRegionController GetRegionController(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;
            m_Regions.TryGetValue(name.ToLowerInvariant(), out var controller);
            return controller;
        }

        /// <summary>
        /// Returns the multiplier for a given region and resource type.  This
        /// method implements the fallback logic described above.
        /// </summary>
        public static double GetMultiplier(string regionName, string regionCategory, ResourceType resourceType, double vendorDefaultRate = 1.0)
        {
            // Priority 1: exact region
            if (!string.IsNullOrWhiteSpace(regionName))
            {
                var ctrl = GetRegionController(regionName);
                if (ctrl != null)
                    return ctrl.GetMultiplier(resourceType);
            }
            // Priority 2: category default
            if (!string.IsNullOrWhiteSpace(regionCategory))
            {
                var cat = GetRegionController(regionCategory);
                if (cat != null)
                    return cat.GetMultiplier(resourceType);
            }
            // Priority 3: vendor default
            return vendorDefaultRate;
        }
    }
}
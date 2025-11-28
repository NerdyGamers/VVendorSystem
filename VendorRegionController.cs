using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.VVendorSystem
{
    /// <summary>
    /// Represents a micro‑region used by the VVendorPack economy.  Each
    /// controller defines a unique region name, a bounding rectangle and a
    /// series of economic multipliers.  Controllers register themselves with
    /// the EconomyManager and may be edited via the RegionEditorGump.  Only
    /// one controller should exist per region name.
    /// </summary>
    public class VendorRegionController : Item
    {
        private string m_RegionName;
        private Rectangle2D m_Bounds;
        private double m_LocalRate = 1.0;
        private string m_Category;
        private readonly Dictionary<ResourceType, double> m_ResourceRates = new Dictionary<ResourceType, double>();

        [CommandProperty(AccessLevel.GameMaster)]
        public string RegionName
        {
            get => m_RegionName;
            set
            {
                if (m_RegionName == value)
                    return;
                if (!string.IsNullOrWhiteSpace(m_RegionName))
                    EconomyManager.DeregisterRegion(m_RegionName);
                m_RegionName = value;
                if (!string.IsNullOrWhiteSpace(m_RegionName))
                    EconomyManager.RegisterRegion(m_RegionName, this);
            }
        }

        /// <summary>
        /// The physical area of influence for this region.  Vendors created
        /// within these bounds will attach to this region.
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public Rectangle2D Bounds
        {
            get => m_Bounds;
            set => m_Bounds = value;
        }

        /// <summary>
        /// Local rate applied to all items sold within this region.  A value
        /// greater than one makes goods more expensive; less than one makes
        /// them cheaper.
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public double LocalRate
        {
            get => m_LocalRate;
            set => m_LocalRate = value;
        }

        /// <summary>
        /// Category fallback name used when vendors cannot match a more
        /// specific region.  Typically set to Town, Dungeon, Wilderness etc.
        /// </summary>
        [CommandProperty(AccessLevel.GameMaster)]
        public string Category
        {
            get => m_Category;
            set => m_Category = value;
        }

        public VendorRegionController() : base(0x1F14) // stone/gem tile as placeholder
        {
            Movable = false;
            Visible = false;
            Hue = 0x0;
        }

        public VendorRegionController(Serial serial) : base(serial)
        {
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();
            EconomyManager.DeregisterRegion(m_RegionName);
        }

        /// <summary>
        /// Gets the effective multiplier for the specified resource type.  This
        /// includes both the local rate and the per‑resource adjustment.
        /// </summary>
        public double GetMultiplier(ResourceType resource)
        {
            double rate = m_LocalRate;
            if (m_ResourceRates.TryGetValue(resource, out var resRate))
                rate *= resRate;
            return rate;
        }

        /// <summary>
        /// Sets the multiplier for a specific resource type.  A value of 1.0
        /// leaves prices unchanged; values above/below 1.0 raise/lower the
        /// price.
        /// </summary>
        public void SetResourceMultiplier(ResourceType resource, double multiplier)
        {
            m_ResourceRates[resource] = multiplier;
        }

        /// <summary>
        /// Called when a game master double‑clicks this controller.  Opens
        /// the region editor UI.
        /// </summary>
        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel < AccessLevel.GameMaster)
                return;
            from.SendGump(new RegionEditorGump(from, this));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
            writer.Write(m_RegionName);
            writer.Write(m_Bounds);
            writer.Write(m_LocalRate);
            writer.Write(m_Category);
            // serialize resource rates
            writer.Write(m_ResourceRates.Count);
            foreach (var kvp in m_ResourceRates)
            {
                writer.Write((int)kvp.Key);
                writer.Write(kvp.Value);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            switch (version)
            {
                case 0:
                    m_RegionName = reader.ReadString();
                    m_Bounds = reader.ReadRect2D();
                    m_LocalRate = reader.ReadDouble();
                    m_Category = reader.ReadString();
                    int count = reader.ReadInt();
                    m_ResourceRates.Clear();
                    for (int i = 0; i < count; i++)
                    {
                        ResourceType res = (ResourceType)reader.ReadInt();
                        double mul = reader.ReadDouble();
                        m_ResourceRates[res] = mul;
                    }
                    // re‑register
                    if (!string.IsNullOrWhiteSpace(m_RegionName))
                        EconomyManager.RegisterRegion(m_RegionName, this);
                    break;
            }
        }
    }
}
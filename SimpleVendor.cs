using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Custom.VVendorSystem
{
    public class SimpleVendor : BaseVendor
    {
        private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();

        protected override List<SBInfo> SBInfos => m_SBInfos;

        private string m_PackName;
        private string m_RegionTag;
        private string m_RegionCategory;
        private double m_DefaultRate = 1.0;

        [CommandProperty(AccessLevel.GameMaster)]
        public string PackName
        {
            get => m_PackName;
            set
            {
                m_PackName = value;
                InvalidateProperties();

                m_SBInfos.Clear();
                InitSBInfo();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string RegionTag
        {
            get => m_RegionTag;
            set => m_RegionTag = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public string RegionCategory
        {
            get => m_RegionCategory;
            set => m_RegionCategory = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public double DefaultRate
        {
            get => m_DefaultRate;
            set => m_DefaultRate = value;
        }

        [Constructable]
        public SimpleVendor()
            : base("simple vendor")
        {
            CantWalk = true;
        }

        public SimpleVendor(Serial serial)
            : base(serial)
        {
        }

        public override void InitSBInfo()
        {
            m_SBInfos.Clear();

            if (string.IsNullOrWhiteSpace(m_PackName))
                return;

            var container = VendorPackContainer.Get(m_PackName);
            if (container != null)
                m_SBInfos.Add(new SBVVendorTemplate(this, container));
        }

        public override void InitOutfit()
        {
            base.InitOutfit();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version

            writer.Write(m_PackName);
            writer.Write(m_RegionTag);
            writer.Write(m_RegionCategory);
            writer.Write(m_DefaultRate);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version >= 0)
            {
                m_PackName = reader.ReadString();
                m_RegionTag = reader.ReadString();
                m_RegionCategory = reader.ReadString();
                m_DefaultRate = reader.ReadDouble();
            }

            m_SBInfos.Clear();

            if (!string.IsNullOrWhiteSpace(m_PackName))
            {
                var container = VendorPackContainer.Get(m_PackName);
                if (container != null)
                    m_SBInfos.Add(new SBVVendorTemplate(this, container));
            }
        }

        private class SBVVendorTemplate : SBInfo
        {
            private readonly SimpleVendor m_Owner;
            private readonly VendorPackContainer m_Container;
            private readonly List<GenericBuyInfo> m_BuyInfo;
            private readonly IShopSellInfo m_SellInfo;

            public SBVVendorTemplate(SimpleVendor vendor, VendorPackContainer container)
            {
                m_Owner = vendor;
                m_Container = container;

                m_BuyInfo = new InternalBuyList(m_Owner, m_Container);
                m_SellInfo = new InternalSellList(m_Owner, m_Container);
            }

            public override List<GenericBuyInfo> BuyInfo => m_BuyInfo;
            public override IShopSellInfo SellInfo => m_SellInfo;

            private class InternalBuyList : List<GenericBuyInfo>
            {
                public InternalBuyList(SimpleVendor vendor, VendorPackContainer container)
                {
                    if (container == null)
                        return;

                    foreach (var tmpl in container.Items.OfType<VendorTemplateItem>())
                    {
                        if (tmpl.ItemType == null)
                            continue;

                        int amount = tmpl.InfiniteStock ? tmpl.MaxStock : tmpl.MaxStock;
                        int basePrice = tmpl.Price;

                        double mul = EconomyManager.GetMultiplier(
                            vendor.RegionTag,
                            vendor.RegionCategory,
                            tmpl.ResourceType,
                            vendor.DefaultRate);

                        int finalPrice = Math.Max(1, (int)Math.Ceiling(basePrice * mul));

                        try
                        {
                            Item inst = (Item)Activator.CreateInstance(tmpl.ItemType);

                            int itemID = ItemIDFromTemplateOrInstance(tmpl, inst);
                            int hue = tmpl.Hue != 0 ? tmpl.Hue : inst.Hue;
                            string displayName = !string.IsNullOrEmpty(tmpl.Name)
                                ? tmpl.Name
                                : inst.Name;

                            Add(new GenericBuyInfo(
                                displayName,
                                tmpl.ItemType,
                                finalPrice,
                                amount,
                                itemID,
                                hue,
                                false));

                            inst.Delete();
                        }
                        catch
                        {
                            // skip bad entries
                        }
                    }
                }

                private static int ItemIDFromTemplateOrInstance(VendorTemplateItem tmpl, Item inst)
                {
                    if (tmpl.ItemID > 0)
                        return tmpl.ItemID;

                    if (inst != null)
                        return inst.ItemID;

                    return 0x1F14;
                }
            }

            private class InternalSellList : GenericSellInfo
            {
                public InternalSellList(SimpleVendor vendor, VendorPackContainer container)
                {
                    if (container == null)
                        return;

                    foreach (var tmpl in container.Items.OfType<VendorTemplateItem>())
                    {
                        if (tmpl.ItemType == null || !tmpl.AllowSellback)
                            continue;

                        double mul = EconomyManager.GetMultiplier(
                            vendor.RegionTag,
                            vendor.RegionCategory,
                            tmpl.ResourceType,
                            vendor.DefaultRate);

                        int basePrice = tmpl.Price;
                        int finalPrice = Math.Max(1, (int)Math.Ceiling(basePrice * mul * 0.5));

                        Add(tmpl.ItemType, finalPrice);
                    }
                }
            }
        }
    }
}

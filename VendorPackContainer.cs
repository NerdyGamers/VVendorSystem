using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;
using Server.Accounting;

namespace Server.Custom.VVendorSystem
{
    /// <summary>
    /// Hidden container that represents a vendor pack.
    /// It is never meant for normal players, only staff via [editvvpack.
    /// </summary>
    public class VendorPackContainer : Container
    {

        private Mobile m_Owner;
        public Mobile Owner { get { return m_Owner; } }
        // === STATIC REGISTRY ===

        private static readonly Dictionary<string, VendorPackContainer> _packs =
            new Dictionary<string, VendorPackContainer>();

        public static VendorPackContainer Get(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            _packs.TryGetValue(name.Trim().ToLower(), out var c);
            return c;
        }

        public static VendorPackContainer GetOrCreate(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var key = name.Trim().ToLower();

            if (_packs.TryGetValue(key, out var existing))
                return existing;

            var pack = new VendorPackContainer()
            {
                PackName = name,
                Map = Map.Internal,
                Location = Point3D.Zero,
                //Visible = false,
                Movable = false
            };

            return pack;
        }


        // === INSTANCE ===
        public static bool SendDeleteOnClose { get; set; }
        private string m_PackName;
        
        private bool m_Open;
        public override bool IsVirtualItem { get { return true; } }
        
        public bool Opened { get { return m_Open; } }

        [CommandProperty(AccessLevel.GameMaster)]
        public string PackName
        {
            get => m_PackName;
            set
            {
                if (m_PackName == value)
                    return;

                if (!string.IsNullOrWhiteSpace(m_PackName))
                    _packs.Remove(m_PackName.Trim().ToLower());

                m_PackName = value;

                if (!string.IsNullOrWhiteSpace(m_PackName))
                    _packs[m_PackName.Trim().ToLower()] = this;

                InvalidateProperties();
            }
        }


        public override int DefaultGumpID => 0x9; // nice container gump
        public override Rectangle2D Bounds => new Rectangle2D(20, 85, 104, 111);



        [Constructable]
        public VendorPackContainer()
            : base(0x42) // some unobtrusive container itemID
        {
            m_Owner = null;
            Movable = false;
            //Visible = false;
            Hue = 0;
        }

        public VendorPackContainer(Serial serial) : base(serial)
        {
        }

        public override bool IsAccessibleTo(Mobile m)
        {
            // Only staff may open packs
            return m != null && m.AccessLevel >= AccessLevel.GameMaster;
        }

        public void OpenFor(Mobile from)
        {
            Mobile m_Owner = from as PlayerMobile;
            if (m_Owner.NetState != null)
            {
                m_Open = true;
                m_Owner.SendMessage("Opening vendor pack '{0}'.", this.PackName);

                m_Owner.Send(new EquipUpdate(this));

                DisplayTo(m_Owner);
            }
        }

        public void Close()
        {
            m_Open = false;

            if (m_Owner != null && SendDeleteOnClose)
            {
                m_Owner.Send(RemovePacket);
            }
        }

        public override void OnSingleClick(Mobile from)
        { }

        public override void OnDoubleClick(Mobile from)
        { }

        public override void OnDelete()
        {
            base.OnDelete();

            if (!string.IsNullOrWhiteSpace(m_PackName))
                _packs.Remove(m_PackName.Trim().ToLower());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
            writer.Write(m_PackName);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            m_PackName = reader.ReadString();

            if (!string.IsNullOrWhiteSpace(m_PackName))
                _packs[m_PackName.Trim().ToLower()] = this;
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped == this || dropped == null)
                return false;

            // Allow moving existing template items within the container
            if (dropped is VendorTemplateItem && dropped.Parent == this)
                return base.OnDragDrop(from, dropped);

            if (from == null || from.AccessLevel < AccessLevel.GameMaster)
            {
                from?.SendMessage("Only staff may edit vendor packs.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PackName))
            {
                from.SendMessage("This container has no PackName set.");
                return false;
            }

            // Only allow items from their own pack/bank
            if (dropped.RootParent != from)
            {
                from.SendMessage("Drag items from your backpack into the pack.");
                return false;
            }

            // Convert real item → template wrapper item
            var tmpl = new VendorTemplateItem(dropped);

            DropItem(tmpl);
            tmpl.Location = dropped.Location;

            dropped.Delete();

            from.SendMessage("Added {0} to vendor pack '{1}'.", tmpl.Name ?? tmpl.GetType().Name, PackName);

            return true;
        }
    }

    /// <summary>
    /// Wrapper item that stores vendor metadata for a single entry.
    /// Exists only inside VendorPackContainer. If it leaves, it deletes itself.
    /// </summary>
    public class VendorTemplateItem : Item
    {
        private Type m_ItemType;
        private int m_Price;
        private int m_MaxStock;
        private ResourceType m_ResourceType;
        private bool m_AllowSellback;
        private bool m_InfiniteStock;
        private bool m_IncludeInRandom;

        [CommandProperty(AccessLevel.GameMaster)]
        public Type ItemType
        {
            get => m_ItemType;
            set => m_ItemType = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Price
        {
            get => m_Price;
            set => m_Price = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxStock
        {
            get => m_MaxStock;
            set => m_MaxStock = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public ResourceType ResourceType
        {
            get => m_ResourceType;
            set => m_ResourceType = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool AllowSellback
        {
            get => m_AllowSellback;
            set => m_AllowSellback = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool InfiniteStock
        {
            get => m_InfiniteStock;
            set => m_InfiniteStock = value;
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool IncludeInRandomBuy
        {
            get => m_IncludeInRandom;
            set => m_IncludeInRandom = value;
        }

        [Constructable]
        public VendorTemplateItem() : base(0x1F14)
        {
            Weight = 0.0;
            Movable = true;
            Name = "Vendor Template Entry";
            m_MaxStock = 20;
            m_Price = 10;
            m_ResourceType = ResourceType.Other0;
            m_AllowSellback = true;
            m_InfiniteStock = true;
            m_IncludeInRandom = true;
        }

        public VendorTemplateItem(Item source) : base(source.ItemID)
        {
            Weight = 0.0;
            Movable = true;

            m_ItemType = source.GetType();
            Hue = source.Hue;
            Name = source.Name ?? source.GetType().Name;

            m_MaxStock = 20;
            m_Price = 10;
            m_ResourceType = ResourceType.Other0;
            m_AllowSellback = true;
            m_InfiniteStock = true;
            m_IncludeInRandom = true;
        }

        public VendorTemplateItem(Serial serial) : base(serial)
        {
        }

        public override void OnAdded(object parent)
        {
            base.OnAdded(parent);

            // Only valid if inside a VendorPackContainer
            if (parent is VendorPackContainer)
                return;

            // leaving the pack → schedule delete
            Timer.DelayCall(TimeSpan.FromSeconds(0.1), DeleteMe);
        }

        public override bool DropToItem(Mobile from, Item target, Point3D p)
        {
            if (target is VendorPackContainer)
                return base.DropToItem(from, target, p);

            // Dropped somewhere else → delete
            Timer.DelayCall(TimeSpan.FromSeconds(0.1), DeleteMe);
            return true;
        }

        private void DeleteMe()
        {
            // Fix: Compare Parent to type, not instance. Check if Parent is VendorPackContainer.
            if (!(Parent is VendorPackContainer))
                Delete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
            writer.Write(m_ItemType != null ? m_ItemType.AssemblyQualifiedName : null);
            writer.Write(m_Price);
            writer.Write(m_MaxStock);
            writer.WriteEncodedInt((int)m_ResourceType);
            writer.Write(m_AllowSellback);
            writer.Write(m_InfiniteStock);
            writer.Write(m_IncludeInRandom);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
            var typeName = reader.ReadString();
            m_ItemType = !string.IsNullOrEmpty(typeName) ? Type.GetType(typeName) : null;
            m_Price = reader.ReadInt();
            m_MaxStock = reader.ReadInt();
            m_ResourceType = (ResourceType)reader.ReadEncodedInt();
            m_AllowSellback = reader.ReadBool();
            m_InfiniteStock = reader.ReadBool();
            m_IncludeInRandom = reader.ReadBool();
        }
    }
}

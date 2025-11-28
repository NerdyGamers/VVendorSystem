using System;
using Server;
using Server.Network;
using Server.Targeting;

namespace Server.Custom.VVendorSystem
{
    /// <summary>
    /// Represents a temporary visual preview of a rectangular region on the
    /// ground.  Instances of this item are created when administrators define
    /// region bounds for a new vendor.  The bounding box is only visible to
    /// the initiating client and disappears after a short duration.
    /// </summary>
    public class BoundingBoxPreview : Item
    {
        private Timer m_Timer;
        public BoundingBoxPreview(Rectangle2D bounds, TimeSpan duration) : base(0x1BFD)
        {
            Movable = false;
            Visible = false;
            // Boundaries are stored; we could use these to send packets to
            // display the area, but ServUO does not provide an API for
            // arbitrary clientside shapes.  In a production environment
            // consider using Effect.SendMovingParticles() or custom hue
            // patches.  Here we simply exist to mark deletion after the
            // duration expires.
            m_Timer = Timer.DelayCall(duration, Delete);
        }

        public BoundingBoxPreview(Serial serial) : base(serial)
        {
        }

        public override void OnDelete()
        {
            if (m_Timer != null)
            {
                m_Timer.Stop();
                m_Timer = null;
            }
            base.OnDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
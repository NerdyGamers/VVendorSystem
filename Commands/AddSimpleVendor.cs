using Server;
using Server.Commands;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Targeting;
using System;

namespace Server.Custom.VVendorSystem.Commands
{
    public static class AddSimpleVendorCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("AddSimpleVendor", AccessLevel.GameMaster, OnAddVendor);
            CommandSystem.Register("addsimplevendor", AccessLevel.GameMaster, OnAddVendor);

            CommandSystem.Register("EditVVendorPack", AccessLevel.GameMaster, OnEditPack);
            CommandSystem.Register("editvvpack", AccessLevel.GameMaster, OnEditPack);
        }

        private static void OnEditPack(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Length < 1)
            {
                from.SendMessage("Usage: [editvvpack <PackName>");
                return;
            }

            string name = e.GetString(0);
            var pack = VendorPackContainer.GetOrCreate(name);

            pack.OpenFor(from);
        }

        private static void OnAddVendor(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Length < 1)
            {
                from.SendMessage("Usage: [addsimplevendor <PackName>");
                return;
            }

            string packName = e.GetString(0);
            var pack = VendorPackContainer.GetOrCreate(packName);

            if (pack.Items.Count == 0)
            {
                from.SendMessage("Vendor pack '{0}' is currently empty. You can still place the vendor, but they will have no stock.", pack.PackName);
            }

            from.SendMessage("Target where you want to place the SimpleVendor.");
            from.Target = new VendorPlacementTarget(pack);
        }

        private class VendorPlacementTarget : Target
        {
            private readonly VendorPackContainer m_Pack;

            public VendorPlacementTarget(VendorPackContainer pack)
                : base(12, true, TargetFlags.None)
            {
                m_Pack = pack;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D ip = targeted as IPoint3D;

                if (ip == null)
                {
                    from.SendMessage("You must target a location.");
                    return;
                }

                Point3D loc = new Point3D(ip.X, ip.Y, ip.Z);
                Map map = from.Map;

                if (map == null || map == Map.Internal)
                {
                    from.SendMessage("You cannot place a vendor there.");
                    return;
                }

                Region region = Region.Find(loc, map);
                string baseRegionName = region != null ? region.Name : "Wilderness";
                string category = GetRegionCategory(region);

                string subName = (m_Pack.PackName ?? "Template").Replace(" ", String.Empty);
                string regionName = string.Format("{0}_{1}", baseRegionName, subName);

                // Region controller / economy hookup
                VendorRegionController controller = EconomyManager.GetRegionController(regionName);

                if (controller == null || controller.Deleted)
                {
                    controller = new VendorRegionController
                    {
                        RegionName = regionName,
                        Category = category,
                        LocalRate = 1.0,
                        Map = map,
                        Location = loc
                    };

                    controller.Bounds = new Rectangle2D(loc.X - 4, loc.Y - 4, 8, 8);

                    from.SendMessage("Now target the first corner for the {0} region bounds.", regionName);
                    from.Target = new FirstCornerTarget(controller);
                }

                SimpleVendor vendor = new SimpleVendor
                {
                    PackName = m_Pack.PackName,
                    RegionTag = regionName,
                    RegionCategory = category,
                    DefaultRate = 1.0,
                    Map = map,
                    Location = loc
                };

                from.SendMessage("SimpleVendor created using template '{0}' in region '{1}'.", m_Pack.PackName, regionName);
            }
        }

        private static string GetRegionCategory(Region region)
        {
            if (region == null)
                return "Wilderness";

            if (region is GuardedRegion)
                return "Town";

            if (region is DungeonRegion)
                return "Dungeon";

            return region.Name ?? "Wilderness";
        }

        private class FirstCornerTarget : Target
        {
            private readonly VendorRegionController m_Controller;

            public FirstCornerTarget(VendorRegionController controller)
                : base(12, true, TargetFlags.None)
            {
                m_Controller = controller;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D ip = targeted as IPoint3D;

                if (ip == null)
                {
                    from.SendMessage("You must target a location.");
                    return;
                }

                Point3D p1 = new Point3D(ip.X, ip.Y, ip.Z);

                from.SendMessage("Now target the opposite corner of the region bounds.");
                from.Target = new SecondCornerTarget(m_Controller, p1);
            }
        }

        private class SecondCornerTarget : Target
        {
            private readonly VendorRegionController m_Controller;
            private readonly Point3D m_FirstCorner;

            public SecondCornerTarget(VendorRegionController controller, Point3D firstCorner)
                : base(12, true, TargetFlags.None)
            {
                m_Controller = controller;
                m_FirstCorner = firstCorner;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                IPoint3D ip = targeted as IPoint3D;

                if (ip == null)
                {
                    from.SendMessage("You must target a location.");
                    return;
                }

                Point3D p2 = new Point3D(ip.X, ip.Y, ip.Z);

                int x = Math.Min(m_FirstCorner.X, p2.X);
                int y = Math.Min(m_FirstCorner.Y, p2.Y);
                int width = Math.Abs(m_FirstCorner.X - p2.X);
                int height = Math.Abs(m_FirstCorner.Y - p2.Y);

                if (width <= 0) width = 1;
                if (height <= 0) height = 1;

                Rectangle2D rect = new Rectangle2D(x, y, width, height);
                m_Controller.Bounds = rect;

                from.SendMessage("Region '{0}' bounds updated to {1}.", m_Controller.RegionName, rect);
            }
        }
    }
}

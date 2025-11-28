using System;
using System.Linq;
using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Custom.VVendorSystem
{
    /// <summary>
    /// Provides a simple user interface for editing a VendorRegionController's
    /// economic parameters.  Game masters can adjust the local rate and each
    /// resource multiplier individually.  Bounding boxes and names are not
    /// editable here – those are set when the region is first created.
    /// </summary>
    public class RegionEditorGump : Gump
    {
        private readonly Mobile m_From;
        private readonly VendorRegionController m_Controller;
        private readonly int m_Page;

        public RegionEditorGump(Mobile from, VendorRegionController controller) : base(50, 50)
        {
            m_From = from;
            m_Controller = controller;
            m_Page = 0;
            DrawGump();
        }

        private void DrawGump()
        {
            AddPage(0);
            AddBackground(0, 0, 420, 400, 5054);
            AddLabel(20, 10, 1152, $"Editing Region: {m_Controller.RegionName}");
            int y = 40;
            // Local rate
            AddLabel(20, y, 1152, "Local Rate:");
            AddTextEntry(150, y, 100, 20, 1152, 0, m_Controller.LocalRate.ToString());
            y += 30;
            // Category
            AddLabel(20, y, 1152, "Category:");
            AddTextEntry(150, y, 150, 20, 1152, 1, m_Controller.Category ?? string.Empty);
            y += 30;
            // Display resource multipliers
            AddLabel(20, y, 1152, "Resource Multipliers:");
            y += 25;
            int index = 0;
            foreach (ResourceType res in Enum.GetValues(typeof(ResourceType)))
            {
                double mul = m_Controller.GetMultiplier(res) / m_Controller.LocalRate;
                AddLabel(20, y, 1152, res.ToString() + ":");
                AddTextEntry(150, y, 60, 20, 1152, 10 + index, mul.ToString("0.00"));
                y += 25;
                index++;
            }
            // Save / cancel
            AddButton(20, y, 4005, 4007, 1, GumpButtonType.Reply, 0);
            AddLabel(50, y, 1152, "Save");
            AddButton(120, y, 4005, 4007, 0, GumpButtonType.Reply, 0);
            AddLabel(150, y, 1152, "Cancel");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0)
                return; // cancel
            // Save changes
            TextRelay localRateRel = info.GetTextEntry(0);
            TextRelay categoryRel = info.GetTextEntry(1);
            double localRate;
            if (!double.TryParse(localRateRel?.Text ?? "1", out localRate))
                localRate = m_Controller.LocalRate;
            m_Controller.LocalRate = localRate;
            string cat = categoryRel?.Text;
            m_Controller.Category = string.IsNullOrWhiteSpace(cat) ? null : cat.Trim();
            // resource multipliers
            int index = 0;
            foreach (ResourceType res in Enum.GetValues(typeof(ResourceType)))
            {
                TextRelay rel = info.GetTextEntry(10 + index);
                double mul;
                if (!double.TryParse(rel?.Text ?? "1", out mul))
                    mul = 1.0;
                m_Controller.SetResourceMultiplier(res, mul);
                index++;
            }
            m_From.SendMessage("Region settings saved.");
            m_From.SendGump(new RegionEditorGump(m_From, m_Controller));
        }
    }
}
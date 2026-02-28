using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ECommons.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Text;

namespace YhumiTweaks.UI
{
    internal class TweaksWindow : Window
    {
        public TweaksWindow() : base($"{P.Name} {P.GetType().Assembly.GetName().Version}###PuzdraLighting")
        {
            this.RespectCloseHotkey = false;
            this.SizeConstraints = new()
            {
                MinimumSize = new(500, 300),
                MaximumSize = new(9999, 9999)
            };
            P.ws.AddWindow(this);
        }

        public void Dispose()
        {
        }

        public override void Draw() 
        {
            var autoGlamWeddingRingRef = P.Config.AutoGlamWeddingRing;
            var autoGlamWeddingRingThrottleMs = P.Config.AutoGlamWeddingRingThrottleMs;
            int defaultThrottle = 50;

            if (ImGui.CollapsingHeader("Wedding Ring AutoGlam Settings.", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Checkbox($"Auto Glam Wedding Ring Enabled", ref autoGlamWeddingRingRef))
                {
                    P.Config.AutoGlamWeddingRing = autoGlamWeddingRingRef;
                    P.Config.Save();
                }

                ImGuiEx.Text(ImGuiColors.DalamudRed, "Setting this value too low WILL cause the autoglam to fail and other issues.");
                if (ImGui.Button("Reset to Defaults"))
                {
                    P.Config.AutoGlamWeddingRingThrottleMs = defaultThrottle;
                    P.Config.Save();
                }

                ImGui.Text("Wedding Glam Throttling");
                ImGuiComponents.HelpMarker("The wait time in miliseconds used for wedding ring glam throttling.");
                if (ImGui.DragInt("###WeddingThrottleTime", ref autoGlamWeddingRingThrottleMs))
                {
                    P.Config.AutoGlamWeddingRingThrottleMs = autoGlamWeddingRingThrottleMs;
                    P.Config.Save();
                }
            }          
        }
    }
}

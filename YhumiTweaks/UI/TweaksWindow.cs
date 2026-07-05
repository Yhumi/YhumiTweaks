using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ECommons.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Text;
using YhumiTweaks.Helpers;

namespace YhumiTweaks.UI
{
    internal struct PenumbraModSettings
    {
        public bool Enabled;
        public int Priority;
        public Dictionary<string, List<string>> Settings;
        public bool Unk;

        public string ModName;
        public Guid ModCollection;
        public string ModCollectionName;

        public PenumbraModSettings((bool enabled, int priority, Dictionary<string, List<string>> settings, bool unk) ipcSettings, string modName, Guid modCol, string modColName) 
        {
            Enabled = ipcSettings.enabled;
            Priority = ipcSettings.priority;
            Settings = ipcSettings.settings;
            Unk = ipcSettings.unk;

            ModName = modName;
            ModCollection = modCol;
            ModCollectionName = modColName;
        }

        public PenumbraModSettings((bool enabled, int priority, Dictionary<string, List<string>> settings, bool unk)? ipcSettings, string modName, Guid modCol, string modColName)
        {
            Enabled = ipcSettings?.enabled ?? false;
            Priority = ipcSettings?.priority ?? 0;
            Settings = ipcSettings?.settings ?? [];
            Unk = ipcSettings?.unk ?? false;

            ModName = modName;
            ModCollection = modCol;
            ModCollectionName = modColName;
        }
    }

    internal class TweaksWindow : Window
    {
        private readonly int MIN_INPUT_RANGE = 0;
        private readonly int MAX_INPUT_RANGE = 100;
        private readonly float MIN_TILT = -0.08f;
        private readonly float MAX_TILT = 0.21f;

        private KeyValuePair<Guid, string>? FromSelectedCollection;
        private KeyValuePair<Guid, string>? ToSelectedCollection;
        private KeyValuePair<string, string>? SelectedMod;

        private PenumbraModSettings? FromCollectionModSettings;
        private PenumbraModSettings? BackupCollectionModSettings;

        public TweaksWindow() : base($"{P.Name} {P.GetType().Assembly.GetName().Version}###YhumiSettings")
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
            if (ImGui.BeginTabBar("Tab Bar##YT_TabBar", ImGuiTabBarFlags.None))
            {
                DrawMainTab();
                DrawWeddingRingTab();
                DrawCameraHeightAdjustmentTab();
                DrawPenumbraSettingsCopy();

                ImGui.EndTabBar();
            }
        }

        private void DrawMainTab()
        {
            if (ImGui.BeginTabItem("Main###YT_MainTab"))
            {
                var currentCharacterId = Svc.PlayerState.ContentId;
                ImGui.Text($"Current Character: {Svc.PlayerState.CharacterName} - FFXIV_CHR{currentCharacterId:X16}");

                ImGui.Spacing();

                if (ImGui.Button($"Log indexes###YT_LogIndexes"))
                {
                    GameConfig.System.LogIndexes("System");
                    GameConfig.UiConfig.LogIndexes("UiConfig");
                    GameConfig.UiControl.LogIndexes("UiControl");
                }

                ImGui.EndTabItem();
            }    
        }

        private void DrawWeddingRingTab()
        {
            if (ImGui.BeginTabItem("Wedding Ring###YT_WeddingRingHeader"))
            {
                var autoGlamWeddingRingRef = P.Config.AutoGlamWeddingRing;
                var autoGlamWeddingRingThrottleMs = P.Config.AutoGlamWeddingRingThrottleMs;
                int defaultThrottle = 50;

                if (ImGui.Checkbox($"Auto Glam Wedding Ring Enabled###YT_AutoGlamWeddingRing", ref autoGlamWeddingRingRef))
                {
                    P.Config.AutoGlamWeddingRing = autoGlamWeddingRingRef;
                    P.Config.Save();
                }

                ImGuiEx.Text(ImGuiColors.DalamudRed, "Setting this value too low WILL cause the autoglam to fail and other issues.");
                if (ImGui.Button("Reset to Defaults###YT_Reset"))
                {
                    P.Config.AutoGlamWeddingRingThrottleMs = defaultThrottle;
                    P.Config.Save();
                }

                ImGui.Text("Wedding Glam Throttling");
                ImGuiComponents.HelpMarker("The wait time in miliseconds used for wedding ring glam throttling.");
                if (ImGui.DragInt("###YT_WeddingThrottleTime", ref autoGlamWeddingRingThrottleMs))
                {
                    P.Config.AutoGlamWeddingRingThrottleMs = autoGlamWeddingRingThrottleMs;
                    P.Config.Save();
                }

                ImGui.EndTabItem();
            }
        }
        
        private void DrawCameraHeightAdjustmentTab()
        {
            if (ImGui.BeginTabItem("Camera Tilt###YT_CamTiltHeader"))
            {
                ImGui.TextWrapped($"This just isn't working rn, I wouldn't enable this. More fairly it just doesn't detect when you enter an instance very well, I need to write better logic.");

                var autoCorrectCamerHeight = P.Config.AutoCorrectCameraHeight;

                if (ImGui.Checkbox($"Auto Adjust Camera Tilt###YT_AutoCameraTilt", ref autoCorrectCamerHeight))
                {
                    P.Config.AutoCorrectCameraHeight = autoCorrectCamerHeight;
                    P.Config.Save();
                }

                var outOfInstanceTilt = P.Config.SavedOutOfInstanceHeight;
                var instanceTilt = P.Config.SavedInstanceHeight;

                var refOutOfInstance = (int)MapFromTiltOffset(outOfInstanceTilt);
                var refInstance = (int)MapFromTiltOffset(instanceTilt);

                var inInstance = Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BoundByDuty];

                ImGui.Text($"Currently instanced: {inInstance}");
                ImGui.Text($"Current Tilt: {GameSettings.GetTilt()}");

                ImGui.Spacing();

                ImGui.Text($"Insance Tilt: {instanceTilt}");
                if (ImGui.SliderInt($"Saved Instance Tilt###YT_IT", ref refInstance, MIN_INPUT_RANGE, MAX_INPUT_RANGE))
                {
                    var tiltOffset = MapToTiltOffset(refOutOfInstance);
                    P.Config.SavedInstanceHeight = tiltOffset;
                    P.Config.Save();
                    if (inInstance && P.Config.AutoCorrectCameraHeight)
                        GameSettings.SetTilt(tiltOffset);
                }

                ImGui.Text($"World Tilt: {outOfInstanceTilt}");
                if (ImGui.SliderInt($"Saved World tilt###YT_WT", ref refOutOfInstance, MIN_INPUT_RANGE, MAX_INPUT_RANGE))
                {
                    var tiltOffset = MapToTiltOffset(refOutOfInstance);
                    P.Config.SavedOutOfInstanceHeight = tiltOffset;
                    P.Config.Save();
                    if (!inInstance && P.Config.AutoCorrectCameraHeight)
                        GameSettings.SetTilt(tiltOffset);
                }

                ImGui.EndTabItem();
            }
        }
        
        private void DrawPenumbraSettingsCopy()
        {
            if (ImGui.BeginTabItem("Penumbra Settings Copy###YT_PSC"))
            {
                ImGui.TextWrapped($"PLEASE NOTE: This is super dangerous, you may lose settings. Please be careful. ♥");
                ImGui.TextWrapped($"You can revert a change up until you make another copy/reload the plugin. The state of the currently ready revert is below.");
                ImGui.TextWrapped($"Also note: reverting wont work if the settings before were default. You'll also need to enable/prio the mod yourself, this just sets the dropdowns/toggles for now.");
                ImGui.Spacing();

                var penumbraCollections = P.PenumbraIPC.GetCollections();
                var penumbraMods = P.PenumbraIPC.GetModList();

                ImGui.Text($"Select Mod to Copy:");
                if (ImGui.BeginCombo("##YT_ModToCopy", SelectedMod?.Value.ToString() ?? ""))
                {
                    foreach (var mod in penumbraMods)
                    {
                        bool isSelected = SelectedMod.HasValue ? SelectedMod.Value.Key == mod.Key : false;
                        if (ImGui.Selectable(mod.Key, isSelected))
                        {
                            SelectedMod = mod;
                        }
                    }

                    ImGui.EndCombo();
                }

                ImGui.SameLine();
                if (ImGui.Button($"Clear##ClearMod"))
                {
                    SelectedMod = null;
                }

                ImGui.Spacing();

                ImGui.Text($"Select Collection to Copy Settings From:");
                if (ImGui.BeginCombo("##YT_PenumbraCollectionsFrom", FromSelectedCollection?.Value.ToString() ?? "")) 
                {
                    foreach (var collection in penumbraCollections)
                    {
                        bool isSelected = FromSelectedCollection.HasValue ? FromSelectedCollection.Value.Key == collection.Key : false;
                        if (ImGui.Selectable(collection.Value, isSelected))
                        {
                            FromSelectedCollection = collection;
                        }
                    }

                    ImGui.EndCombo();
                }

                ImGui.SameLine();
                if (ImGui.Button($"Clear##ClearFrom"))
                {
                    FromSelectedCollection = null;
                }

                ImGui.Spacing();

                ImGui.Text($"Select Target Colletion to apply settings to:");
                if (ImGui.BeginCombo("##YT_PenumbraCollectionsTo", ToSelectedCollection?.Value.ToString() ?? ""))
                {
                    foreach (var collection in penumbraCollections)
                    {
                        bool isSelected = ToSelectedCollection.HasValue ? ToSelectedCollection.Value.Key == collection.Key : false;
                        if (ImGui.Selectable(collection.Value, isSelected))
                        {
                            ToSelectedCollection = collection;
                        }
                    }

                    ImGui.EndCombo();
                }

                ImGui.SameLine();
                if (ImGui.Button($"Clear##ClearTo"))
                {
                    ToSelectedCollection = null;
                }

                ImGui.Spacing();

                if (ImGuiEx.ButtonCtrl($"Copy Settings"))
                {
                    if (SelectedMod.HasValue && FromSelectedCollection.HasValue && ToSelectedCollection.HasValue)
                    {
                        Svc.Log.Info($"Copying [{SelectedMod.Value.Key}] {SelectedMod.Value.Value} from [{FromSelectedCollection.Value.Key}] {FromSelectedCollection.Value.Value} to [{ToSelectedCollection.Value.Key}] {ToSelectedCollection.Value.Value}");
                        var collectionModInformation = P.PenumbraIPC.GetModSettings(FromSelectedCollection.Value.Key, SelectedMod.Value.Key);
                        var targetColletionModInformation = P.PenumbraIPC.GetModSettings(ToSelectedCollection.Value.Key, SelectedMod.Value.Key);

                        Svc.Log.Debug($"[Col: {FromSelectedCollection.Value.Key}, Mod: {SelectedMod.Value.Key}] Result: {collectionModInformation.result}");
                        Svc.Log.Debug($"Value: {collectionModInformation.settings}");

                        if (collectionModInformation.result == Penumbra.Api.Enums.PenumbraApiEc.Success)
                        {
                            if (collectionModInformation.settings.HasValue)
                            {
                                BackupCollectionModSettings = new PenumbraModSettings(targetColletionModInformation.settings ?? null, SelectedMod.Value.Key, ToSelectedCollection.Value.Key, ToSelectedCollection.Value.Value);
                                FromCollectionModSettings = new PenumbraModSettings(collectionModInformation.settings.Value, SelectedMod.Value.Key, FromSelectedCollection.Value.Key, FromSelectedCollection.Value.Value);
                                P.PenumbraIPC.TrySetModSettings(ToSelectedCollection.Value.Key, SelectedMod.Value.Key, collectionModInformation.settings.Value.settings);
                            }       
                        }
                    }
                }

                ImGui.SameLine();
                if (ImGuiEx.Button($"Revert", BackupCollectionModSettings.HasValue))
                {
                    if (BackupCollectionModSettings.HasValue)
                        P.PenumbraIPC.TrySetModSettings(BackupCollectionModSettings.Value.ModCollection, BackupCollectionModSettings.Value.ModName, BackupCollectionModSettings.Value.Settings);
                }

                ImGui.Separator();
                ImGui.Text($"Current revertible item state:");
                if (BackupCollectionModSettings.HasValue)
                {
                    ImGui.Text($"Mod: {BackupCollectionModSettings.Value.ModName}");
                    ImGui.Text($"Collection: {BackupCollectionModSettings.Value.ModCollectionName} ({BackupCollectionModSettings.Value.ModCollection})");
                }

                ImGui.EndTabItem();
            }
        }

        private float MapToTiltOffset(float value)
        {
            return (value - MIN_INPUT_RANGE) / (MAX_INPUT_RANGE - MIN_INPUT_RANGE) * (MAX_TILT - MIN_TILT) + MIN_TILT;
        }

        private float MapFromTiltOffset(float value)
        {
            return (value - MIN_TILT) / (MAX_TILT - MIN_TILT) * (MAX_INPUT_RANGE - MIN_INPUT_RANGE) + MIN_INPUT_RANGE;
        }
    }
}

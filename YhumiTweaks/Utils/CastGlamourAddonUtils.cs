using Dalamud.Game.NativeWrapper;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using YhumiTweaks.Readers;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
using static YhumiTweaks.Readers.ReaderMiragePrismAddon;

namespace YhumiTweaks.Utils
{
    internal class CastGlamourAddonUtils
    {
        public static unsafe bool IsMiragePrismUnlocked() => UIState.Instance()->IsUnlockLinkUnlocked(15);
        public static bool IsMiragePrismOpen() => Svc.GameGui.GetAddonByName("MiragePrism", 1) != IntPtr.Zero;

        public unsafe static bool OpenMiragePrism()
        {
            if (!IsMiragePrismOpen())
            {
                ActionManager.Instance()->UseAction(ActionType.GeneralAction, 22);
            }
            return true;
        }

        public unsafe static bool SelectItemToGlam(string itemName)
        {
            if (TryGetAddonByName<AtkUnitBase>("MiragePrism", out var miragePrism) && IsAddonReady(miragePrism))
            {
                var itemReader = new ReaderMiragePrismAddon(((AtkUnitBasePtr)miragePrism).Address);

                foreach(var item in itemReader.ItemNameList)
                {
                    Svc.Log.Verbose($"{item.Name}");
                }

                var itemIndex = itemReader.ItemNameList.IndexOf(x => x.Name.Contains(itemName, StringComparison.CurrentCultureIgnoreCase));
                Svc.Log.Debug($"Item Reader Item Index: {itemIndex} for {itemName} @ atkPos {itemReader.ItemListAtkPosition(itemIndex)}");

                if (itemIndex == -1) return false;

                Callback.Fire(miragePrism, true, 1, itemIndex);

                return true;
            }
            return false;
        }

        public unsafe static bool SelectEternityRing()
        {
            if (TryGetAddonByName<AtkUnitBase>("MiragePrism", out var miragePrism) && IsAddonReady(miragePrism))
            {
                var itemReader = new ReaderMiragePrismAddon(((AtkUnitBasePtr)miragePrism).Address);
                var itemIndex = itemReader.GlamourNamesList.IndexOf(x => x.Name.Equals("Eternity Ring", StringComparison.CurrentCultureIgnoreCase));
                Svc.Log.Debug($"Item Reader Glamour Index: {itemIndex} for Eternity Ring @ atkPos {itemReader.GlamourListAtkPosition(itemIndex)}");

                if (itemIndex == -1) return false;

                Callback.Fire(miragePrism, true, 2, itemIndex);

                return true;
            }
            return false;
        }

        public unsafe static bool ConfirmGlam()
        {
            if (TryGetAddonByName<AtkUnitBase>("MiragePrismExecute", out var miragePrismExectute) && IsAddonReady(miragePrismExectute))
            {
                Svc.Log.Debug($"Executing glam.");
                Callback.Fire(miragePrismExectute, true, 0);
                return true;
            }
            return false;
        }

        public unsafe static bool CloseWindow()
        {
            if (TryGetAddonByName<AtkUnitBase>("MiragePrism", out var miragePrism) && IsAddonReady(miragePrism))
            {
                Callback.Fire(miragePrism, true, -1);
                return true;
            }
            return false;
        }
    }
}

using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using ECommons;
using ECommons.DalamudServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YhumiTweaks.Data;
using YhumiTweaks.Utils;

namespace YhumiTweaks.Controllers
{
    internal unsafe class GlamController : IDisposable
    {
        private uint EternityRingId = 0;
        private uint GlamourPrismId = 0;

        private GlamStages currentGlamStage = GlamStages.None;
        private int currentStageAttempts = 0;
        private bool ringsMatch = false;

        private string EquippedLeftRingName = string.Empty;

        private List<uint> previouslyEquipped = new List<uint>();

        public GlamController()
        {
            EternityRingId = LuminaSheets.ItemSheet.Values.Where(x => x.Name.ToString().ToLower() == "eternity ring").FirstOrNull()?.RowId ?? 0;
            GlamourPrismId = LuminaSheets.ItemSheet.Values.Where(x => x.Name.ToString().ToLower() == "glamour prism").FirstOrNull()?.RowId ?? 0;

            if (EternityRingId != 0 && GlamourPrismId != 0)
            {
                Svc.GameInventory.InventoryChanged += GameInventory_ItemMoved;
            }
        }

        private void GameInventory_ItemMoved(IReadOnlyCollection<InventoryEventArgs> events)
        {
            if (!P.Config.AutoGlamWeddingRing)
                return;

            if (!CastGlamourAddonUtils.IsMiragePrismUnlocked())
                return;

            var currentlyEquipped = new List<uint>();
            foreach (var e in events.Where(x => x.Item.ContainerType == GameInventoryType.EquippedItems))
            {
                currentlyEquipped.Add(e.Item.ItemId);
            }

            if (!previouslyEquipped.Except(currentlyEquipped).Any() && !currentlyEquipped.Except(previouslyEquipped).Any())
            {
                Svc.Log.Debug($"No changes to equipped items.");
                return;
            }
            previouslyEquipped = currentlyEquipped;

            var eternityRing = CharacterInfo.FindInventoryItem(EternityRingId);
            if (eternityRing == null)
            {
                Svc.Chat.PrintError("No Eternity Ring found.");
                return;
            }
            Svc.Log.Debug($"ERing Found. Aww ♥");

            var glamourPrisms = CharacterInfo.FindInventoryItem(GlamourPrismId);
            if (eternityRing == null)
            {
                Svc.Chat.PrintError("No Glamour Prisms found.");
                return;
            }
            Svc.Log.Debug($"Glamour prisms found.");

            if (events.Any(x => x.Type == GameInventoryEvent.Added || x.Type == GameInventoryEvent.Removed || x.Type == GameInventoryEvent.Moved))
            {
                var equippedLeftRing = CharacterInfo.EquippedGear->GetInventorySlot((int)CharacterEquippedGearSlotIndex.LeftRing);

                if (equippedLeftRing->GlamourId == EternityRingId || equippedLeftRing->GetBaseItemId() == EternityRingId)
                {
                    Svc.Log.Debug($"Left ring is already glammed as the eternity ring or already is the eternity ring.");
                    return;
                }

                var equippedRightRing = CharacterInfo.EquippedGear->GetInventorySlot((int)CharacterEquippedGearSlotIndex.RightRing);
                if (equippedLeftRing->GetBaseItemId() == equippedRightRing->GetBaseItemId())
                {
                    Svc.Log.Debug($"Rings match.");
                    ringsMatch = true;
                }

                var newLeftRingItem = LuminaSheets.ItemSheet?.Where(x => x.Value.RowId == equippedLeftRing->GetBaseItemId()).FirstOrNull()?.Value.Name.GetText() ?? string.Empty;
                Svc.Log.Debug($"Left Ring: {equippedLeftRing->GetBaseItemId()} - {newLeftRingItem}.");

                if (EquippedLeftRingName == newLeftRingItem)
                    return;

                EquippedLeftRingName = newLeftRingItem;

                if (string.IsNullOrEmpty(newLeftRingItem))
                    return;

                Svc.Log.Debug($"Starting glamming...");

                P.TM.Enqueue(() => CastGlamourAddonUtils.OpenMiragePrism(), "Open Cast Glamour");

                if (ringsMatch)
                    currentGlamStage = GlamStages.UnequipRightRing;
                else
                    currentGlamStage = GlamStages.SelectItem;

                currentStageAttempts = 0;
            }
        }

        public void Tick()
        {
            if (!EzThrottler.Check($"YhumiTweaks.SecondRing")) return;
            if (!EzThrottler.Throttle("YhumiTweaks.GlamControllerLoop", P.Config.AutoGlamWeddingRingThrottleMs)) return;

            if (!Svc.ClientState.IsLoggedIn) return;
            if (!Svc.PlayerState.IsLoaded) return;
            if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas]) return;
            if (currentGlamStage == GlamStages.None) return;

            if (currentStageAttempts > 10)
            {
                Svc.Log.Debug($"Stopping after 10 failed attempts.");
                currentGlamStage = GlamStages.None;
            }

            if (currentGlamStage == GlamStages.UnequipRightRing)
            {
                currentGlamStage = GlamStages.SelectItem;
            }

            if (currentGlamStage == GlamStages.SelectItem)
            {
                if (CastGlamourAddonUtils.SelectItemToGlam(EquippedLeftRingName))
                {
                    Svc.Log.Debug($"Selected ring by name.");
                    currentGlamStage = GlamStages.GlamEternityRing;
                    currentStageAttempts = 0;
                }
                else { currentStageAttempts++; }
            }

            if (currentGlamStage == GlamStages.GlamEternityRing)
            {
                if (CastGlamourAddonUtils.SelectEternityRing())
                {
                    Svc.Log.Debug($"Selected ERing to glam.");
                    currentGlamStage = GlamStages.ConfirmGlamour;
                    currentStageAttempts = 0;
                }
                else { currentStageAttempts++; }
            }

            if (currentGlamStage == GlamStages.ConfirmGlamour)
            {
                if (CastGlamourAddonUtils.ConfirmGlam())
                {
                    Svc.Log.Debug($"Yay wahoo.");
                    currentGlamStage = GlamStages.CloseWindow;
                    currentStageAttempts = 0;
                }
                else { currentStageAttempts++; }
            }

            if (currentGlamStage == GlamStages.CloseWindow)
            {
                if (CastGlamourAddonUtils.CloseWindow())
                {
                    if (!ringsMatch)
                        currentGlamStage = GlamStages.None;
                    else
                        currentGlamStage = GlamStages.ReEquipRightRing;

                    currentStageAttempts = 0;
                }
                else { currentStageAttempts++; }
            }

            if (currentGlamStage == GlamStages.ReEquipRightRing)
            {
                currentGlamStage = GlamStages.None  ;
            }
        }

        public void Dispose()
        {
            GenericHelpers.Safe(() => Svc.GameInventory.InventoryChanged -= GameInventory_ItemMoved);
        }
    }
    public enum GlamStages
    {
        None,
        UnequipRightRing,
        SelectItem,
        GlamEternityRing,
        ConfirmGlamour,
        CloseWindow,
        ReEquipRightRing
    }
}

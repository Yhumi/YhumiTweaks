using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;

namespace YhumiTweaks.Data
{
    internal static unsafe class CharacterInfo
    {
        public static InventoryContainer* EquippedGear;

        public static unsafe void SetCharaInventoryPointers()
        {
            EquippedGear = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems);
        }

        public static unsafe InventoryItem* FindInventoryItem(uint itemId)
        {
            //Now check in AC
            var equipSlotCategory = LuminaSheets.ItemSheet[(uint)itemId].EquipSlotCategory.Value;
            var equippedSlot = GetSlotIndexFromEquipSlotCategory(equipSlotCategory);
            if (equippedSlot == null)
                return null;

            var acItem = SearchForItemInArmouryChest(itemId, equippedSlot.Value);
            if (acItem != null)
                return acItem;

            //Finally check in inv itself
            return SearchForItemInPlayerInventory(itemId);
        }

        public static unsafe InventoryItem* SearchForItemInArmouryChest(uint itemId, CharacterEquippedGearSlotIndex gearSlot)
        {
            switch (gearSlot)
            {
                case CharacterEquippedGearSlotIndex.MainHand:
                    return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.ArmoryMainHand), itemId);

                case CharacterEquippedGearSlotIndex.OffHand:
                    return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.ArmoryOffHand), itemId);

                case CharacterEquippedGearSlotIndex.Head:
                    return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.ArmoryHead), itemId);

                case CharacterEquippedGearSlotIndex.Body:
                    return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.ArmoryBody), itemId);

                case CharacterEquippedGearSlotIndex.Gloves:
                    return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.ArmoryHands), itemId);

                case CharacterEquippedGearSlotIndex.Legs:
                    return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.ArmoryLegs), itemId);

                case CharacterEquippedGearSlotIndex.Feet:
                    return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.ArmoryFeets), itemId);

                case CharacterEquippedGearSlotIndex.Ears:
                    return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.ArmoryEar), itemId);

                case CharacterEquippedGearSlotIndex.Neck:
                    return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.ArmoryNeck), itemId);

                case CharacterEquippedGearSlotIndex.Wrists:
                    return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.ArmoryWrist), itemId);

                case CharacterEquippedGearSlotIndex.RightRing:
                case CharacterEquippedGearSlotIndex.LeftRing:
                    return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.ArmoryRings), itemId);

                default:
                    return null;
            }
        }

        public static InventoryItem* SearchForItemInPlayerInventory(uint itemId)
        {
            var inv1Item = SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.Inventory1), itemId);
            if (inv1Item != null) return inv1Item;

            var inv2Item = SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.Inventory2), itemId);
            if (inv2Item != null) return inv2Item;

            var inv3Item = SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.Inventory3), itemId);
            if (inv3Item != null) return inv3Item;

            return SearchForItemInInventory(InventoryManager.Instance()->GetInventoryContainer(InventoryType.Inventory4), itemId);
        }

        private static InventoryItem* SearchForItemInInventory(InventoryContainer* inv, uint itemId)
        {
            int invSize = inv->Size;
            for (int i = 0; i < invSize; i++)
            {
                if (inv->GetInventorySlot(i)->ItemId == itemId)
                    return inv->GetInventorySlot(i);
            }

            return null;
        }

        public static CharacterEquippedGearSlotIndex? GetSlotIndexFromEquipSlotCategory(EquipSlotCategory? category)
        {
            if (category == null) return null;
            if (category.Value.MainHand == 1) return CharacterEquippedGearSlotIndex.MainHand;
            if (category.Value.OffHand == 1) return CharacterEquippedGearSlotIndex.OffHand;
            if (category.Value.Head == 1) return CharacterEquippedGearSlotIndex.Head;
            if (category.Value.Body == 1) return CharacterEquippedGearSlotIndex.Body;
            if (category.Value.Gloves == 1) return CharacterEquippedGearSlotIndex.Gloves;
            if (category.Value.Waist == 1) return CharacterEquippedGearSlotIndex.Waist;
            if (category.Value.Legs == 1) return CharacterEquippedGearSlotIndex.Legs;
            if (category.Value.Feet == 1) return CharacterEquippedGearSlotIndex.Feet;
            if (category.Value.Ears == 1) return CharacterEquippedGearSlotIndex.Ears;
            if (category.Value.Neck == 1) return CharacterEquippedGearSlotIndex.Neck;
            if (category.Value.Wrists == 1) return CharacterEquippedGearSlotIndex.Wrists;
            if (category.Value.FingerR == 1) return CharacterEquippedGearSlotIndex.RightRing;
            if (category.Value.FingerL == 1) return CharacterEquippedGearSlotIndex.LeftRing;
            if (category.Value.SoulCrystal == 1) return CharacterEquippedGearSlotIndex.SoulCrystal;
            return null;
        }
    }

    public enum CharacterEquippedGearSlotIndex : uint
    {
        MainHand = 0,
        OffHand = 1,
        Head = 2,
        Body = 3,
        Gloves = 4,
        Waist = 5,
        Legs = 6,
        Feet = 7,
        Ears = 8,
        Neck = 9,
        Wrists = 10,
        RightRing = 11,
        LeftRing = 12,
        SoulCrystal = 13
    }
}

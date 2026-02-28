using ECommons.UIHelpers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace YhumiTweaks.Readers
{
    internal class ReaderMiragePrismAddon(nint Addon) : AtkReader(Addon)
    {
        public int ItemListNameLength => ReadInt(6) ?? 0;
        public int ValidGlamourCount => ReadInt(708) ?? 0;

        public List<ItemNames> ItemNameList => Loop<ItemNames>(147, 1, (int) ItemListNameLength);
        public List<GlamourNames> GlamourNamesList => Loop<GlamourNames>(2313, 1, (int) ValidGlamourCount);

        public int ItemListAtkPosition(int index) => 147 + index;
        public int GlamourListAtkPosition(int index) => 2313 + index;

        public unsafe class ItemNames(nint Addon, int start) : AtkReader(Addon, start)
        {
            public string Name => ReadSeString(0).GetText();
        }

        public unsafe class GlamourNames(nint Addon, int start) : AtkReader(Addon, start)
        {
            public string Name => ReadSeString(0).GetText();
        }
    }
}

using ECommons.DalamudServices;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;
using System;

namespace YhumiTweaks.Data
{
    internal class LuminaSheets
    {
        public static Dictionary<uint, Item>? ItemSheet;

        public static void Init()
        {
            ItemSheet = Svc.Data?.GetExcelSheet<Item>()?
                       .ToDictionary(i => i.RowId, i => i);
        }

        public static void Dispose()
        {
            var type = typeof(LuminaSheets);
            foreach (var prop in type.GetFields(System.Reflection.BindingFlags.Static))
            {
                prop.SetValue(null, null);
            }
        }
    }
}

using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YhumiTweaks.Models;

namespace YhumiTweaks.Helpers
{
    public static class CarbyPlushyHelper
    {
        public static unsafe string FishFinder(string inputFromCarby)
        {
            if (string.IsNullOrWhiteSpace(inputFromCarby)) return string.Empty;

            var ps = PlayerState.Instance();
            if (ps == null) return string.Empty;

            var caughtFish = new List<uint>();
            var fishSheet = Svc.Data.GetExcelSheet<FishParameter>();
            foreach (var fish in fishSheet.Where(x => x.RowId < 1_000_000))
            {
                if (!fish.IsInLog) continue;
                if (fish.Item.RowId == 0) continue;

                var itemNullable = fish.Item.GetValueOrDefault<Item>();
                if (itemNullable == null) continue;
                var item = itemNullable.Value;

                var name = item.Name.ToString();
                if (string.IsNullOrWhiteSpace(name)) continue;

                Svc.Log.Verbose($"[YT Fish Export] Fish: {name}, Param Id: {fish.RowId}, Item Id: {fish.Item.RowId} Caught? {ps->IsFishCaught(fish.RowId)}");

                if (ps->IsFishCaught(fish.RowId))
                    caughtFish.Add(fish.Item.RowId);
            }

            var json = JsonConvert.DeserializeObject<CarbyPlushyConfig>(inputFromCarby);
            if (json == null) return string.Empty;

            json.Completed = caughtFish;
            return JsonConvert.SerializeObject(json);
        }
    }
}

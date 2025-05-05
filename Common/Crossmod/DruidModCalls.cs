using System;

using RoA.Content.Items;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Crossmod;

public static class DruidModCalls {
    public static object Call(params object[] args) {
        RoA riseOfAges = RoA.Instance;
        Array.Resize(ref args, 2);
        string success = "Success";
        try {
            string message = args[0] as string;
            if (message == "MakeThisItemNature") {
                ModItem item = (args[1] as Item).ModItem ?? throw new Exception($"Item is not ModItem");
                NatureItems.RegisterNatureItem(item);
                return success;
            }
        }
        catch (Exception e) {
            riseOfAges.Logger.Error($"Call Error: {e.StackTrace} {e.Message}");
        }
        return null;
    }
}

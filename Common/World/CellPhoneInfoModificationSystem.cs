using Microsoft.CodeAnalysis.Differencing;

using Mono.Cecil.Cil;

using MonoMod.Cil;

﻿using RoA.Common.World;
using RoA.Content.Biomes.Backwoods;
using RoA.Core;

using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.World;

// wrath of the gods
sealed class CellPhoneInfoModificationSystem : IInitializer {
    public enum InfoType {
        Time,
        Weather,
        FishingPower,
        MoonPhase,
        TreasureTiles,
        PlayerSpeed,
        PlayerXPosition,
        PlayerYPosition
    }

    public delegate string TextReplacementFunction(string originalText);

    void ILoadable.Load(Mod mod) {
        IL_Main.DrawInfoAccs += IL_Main_DrawInfoAccs;
    }

    private void IL_Main_DrawInfoAccs(ILContext il) {
        int displayTextIndex = 0;
        ILCursor cursor = new ILCursor(il);

        // Find the text display string.
        for (int i = 0; i < 2; i++) {
            if (!cursor.TryGotoNext(MoveType.Before, c => c.MatchLdstr(out _))) {
                return;
            }
        }

        // Store the display string's local index.
        if (!cursor.TryGotoNext(c => c.MatchStloc(out displayTextIndex))) {
            return;
        }

        // Change text for the watch.
        if (!cursor.TryGotoNext(MoveType.After, c => c.MatchLdsfld<Main>("time"))) {
            return;
        }

        cursor.Emit(OpCodes.Pop);
        cursor.EmitDelegate(() => {
            return (double)Main.rand.NextFloat(86400f);
        });

        ApplyReplacementTweak(cursor, InfoType.Weather, "GameUI.PartlyCloudy", displayTextIndex, ChooseWeatherText);
    }

    private static string ChooseWeatherText(string originalText) {
        if (BackwoodsFogHandler.IsFogActive) {
            if (Main.GlobalTimeWrappedHourly % 15f >= 10f)
                return Language.GetTextValue("Mods.RoA.Biomes.BackwoodsBiomeFog.DisplayName");
        }

        return originalText;
    }

    private static void ApplyReplacementTweak(ILCursor cursor, InfoType infoType, string searchString, int displayTextIndex, TextReplacementFunction replacementFunction, int loopCount = 1) {
        if (!cursor.TryGotoNext(c => c.MatchLdstr(searchString))) {
            return;
        }

        for (int i = 1; i <= loopCount; i++) {
            if (!cursor.TryGotoNext(MoveType.After, c => c.MatchStloc(displayTextIndex))) {
                return;
            }

            cursor.Emit(OpCodes.Ldloc, displayTextIndex);
            cursor.EmitDelegate(replacementFunction);
            cursor.Emit(OpCodes.Stloc, displayTextIndex);
        }
    }
}

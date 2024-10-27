using Microsoft.Xna.Framework;

using RoA.Common.Utilities.Extensions;
using RoA.Common.WorldEvents;

using System;

using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace RoA.Content.Biomes.Backwoods;

sealed partial class BackwoodsBiome : ModBiome {
    public override void Load() {
        On_TileDrawing.UpdateLeafFrequency += On_TileDrawing_UpdateLeafFrequency;
        On_TileDrawing.Update += On_TileDrawing_Update;
    }

    private bool IsValid() => !BackwoodsFogHandler.IsFogActive && Main.LocalPlayer.InModBiome<BackwoodsBiome>();

    private void On_TileDrawing_Update(On_TileDrawing.orig_Update orig, TileDrawing self) {
        if (Main.dedServ) {
            return;
        }

        double treeWindCounter = typeof(TileDrawing).GetFieldValue<double>("_treeWindCounter", Main.instance.TilesRenderer);
        double grassWindCounter = typeof(TileDrawing).GetFieldValue<double>("_grassWindCounter", Main.instance.TilesRenderer);
        double sunflowerWindCounter = typeof(TileDrawing).GetFieldValue<double>("_sunflowerWindCounter", Main.instance.TilesRenderer);
        double vineWindCounter = typeof(TileDrawing).GetFieldValue<double>("_vineWindCounter", Main.instance.TilesRenderer);
        orig(self);
        if (IsValid()) {
            typeof(TileDrawing).SetFieldValue<double>("_treeWindCounter", treeWindCounter, Main.instance.TilesRenderer);
            typeof(TileDrawing).SetFieldValue<double>("_grassWindCounter", grassWindCounter, Main.instance.TilesRenderer);
            typeof(TileDrawing).SetFieldValue<double>("_sunflowerWindCounter", sunflowerWindCounter, Main.instance.TilesRenderer);
            typeof(TileDrawing).SetFieldValue<double>("_vineWindCounter", vineWindCounter, Main.instance.TilesRenderer);
            double num = BackwoodsFogHandler.IsFogActive ? 0 : Math.Max(Math.Abs(Main.WindForVisuals), 401 * 0.001f);
            num = Utils.GetLerpValue(0.08f, 1.2f, (float)num, clamped: true);
            treeWindCounter += 1.0 / 240.0 + 1.0 / 240.0 * num;
            grassWindCounter += 1.0 / 180.0 + 1.0 / 180.0 * num * 4.0;
            sunflowerWindCounter += 1.0 / 420.0 + 1.0 / 420.0 * num * 5.0;
            vineWindCounter += 1.0 / 120.0 + 1.0 / 120.0 * num * 0.4000000059604645;
            typeof(TileDrawing).SetFieldValue<double>("_treeWindCounter", treeWindCounter, Main.instance.TilesRenderer);
            typeof(TileDrawing).SetFieldValue<double>("_grassWindCounter", grassWindCounter, Main.instance.TilesRenderer);
            typeof(TileDrawing).SetFieldValue<double>("_sunflowerWindCounter", sunflowerWindCounter, Main.instance.TilesRenderer);
            typeof(TileDrawing).SetFieldValue<double>("_vineWindCounter", vineWindCounter, Main.instance.TilesRenderer);
        }
    }

    private void On_TileDrawing_UpdateLeafFrequency(On_TileDrawing.orig_UpdateLeafFrequency orig, TileDrawing self) {
        //float num = Math.Abs(Main.WindForVisuals);
        if (IsValid()) {
            float num = Math.Abs(Main.WindForVisuals);
            int leafFrequency;
            if (num <= 0.1f)
                leafFrequency = 100;
            else if (num <= 0.2f)
                leafFrequency = 100;
            else if (num <= 0.3f)
                leafFrequency = 100;
            else if (num <= 0.4f)
                leafFrequency = 100;
            else if (num <= 0.5f)
                leafFrequency = 100;
            else if (num <= 0.6f)
                leafFrequency = 100;
            else if (num <= 0.7f)
                leafFrequency = 75;
            else if (num <= 0.8f)
                leafFrequency = 50;
            else if (num <= 0.9f)
                leafFrequency = 40;
            else if (num <= 1f)
                leafFrequency = 30;
            else if (num <= 1.1f)
                leafFrequency = 20;
            else
                leafFrequency = 10;

            leafFrequency *= 7;

            typeof(TileDrawing).SetFieldValue<int>("_leafFrequency", BackwoodsFogHandler.IsFogActive ? (2000 * 7) : leafFrequency, Main.instance.TilesRenderer);
            return;
        }

        orig(self);
    }
}
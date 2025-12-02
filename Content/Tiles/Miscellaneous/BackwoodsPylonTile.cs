using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.CustomConditions;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Placeable.Miscellaneous;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class BackwoodsPylonTile : ModPylon {
    public const int CrystalVerticalFrameCount = 8;

    private static Asset<Texture2D> _crystalTexture = null!;
    private static Asset<Texture2D> _crystalHighlightTexture = null!;
    private static Asset<Texture2D> _mapIcon = null!;

    public override void SetStaticDefaults() {
        if (!Main.dedServ) {
            _crystalTexture = ModContent.Request<Texture2D>(Texture + "_Crystal");
            _crystalHighlightTexture = ModContent.Request<Texture2D>(Texture + "_CrystalHighlight");
            _mapIcon = ModContent.Request<Texture2D>(Texture + "_MapIcon");
        }

        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        // These definitions allow for vanilla's pylon TileEntities to be placed.
        // tModLoader has a built in Tile Entity specifically for modded pylons, which we must extend (see SimplePylonTileEntity)
        TEModdedPylon moddedPylon = ModContent.GetInstance<SimplePylonEntity>();
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

        TileObjectData.addTile(Type);

        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;
        TileID.Sets.AvoidedByMeteorLanding[Type] = true;

        // Adds functionality for proximity of pylons; if this is true, then being near this tile will count as being near a pylon for the teleportation process.
        AddToArray(ref TileID.Sets.CountsAsPylon);

        LocalizedText pylonName = CreateMapEntryName(); //Name is in the localization file
        AddMapEntry(new Color(79, 172, 211), pylonName);
    }

    public override NPCShop.Entry GetNPCShopEntry() {
        // return a new NPCShop.Entry with the desired conditions for sale.

        // As an example, if we want to sell the pylon if we're in the example surface, or example underground, when there is another NPC nearby.
        // Lets assume we don't care about happiness or crimson or corruption, so we won't include those conditions
        // This does not affect the teleport conditions, only the sale conditions
        return new NPCShop.Entry(ModContent.ItemType<BackwoodsPylon>(), Condition.AnotherTownNPCNearby, RoAConditions.InBackwoods);

        // Other standard pylon conditions are:
        // Condition.HappyEnoughToSellPylons
        // Condition.NotInEvilBiome
    }

    public override void MouseOver(int i, int j) {
        // Show a little pylon icon on the mouse indicating we are hovering over it.
        Main.LocalPlayer.cursorItemIconEnabled = true;
        Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<BackwoodsPylon>();
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        // We need to clean up after ourselves, since this is still a "unique" tile, separate from Vanilla Pylons, so we must kill the TileEntity.
        ModContent.GetInstance<SimplePylonEntity>().Kill(i, j);
    }

    public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData) {
        // Right before this hook is called, the sceneData parameter exports its information based on wherever the destination pylon is,
        // and by extension, it will call ALL ModSystems that use the TileCountsAvailable method. This means, that if you determine biomes
        // based off of tile count, when this hook is called, you can simply check the tile threshold, like we do here. In the context of ExampleMod,
        // something is considered within the Example Surface/Underground biome if there are 40 or more example blocks at that location.

        return BackwoodsBiome.BiomeShouldBeActive;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        // Pylons in vanilla light up, which is just a simple functionality we add using ModTile's ModifyLight.
        // Let's just add a simple white light for our pylon:
        r = 0.15f;
        g = 0.7f;
        b = 0.9f;
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
        // We want to draw the pylon crystal the exact same way vanilla does, so we can use this built in method in ModPylon for default crystal drawing:
        // For the sake of example, lets make our pylon create a bit more dust by decreasing the dustConsequent value down to 1. If you want your dust spawning to be identical to vanilla, set dustConsequent to 4.
        // We also multiply the pylonShadowColor in order to decrease its opacity, so it actually looks like a "shadow"
        NewDefaultDrawPylonCrystal(spriteBatch,
            i,
            j,
            _crystalTexture,
            _crystalHighlightTexture,
            new Vector2(0f, -12f),
            new Color(255, 255, 255, 0) * 0.1f,
            new Color(79, 172, 211),
            7,
            CrystalVerticalFrameCount);
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    private void NewDefaultDrawPylonCrystal(SpriteBatch spriteBatch, int i, int j, Asset<Texture2D> crystalTexture, Asset<Texture2D> crystalHighlightTexture, Vector2 crystalOffset, Color pylonShadowColor, Color dustColor, int dustChanceDenominator, int crystalVerticalFrameCount) {
        // Gets offscreen vector for different lighting modes
        Vector2 offscreenVector = new Vector2(Main.offScreenRange);
        if (Main.drawToScreen) {
            offscreenVector = Vector2.Zero;
        }

        // Double check that the tile exists
        Point point = new Point(i, j);
        Tile tile = Main.tile[point.X, point.Y];
        if (tile == null || !tile.HasTile) {
            return;
        }

        TileObjectData tileData = TileObjectData.GetTileData(tile);

        // Calculate frame based on vanilla counters in order to line up the animation
        int frameY = Main.tileFrameCounter[TileID.TeleportationPylon] / crystalVerticalFrameCount;

        // UsedFrame our modded crystal sheet accordingly for proper drawing
        Rectangle crystalFrame = crystalTexture.Frame(1, crystalVerticalFrameCount, 0, frameY);
        Rectangle smartCursorGlowFrame = crystalHighlightTexture.Frame(1, crystalVerticalFrameCount, 0, frameY);
        // I have no idea what is happening here; but it fixes the frame bleed issue. All I know is that the vertical sinusoidal motion has something to with it.
        // If anyone else has a clue as to why, please do tell. - MutantWafflez
        crystalFrame.Height -= 1;
        smartCursorGlowFrame.Height -= 1;

        // Calculate positional variables for actually drawing the crystal
        Vector2 origin = crystalFrame.Size() / 2f;
        Vector2 tileOrigin = new Vector2(tileData.CoordinateFullWidth / 2f, tileData.CoordinateFullHeight / 2f);
        Vector2 crystalPosition = point.ToWorldCoordinates(tileOrigin.X - 2f, tileOrigin.Y) + crystalOffset;

        // Calculate additional drawing positions with a sine wave movement
        float sinusoidalOffset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (Math.PI * 2) / 5);
        Vector2 drawingPosition = crystalPosition + offscreenVector + new Vector2(0f, sinusoidalOffset * 4f);

        // Do dust drawing
        if (!Main.gamePaused && Main.instance.IsActive && (!Lighting.UpdateEveryFrame || Main.rand.NextBool(4)) && Main.rand.NextBool(dustChanceDenominator)) {
            Rectangle dustBox = Utils.CenteredRectangle(crystalPosition, crystalFrame.Size());
            int numForDust = Dust.NewDust(dustBox.TopLeft(), dustBox.Width, dustBox.Height, DustID.TintableDustLighted, 0f, 0f, 254, dustColor, 0.5f);
            Dust obj = Main.dust[numForDust];
            obj.velocity *= 0.1f;
            Main.dust[numForDust].velocity.Y -= 0.2f;
        }

        // Get color value and draw the the crystal
        Color color = Lighting.GetColor(point.X, point.Y);
        color = Color.Lerp(color, Color.White, 0.8f);
        spriteBatch.Draw(crystalTexture.Value, drawingPosition - Main.screenPosition, crystalFrame, color * 0.7f, 0f, origin, 1f, SpriteEffects.None, 0f);

        // DrawSelf the shadow effect for the crystal
        float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * ((float)Math.PI * 2f) / 1f) * 0.2f + 0.8f;
        Color shadowColor = pylonShadowColor * scale;
        for (float shadowPos = 0f; shadowPos < 1f; shadowPos += 1f / 6f) {
            spriteBatch.Draw(crystalTexture.Value, drawingPosition - Main.screenPosition + ((float)Math.PI * 2f * shadowPos).ToRotationVector2() * (6f + sinusoidalOffset * 2f), crystalFrame, shadowColor, 0f, origin, 1f, SpriteEffects.None, 0f);
        }

        // Interpret smart cursor outline color & draw it
        int selectionLevel = 0;
        if (Main.InSmartCursorHighlightArea(point.X, point.Y, out bool actuallySelected)) {
            selectionLevel = 1;
            if (actuallySelected) {
                selectionLevel = 2;
            }
        }

        if (selectionLevel == 0) {
            return;
        }

        int averageBrightness = (color.R + color.G + color.B) / 3;

        if (averageBrightness <= 10) {
            return;
        }

        Color selectionGlowColor = Colors.GetSelectionGlowColor(selectionLevel == 2, averageBrightness);
        spriteBatch.Draw(crystalHighlightTexture.Value, drawingPosition - Main.screenPosition, smartCursorGlowFrame, selectionGlowColor, 0f, origin, 1f, SpriteEffects.None, 0f);
    }

    public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale) {
        // Just like in SpecialDraw, we want things to be handled the EXACT same way vanilla would handle it, which ModPylon also has built in methods for:
        bool mouseOver = DefaultDrawMapIcon(ref context, _mapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), drawColor, deselectedScale, selectedScale);
        DefaultMapClickHandle(mouseOver, pylonInfo, ModContent.GetInstance<BackwoodsPylon>().DisplayName.Key, ref mouseOverText);
    }
}
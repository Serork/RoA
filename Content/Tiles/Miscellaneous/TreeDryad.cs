using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.NPCs;
using RoA.Common.Sets;
using RoA.Common.Tiles;
using RoA.Content.Emotes;
using RoA.Content.WorldGenerations;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class TreeDryad : ModTile, IRequestAssets, TileHooks.IPreDraw, TileHooks.IPostDraw {
    private static byte RAYCOUNT => 8;

    private static bool _hammerEmoteShown;
    private static short _frameX;

    public enum TreeDryadRequstedTextureType : byte {
        Ray,
        Glow
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture => [((byte)TreeDryadRequstedTextureType.Ray, ResourceManager.VisualEffectTextures + "Ray"),
                                                             ((byte)TreeDryadRequstedTextureType.Glow, TileLoader.GetTile(ModContent.TileType<TreeDryad>()).Texture + "_Glow")];

    void TileHooks.IPreDraw.PreDrawExtra(SpriteBatch spriteBatch, Point16 tilePosition) {
        if (AbleToBeDestroyed) {
            DrawRays(spriteBatch,tilePosition, 0.1f);
        }
    }

    void TileHooks.IPostDraw.PostDrawExtra(SpriteBatch spriteBatch, Point16 tilePosition) {
        uint seedForPseudoRandomness = (uint)(tilePosition.GetHashCode() + tilePosition.GetHashCode());
        float lightStrengthFactor = Helper.Wave(0.75f, 1f, 2f, MathUtils.PseudoRandRange(ref seedForPseudoRandomness, 0f, MathHelper.Pi));
        float lightStrengthMainFactor = MathUtils.PseudoRandRange(ref seedForPseudoRandomness, 0.25f, 0.6f);
        if (AbleToBeDestroyed) {
            Vector2 tileWorldPosition = tilePosition.ToWorldCoordinates();
            int i = tilePosition.X, j = tilePosition.Y;
            Point16 topLeft = TileHelper.GetTileTopLeft<TreeDryad>(i, j);
            bool isOrigin = topLeft == tilePosition;
            if (isOrigin) {
                _frameX = Main.tile[topLeft.X, topLeft.Y].TileFrameX;
                Vector2 emotePosition = TileHelper.GetTileTopLeft<TreeDryad>(i, j).ToWorldCoordinates() + new Vector2(8f, -16f);
                bool turnedLeft = _frameX <= 18;
                if (!turnedLeft) {
                    emotePosition.X += 16f;
                }
                if (Main.LocalPlayer.Distance(tileWorldPosition) < 200f/*Helper.OnScreenWorld(emotePosition)*/) {
                    if (!_hammerEmoteShown) {
                        _hammerEmoteShown = true;

                        int emoteType = ModContent.EmoteBubbleType<HammerEmote>();
                        EmoteBubble.NewBubble(emoteType, new WorldUIAnchor(emotePosition), 180);
                    }
                }
                else {
                    _hammerEmoteShown = false;
                }
                Lighting.AddLight(tileWorldPosition, Color.Lerp(Color.Green, Color.White, lightStrengthMainFactor * lightStrengthFactor).ToVector3() * 0.45f * lightStrengthFactor);
            }

            DrawRays(spriteBatch, tilePosition, 0.075f);

            if (isOrigin) {
                if (AssetInitializer.TryGetRequestedTextureAsset<TreeDryad>((byte)TreeDryadRequstedTextureType.Glow, out Asset<Texture2D> glowTextureAsset)) {
                    if (TileDrawing.IsVisible(Main.tile[tilePosition.X, tilePosition.Y])) {
                        Texture2D glowTexture = glowTextureAsset!.Value;
                        const byte GLOWFRAMECOUNT = 8;
                        SpriteFrame glowFrame = new(1, GLOWFRAMECOUNT, 0, (byte)(Utils.Remap(lightStrengthFactor, 0.75f, 1f, 1f, 0f) * GLOWFRAMECOUNT));
                        Rectangle glowClip = glowFrame.GetSourceRectangle(glowTexture);
                        Vector2 glowPosition = topLeft.ToWorldCoordinates() + new Vector2(9f, 1f);
                        Vector2 glowOrigin = glowClip.Size() / 2f;
                        Color glowColor = Color.White * 0.4f;
                        bool turnedLeft = _frameX <= 18;
                        if (turnedLeft) {
                            glowPosition.X -= 18f;
                        }
                        SpriteEffects glowFlip = !turnedLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        spriteBatch.Draw(glowTexture, glowPosition, DrawInfo.Default with {
                            Clip = glowClip,
                            Origin = glowOrigin,
                            Color = glowColor,
                            ImageFlip = glowFlip
                        });
                    }
                }
            }
        }
    }

    private static void DrawRays(SpriteBatch spriteBatch, Point16 tilePosition, float colorOpacity) {
        int i = tilePosition.X, j = tilePosition.Y;
        if (!TileDrawing.IsVisible(Main.tile[tilePosition.X, tilePosition.Y])) {
            return;
        }
        Point16 topLeft = TileHelper.GetTileTopLeft<TreeDryad>(i, j);
        if (topLeft == tilePosition && AssetInitializer.TryGetRequestedTextureAsset<TreeDryad>((byte)TreeDryadRequstedTextureType.Ray, out Asset<Texture2D> rayTextureAsset)) {
            Texture2D rayTexture = rayTextureAsset!.Value;
            Vector2 rayPosition = TileHelper.GetTileTopLeft<TreeDryad>(i, j).ToWorldCoordinates() + new Vector2(8f, 4f);
            bool turnedLeft = Main.tile[topLeft.X, topLeft.Y].TileFrameX <= 18;
            if (turnedLeft) {
                rayPosition.X -= 16f;
            }
            Rectangle rayClip = rayTexture.Bounds;
            Vector2 rayOrigin = Utils.Top(rayClip);
            for (int k = 0; k < RAYCOUNT; k++) {
                uint seedForPseudoRandomness = (uint)((tilePosition.GetHashCode() + tilePosition.GetHashCode()) * (k + 1));
                float rayRotation = (float)k / RAYCOUNT * MathHelper.TwoPi + MathUtils.PseudoRandRange(ref seedForPseudoRandomness, -MathHelper.TwoPi, MathHelper.TwoPi);
                float maxRayExtraScale = 0.3f;
                float opacityFactor = Helper.Wave(0.5f, 1f, 2f, MathUtils.PseudoRandRange(ref seedForPseudoRandomness, 0f, MathHelper.Pi));
                Vector2 rayScale = new(0.5f + MathUtils.PseudoRandRange(ref seedForPseudoRandomness, -maxRayExtraScale, maxRayExtraScale), 0.8f + MathUtils.PseudoRandRange(ref seedForPseudoRandomness, -maxRayExtraScale * 2f, maxRayExtraScale * 2f));
                rayScale.Y *= opacityFactor;
                Color rayColor = Color.Lerp(Color.Green, Color.White, MathUtils.PseudoRandRange(ref seedForPseudoRandomness, 1f)) * colorOpacity;
                rayColor *= opacityFactor;
                spriteBatch.Draw(rayTexture, rayPosition, DrawInfo.Default with {
                    Scale = rayScale,
                    Clip = rayClip,
                    Origin = rayOrigin,
                    Color = rayColor,
                    Rotation = rayRotation
                });
            }
        }
    }

    public static bool AbleToBeDestroyed => NPC.downedBoss1 || NPC.downedBoss2 || NPC.downedBoss3;

    public override void Load() {
        On_Main.UpdateTime_SpawnTownNPCs += On_Main_UpdateTime_SpawnTownNPCs;
    }

    private void On_Main_UpdateTime_SpawnTownNPCs(On_Main.orig_UpdateTime_SpawnTownNPCs orig) {
        orig();
        if (DryadEntrance._dryadStructureGenerated && !DryadAwakeHandler.DryadAwake && !DryadAwakeHandler.DryadAwake2) {
            Main.townNPCCanSpawn[NPCID.Dryad] = false;
        }
    }

    public override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileLighted[Type] = true;
        Main.tileHammer[Type] = true;

        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.addTile(Type);

        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;
        TileSets.ShouldKillTileBelow[Type] = false;
        TileSets.PreventsSlopesBelow[Type] = true;
        CanBeSlopedTileSystem.Included[Type] = true;

        var mapText = CreateMapEntryName();
        AddMapEntry(new Color(191, 143, 111), mapText);
        AddMapEntry(new Color(168, 153, 136), mapText);
        AddMapEntry(new Color(184, 118, 124), mapText);

        DustType = DustID.WoodFurniture;
    }

    public override ushort GetMapOption(int i, int j) {
        if (DryadEntrance.HasSpiritModAndSavannahSeed) {
            return 1;
        }
        if (Main.notTheBeesWorld) {
            return 2;
        }

        return 0;
    }

    public override bool CreateDust(int i, int j, ref int type) {
        if (DryadEntrance.HasSpiritModAndSavannahSeed) {
            if (Main.rand.NextBool(4) && AbleToBeDestroyed) {
                type = DustID.JunglePlants;
            }
            else {
                type = DustID.t_PearlWood;
            }
        }
        else if (Main.notTheBeesWorld) {
            if (Main.rand.NextBool(4) && AbleToBeDestroyed) {
                type = DustID.JungleGrass;
            }
            else {
                type = DustID.RichMahogany;
            }
        }
        else if (Main.rand.NextBool(4) && AbleToBeDestroyed) {
            type = DustID.Grass;
        }

        return base.CreateDust(i, j, ref type);
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return false;
        }

        Tile tile = Main.tile[i, j];
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) {
            zero = Vector2.Zero;
        }
        bool flag = tile.TileFrameY == 0;
        int frameY = !flag ? tile.TileFrameY + 4 : 0;
        if (DryadEntrance.HasSpiritModAndSavannahSeed) {
            frameY += 58;
        }
        if (Main.notTheBeesWorld) {
            frameY += 116;
        }
        int height = flag ? 22 : 16;
        int frameX = tile.TileFrameX;
        if (AbleToBeDestroyed) {
            frameX += 36;
        }
        Texture2D texture = Main.instance.TilesRenderer.GetTileDrawTexture(tile, i, j);
        texture ??= TextureAssets.Tile[Type].Value;

        Main.spriteBatch.Draw(texture,
                              new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y - (flag ? 4 : 0)) + zero,
                              new Rectangle(frameX, frameY, 16, height),
                              Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        TileHelper.AddPostSolidTileDrawPoint(this, i, j);

        return false;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) {
        Vector2 position = new Point(i, j).ToWorldCoordinates();
        {
            int dustType = DustID.WoodFurniture;
            if (DryadEntrance.HasSpiritModAndSavannahSeed) {
                dustType = DustID.t_PearlWood;
            }
            else if (Main.notTheBeesWorld) {
                dustType = DustID.RichMahogany;
            }
            for (int k = 0; k < 20; k++) {
                Dust.NewDust(position, 36, 54, dustType, 2.5f * Main.rand.NextFloatDirection(), 2.5f * Main.rand.NextFloatDirection());
            }
            if (!Main.dedServ) {
                Vector2 offset = new(-12f, 4f);
                int variant = 1;
                if (DryadEntrance.HasSpiritModAndSavannahSeed) {
                    variant = 2;
                }
                else if (Main.notTheBeesWorld) {
                    variant = 3;
                }
                Gore.NewGore(NPC.GetSource_TownSpawn(), position + offset, Vector2.Zero, $"TreeDryadGore1_{variant}".GetGoreType());
                Gore.NewGore(NPC.GetSource_TownSpawn(), position + offset + new Vector2(8, 0), Vector2.Zero, $"TreeDryadGore2_{variant}".GetGoreType());
                Gore.NewGore(NPC.GetSource_TownSpawn(), position + offset + new Vector2(16, 0), Vector2.Zero, $"TreeDryadGore3_{variant}".GetGoreType());
            }
        }
        int whoAmI = NPC.NewNPC(NPC.GetSource_TownSpawn(), (int)position.X + 10, (int)position.Y + 40, NPCID.Dryad);
        Main.npc[whoAmI].ai[0] = -20f;
        Main.npc[whoAmI].ai[1] = 150f;
        Main.npc[whoAmI].ai[2] = _frameX >= 72 ? -1 : 1;
        Main.npc[whoAmI].ai[2] = -Main.npc[whoAmI].ai[2];
        Main.npc[whoAmI].direction = Main.npc[whoAmI].spriteDirection = (int)Main.npc[whoAmI].ai[2];
        Main.npc[whoAmI].homeless = true;
        Main.npc[whoAmI].homeTileX = Main.npc[whoAmI].homeTileY = -1;
        Main.npc[whoAmI].netUpdate = true;

        DryadAwakeHandler.DryadAwake = DryadAwakeHandler.DryadAwake2 = true;

        if (Main.netMode == NetmodeID.Server) {
            NetMessage.SendData(MessageID.WorldData);
        }
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;

    public override bool CanExplode(int i, int j) => false;

    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => AbleToBeDestroyed;
}
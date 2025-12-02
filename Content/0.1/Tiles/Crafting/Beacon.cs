using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Common.Tiles;
using RoA.Content.Tiles.Ambient;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

sealed class Beacon : ModTile, TileHooks.IPostDraw, IPostSetupContent {
    public static bool DoesBeaconHaveThoriumGem(int i, int j) => WorldGenHelper.GetTileSafely(i, j).TileFrameX == 18;

    private static int _variantToShow;

    public static short[] Gems = [ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Amber];

    void IPostSetupContent.PostSetupContent() {
        if (RoA.TryGetThoriumMod(out Mod thoriumMod)) {
            Gems = [ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Amber, (short)thoriumMod.Find<ModItem>("Aquamarine").Type, (short)thoriumMod.Find<ModItem>("Opal").Type];
        }
    }

    public override void Load() {
        On_Player.UpdateTeleportVisuals += On_Player_UpdateTeleportVisuals;
    }

    private void On_Player_UpdateTeleportVisuals(On_Player.orig_UpdateTeleportVisuals orig, Player self) {
        orig(self);

        if (self.teleportStyle >= 12 && self.teleportStyle <= 20) {
            Rectangle hitbox = self.Hitbox;
            hitbox.Inflate(5, 5);
            if ((float)Main.rand.Next(100) <= 75f * self.teleportTime) {
                SpawnInWorldDust(self.teleportStyle - 12, hitbox);
            }
        }
    }

    private static void SpawnInWorldDust(int tileStyle, Rectangle dustBox) {
        Color color = Color.White;
        switch (tileStyle) {
            case 0:
                color = new Color(238, 51, 53);
                break;
            case 1:
                color = new Color(13, 107, 216);
                break;
            case 2:
                color = new Color(33, 184, 115);
                break;
            case 3:
                color = new Color(255, 221, 62);
                break;
            case 4:
                color = new Color(165, 0, 236);
                break;
            case 5:
                color = new Color(223, 230, 238);
                break;
            case 6:
                color = new Color(207, 101, 0);
                break;
        }

        bool flag = tileStyle == 7;
        if ((flag || tileStyle == 8) && RoA.TryGetThoriumMod(out Mod thoriumMod)) {
            if (flag) {
                color = new Color(109, 255, 216);
            }
            else {
                color = new Color(255, 146, 163);
            }
        }

        int dust = Dust.NewDust(dustBox.TopLeft(), dustBox.Width, dustBox.Height, 267, Scale: Main.rand.NextFloat(1.5f) * 0.85f, newColor: color, Alpha: 0);
        Main.dust[dust].noGravity = true;
        Main.dust[dust].color = color;
        Main.dust[dust].velocity *= 0.1f;
        Main.dust[dust].velocity.Y -= 0.25f;
        Main.dust[dust].scale = 0.8f + Main.rand.NextFloat() * 0.6f;
        Main.dust[dust].fadeIn = 0.5f;
    }

    public override void SetStaticDefaults() {
        Main.tileTable[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileLighted[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileID.Sets.HasOutlines[Type] = true;

        AddMapEntry(new Color(238, 51, 53), Lang.GetItemName(ItemID.Ruby));
        AddMapEntry(new Color(13, 107, 216), Lang.GetItemName(ItemID.Sapphire));
        AddMapEntry(new Color(33, 184, 115), Lang.GetItemName(ItemID.Emerald));
        AddMapEntry(new Color(255, 221, 62), Lang.GetItemName(ItemID.Topaz));
        AddMapEntry(new Color(165, 0, 236), Lang.GetItemName(ItemID.Amethyst));
        AddMapEntry(new Color(223, 230, 238), Lang.GetItemName(ItemID.Diamond));
        AddMapEntry(new Color(207, 101, 0), Lang.GetItemName(ItemID.Amber));
        AddMapEntry(new Color(85, 84, 105), Language.GetOrRegister("Mods.RoA.Map.Beacon"));

        if (RoA.TryGetThoriumMod(out Mod thoriumMod)) {
            AddMapEntry(new Color(26, 236, 214), thoriumMod.Find<ModItem>("Aquamarine").DisplayName);
            AddMapEntry(new Color(239, 101, 154), thoriumMod.Find<ModItem>("Opal").DisplayName);
        }

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, -2);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<BeaconTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);
    }

    public override void PlaceInWorld(int i, int j, Item item) {
        ModContent.GetInstance<BeaconTE>().Place(i, j);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new PlaceBeaconTEPacket(i, j));
        }
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
        if (Main.netMode != NetmodeID.Server) {
            if (!fail) {
                ModContent.GetInstance<BeaconTE>().Kill(i, j);
                if (Main.netMode != NetmodeID.SinglePlayer) {
                    MultiplayerSystem.SendPacket(new RemoveBeaconTileEntityOnServerPacket(i, j));
                }
            }
        }
    }
    
    void TileHooks.IPostDraw.PostDrawExtra(SpriteBatch spriteBatch, Point16 pos) {
        int i = pos.X;
        int j = pos.Y;
        if (!(WorldGenHelper.GetTileSafely(i, j + 1).TileType == ModContent.TileType<Beacon>() &&
              WorldGenHelper.GetTileSafely(i, j - 1).TileType == ModContent.TileType<Beacon>())) {
            return;
        }
        BeaconTE beaconTE = GetTE(i, j);
        if (HasGemInIt(i, j) && beaconTE != null) {
            if (!Main.dedServ) {
                for (int k = 0; k < 10; k++) {
                    Lighting.AddLight(new Vector2(i, j - k).ToWorldCoordinates(), GetEffectsColor(i, j).ToVector3() * 0.85f);
                }
            }

            Vector2 zero = Vector2.Zero;
            Vector2 position = new Point(i, j).ToWorldCoordinates();
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.VisualEffectTextures + "Beacon_Light");
            Vector2 drawPos = position - Main.screenPosition;
            drawPos.X += 1f;
            drawPos.Y -= 4f;
            Color color = GetEffectsColor(i, j).MultiplyRGB(new Color(250, 250, 250));
            Vector2 scale = beaconTE.IsUsed ? beaconTE.Scale : new(0.2f, 1f);
            if (TileDrawing.IsVisible(Main.tile[i, j])) {
                for (float k = -MathHelper.Pi; k <= MathHelper.Pi; k += MathHelper.PiOver2) {
                    spriteBatch.Draw(texture, drawPos + zero + beaconTE.OffsetPosition,
                        null, color.MultiplyAlpha(Helper.Wave(0.3f, 0.9f, 4f, k)) * 0.3f, 0f, new(texture.Width / 2f, texture.Height), scale * Helper.Wave(0.85f, 1.15f, speed: 2f), SpriteEffects.None, 0f);
                }
            }

            Vector2 spinningpoint4 = Vector2.UnitX * 14f;
            Vector2 velocity = -Vector2.UnitY;
            float rotation = velocity.ToRotation() + MathHelper.PiOver2;
            spinningpoint4 = spinningpoint4.RotatedBy(rotation - (float)Math.PI / 2f);
            Vector2 vector13 = position - Vector2.UnitY * 10f + spinningpoint4;
            if (beaconTE.IsUsed) {
                for (int l = 0; l < 2; l++) {
                    if (Main.rand.NextBool()) {
                        int num26 = 267;
                        float num27 = 0.35f;
                        if (l % 2 == 1) {
                            num27 = 0.45f;
                        }
                        num27 *= 1.5f;
                        num27 *= 1.25f * Main.rand.NextFloat(0.75f, 1f);
                        num27 *= 1.25f * Main.rand.NextFloat(0.75f, 1f);
                        num27 *= 1.01f;

                        float num28 = Main.rand.NextFloatDirection();
                        Vector2 vector14 = vector13 + (rotation + num28 * ((float)Math.PI / 4f) * 0.8f - (float)Math.PI / 2f).ToRotationVector2() * 6f;
                        int num29 = 18;
                        int num30 = Dust.NewDust(vector14 - Vector2.One * (num29 / 2) - new Vector2(5f, -4f) -
                            Vector2.UnitY * 20f * Main.rand.NextFloat() + Vector2.UnitY * 5f, 26, num29, num26, velocity.X / 2f, velocity.Y / 2f);
                        Main.dust[num30].velocity = (vector14 - vector13).SafeNormalize(Vector2.Zero) * MathHelper.Lerp(1.5f, 9f, Utils.GetLerpValue(1f, 0f, Math.Abs(num28), clamped: true)) * 0.5f;
                        Main.dust[num30].noGravity = true;
                        Main.dust[num30].velocity.Y *= 1f * Main.rand.NextFloat(1f, 2.5f);
                        Main.dust[num30].scale = num27;
                        Main.dust[num30].fadeIn = 0.5f;
                        Main.dust[num30].color = color;
                    }
                }
            }
            else {
                for (int l = 0; l < 2; l++) {
                    if (Main.rand.NextBool(8)) {
                        int num26 = 267;
                        float num27 = 0.35f;
                        if (l % 2 == 1) {
                            num27 = 0.45f;
                        }
                        num27 *= 1.5f;
                        num27 *= 1.25f * Main.rand.NextFloat(0.75f, 1f);
                        num27 *= 1.25f * Main.rand.NextFloat(0.75f, 1f);
                        num27 *= 1.01f;

                        float num28 = Main.rand.NextFloatDirection();
                        Vector2 vector14 = vector13 + Vector2.UnitX * 2f + (rotation + num28 * ((float)Math.PI / 4f) * 0.8f - (float)Math.PI / 2f).ToRotationVector2() * 6f;
                        int num29 = 10;
                        int width = 15;
                        float value = Main.rand.NextFloat();
                        if (value > 0.5f) {
                            num29 = 8;
                            width = 13;
                        }
                        int num30 = Dust.NewDust(vector14 - Vector2.One * (num29 / 2) - new Vector2(5f, -4f) -
                            Vector2.UnitY * 5f * value + Vector2.UnitY * 2.5f, width, num29, num26, velocity.X / 2f, velocity.Y / 2f);
                        Main.dust[num30].velocity = (vector14 - vector13).SafeNormalize(Vector2.Zero) * MathHelper.Lerp(1.5f, 9f, Utils.GetLerpValue(1f, 0f, Math.Abs(num28), clamped: true)) * 0.15f;
                        Main.dust[num30].velocity.Y *= 5f * Main.rand.NextFloat();
                        Main.dust[num30].velocity *= 0.75f;
                        Main.dust[num30].noGravity = true;
                        Main.dust[num30].scale = num27;
                        Main.dust[num30].fadeIn = 0.5f;
                        Main.dust[num30].color = color;
                    }
                }
            }
        }
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
        if (!TileDrawing.IsVisible(Main.tile[i, j])) {
            return;
        }

        TileHelper.AddPostSolidTileDrawPoint(this, i, j);
    }

    public static void TeleportPlayerTo(int i, int j, Player player) {
        if (WorldGenHelper.GetTileSafely(i, j + 1).TileType != ModContent.TileType<Beacon>()) {
            return;
        }
        Color color = new(255, 240, 20);
        if (!player.HasItemInInventoryOrOpenVoidBag(GetLargeGemItemID(i, j))) {
            string key = "Mods.RoA.NoNeededLargeGem";
            if (Main.netMode == NetmodeID.SinglePlayer) {
                Main.NewText(Language.GetTextValue(key), color);
            }
            else {
                ChatHelper.SendChatMessageToClient(NetworkText.FromKey(key), color, player.whoAmI);
            }
            return;
        }

        HandleTeleport(player, i, j);

        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new BeaconTeleportPacket(player, i, j));
        }

        BeaconTE beaconTE = GetTE(i, j);
        beaconTE?.UseAnimation();

        if (Main.netMode == NetmodeID.Server) {
            RemoteClient.CheckSection(player.whoAmI, player.position);
        }
    }

    public static void HandleTeleport(Player player, int i, int j) {
        Vector2 newPos = new Point(i - 1, j - 1).ToWorldCoordinates() + new Vector2(4f, 0f) - new Vector2(0f, player.HeightOffsetBoost);
        int num2 = TileLoader.GetTile(WorldGenHelper.GetTileSafely(i, j).TileType).GetMapOption(i, j) + 12;
        if (num2 == 13) {
            num2 = 11;
        }
        if (DoesBeaconHaveThoriumGem(i, j)) {
            num2 -= 1;
        }
        void dusts(Rectangle effectRect, int num4) {
            for (int k = 0; k < 50; k++) {
                Color color = new Color(136, 219, 227);
                int num5 = Main.rand.Next(4);
                switch (num5) {
                    case 0:
                    case 1:
                        color = GetEffectsColor(i, j);
                        break;
                    case 3:
                        color = Color.White;
                        break;
                }

                Dust obj = Dust.NewDustPerfect(Main.rand.NextVector2FromRectangle(effectRect), 267);
                obj.noGravity = true;
                obj.color = color;
                obj.velocity *= 2f;
                obj.scale = 0.8f + Main.rand.NextFloat() * 0.6f;
                obj.fadeIn = 0.5f;
            }
        }
        int style = WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54;
        if (DoesBeaconHaveThoriumGem(i, j)) {
            style += 8;
        }
        dusts(player.getRect(), style);
        player.Teleport(newPos, num2);
        player.velocity = Vector2.Zero;

        SoundEngine.PlaySound(SoundID.Item6 with { Volume = 0.7f }, player.Center);
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "StrongSignal") { Volume = 0.7f, PitchVariance = 0.1f }, player.Center);
        SoundEngine.PlaySound(SoundID.Item8, player.Center);

        dusts(player.getRect(), style);
    }

    public static short GetLargeGemItemID(int i, int j) {
        int value = WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54;
        if (DoesBeaconHaveThoriumGem(i, j)) {
            value += 8;
        }
        switch (value) {
            case 1:
                return ItemID.LargeAmethyst;
            case 2:
                return ItemID.LargeTopaz;
            case 3:
                return ItemID.LargeSapphire;
            case 4:
                return ItemID.LargeEmerald;
            case 5:
                return ItemID.LargeRuby;
            case 6:
                return ItemID.LargeDiamond;
            case 7:
                return ItemID.LargeAmber;
        }

        bool flag = value == 8;
        if ((flag || value == 9) && RoA.TryGetThoriumMod(out Mod thoriumMod)) {
            if (flag) {
                return (short)thoriumMod.Find<ModItem>("LargeAquamarine").Type;
            }
            else {
                return (short)thoriumMod.Find<ModItem>("LargeOpal").Type;
            }
        }

        return -1;
    }

    public static short GetLargeGemDustID(int i, int j) {
        int value = WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54;
        if (DoesBeaconHaveThoriumGem(i, j)) {
            value += 8;
        }
        switch (value) {
            case 1:
                return DustID.GemAmethyst;
            case 2:
                return DustID.GemTopaz;
            case 3:
                return DustID.GemSapphire;
            case 4:
                return DustID.GemEmerald;
            case 5:
                return DustID.GemRuby;
            case 6:
                return DustID.GemDiamond;
            case 7:
                return DustID.GemAmber;
        }

        bool flag = value == 8;
        if ((flag || value == 9) && RoA.TryGetThoriumMod(out Mod thoriumMod)) {
            if (flag) {
                return 92;
            }
            else {
                return 86;
            }
        }

        return -1;
    }

    public static Color GetEffectsColor(int i, int j) {
        ModTile tile = TileLoader.GetTile(WorldGenHelper.GetTileSafely(i, j).TileType);
        ushort option = tile.GetMapOption(i, j);
        bool flag = option == 8;
        if ((flag || option == 9) && RoA.TryGetThoriumMod(out Mod thoriumMod)) {
            if (flag) {
                return new Color(26, 236, 214);
            }
            else {
                return new Color(239, 101, 154);
            }
        }
        return option switch {
            0 => new Color(238, 51, 53),
            1 => new Color(13, 107, 216),
            2 => new Color(33, 184, 115),
            3 => new Color(255, 221, 62),
            4 => new Color(165, 0, 236),
            5 => new Color(223, 230, 238),
            6 => new Color(207, 101, 0),
            _ => new Color(238, 51, 53),
        };
    }

    public static LocalizedText GetMapText(int i, int j) {
        ModTile tile = TileLoader.GetTile(WorldGenHelper.GetTileSafely(i, j).TileType);
        ushort option = tile.GetMapOption(i, j);
        bool flag = option == 8;
        if ((flag || option == 9) && RoA.TryGetThoriumMod(out Mod thoriumMod)) {
            if (flag) {
                return thoriumMod.Find<ModItem>("Aquamarine").DisplayName;
            }
            else {
                return thoriumMod.Find<ModItem>("Opal").DisplayName;
            }
        }
        switch (option) {
            case 0:
                return Lang.GetItemName(ItemID.Ruby);
            case 1:
                return Lang.GetItemName(ItemID.Sapphire);
            case 2:
                return Lang.GetItemName(ItemID.Emerald);
            case 3:
                return Lang.GetItemName(ItemID.Topaz);
            case 4:
                return Lang.GetItemName(ItemID.Amethyst);
            case 5:
                return Lang.GetItemName(ItemID.Diamond);
            case 6:
                return Lang.GetItemName(ItemID.Amber);
            default:
                return Lang.GetItemName(ItemID.Ruby);
        }
    }

    public override ushort GetMapOption(int i, int j) {
        if (WorldGenHelper.GetTileSafely(i, j + 1).TileType != Type) {
            return 7;
        }

        int value = WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54;
        if (DoesBeaconHaveThoriumGem(i, j)) {
            value += 8;
        }
        if ((value == 8 || value == 9) && RoA.TryGetThoriumMod(out Mod thoriumMod)) {
            return (ushort)value;
        }
        switch (value) {
            case 0:
                return 7;
            case 1:
                return 4;
            case 2:
                return 3;
            case 3:
                return 1;
            case 4:
                return 2;
            case 5:
                return 0;
            case 6:
                return 5;
            case 7:
                return 6;
            default:
                break;
        }

        return base.GetMapOption(i, j);
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY) {
        height = 2;
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

    private static BeaconTE GetTE(int i, int j) {
        BeaconTE foundTE = null;
        int j2 = 0;
        TileObjectData tileObjectData = TileObjectData.GetTileData(WorldGenHelper.GetTileSafely(i, j));
        if (tileObjectData == null) {
            return null;
        }
        while (j2 < tileObjectData.CoordinateHeights.Length) {
            BeaconTE desiredTE = TileHelper.GetTE<BeaconTE>(i, j + j2);
            if (desiredTE is null) {
                j2++;
                continue;
            }

            foundTE = desiredTE;
            break;
        }

        return foundTE;
    }

    public static bool IsTileValidToBeHovered(int i, int j) {
        if (WorldGenHelper.GetTileSafely(i, j + 1).TileType != ModContent.TileType<Beacon>()) {
            return false;
        }

        return true;
    }

    public static void UpdateVariants() {
        double perTime = 50.0;
        if (Main.timeForVisualEffects % perTime == 0.0) {
            _variantToShow++;
            if (_variantToShow >= Gems.Length) {
                _variantToShow = 0;
            }
        }
    }

    public static int GetGemDropID(int i, int j) => Gems[(DoesBeaconHaveThoriumGem(i, j) ? 8 : 0) + WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54 - 1];

    public static int GetGemItemID(int i, int j, bool forVisuals = false) {
        bool flag2 = HasGemInIt(i, j);
        if (IsTileValidToBeHovered(i, j)) {
            if (flag2) {
                return GetGemDropID(i, j);
            }

            Item item = Main.LocalPlayer.GetSelectedItem();
            if (forVisuals && Gems.Contains((short)item.type)) {
                return item.type;
            }
            else {
                UpdateVariants();
                return Gems[_variantToShow];
            }
        }
        else {
            return ModContent.ItemType<Items.Placeable.Crafting.Beacon>();
        }
    }

    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        player.cursorItemIconID = GetGemItemID(i, j, true);
        if (player.cursorItemIconID != -1) {
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
        }
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) {
        if (HasGemInIt(i, j) && !WorldGenHelper.GetTileSafely(i, j - 1).ActiveTile(Type)) {
            yield return new Item(GetGemDropID(i, j));
        }

        yield return new Item(ModContent.ItemType<Items.Placeable.Crafting.Beacon>());
    }

    public static bool HasGemInIt(int i, int j) => DoesBeaconHaveThoriumGem(i, j) || WorldGenHelper.GetTileSafely(i, j).TileFrameY >= 54;

    public static void ActionWithGem(int i, int j, bool remove = false, bool dropItem = true, bool makeDusts = false) {
        Player player = Main.LocalPlayer;
        Item item = player.GetSelectedItem();
        int Type = WorldGenHelper.GetTileSafely(i, j).TileType;
        int num3 = 0;
        int variant = 0;
        bool flag2 = HasGemInIt(i, j);
        bool flag = false;
        if (!remove) {
            for (int k = 0; k < Gems.Length; k++) {
                if (Gems[k] == (short)item.type) {
                    variant = k + 1;
                }
            }
        }
        int gemType = remove || flag2 ? GetGemDropID(i, j) : Gems[Math.Max(0, variant - 1)];
        for (int l = j - 2; l < j + 3; l++) {
            Tile tile2 = WorldGenHelper.GetTileSafely(i, l);
            if (tile2.ActiveTile(Type)) {
                short getTileFrameY(int usedVariant) {
                    if (usedVariant >= 8) {
                        usedVariant -= 8;
                    }
                    short result = (short)(num3 * 18 + 54 * usedVariant);
                    return result;
                }
                void setFrame(int usedVariant) {
                    tile2.TileFrameX = (short)(usedVariant >= 8 ? 18 : 0);
                    tile2.TileFrameY = getTileFrameY(usedVariant);
                    WorldGen.SquareTileFrame(i, l);
                    NetMessage.SendTileSquare(-1, i, l, 1, 1);
                    num3++;
                }
                bool flag3 = !WorldGenHelper.GetTileSafely(i, l - 1).ActiveTile(Type);
                if (flag3) {
                    if (dropItem) {
                        if (flag2) {
                            int itemWhoAmI = Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 32, 32,
                                gemType);
                            if (Main.netMode == NetmodeID.MultiplayerClient && itemWhoAmI >= 0) {
                                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemWhoAmI, 1f, 0f, 0f, 0, 0, 0);
                            }
                        }
                        else {
                            if (--item.stack <= 0) {
                                item.TurnToAir();
                            }
                        }
                    }
                    if (makeDusts) {
                        if (Main.netMode == NetmodeID.SinglePlayer) {
                            RemoveGemEffects(i, j);
                        }
                        else {
                            MultiplayerSystem.SendPacket(new BeaconRemoveGemEffectsPacket(i, j));
                        }
                    }
                    else {
                        //SoundEngine.PlaySound(SoundID.MenuTick, new Point(i, l).ToWorldCoordinates());
                        SoundEngine.PlaySound(SoundID.Unlock, new Point(i, l).ToWorldCoordinates());
                        if (Main.netMode == NetmodeID.MultiplayerClient) {
                            MultiplayerSystem.SendPacket(new PlayOtherItemSoundPacket(player, 8, new Point(i, l).ToWorldCoordinates()));
                        }
                    }
                }
                bool hasGem = (variant < 8 ? tile2.TileFrameX == 0 : tile2.TileFrameX == 18) && tile2.TileFrameY >= getTileFrameY(variant) && tile2.TileFrameY < getTileFrameY(variant + 1);
                if (flag || hasGem) {
                    flag = true;
                    setFrame(0);
                }
                if (!flag) {
                    setFrame(variant);
                }
            }
        }
    }

    public static void RemoveGemEffects(int i, int j) {
        Vector2 position = new Point(i, j).ToWorldCoordinates();
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "GemCrack") { PitchVariance = 0.2f }, position);
        for (int k = 0; k < 8; k++) {
            Color color = GetEffectsColor(i, j);
            int dust = Dust.NewDust(position + Vector2.UnitY * 2f, 4, 4, DustID.RainbowMk2, Scale: Main.rand.NextFloat(1.5f) * 0.85f, newColor: color, Alpha: 0);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].color = color;
            Main.dust[dust].velocity *= 0.5f;
            Main.dust[dust].scale = 0.8f + Main.rand.NextFloat() * 0.6f;
            Main.dust[dust].fadeIn = 0.5f;
        }
        Vector2 gorePosition = position - new Vector2(4f, 0f);
        string name = string.Empty;
        int value = WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54;
        if (DoesBeaconHaveThoriumGem(i, j)) {
            value += 8;
        }
        switch (value) {
            case 1:
                name = "Amethyst";
                break;
            case 2:
                name = "Topaz";
                break;
            case 3:
                name = "Sapphire";
                break;
            case 4:
                name = "Emerald";
                break;
            case 5:
                name = "Ruby";
                break;
            case 6:
                name = "Diamond";
                break;
            case 7:
                name = "Amber";
                break;
        }
        bool flag = value == 8;
        if ((flag || value == 9) && RoA.TryGetThoriumMod(out Mod thoriumMod)) {
            if (flag) {
                name = "Aquamarine";
            }
            else {
                name = "Opal";
            }
        }
        if (!Main.dedServ) {
            if (name != string.Empty) {
                for (int k = 0; k < 3; k++) {
                    int gore = Gore.NewGore(WorldGen.GetItemSource_FromTileBreak(i, j),
                        gorePosition,
                        Vector2.Zero, ModContent.Find<ModGore>(RoA.ModName + $"/{name}").Type, 1f);
                    Main.gore[gore].velocity *= 0.5f;
                }
            }
        }
    }

    public override bool RightClick(int i, int j) {
        if (!Main.mouseItem.IsEmpty()) {
            return base.RightClick(i, j);
        }

        BeaconTE te = GetTE(i, j);
        if (te is null) {
            return false;
        }

        if (te.IsUsed) {
            return false;
        }

        if (IsTileValidToBeHovered(i, j)) {
            Player player = Main.LocalPlayer;
            Item item = player.GetSelectedItem();
            if (Gems.Contains((short)item.type)) {
                ActionWithGem(i, j);
            }
            else if (HasGemInIt(i, j)) {
                ActionWithGem(i, j, true);
            }
        }
        //else {
        //    if (HasGemInIt(i, j)) {
        //        int item = Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 32, 32,
        //                     GetGemDropID(i, j));
        //        if (Main.netMode != NetmodeID.SinglePlayer)
        //            NetMessage.SendData(MessageID.SyncItem, number: item);
        //    }
        //    WorldGen.KillTile(i, j);
        //    if (!Main.tile[i, j].HasTile && Main.netMode == NetmodeID.MultiplayerClient) {
        //        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
        //    }
        //}

        return true;
    }
}
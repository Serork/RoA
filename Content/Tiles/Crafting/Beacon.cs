using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Crafting;

sealed class BeaconTE : ModTileEntity {
    private enum AnimationState : byte {
        Animation1,
        Animation2,
        Animation3
    }

    private AnimationState _state = AnimationState.Animation1;
    private float _animationTimer;

    public bool IsUsed { get; private set; }
    public Vector2 Scale { get; private set; }
    public Vector2 OffsetPosition { get; private set; }

    public void UseAnimation() {
        if (IsUsed) {
            return;
        }

        ResetAnimation();

        IsUsed = true;
    }

    public void ResetAnimation() {
        IsUsed = false;
        Scale = new Vector2(0.2f, 1f);
        OffsetPosition = Vector2.Zero;
        _state = AnimationState.Animation1;
        _animationTimer = 0f;
    }

    public override void Update() {
        int i = Position.X;
        int j = Position.Y - 2;
        if (Beacon.HasGemInIt(i, j)) {
            Lighting.AddLight(new Vector2(i, j).ToWorldCoordinates(), Beacon.GetEffectsColor(i, j).ToVector3());
        }

        UpdateAnimation(() => {
            if (WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<Beacon>())) {
                int gemType = Beacon.GetGemDropID(i, j);
                bool flag =
                    gemType == ItemID.Diamond ? Main.rand.NextBool(33) :
                    (gemType == ItemID.Ruby || gemType == ItemID.Amber) ? Main.rand.NextChance(0.66) :
                    gemType == ItemID.Emerald ? Main.rand.NextChance(0.8) :
                    gemType == ItemID.Sapphire ? Main.rand.NextChance(0.9) :
                    gemType != ItemID.Topaz || Main.rand.NextChance(0.95);
                if (flag) {
                    Beacon.ActionWithGem(i, j, true, false, true);
                }
            }
        });
    }

    public void UpdateAnimation(Action onEnd) {
        if (!IsUsed) {
            return;
        }

        float num = 1f;
        _animationTimer += num;

        float time = num;
        if (_animationTimer < time) {
            return;
        }

        float animationTimer = _animationTimer - time;
        switch (_state) {
            case AnimationState.Animation1:
                time = 20f;
                if (animationTimer < time) {
                    Scale = Vector2.Lerp(Scale, new Vector2(1.25f, 0.5f), animationTimer / time);
                }
                else {
                    _state = AnimationState.Animation2;
                    _animationTimer -= time;
                }
                break;
            case AnimationState.Animation2:
                time = 10f;
                if (animationTimer < time) {
                    Scale = Vector2.Lerp(Scale, new Vector2(0.85f, 1.25f) * 1.1f, animationTimer / time);
                }
                else {
                    _state = AnimationState.Animation3;
                    _animationTimer -= time;
                }
                break;
            case AnimationState.Animation3:
                time = 30f;
                if (animationTimer < time) {
                    if (animationTimer < time / 2f + time / 3f) {
                        OffsetPosition = Vector2.Lerp(OffsetPosition, Main.rand.NextVector2(-5f, 0f, 5f, 10f), 0.75f);
                    }
                    else {
                        OffsetPosition = Vector2.Lerp(OffsetPosition, Vector2.Zero, 0.25f);
                    }
                }
                else {
                    OffsetPosition = Vector2.Zero;
                    float time2 = 15f;
                    Scale = Vector2.Lerp(Scale, new Vector2(0.2f, 1f), (animationTimer - time) / time2);
                    if (animationTimer >= time + time2) {
                        ResetAnimation();
                        onEnd();
                    }
                }
                break;
        }
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendTileSquare(Main.myPlayer, i, j);
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

            return -1;
        }

        return Place(i, j);
    }

    //public override void OnKill() => DropGem(Main.LocalPlayer, (int)(Position.X * 16f), (int)((Position.Y - 2) * 16f + 8f));

    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y, 0f, 0, 0, 0);

    public override bool IsTileValidForEntity(int i, int j) => WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<Beacon>());


    //public static readonly short[] Gems = [ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Amber];

    //public enum BeaconVariant : byte {
    //    None,
    //    Amethyst,
    //    Topaz,
    //    Sapphire,
    //    Emerald,
    //    Ruby,
    //    Diamond,
    //    Amber,
    //    Length
    //}

    //private BeaconVariant _variant = BeaconVariant.None;

    //public bool HasGemInIt => GetItemID(num3: 0) != -1;

    //public void RemoveGem() => _variant = BeaconVariant.None;

    //public void DropGem(Player player, int x, int y) {
    //    if (!HasGemInIt) {
    //        return;
    //    }

    //    int num = Item.NewItem(player.GetSource_Misc("frombeacon"), x, y, 18, 18, GetItemID());
    //    if (Main.netMode == NetmodeID.MultiplayerClient) {
    //        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num);
    //    }
    //}

    //public void InsertGem(short gemType) {
    //    for (int i = 0; i < Gems.Length; i++) {
    //        if (Gems[i] == gemType) {
    //            _variant = (BeaconVariant)(i + 1);
    //        }
    //    }
    //}

    //public byte GetVariant(bool forDrawing = true, int num = 0) {
    //    bool flag = forDrawing && _variant == BeaconVariant.None;
    //    return Math.Max((byte)((byte)(flag ? BeaconVariant.Length : _variant) + (flag ? -1 : 0) + num), (byte)0);
    //}

    //public int GetItemID(bool forDrawing = true, int num2 = -1, int num3 = -1) {
    //    int num = num2 != -1 ? num2 : GetVariant(forDrawing, num3);
    //    if (_variant != BeaconVariant.None && num == 7) {
    //        num = 6;
    //    }
    //    return num switch {
    //        >= 0 and <= 6 => Gems[num],
    //        _ => -1,
    //    };
    //}

    //public override void SaveData(TagCompound tag) {
    //    tag[nameof(BeaconTE)] = (byte)_variant;
    //}

    //public override void LoadData(TagCompound tag) {
    //    _variant = (BeaconVariant)tag.Get<byte>(nameof(BeaconTE));
    //}
}

sealed class Beacon : ModTile, TileHooks.ITileHaveExtraDraws {
    private static int _variantToShow;

    public static readonly short[] Gems = [ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire, ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Amber];

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

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, -2);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<BeaconTE>().Hook_AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);
    }

    void TileHooks.ITileHaveExtraDraws.PostDrawExtra(SpriteBatch spriteBatch, Point pos) {
        int i = pos.X;
        int j = pos.Y;
        if (!(WorldGenHelper.GetTileSafely(i, j + 1).TileType == Type &&
              WorldGenHelper.GetTileSafely(i, j - 1).TileType == Type)) {
            return;
        }
        BeaconTE beaconTE = GetTE(i, j);
        if (HasGemInIt(i, j) && beaconTE != null) {
            Vector2 zero = Vector2.Zero;
            Vector2 position = new Point(i, j).ToWorldCoordinates();
            Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.Textures + "Beacon_Light");
            Vector2 drawPos = position - Main.screenPosition;
            drawPos.X += 1f;
            drawPos.Y -= 4f;
            Color color = GetEffectsColor(i, j).MultiplyRGB(new Color(250, 250, 250));
            Vector2 scale = beaconTE.IsUsed ? beaconTE.Scale : new(0.2f, 1f);
            //color *= MathHelper.Clamp(Lighting.Brightness(i, j), 0.25f, 1f);
            for (float k = -MathHelper.Pi; k <= MathHelper.Pi; k += MathHelper.PiOver2) {
                spriteBatch.Draw(texture, drawPos + zero + beaconTE.OffsetPosition,
                    null, color.MultiplyAlpha(Helper.Wave(0.3f, 0.9f, 4f, k)) * 0.3f, 0f, new(texture.Width / 2f, texture.Height), scale * Helper.Wave(0.85f, 1.15f, speed: 2f), SpriteEffects.None, 0f);
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

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch) => TileHelper.AddPostDrawPoint(this, i, j);

    public static void TeleportPlayerTo(int i, int j, Player player) {
        if (WorldGenHelper.GetTileSafely(i, j + 1).TileType != ModContent.TileType<Beacon>()) {
            return;
        }
        Color color = new(255, 240, 20);
        //if (!player.IsTileTypeInInteractionRange(597, TileReachCheckSettings.Pylons)) {
        //    string key = "Mods.RoA.TooFar";
        //    if (Main.netMode == NetmodeID.SinglePlayer) {
        //        Main.NewText(Language.GetTextValue(key), color);
        //    }
        //    else {
        //        ChatHelper.SendChatMessageToClient(NetworkText.FromKey(key), color, player.whoAmI);
        //    }
        //    return;
        //}
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

        Vector2 newPos = new Point(i - 1, j - 1).ToWorldCoordinates() - new Vector2(0f, player.HeightOffsetBoost);
        int num2 = TileLoader.GetTile(WorldGenHelper.GetTileSafely(i, j).TileType).GetMapOption(i, j) + 12;
        if (num2 == 13) {
            num2 = 11;
        }
        void dusts(Rectangle effectRect, int num4) {
            for (int k = 0; k < 50; k++) {
                Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Green;
                int num5 = Main.rand.Next(4);
                switch (num5) {
                    case 0:
                    case 1:
                        color = GetEffectsColor(i, j);
                        //switch (num4) {
                        //    case 1:
                        //        color = Color.Purple;
                        //        break;
                        //    case 2:
                        //        color = Color.Yellow;
                        //        break;
                        //    case 3:
                        //        color = Color.Blue;
                        //        break;
                        //    case 4:
                        //        color = Color.Green;
                        //        break;
                        //    case 5:
                        //        color = Color.Red;
                        //        break;
                        //    case 6:
                        //        color = Color.GhostWhite;
                        //        break;
                        //    case 7:
                        //        color = Color.Orange;
                        //        break;
                        //}
                        break;
                    //case 2:
                    //    color = Microsoft.Xna.Framework.Color.Yellow;
                    //    break;
                    case 3:
                        color = Microsoft.Xna.Framework.Color.White;
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
        dusts(player.getRect(), style);
        SoundEngine.PlaySound(SoundID.Item6, player.position);
        player.Teleport(newPos, num2);
        player.velocity = Vector2.Zero;
        SoundEngine.PlaySound(SoundID.Item6, player.position);
        SoundEngine.PlaySound(SoundID.Item8, player.position);
        dusts(player.getRect(), style);
        //if (flag) {
        //    //ActionWithGem(i, j, true, false, true);
        //}
        //else {
        //    BeaconTE beaconTE = GetTE(i, j);
        //    beaconTE?.UseAnimation();
        //}
        BeaconTE beaconTE = GetTE(i, j);
        beaconTE?.UseAnimation();
        if (Main.netMode == NetmodeID.Server) {
            RemoteClient.CheckSection(player.whoAmI, player.position);
        }
    }

    public static short GetLargeGemItemID(int i, int j) {
        switch (WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54) {
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

        return -1;
    }

    public static short GetLargeGemDustID(int i, int j) {
        switch (WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54) {
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

        return -1;
    }

    public static Color GetEffectsColor(int i, int j) {
        ModTile tile = TileLoader.GetTile(WorldGenHelper.GetTileSafely(i, j).TileType);
        ushort option = tile.GetMapOption(i, j);
        switch (option) {
            case 0:
                return new Color(238, 51, 53);
            case 1:
                return new Color(13, 107, 216);
            case 2:
                return new Color(33, 184, 115);
            case 3:
                return new Color(255, 221, 62);
            case 4:
                return new Color(165, 0, 236);
            case 5:
                return new Color(223, 230, 238);
            case 6:
                return new Color(207, 101, 0);
        }

        throw new InvalidOperationException();
    }

    public static LocalizedText GetMapText(int i, int j) {
        ModTile tile = TileLoader.GetTile(WorldGenHelper.GetTileSafely(i, j).TileType);
        ushort option = tile.GetMapOption(i, j);
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
        }

        throw new InvalidOperationException();
    }

    public override ushort GetMapOption(int i, int j) {
        if (WorldGenHelper.GetTileSafely(i, j + 1).TileType != Type) {
            return 7;
        }

        switch (WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54) {
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

    public override void PlaceInWorld(int i, int j, Item item) => ModContent.GetInstance<BeaconTE>().Place(i, j);

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => ModContent.GetInstance<BeaconTE>().Kill(i, j);

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

    //public static int VariantToShow => _variantToShow;

    public static void UpdateVariants() {
        double perTime = 50.0;
        if (Main.timeForVisualEffects % perTime == 0.0) {
            _variantToShow++;
            if (_variantToShow >= Gems.Length) {
                _variantToShow = 0;
            }
        }
    }

    //public static int GetGemItemID(int i, int j) {
    //    BeaconTE te = GetTE(i, j);
    //    if (te is null) {
    //        return -1;
    //    }

    //    bool flag2 = te.HasGemInIt;
    //    bool flag = !flag2 && ModContent.GetInstance<BeaconInterface>().Active;
    //    if (flag) {
    //        UpdateVariants();
    //        return te.GetItemID(false, VariantToShow);
    //    }
    //    else {
    //        return flag2 ? te.GetItemID() : ModContent.ItemType<Items.Placeable.Crafting.Beacon>();
    //    }
    //}

    //public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
    //    BeaconTE te = GetTE(i, j);
    //    if (te is null) {
    //        return;
    //    }

    //    tileFrameY += (short)(54 * te.GetVariant(false));
    //}

    public static int GetGemDropID(int i, int j) => Gems[WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54 - 1];

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
        //BeaconTE te = GetTE(i, j);
        //if (te is null) {
        //    return;
        //}

        //Player player = Main.LocalPlayer;
        //player.cursorItemIconID = !BeaconInterface.HasOpened(te) ? ModContent.ItemType<Items.Placeable.Crafting.Beacon>() : GetGemItemID(i, j);
        //if (player.cursorItemIconID != -1) {
        //    player.noThrow = 2;
        //    player.cursorItemIconEnabled = true;
        //}

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

    public static bool HasGemInIt(int i, int j) => WorldGenHelper.GetTileSafely(i, j).TileFrameY >= 54;

    public static void ActionWithGem(int i, int j, bool remove = false, bool dropItem = true, bool makeDusts = false) {
        Player player = Main.LocalPlayer;
        Item item = player.GetSelectedItem();
        if (!Main.mouseItem.IsEmpty()) {
            item = Main.mouseItem;
        }
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
        for (int l = j - 2; l < j + 2; l++) {
            Tile tile2 = WorldGenHelper.GetTileSafely(i, l);
            if (tile2.ActiveTile(Type)) {
                short getTileFrameY(int usedVariant) {
                    return (short)(num3 * 18 + 54 * usedVariant);
                }
                void setFrame(int usedVariant) {
                    tile2.TileFrameY = getTileFrameY(usedVariant);
                    WorldGen.SquareTileFrame(i, l);
                    NetMessage.SendTileSquare(-1, i, l, 1, 1);
                    num3++;
                }
                bool flag3 = !WorldGenHelper.GetTileSafely(i, l - 1).ActiveTile(Type);
                if (flag3) {
                    if (dropItem) {
                        if (flag2) {
                            Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 32, 32,
                                gemType);
                        }
                        else {
                            if (--item.stack <= 0) {
                                item.TurnToAir();
                            }
                        }
                    }
                    Vector2 position = new Point(i, j).ToWorldCoordinates();
                    if (makeDusts) {
                        SoundEngine.PlaySound(SoundID.Dig, position);
                        for (int k = 0; k < 8; k++) {
                            Color color = GetEffectsColor(i, j);
                            int dust = Dust.NewDust(position + Vector2.UnitY * 2f, 4, 4, 267, Scale: Main.rand.NextFloat(1.5f) * 0.85f, newColor: color, Alpha: 0);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].color = color;
                            Main.dust[dust].velocity *= 0.5f;
                            Main.dust[dust].scale = 0.8f + Main.rand.NextFloat() * 0.6f;
                            Main.dust[dust].fadeIn = 0.5f;
                        }
                        Vector2 gorePosition = position - new Vector2(4f, 0f);
                        string name = string.Empty;
                        switch (WorldGenHelper.GetTileSafely(i, j).TileFrameY / 54) {
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
                        if (name != string.Empty) {
                            for (int k = 0; k < 3; k++) {
                                int gore = Gore.NewGore(WorldGen.GetItemSource_FromTileBreak(i, j),
                                    gorePosition,
                                    Vector2.Zero, ModContent.Find<ModGore>(RoA.ModName + $"/{name}").Type, 1f);
                                Main.gore[gore].velocity *= 0.5f;
                            }
                        }
                    }
                    else {
                        SoundEngine.PlaySound(SoundID.MenuTick, new Point(i, l).ToWorldCoordinates());
                    }
                }
                if (flag || (tile2.TileFrameY >= getTileFrameY(variant) &&
                    tile2.TileFrameY < getTileFrameY(variant + 1))) {
                    flag = true;
                    setFrame(0);
                }
                if (!flag) {
                    setFrame(variant);
                }
            }
        }
    }

    public override bool RightClick(int i, int j) {
        //BeaconTE te = GetTE(i, j);
        //if (te is null) {
        //    return false;
        //}

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
        else {
            if (HasGemInIt(i, j)) {
                Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 32, 32,
                             GetGemDropID(i, j));
            }
            WorldGen.KillTile(i, j);
            if (!Main.tile[i, j].HasTile && Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
            }
        }

        //while (WorldGenHelper.ActiveTile(i, j, Type)) {
        //    j--;
        //}
        //BeaconInterface.ToggleUI(i, j + 1, te);

        return true;
    }
}
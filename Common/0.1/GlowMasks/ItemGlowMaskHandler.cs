using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;
using ReLogic.Utilities;

using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.GlowMasks;

[Autoload(Side = ModSide.Client)]
sealed class ItemGlowMaskHandler : PlayerDrawLayer {
    public interface IDrawArmorGlowMask {
        void SetDrawSettings(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo);
    }

    // only head layer support for now
    public interface IAdvancedGlowMaskDraw {
        void Draw(ref PlayerDrawSet drawInfo, ref Texture2D texture, ref Color color);
    }

    public readonly struct GlowMaskInfo(Asset<Texture2D> glowMask, Color glowColor, bool shouldApplyItemAlpha) {
        public readonly Asset<Texture2D> Texture = glowMask;
        public readonly Color Color = glowColor;
        public readonly bool ShouldApplyItemAlpha = shouldApplyItemAlpha;
    }

    public readonly record struct ArmorGlowMaskInfo(ModItem ModItem, Asset<Texture2D> GlowMaskTexture);

    public delegate void DrawSettings(Player player, ref Texture2D texture, ref Color color, ref PlayerDrawSet drawInfo);
    public readonly record struct ArmorGlowMaskByTypeInfo(ushort Type, Asset<Texture2D> GlowMaskTexture, DrawSettings? DrawSettings = null);

    public static Dictionary<int, GlowMaskInfo> GlowMasks { get; private set; } = [];
    public static Dictionary<int, ArmorGlowMaskInfo> ArmorGlowMasks { get; private set; } = [];
    public static Dictionary<int, ArmorGlowMaskByTypeInfo> ArmorGlowMasksByType { get; private set; } = [];

    public override void Load() {
        On_PlayerDrawLayers.DrawCompositeArmorPiece += On_PlayerDrawLayers_DrawCompositeArmorPiece;
    }

    private void On_PlayerDrawLayers_DrawCompositeArmorPiece(On_PlayerDrawLayers.orig_DrawCompositeArmorPiece orig, ref PlayerDrawSet drawinfo, CompositePlayerDrawContext context, DrawData data) {
        orig(ref drawinfo, context, data);

        Player player = drawinfo.drawPlayer;
        if (player.body == -1) {
            return;
        }

        if (ArmorGlowMasks.TryGetValue(player.body, out ArmorGlowMaskInfo armorGlowMaskInfo)) {
            switch (context) {
                case CompositePlayerDrawContext.BackShoulder:
                case CompositePlayerDrawContext.BackArm:
                case CompositePlayerDrawContext.FrontArm:
                case CompositePlayerDrawContext.FrontShoulder: {
                        DrawData item2 = data;
                        var armorGlowMaskModItem = armorGlowMaskInfo.ModItem;
                        Texture2D glowMaskTexture = armorGlowMaskInfo.GlowMaskTexture.Value;
                        Color glowMaskColor = Color.White;
                        (armorGlowMaskModItem as IDrawArmorGlowMask)?.SetDrawSettings(player, ref glowMaskTexture, ref glowMaskColor, ref drawinfo);
                        item2.texture = glowMaskTexture;
                        glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
                        item2.color = glowMaskColor;
                        item2.shader = drawinfo.cBody;
                        drawinfo.DrawDataCache.Add(item2);
                        break;
                    }
            }
        }
        if (ArmorGlowMasksByType.TryGetValue(player.body, out ArmorGlowMaskByTypeInfo armorGlowMaskByTypesInfo)) {
            switch (context) {
                case CompositePlayerDrawContext.BackShoulder:
                case CompositePlayerDrawContext.BackArm:
                case CompositePlayerDrawContext.FrontArm:
                case CompositePlayerDrawContext.FrontShoulder: {
                        DrawData item2 = data;
                        Texture2D glowMaskTexture = armorGlowMaskByTypesInfo.GlowMaskTexture.Value;
                        Color glowMaskColor = Color.White;
                        armorGlowMaskByTypesInfo.DrawSettings?.Invoke(player, ref glowMaskTexture, ref glowMaskColor, ref drawinfo);
                        item2.texture = glowMaskTexture;
                        glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, (float)drawinfo.shadow);
                        item2.color = glowMaskColor;
                        item2.shader = drawinfo.cBody;
                        drawinfo.DrawDataCache.Add(item2);
                        break;
                    }
            }
        }
    }

    private class ItemGlowMaskWorld : GlobalItem {
        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            if (!TileHelper.DrawingTiles) {
                return;
            }

            DrawGlowMask(item, spriteBatch, itemColor, 0f, scale, position);
        }

        public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
            DrawGlowMask(item, spriteBatch, lightColor, rotation);
        }

        private static void DrawGlowMask(Item item, SpriteBatch spriteBatch, Color lightColor, float rotation, float scale = 1f, Vector2? position = null) {
            if (item.type >= ItemID.Count && GlowMasks.TryGetValue(item.type, out GlowMaskInfo glowMaskInfo)) {
                Texture2D glowMaskTexture = glowMaskInfo.Texture.Value;
                Vector2 origin = glowMaskTexture.Size() / 2f;
                Vector2 colorPosition = position ?? item.Center;
                if (position != null) {
                    colorPosition -= TileHelper.ScreenOffset;
                    colorPosition -= new Point(1, 1).ToWorldCoordinates();
                    colorPosition += Main.screenPosition;
                }
                float brightnessFactor = Lighting.Brightness((int)colorPosition.X / 16, (int)colorPosition.Y / 16);
                Color color = Color.Lerp(glowMaskInfo.Color, lightColor, brightnessFactor);
                if (item.shimmered) {
                    color.R = (byte)(255f * (1f - item.shimmerTime));
                    color.G = (byte)(255f * (1f - item.shimmerTime));
                    color.B = (byte)(255f * (1f - item.shimmerTime));
                    color.A = (byte)(255f * (1f - item.shimmerTime));
                }
                else if (item.shimmerTime > 0f) {
                    color.R = (byte)((float)(int)color.R * (1f - item.shimmerTime));
                    color.G = (byte)((float)(int)color.G * (1f - item.shimmerTime));
                    color.B = (byte)((float)(int)color.B * (1f - item.shimmerTime));
                    color.A = (byte)((float)(int)color.A * (1f - item.shimmerTime));
                }

                position ??= item.Center - Main.screenPosition;
                spriteBatch.Draw(glowMaskTexture, position.Value, null,
                    glowMaskInfo.ShouldApplyItemAlpha ? color * (1f - item.alpha / 255f) : glowMaskInfo.Color,
                    rotation, origin, scale, SpriteEffects.None, 0f);
                if (item.shimmered)
                    spriteBatch.Draw(glowMaskTexture, position.Value, null, new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, 0), rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }
    }

    public override void SetStaticDefaults() {
        LoadGlowMasks();
    }

    private static void LoadGlowMasks() {
        foreach (ModItem item in RoA.Instance.GetContent<ModItem>()) {
            AutoloadGlowMaskAttribute? attribute = item?.GetType().GetAttribute<AutoloadGlowMaskAttribute>();
            if (attribute != null) {
                string modItemTexture = item.Texture;
                Asset<Texture2D> texture = ModContent.Request<Texture2D>(modItemTexture + attribute.Requirement, AssetRequestMode.ImmediateLoad);
                texture.Value.Name = modItemTexture;
                GlowMaskInfo glowMaskInfo = new(texture, attribute.GlowColor, attribute.ShouldApplyItemAlpha);
                GlowMasks[item.Type] = glowMaskInfo;
            }
        }
    }

    public static void RegisterArmorGlowMask(EquipType equipType, ModItem modItem) {
        string textureTypePath = modItem.Texture;
        switch (equipType) {
            case EquipType.Head:
                textureTypePath += "_Head_Glow";
                break;
            case EquipType.Body:
                textureTypePath += "_Body_Glow";
                break;
            case EquipType.Legs:
                textureTypePath += "_Legs_Glow";
                break;
        }
        int slotId = 0;
        Item item = modItem.Item;
        switch (equipType) {
            case EquipType.Head:
                slotId = item.headSlot;
                break;
            case EquipType.Body:
                slotId = item.bodySlot;
                break;
            case EquipType.Legs:
                slotId = item.legSlot;
                break;
        }
        ArmorGlowMasks.TryAdd(slotId, new ArmorGlowMaskInfo(modItem, ModContent.Request<Texture2D>(textureTypePath)));
    }

    public static void RegisterArmorGlowMask(int slotId, EquipType equipType, ushort type, DrawSettings? drawSettings = null) {
        string textureTypePath = ResourceManager.GlowMaskItemTextures + $"Item_{type}";
        switch (equipType) {
            case EquipType.Head:
                textureTypePath += "_Head_Glow";
                break;
            case EquipType.Body:
                textureTypePath += "_Body_Glow";
                break;
            case EquipType.Legs:
                textureTypePath += "_Legs_Glow";
                break;
        }
        ArmorGlowMasksByType.TryAdd(slotId, new ArmorGlowMaskByTypeInfo(type, ModContent.Request<Texture2D>(textureTypePath), drawSettings));
    }

    public override void Unload() {
        GlowMasks.Clear();
        GlowMasks = null!;
        ArmorGlowMasks.Clear();
        ArmorGlowMasks = null!;
        ArmorGlowMasksByType.Clear();
        ArmorGlowMasksByType = null!;
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => GlowMasks != null;

    public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ArmOverItem);

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        if (drawInfo.hideEntirePlayer) {
            return;
        }

        Player player = drawInfo.drawPlayer;
        if (!player.active) {
            return;
        }
        if (drawInfo.shadow != 0f) {
            return;
        }
        DrawUsableItemGlowMask(ref drawInfo);
    }

    private static void DrawUsableItemGlowMask(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        Item item = player.HeldItem;
        if (!player.IsAliveAndFree() || ((player.itemAnimation <= 0 || item.useStyle == ItemUseStyleID.None) && (item.holdStyle <= 0 || player.pulley)) || player.dead || item.noUseGraphic || (player.wet && item.noWet)) {
            return;
        }

        if (item.type >= ItemID.Count && GlowMasks.TryGetValue(item.type, out GlowMaskInfo glowMaskInfo)) {
            Texture2D texture = glowMaskInfo.Texture.Value;
            Vector2 offset = new();
            float rotOffset = 0f;
            Vector2 origin = new();
            bool gravReversed = player.gravDir == -1f,
                 leftDirection = player.direction == -1;
            if (item.useStyle == ItemUseStyleID.Shoot) {
                if (Item.staff[item.type]) {
                    rotOffset = MathHelper.PiOver4 * player.direction;
                    if (gravReversed) {
                        rotOffset -= MathHelper.PiOver2 * player.direction;
                    }
                    origin = new Vector2(texture.Width * 0.5f * (1f - player.direction), gravReversed ? 0 : texture.Height);
                    int x = -(int)origin.X;
                    ItemLoader.HoldoutOrigin(player, ref origin);
                    offset = new Vector2(origin.X + x, 0f);
                }
                else {
                    offset = new Vector2(10f, texture.Height / 2f);
                    ItemLoader.HoldoutOffset(player.gravDir, item.type, ref offset);
                    origin = new Vector2(-offset.X, texture.Height / 2);
                    if (leftDirection) {
                        origin.X = texture.Width + offset.X;
                    }
                    offset = new Vector2(0f, offset.Y);
                }
            }
            else {
                origin = new Vector2(texture.Width * 0.5f * (1 - player.direction), gravReversed ? 0 : texture.Height);
            }

            Vector2 position = (player.itemLocation + offset + new Vector2(0f, player.gfxOffY)).Floor();
            Color color = Color.Lerp(glowMaskInfo.Color, drawInfo.itemColor, Lighting.Brightness((int)position.X / 16, (int)position.Y / 16));
            drawInfo.DrawDataCache.Add(new DrawData(texture, position - Main.screenPosition, texture.Bounds,
                                                     glowMaskInfo.ShouldApplyItemAlpha ? item.GetAlpha(color) : glowMaskInfo.Color, player.itemRotation + rotOffset, origin, item.scale, drawInfo.itemEffect, 0));
        }
    }

    public sealed class HeadGlowMaskHandler : PlayerDrawLayer {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            if (drawInfo.hideEntirePlayer) {
                return;
            }

            Player player = drawInfo.drawPlayer;
            if (!player.active || player.invis || player.dead) {
                return;
            }
            DrawHeadGlowMask(ref drawInfo);
        }

        private static void DrawHeadGlowMask(ref PlayerDrawSet drawInfo) {
            Player player = drawInfo.drawPlayer;
            if (player.head != -1) {
                if (ArmorGlowMasks.TryGetValue(player.head, out ArmorGlowMaskInfo armorGlowMaskInfo)) {
                    var armorGlowMaskModItem = armorGlowMaskInfo.ModItem;
                    Texture2D glowMaskTexture = armorGlowMaskInfo.GlowMaskTexture.Value;
                    Color glowMaskColor = Color.White;
                    glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, drawInfo.shadow);
                    if (armorGlowMaskModItem is IAdvancedGlowMaskDraw advancedGlowMaskDraw) {
                        advancedGlowMaskDraw.Draw(ref drawInfo, ref glowMaskTexture, ref glowMaskColor);
                        return;
                    }
                    if (armorGlowMaskModItem is IDrawArmorGlowMask armorGlowMask) {
                        armorGlowMask.SetDrawSettings(player, ref glowMaskTexture, ref glowMaskColor, ref drawInfo);
                    }
                    DrawData item = GetHeadGlowMask(ref drawInfo, glowMaskTexture, glowMaskColor);
                    drawInfo.DrawDataCache.Add(item);
                }
                if (ArmorGlowMasksByType.TryGetValue(player.head, out ArmorGlowMaskByTypeInfo armorGlowMaskByTypeInfo)) {
                    Texture2D glowMaskTexture = armorGlowMaskByTypeInfo.GlowMaskTexture.Value;
                    Color glowMaskColor = Color.White;
                    armorGlowMaskByTypeInfo.DrawSettings?.Invoke(player, ref glowMaskTexture, ref glowMaskColor, ref drawInfo);
                    glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, drawInfo.shadow);
                    DrawData item = GetHeadGlowMask(ref drawInfo, glowMaskTexture, glowMaskColor);
                    drawInfo.DrawDataCache.Add(item);
                }
            }
        }

        public static DrawData GetHeadGlowMask(ref PlayerDrawSet drawInfo, Texture2D glowMaskTexture, Color glowMaskColor) {
            Rectangle bodyFrame = drawInfo.drawPlayer.bodyFrame;
            bodyFrame.Width += 2;
            Vector2 helmetOffset = drawInfo.helmetOffset;
            if (drawInfo.drawPlayer.direction == -1) {
                helmetOffset.X -= 2f;
            }
            DrawData item = new(glowMaskTexture,
                helmetOffset + new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) +
                (float)(drawInfo.drawPlayer.width / 2)),
                (int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height -
                (float)drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.headPosition + drawInfo.headVect,
                bodyFrame, glowMaskColor, drawInfo.drawPlayer.headRotation, drawInfo.headVect, 1f, drawInfo.playerEffect) {
                shader = drawInfo.cHead
            };
            return item;
        }
    }

    private class LegsGlowMaskHandler : PlayerDrawLayer {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Leggings);

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            if (drawInfo.hideEntirePlayer) {
                return;
            }

            Player player = drawInfo.drawPlayer;
            if (!player.active || player.invis || player.dead) {
                return;
            }
            DrawArmorGlowMask(ref drawInfo);
        }

        private static void DrawArmorGlowMask(ref PlayerDrawSet drawInfo) {
            Player player = drawInfo.drawPlayer;
            if (player.legs != -1) {
                if (ArmorGlowMasks.TryGetValue(player.legs, out ArmorGlowMaskInfo armorGlowMaskInfo)) {
                    var armorGlowMaskModItem = armorGlowMaskInfo.ModItem;
                    Texture2D glowMaskTexture = armorGlowMaskInfo.GlowMaskTexture.Value;
                    Color glowMaskColor = Color.White;
                    if (armorGlowMaskModItem is IDrawArmorGlowMask armorGlowMask) {
                        armorGlowMask.SetDrawSettings(player, ref glowMaskTexture, ref glowMaskColor, ref drawInfo);
                    }
                    glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, drawInfo.shadow);
                    Vector2 drawPos = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2 - player.legFrame.Width / 2, player.height - player.legFrame.Height + 4f) + player.legPosition;
                    Vector2 legsOffset = drawInfo.legsOffset;
                    DrawData drawData = new(glowMaskTexture, drawPos.Floor() + legsOffset, player.legFrame, glowMaskColor, player.legRotation, legsOffset, 1f, drawInfo.playerEffect, 0) {
                        shader = drawInfo.cLegs
                    };
                    drawInfo.DrawDataCache.Add(drawData);
                }
                if (ArmorGlowMasksByType.TryGetValue(player.legs, out ArmorGlowMaskByTypeInfo armorGlowMaskByTypeInfo)) {
                    Texture2D glowMaskTexture = armorGlowMaskByTypeInfo.GlowMaskTexture.Value;
                    Color glowMaskColor = Color.White;
                    armorGlowMaskByTypeInfo.DrawSettings?.Invoke(player, ref glowMaskTexture, ref glowMaskColor, ref drawInfo);
                    glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, drawInfo.shadow);
                    Vector2 drawPos = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2 - player.legFrame.Width / 2, player.height - player.legFrame.Height + 4f) + player.legPosition;
                    Vector2 legsOffset = drawInfo.legsOffset;
                    DrawData drawData = new(glowMaskTexture, drawPos.Floor() + legsOffset, player.legFrame, glowMaskColor, player.legRotation, legsOffset, 1f, drawInfo.playerEffect, 0) {
                        shader = drawInfo.cLegs
                    };
                    drawInfo.DrawDataCache.Add(drawData);
                }
            }
        }
    }

    private class BodyGlowMaskHandler : PlayerDrawLayer {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);

        protected override void Draw(ref PlayerDrawSet drawInfo) {
            if (drawInfo.hideEntirePlayer) {
                return;
            }

            Player player = drawInfo.drawPlayer;
            if (!player.active || player.invis || player.dead) {
                return;
            }
            DrawArmorGlowMask(ref drawInfo);
        }

        private static void DrawArmorGlowMask(ref PlayerDrawSet drawInfo) {
            Player player = drawInfo.drawPlayer;
            if (player.body != -1) {
                if (ArmorGlowMasks.TryGetValue(player.body, out ArmorGlowMaskInfo armorGlowMaskInfo)) {
                    var armorGlowMaskModItem = armorGlowMaskInfo.ModItem;
                    Texture2D glowMaskTexture = armorGlowMaskInfo.GlowMaskTexture.Value;
                    Color glowMaskColor = Color.White;
                    (armorGlowMaskModItem as IDrawArmorGlowMask)?.SetDrawSettings(player, ref glowMaskTexture, ref glowMaskColor, ref drawInfo);
                    glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, (float)drawInfo.shadow);
                    Rectangle bodyFrame = drawInfo.compTorsoFrame;
                    Vector2 vector = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawInfo.drawPlayer.width / 2)), (int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height - (float)drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2);
                    vector.Y += drawInfo.torsoOffset;
                    Vector2 vector2 = Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height];
                    vector2.Y -= 2f;
                    vector += vector2 * -Utils.ToDirectionInt(drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically));
                    Vector2 drawPos = vector;
                    DrawData drawData = new(glowMaskTexture, drawPos.Floor(),
                        bodyFrame, glowMaskColor, player.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0) {
                        shader = drawInfo.cBody
                    };
                    drawInfo.DrawDataCache.Add(drawData);
                }
                if (ArmorGlowMasksByType.TryGetValue(player.body, out ArmorGlowMaskByTypeInfo armorGlowMaskByTypeInfo)) {
                    Texture2D glowMaskTexture = armorGlowMaskByTypeInfo.GlowMaskTexture.Value;
                    Color glowMaskColor = Color.White;
                    armorGlowMaskByTypeInfo.DrawSettings?.Invoke(player, ref glowMaskTexture, ref glowMaskColor, ref drawInfo);
                    glowMaskColor = player.GetImmuneAlphaPure(glowMaskColor, (float)drawInfo.shadow);
                    Rectangle bodyFrame = drawInfo.compTorsoFrame;
                    Vector2 vector = new Vector2((int)(drawInfo.Position.X - Main.screenPosition.X - (float)(drawInfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawInfo.drawPlayer.width / 2)), (int)(drawInfo.Position.Y - Main.screenPosition.Y + (float)drawInfo.drawPlayer.height - (float)drawInfo.drawPlayer.bodyFrame.Height + 4f)) + drawInfo.drawPlayer.bodyPosition + new Vector2(drawInfo.drawPlayer.bodyFrame.Width / 2, drawInfo.drawPlayer.bodyFrame.Height / 2);
                    vector.Y += drawInfo.torsoOffset;
                    Vector2 vector2 = Main.OffsetsPlayerHeadgear[drawInfo.drawPlayer.bodyFrame.Y / drawInfo.drawPlayer.bodyFrame.Height];
                    vector2.Y -= 2f;
                    vector += vector2 * -Utils.ToDirectionInt(drawInfo.playerEffect.HasFlag(SpriteEffects.FlipVertically));
                    Vector2 drawPos = vector;
                    DrawData drawData = new(glowMaskTexture, drawPos.Floor(),
                        bodyFrame, glowMaskColor, player.bodyRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0) {
                        shader = drawInfo.cBody
                    };
                    drawInfo.DrawDataCache.Add(drawData);
                }
            }
        }
    }

    public sealed class BodyGlowMaskHandler2 : ModPlayer {
        private static Dictionary<int, Func<Color>> _bodyColor;

        public static void RegisterData(int bodySlot, Func<Color> color) {
            if (!_bodyColor.ContainsKey(bodySlot)) {
                _bodyColor.Add(bodySlot, color);
            }
        }

        public override void Load()
            => _bodyColor = new Dictionary<int, Func<Color>>();

        public override void Unload()
            => _bodyColor = null;

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo) {
            if (!_bodyColor.TryGetValue(Player.body, out Func<Color> color))
                return;

            drawInfo.bodyGlowColor = color();
            drawInfo.bodyGlowColor = Player.GetImmuneAlphaPure(drawInfo.bodyGlowColor, drawInfo.shadow);
            drawInfo.armGlowColor = color();
            drawInfo.armGlowColor = Player.GetImmuneAlphaPure(drawInfo.armGlowColor, drawInfo.shadow);
        }
    }
}
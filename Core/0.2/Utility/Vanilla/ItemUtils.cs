using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Dyes;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Core.Utility.Vanilla;

static class ItemUtils {
    public static string GetTexturePath<T>() where T : ModItem => ItemLoader.GetItem(ModContent.ItemType<T>()).Texture;

    public static void DrawHeldItem(Item heldItem, ref PlayerDrawSet drawinfo, Texture2D? texture = null, Texture2D? glowMaskTexture = null) {
        if (drawinfo.drawPlayer.JustDroppedAnItem)
            return;

        float adjustedItemScale = drawinfo.drawPlayer.GetAdjustedItemScale(heldItem);
        //Main.instance.LoadItem(num);
        int num = heldItem.type;
        Texture2D value = texture ?? TextureAssets.Item[heldItem.type].Value;
        Vector2 position = new Vector2((int)(drawinfo.ItemLocation.X - Main.screenPosition.X), (int)(drawinfo.ItemLocation.Y - Main.screenPosition.Y));
        Rectangle itemDrawFrame = drawinfo.drawPlayer.GetItemDrawFrame(num);
        drawinfo.itemColor = Lighting.GetColor((int)((double)drawinfo.Position.X + (double)drawinfo.drawPlayer.width * 0.5) / 16, (int)(((double)drawinfo.Position.Y + (double)drawinfo.drawPlayer.height * 0.5) / 16.0));
        if (drawinfo.drawPlayer.shroomiteStealth && heldItem.DamageType == DamageClass.Ranged) {
            float num2 = drawinfo.drawPlayer.stealth;
            if ((double)num2 < 0.03)
                num2 = 0.03f;
            float num3 = (1f + num2 * 10f) / 11f;
            drawinfo.itemColor = new Color((byte)((float)(int)drawinfo.itemColor.R * num2), (byte)((float)(int)drawinfo.itemColor.G * num2), (byte)((float)(int)drawinfo.itemColor.B * num3), (byte)((float)(int)drawinfo.itemColor.A * num2));
        }
        if (drawinfo.drawPlayer.setVortex && heldItem.DamageType == DamageClass.Ranged) {
            float num4 = drawinfo.drawPlayer.stealth;
            if ((double)num4 < 0.03)
                num4 = 0.03f;
            _ = (1f + num4 * 10f) / 11f;
            drawinfo.itemColor = drawinfo.itemColor.MultiplyRGBA(new Color(Vector4.Lerp(Vector4.One, new Vector4(0f, 0.12f, 0.16f, 0f), 1f - num4)));
        }
        bool flag = drawinfo.drawPlayer.itemAnimation > 0 && heldItem.useStyle != 0;
        bool flag2 = heldItem.holdStyle != 0 && !drawinfo.drawPlayer.pulley;
        if (!drawinfo.drawPlayer.CanVisuallyHoldItem(heldItem))
            flag2 = false;
        if (drawinfo.shadow != 0f || drawinfo.drawPlayer.frozen || !(flag || flag2) || num <= 0 || drawinfo.drawPlayer.dead || heldItem.noUseGraphic || (drawinfo.drawPlayer.wet && heldItem.noWet && !ItemID.Sets.WaterTorches[num]/*Allow biome torches underwater.*/) || (drawinfo.drawPlayer.happyFunTorchTime && drawinfo.drawPlayer.inventory[drawinfo.drawPlayer.selectedItem].createTile == 4 && drawinfo.drawPlayer.itemAnimation == 0))
            return;
        _ = drawinfo.drawPlayer.name;
        Color color = Color.White * (1f - heldItem.alpha / 255f);
        Vector2 vector = Vector2.Zero;
        Vector2 origin = new Vector2((float)itemDrawFrame.Width * 0.5f - (float)itemDrawFrame.Width * 0.5f * (float)drawinfo.drawPlayer.direction, itemDrawFrame.Height);
        if (drawinfo.drawPlayer.gravDir == -1f)
            origin.Y = (float)itemDrawFrame.Height - origin.Y;
        origin += vector;
        float num5 = drawinfo.drawPlayer.itemRotation;
        ItemSlot.GetItemLight(ref drawinfo.itemColor, heldItem);
        DrawData item;

        //DrawColor itemColor = heldItem.GetAlpha(drawinfo.itemColor);
        Color itemColor = drawinfo.itemColor;

        if (heldItem.type == ItemID.Hammush) {
            DelegateMethods.v3_1 = new Vector3(0.1f, 0.4f, 1f);
            Utils.PlotTileLine(drawinfo.ItemLocation, drawinfo.ItemLocation + Vector2.UnitY.RotatedBy(num5) * value.Width, 4, DelegateMethods.CastLightOpen);
        }

        if (drawinfo.drawPlayer.gravDir == -1f) {
            item = new DrawData(value, position, itemDrawFrame, itemColor, num5, origin, adjustedItemScale, drawinfo.itemEffect);
            drawinfo.DrawDataCache.Add(item);
            if (heldItem.color != default(Color)) {
                item = new DrawData(value, position, itemDrawFrame, itemColor, num5, origin, adjustedItemScale, drawinfo.itemEffect);
                drawinfo.DrawDataCache.Add(item);
            }
            if (heldItem.glowMask != -1) {
                item = new DrawData(TextureAssets.GlowMask[heldItem.glowMask].Value, position, itemDrawFrame, new Color(250, 250, 250, heldItem.alpha), num5, origin, adjustedItemScale, drawinfo.itemEffect);
                drawinfo.DrawDataCache.Add(item);
            }

            if (glowMaskTexture != null) {
                // glowmask
                item = new DrawData(glowMaskTexture, position, itemDrawFrame, new Color(250, 250, 250, heldItem.alpha) * 0.9f, num5, origin, adjustedItemScale, drawinfo.itemEffect);
                drawinfo.DrawDataCache.Add(item);
            }
            return;
        }
        item = new DrawData(value, position, itemDrawFrame, itemColor, num5, origin, adjustedItemScale, drawinfo.itemEffect);
        drawinfo.DrawDataCache.Add(item);
        if (heldItem.color != default(Color)) {
            item = new DrawData(value, position, itemDrawFrame, itemColor, num5, origin, adjustedItemScale, drawinfo.itemEffect);
            drawinfo.DrawDataCache.Add(item);
        }
        if (heldItem.glowMask != -1) {
            item = new DrawData(TextureAssets.GlowMask[heldItem.glowMask].Value, position, itemDrawFrame, color, num5, origin, adjustedItemScale, drawinfo.itemEffect);
            drawinfo.DrawDataCache.Add(item);
        }

        if (glowMaskTexture != null) {
            // glowmask
            if (heldItem.type == ItemID.TerraBlade) {
                color = Color.White * 0.75f;
            }
            item = new DrawData(glowMaskTexture, position, itemDrawFrame, color, num5, origin, adjustedItemScale, drawinfo.itemEffect);
            if (heldItem.type == ItemID.TerraBlade) {
                item.shader = GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<TerraDye>());
            }
            drawinfo.DrawDataCache.Add(item);
        }
    }

    public static void DrawItem(Item item, Color color, float rotation = 0f, Texture2D? texture = null, float scale = 1f, Vector2? position = null) {
        texture ??= TextureAssets.Item[item.type].Value;

        Vector2 origin = texture.Size() / 2f;
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

        var frame = texture.Frame();
        Vector2 vector = frame.Size() / 2f;
        Vector2 vector2 = new Vector2((float)(item.width / 2) - vector.X, item.height - frame.Height);
        position ??= item.position - Main.screenPosition + vector + vector2;

        if (item.type == ItemID.TerraBlade && color == Color.White * 0.75f) {
            Main.spriteBatch.DrawWithSnapshot(() => {
                DrawData data = new(texture, position.Value, null,
                                    color,
                                    rotation, origin, scale, SpriteEffects.None, 0f);
                GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<TerraDye>()), item, data);
                data.Draw(Main.spriteBatch);
            }, sortMode: SpriteSortMode.Immediate);
        }
        else {
            Main.spriteBatch.Draw(texture, position.Value, null,
                                  color,
                                  rotation, origin, scale, SpriteEffects.None, 0f);
        }

        if ((item.type == ItemID.MushroomSpear || item.type == ItemID.Hammush) && !TileHelper.DrawingTiles) {
            Lighting.AddLight(position.Value + Main.screenPosition, new Vector3(0.1f, 0.4f, 1f));
        }

        if (item.shimmered) {
            Main.spriteBatch.Draw(texture, position.Value, null, new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, 0), rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}

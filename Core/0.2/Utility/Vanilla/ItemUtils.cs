using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using RoA.Common;
using RoA.Common.Items;
using RoA.Content.Items.Miscellaneous;

using System;

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

        int num = heldItem.type;
        float adjustedItemScale = drawinfo.drawPlayer.GetAdjustedItemScale(heldItem);
        Texture2D value = TextureAssets.Item[num].Value;
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

        Color glowColor_New = new Color(250, 250, 250, heldItem.alpha) * 0.9f;
        if (heldItem.useStyle == ItemUseStyleID.Shoot) {
            ItemSlot.GetItemLight(ref drawinfo.itemColor, heldItem);
            if (Item.staff[num]) {
                float num9 = drawinfo.drawPlayer.itemRotation + 0.785f * (float)drawinfo.drawPlayer.direction;
                float num10 = 0f;
                float num11 = 0f;
                Vector2 origin5 = new Vector2(0f, itemDrawFrame.Height);
                if (num == 3210) {
                    num10 = 8 * -drawinfo.drawPlayer.direction;
                    num11 = 2 * (int)drawinfo.drawPlayer.gravDir;
                }

                if (num == 3870) {
                    Vector2 vector6 = (drawinfo.drawPlayer.itemRotation + (float)Math.PI / 4f * (float)drawinfo.drawPlayer.direction).ToRotationVector2() * new Vector2((float)(-drawinfo.drawPlayer.direction) * 1.5f, drawinfo.drawPlayer.gravDir) * 3f;
                    num10 = (int)vector6.X;
                    num11 = (int)vector6.Y;
                }

                if (num == 3787)
                    num11 = (int)((float)(8 * (int)drawinfo.drawPlayer.gravDir) * (float)Math.Cos(num9));

                if (num == 3209) {
                    Vector2 vector7 = (new Vector2(-8f, 0f) * drawinfo.drawPlayer.Directions).RotatedBy(drawinfo.drawPlayer.itemRotation);
                    num10 = vector7.X;
                    num11 = vector7.Y;
                }

                if (drawinfo.drawPlayer.gravDir == -1f) {
                    if (drawinfo.drawPlayer.direction == -1) {
                        num9 += 1.57f;
                        origin5 = new Vector2(itemDrawFrame.Width, 0f);
                        num10 -= (float)itemDrawFrame.Width;
                    }
                    else {
                        num9 -= 1.57f;
                        origin5 = Vector2.Zero;
                    }
                }
                // Extra patch context.
                else if (drawinfo.drawPlayer.direction == -1) {
                    origin5 = new Vector2(itemDrawFrame.Width, itemDrawFrame.Height);
                    num10 -= (float)itemDrawFrame.Width;
                }

                ItemLoader.HoldoutOrigin(drawinfo.drawPlayer, ref origin5);

                DrawData item2 = new DrawData(value, new Vector2((int)(drawinfo.ItemLocation.X - Main.screenPosition.X + origin5.X + num10), (int)(drawinfo.ItemLocation.Y - Main.screenPosition.Y + num11)), itemDrawFrame, heldItem.GetAlpha(drawinfo.itemColor), num9, origin5, adjustedItemScale, drawinfo.itemEffect);

                drawinfo.DrawDataCache.Add(item2);
                if (glowMaskTexture != null) {
                    item2 = new DrawData(glowMaskTexture, new Vector2((int)(drawinfo.ItemLocation.X - Main.screenPosition.X + origin5.X + num10), (int)(drawinfo.ItemLocation.Y - Main.screenPosition.Y + num11)), itemDrawFrame, new Color(255, 255, 255, 127), num9, origin5, adjustedItemScale, drawinfo.itemEffect);
                    drawinfo.DrawDataCache.Add(item2);
                }

                return;
            }

            Vector2 getOrigin(PlayerDrawSet drawinfo, Rectangle frame, out Vector2 vector9, int num12_2 = 0) {
                int num12 = 10;
                vector9 = new Vector2(0, frame.Height / 2); // forward port from 1.4.5
                Vector2 vector10 = Main.DrawPlayerItemPos(drawinfo.drawPlayer.gravDir, num);
                num12 = (int)vector10.X;
                num12 += num12_2;
                vector9.Y = vector10.Y;
                Vector2 origin7 = new Vector2(-num12, frame.Height / 2);
                if (drawinfo.drawPlayer.direction == -1)
                    origin7 = new Vector2(frame.Width + num12, frame.Height / 2);
                return origin7;
            }

            Vector2 origin7 = getOrigin(drawinfo, itemDrawFrame, out Vector2 vector9);

            DrawData item;
            if (num == ItemID.LunarFlareBook && DifferentGlowMaskOnVanillaWeapons_Usage.LunarFlare_Use?.IsLoaded == true) {
                Texture2D useTexture = DifferentGlowMaskOnVanillaWeapons_Usage.LunarFlare_Use.Value;
                Rectangle frame = new SpriteFrame(10, 1, (byte)(TimeSystem.TimeForVisualEffects * 16f % 10), 0).GetSourceRectangle(useTexture);
                origin7 = getOrigin(drawinfo, frame, out vector9, -10);
                item = new DrawData(useTexture, new Vector2((int)(drawinfo.ItemLocation.X - Main.screenPosition.X + vector9.X), (int)(drawinfo.ItemLocation.Y - Main.screenPosition.Y + vector9.Y)),
                    frame,
                    heldItem.GetAlpha(drawinfo.itemColor), drawinfo.drawPlayer.itemRotation, origin7, adjustedItemScale, drawinfo.itemEffect);
                drawinfo.DrawDataCache.Add(item);
                if (DifferentGlowMaskOnVanillaWeapons_Usage.LunarFlare_Use_Glow?.IsLoaded == true) {
                    useTexture = DifferentGlowMaskOnVanillaWeapons_Usage.LunarFlare_Use_Glow.Value;
                    item = new DrawData(useTexture, new Vector2((int)(drawinfo.ItemLocation.X - Main.screenPosition.X + vector9.X), (int)(drawinfo.ItemLocation.Y - Main.screenPosition.Y + vector9.Y)),
                                        frame,
                                        glowColor_New, drawinfo.drawPlayer.itemRotation, origin7, adjustedItemScale, drawinfo.itemEffect);
                    drawinfo.DrawDataCache.Add(item);
                }
            }
            else if (num == ItemID.MagnetSphere && DifferentGlowMaskOnVanillaWeapons_Usage.MagnetSphere_Use?.IsLoaded == true) {
                Texture2D useTexture = DifferentGlowMaskOnVanillaWeapons_Usage.MagnetSphere_Use.Value;
                Rectangle frame = new SpriteFrame(5, 1, (byte)(TimeSystem.TimeForVisualEffects * 12f % 5), 0).GetSourceRectangle(useTexture);
                origin7 = getOrigin(drawinfo, frame, out vector9, -10);
                item = new DrawData(useTexture, new Vector2((int)(drawinfo.ItemLocation.X - Main.screenPosition.X + vector9.X), (int)(drawinfo.ItemLocation.Y - Main.screenPosition.Y + vector9.Y)),
                    frame,
                    heldItem.GetAlpha(drawinfo.itemColor), drawinfo.drawPlayer.itemRotation, origin7, adjustedItemScale, drawinfo.itemEffect);
                drawinfo.DrawDataCache.Add(item);
                if (DifferentGlowMaskOnVanillaWeapons_Usage.MagnetSphere_Use_Glow?.IsLoaded == true) {
                    useTexture = DifferentGlowMaskOnVanillaWeapons_Usage.MagnetSphere_Use_Glow.Value;
                    item = new DrawData(useTexture, new Vector2((int)(drawinfo.ItemLocation.X - Main.screenPosition.X + vector9.X), (int)(drawinfo.ItemLocation.Y - Main.screenPosition.Y + vector9.Y)),
                                        frame,
                                        glowColor_New, drawinfo.drawPlayer.itemRotation, origin7, adjustedItemScale, drawinfo.itemEffect);
                    drawinfo.DrawDataCache.Add(item);
                }
            }
            else {
                item = new DrawData(value, new Vector2((int)(drawinfo.ItemLocation.X - Main.screenPosition.X + vector9.X), (int)(drawinfo.ItemLocation.Y - Main.screenPosition.Y + vector9.Y)), itemDrawFrame, heldItem.GetAlpha(drawinfo.itemColor), drawinfo.drawPlayer.itemRotation, origin7, adjustedItemScale, drawinfo.itemEffect);
                drawinfo.DrawDataCache.Add(item);

                if (heldItem.color != default(Color)) {
                    item = new DrawData(value, new Vector2((int)(drawinfo.ItemLocation.X - Main.screenPosition.X + vector9.X), (int)(drawinfo.ItemLocation.Y - Main.screenPosition.Y + vector9.Y)), itemDrawFrame, heldItem.GetColor(drawinfo.itemColor), drawinfo.drawPlayer.itemRotation, origin7, adjustedItemScale, drawinfo.itemEffect);
                    drawinfo.DrawDataCache.Add(item);
                }

                if (heldItem.glowMask != -1) {
                    item = new DrawData(TextureAssets.GlowMask[heldItem.glowMask].Value, new Vector2((int)(drawinfo.ItemLocation.X - Main.screenPosition.X + vector9.X), (int)(drawinfo.ItemLocation.Y - Main.screenPosition.Y + vector9.Y)), itemDrawFrame, new Color(250, 250, 250, heldItem.alpha), drawinfo.drawPlayer.itemRotation, origin7, adjustedItemScale, drawinfo.itemEffect);
                    drawinfo.DrawDataCache.Add(item);
                }

                if (glowMaskTexture != null) {
                    item = new DrawData(glowMaskTexture, new Vector2((int)(drawinfo.ItemLocation.X - Main.screenPosition.X + vector9.X), (int)(drawinfo.ItemLocation.Y - Main.screenPosition.Y + vector9.Y)), itemDrawFrame, glowColor_New, drawinfo.drawPlayer.itemRotation, origin7, adjustedItemScale, drawinfo.itemEffect);
                    drawinfo.DrawDataCache.Add(item);
                }
            }
        }
        else if (heldItem.useStyle == ItemUseStyleID.Swing) {
            //Main.instance.LoadItem(num);
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
                    item = new DrawData(glowMaskTexture, position, itemDrawFrame, glowColor_New, num5, origin, adjustedItemScale, drawinfo.itemEffect);
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
    }

    public static void DrawItem(Item item, Color color, float rotation = 0f, Texture2D? texture = null, float scale = 1f, Vector2? position = null, SpriteEffects? spriteEffects = null) {
        texture ??= TextureAssets.Item[item.type].Value;

        spriteEffects ??= SpriteEffects.None;

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
                                    rotation, origin, scale, spriteEffects.Value, 0f);
                GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(ModContent.ItemType<TerraDye>()), item, data);
                data.Draw(Main.spriteBatch);
            }, sortMode: SpriteSortMode.Immediate);
        }
        else {
            Main.spriteBatch.Draw(texture, position.Value, null,
                                  color,
                                  rotation, origin, scale, spriteEffects.Value, 0f);
        }

        if ((item.type == ItemID.MushroomSpear || item.type == ItemID.Hammush) && !TileHelper.DrawingTiles) {
            Lighting.AddLight(position.Value + Main.screenPosition, new Vector3(0.1f, 0.4f, 1f));
        }

        if (item.shimmered) {
            Main.spriteBatch.Draw(texture, position.Value, null, new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, 0), rotation, origin, scale, spriteEffects.Value, 0f);
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace RoA.Common.Items;

// TODO: separate
sealed class DifferentGlowMaskOnVanillaTruffleWeaponsSystem_GlowMaskInWorld : GlobalItem {
    private static Asset<Texture2D>? _mushroomSpearTexture, _mushroomSpearGlowMaskTexture;

    public override void Load() {
        LoadMushroomSpearTextures();
    }

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        bool hammush = item.type == ItemID.Hammush;
        if (item.type == ItemID.MushroomSpear || hammush) {
            if (_mushroomSpearTexture?.IsLoaded != true || _mushroomSpearGlowMaskTexture?.IsLoaded != true) {
                return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI); ;
            }

            Texture2D itemTexture;
            if (hammush) {
                itemTexture = DifferentGlowMaskOnVanillaTruffleWeaponsSystem_Hammush.HammushTexture!.Value;
            }
            else {
                itemTexture = _mushroomSpearTexture.Value;
            }

            Texture2D glowMaskTexture;
            if (hammush) {
                glowMaskTexture = DifferentGlowMaskOnVanillaTruffleWeaponsSystem_Hammush.HammushGlowMaskTexture!.Value;
            }
            else {
                glowMaskTexture = _mushroomSpearGlowMaskTexture.Value;
            }
            Color glowColor = Color.White;
            Vector2 origin = glowMaskTexture.Size() / 2f;
            Color color = glowColor;
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

            if (item.shimmered) {
                lightColor.R = (byte)(255f * (1f - item.shimmerTime));
                lightColor.G = (byte)(255f * (1f - item.shimmerTime));
                lightColor.B = (byte)(255f * (1f - item.shimmerTime));
                lightColor.A = (byte)(255f * (1f - item.shimmerTime));
            }
            else if (item.shimmerTime > 0f) {
                lightColor.R = (byte)((float)(int)lightColor.R * (1f - item.shimmerTime));
                lightColor.G = (byte)((float)(int)lightColor.G * (1f - item.shimmerTime));
                lightColor.B = (byte)((float)(int)lightColor.B * (1f - item.shimmerTime));
                lightColor.A = (byte)((float)(int)lightColor.A * (1f - item.shimmerTime));
            }

            var frame = itemTexture.Frame();
            Vector2 vector = frame.Size() / 2f;
            Vector2 vector2 = new Vector2((float)(item.width / 2) - vector.X, item.height - frame.Height);
            Vector2 vector3 = item.position - Main.screenPosition + vector + vector2;

            spriteBatch.Draw(itemTexture, vector3, null,
                lightColor,
                rotation, origin, 1f, SpriteEffects.None, 0f);

            spriteBatch.Draw(glowMaskTexture, vector3, null,
                color,
                rotation, origin, 1f, SpriteEffects.None, 0f);

            Lighting.AddLight(vector3 + Main.screenPosition, new Vector3(0.1f, 0.4f, 1f));

            if (item.shimmered) {
                spriteBatch.Draw(itemTexture, vector3, null, new Microsoft.Xna.Framework.Color(lightColor.R, lightColor.G, lightColor.B, 0), rotation, origin, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(glowMaskTexture, vector3, null, new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, 0), rotation, origin, 1f, SpriteEffects.None, 0f);
            }

            return false;
        }

        return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
    }

    private void LoadMushroomSpearTextures() {
        if (Main.dedServ) {
            return;
        }

        string texturePath = ResourceManager.ItemsWeaponsMeleeTextures + "MushroomSpear";
        _mushroomSpearTexture = ModContent.Request<Texture2D>(texturePath);
        _mushroomSpearGlowMaskTexture = ModContent.Request<Texture2D>(texturePath + "_Glow");
    }
}

sealed class DifferentGlowMaskOnVanillaTruffleWeaponsSystem_Hammush : IInitializer {
    public static Asset<Texture2D>? HammushTexture, HammushGlowMaskTexture;

    void ILoadable.Load(Mod mod) {
        LoadHammushTextures();

        On_PlayerDrawLayers.DrawPlayer_27_HeldItem += On_PlayerDrawLayers_DrawPlayer_27_HeldItem;
    }

    private void LoadHammushTextures() {
        if (Main.dedServ) {
            return;
        }

        string texturePath = ResourceManager.ItemsWeaponsMeleeTextures + "Hammush";
        HammushTexture = ModContent.Request<Texture2D>(texturePath);
        HammushGlowMaskTexture = ModContent.Request<Texture2D>(texturePath + "_Glow");
    }

    private void On_PlayerDrawLayers_DrawPlayer_27_HeldItem(On_PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawinfo) {
        Item heldItem = drawinfo.heldItem;
        int num = heldItem.type;
        
        if (num == ItemID.Hammush) {
            if (HammushTexture?.IsLoaded != true || HammushGlowMaskTexture?.IsLoaded != true) {
                orig(ref drawinfo);

                return;
            }

            float adjustedItemScale = drawinfo.drawPlayer.GetAdjustedItemScale(heldItem);
            //Main.instance.LoadItem(num);
            Texture2D value = HammushTexture.Value;
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

            //Color hammushColor = heldItem.GetAlpha(drawinfo.itemColor);
            Color hammushColor = drawinfo.itemColor;

            DelegateMethods.v3_1 = new Vector3(0.1f, 0.4f, 1f);
            Utils.PlotTileLine(drawinfo.ItemLocation, drawinfo.ItemLocation + Vector2.UnitY.RotatedBy(num5) * value.Width, 4, DelegateMethods.CastLightOpen);

            if (drawinfo.drawPlayer.gravDir == -1f) {
                item = new DrawData(value, position, itemDrawFrame, hammushColor, num5, origin, adjustedItemScale, drawinfo.itemEffect);
                drawinfo.DrawDataCache.Add(item);
                if (heldItem.color != default(Color)) {
                    item = new DrawData(value, position, itemDrawFrame, hammushColor, num5, origin, adjustedItemScale, drawinfo.itemEffect);
                    drawinfo.DrawDataCache.Add(item);
                }

                if (heldItem.glowMask != -1) {
                    item = new DrawData(TextureAssets.GlowMask[heldItem.glowMask].Value, position, itemDrawFrame, new Color(250, 250, 250, heldItem.alpha), num5, origin, adjustedItemScale, drawinfo.itemEffect);
                    drawinfo.DrawDataCache.Add(item);
                }

                // glowmask
                item = new DrawData(HammushGlowMaskTexture.Value, position, itemDrawFrame, new Color(250, 250, 250, heldItem.alpha), num5, origin, adjustedItemScale, drawinfo.itemEffect);
                drawinfo.DrawDataCache.Add(item);

                return;
            }

            item = new DrawData(value, position, itemDrawFrame, hammushColor, num5, origin, adjustedItemScale, drawinfo.itemEffect);
            drawinfo.DrawDataCache.Add(item);
            if (heldItem.color != default(Color)) {
                item = new DrawData(value, position, itemDrawFrame, hammushColor, num5, origin, adjustedItemScale, drawinfo.itemEffect);
                drawinfo.DrawDataCache.Add(item);
            }

            if (heldItem.glowMask != -1) {
                item = new DrawData(TextureAssets.GlowMask[heldItem.glowMask].Value, position, itemDrawFrame, color, num5, origin, adjustedItemScale, drawinfo.itemEffect);
                drawinfo.DrawDataCache.Add(item);
            }

            // glowmask
            item = new DrawData(HammushGlowMaskTexture.Value, position, itemDrawFrame, color, num5, origin, adjustedItemScale, drawinfo.itemEffect);
            drawinfo.DrawDataCache.Add(item);

            return;
        }

        orig(ref drawinfo);
    }
}

sealed class DifferentGlowMaskOnVanillaTruffleWeaponsSystem_MushroomSpear : GlobalProjectile {
    private static Asset<Texture2D>? _mushroomSpearTexture, _mushroomSpearGlowMaskTexture;

    public override void Load() {
        LoadMushroomSpearTextures();
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor) {
        if (projectile.type == ProjectileID.MushroomSpear) {
            DrawMushroomSpear(projectile);

            return false;
        }

        return base.PreDraw(projectile, ref lightColor);
    }

    private void LoadMushroomSpearTextures() {
        if (Main.dedServ) {
            return;
        }

        string texturePath = ResourceManager.MeleeProjectileTextures + "MushroomSpear";
        _mushroomSpearTexture = ModContent.Request<Texture2D>(texturePath);
        _mushroomSpearGlowMaskTexture = ModContent.Request<Texture2D>(texturePath + "_Glow");
    }

    private void DrawMushroomSpear(Projectile projectile) {
        if (_mushroomSpearTexture?.IsLoaded != true || _mushroomSpearGlowMaskTexture?.IsLoaded != true) {
            return;
        }

        Projectile proj = projectile;
        SpriteEffects dir = SpriteEffects.None;
        float num = (float)Math.Atan2(proj.velocity.Y, proj.velocity.X) + 2.355f;
        Asset<Texture2D> asset = _mushroomSpearTexture;
        Asset<Texture2D> asset_glow = _mushroomSpearGlowMaskTexture;
        Player player = Main.player[proj.owner];
        Microsoft.Xna.Framework.Rectangle value = asset.Frame();
        Microsoft.Xna.Framework.Rectangle rect = proj.getRect();
        Vector2 vector = Vector2.Zero;
        if (player.direction > 0) {
            dir = SpriteEffects.FlipHorizontally;
            vector.X = asset.Width();
            num -= (float)Math.PI / 2f;
        }

        if (player.gravDir == -1f) {
            if (proj.direction == 1) {
                dir = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                vector = new Vector2(asset.Width(), asset.Height());
                num -= (float)Math.PI / 2f;
            }
            else if (proj.direction == -1) {
                dir = SpriteEffects.FlipVertically;
                vector = new Vector2(0f, asset.Height());
                num += (float)Math.PI / 2f;
            }
        }

        Vector2.Lerp(vector, value.Center.ToVector2(), 0.25f);
        float num2 = 0f;
        Vector2 vector2 = proj.Center + new Vector2(0f, proj.gfxOffY);
        Color color = Lighting.GetColor((int)proj.Center.X / 16, (int)proj.Center.Y / 16);
        Main.EntitySpriteDraw(asset.Value, vector2 - Main.screenPosition, value, color, num, vector, proj.scale, dir);
        color = Color.White * (1f - proj.alpha / 255f);

        DelegateMethods.v3_1 = new Vector3(0.1f, 0.4f, 1f);
        Utils.PlotTileLine(vector2, vector2 + Vector2.UnitY.RotatedBy(num) * value.Width, 4, DelegateMethods.CastLightOpen);

        Main.EntitySpriteDraw(asset_glow.Value, vector2 - Main.screenPosition, value, color, num, vector, proj.scale, dir);
    }
}

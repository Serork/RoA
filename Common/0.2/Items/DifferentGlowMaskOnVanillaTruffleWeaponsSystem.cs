using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;
using RoA.Core.Utility.Vanilla;

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
                return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
            }

            Lighting.AddLight(item.Center + Main.screenPosition, new Vector3(0.1f, 0.4f, 1f));

            if (hammush) {
                ItemUtils.DrawItem(item, lightColor, rotation, DifferentGlowMaskOnVanillaTruffleWeaponsSystem_Hammush.HammushTexture!.Value);
                ItemUtils.DrawItem(item, Color.White, rotation, DifferentGlowMaskOnVanillaTruffleWeaponsSystem_Hammush.HammushGlowMaskTexture!.Value);
            }
            else {
                ItemUtils.DrawItem(item, lightColor, rotation, _mushroomSpearTexture.Value);
                ItemUtils.DrawItem(item, Color.White, rotation, _mushroomSpearGlowMaskTexture.Value);
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

            ItemUtils.DrawHeldItem(heldItem, ref drawinfo, HammushTexture.Value, HammushGlowMaskTexture.Value);
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

        ProjectileUtils.DrawSpearProjectile(projectile, _mushroomSpearTexture.Value, _mushroomSpearGlowMaskTexture.Value);
    }
}

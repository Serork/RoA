using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Configs;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

// TODO: separate

// mushroom spear
// hammush
// terra blade

sealed class DifferentGlowMaskOnVanillaWeapons_GlowMaskInWorld : GlobalItem {
    private static Asset<Texture2D>? _mushroomSpearTexture, _mushroomSpearGlowMaskTexture;
    internal static Asset<Texture2D>? _terraBladeGlowMaskTexture;

    public override Color? GetAlpha(Item item, Color lightColor) {
        if (ModContent.GetInstance<RoAClientConfig>().VanillaResprites && item.type == ItemID.TerraBlade) {
            return lightColor * (1f - item.alpha / 255f);
        }

        return base.GetAlpha(item, lightColor);
    }

    public override void Load() {
        LoadMushroomSpearTextures();

        LoadTerraBladeGlowMask();
    }

    private void LoadTerraBladeGlowMask() {
        if (Main.dedServ) {
            return;
        }

        if (!ModContent.GetInstance<RoAClientConfig>().VanillaResprites) {
            return;
        }

        _terraBladeGlowMaskTexture = ModContent.Request<Texture2D>(ResourceManager.ItemTextures + "TerraBlade_Glow");
    }

    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        bool hammush = item.type == ItemID.Hammush;

        Color color = Color.White;

        if (item.type == ItemID.MushroomSpear || hammush) {
            if (_mushroomSpearTexture?.IsLoaded != true || _mushroomSpearGlowMaskTexture?.IsLoaded != true) {
                return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
            }

            if (hammush) {
                ItemUtils.DrawItem(item, color, 0f, DifferentGlowMaskOnVanillaTruffleWeapons_Hammush.HammushTexture!.Value, scale, position);
                ItemUtils.DrawItem(item, Color.White * 0.9f, 0f, DifferentGlowMaskOnVanillaTruffleWeapons_Hammush.HammushGlowMaskTexture!.Value, scale, position);
            }
            else {
                ItemUtils.DrawItem(item, color, 0f, _mushroomSpearTexture.Value, scale, position);
                ItemUtils.DrawItem(item, Color.White * 0.9f, 0f, _mushroomSpearGlowMaskTexture.Value, scale, position);
            }

            return false;
        }

        if (item.type == ItemID.TerraBlade) {
            if (_terraBladeGlowMaskTexture?.IsLoaded != true) {
                return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
            }

            ItemUtils.DrawItem(item, color, 0f, TextureAssets.Item[ItemID.TerraBlade].Value, scale, position);
            ItemUtils.DrawItem(item, Color.White * 0.75f, 0f, _terraBladeGlowMaskTexture.Value, scale, position);

            return false;
        }

        return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        bool hammush = item.type == ItemID.Hammush;
        if (item.type == ItemID.MushroomSpear || hammush) {
            if (_mushroomSpearTexture?.IsLoaded != true || _mushroomSpearGlowMaskTexture?.IsLoaded != true) {
                return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
            }

            if (!TileHelper.DrawingTiles) {
                Lighting.AddLight(item.Center + Main.screenPosition, new Vector3(0.1f, 0.4f, 1f));
            }

            if (hammush) {
                ItemUtils.DrawItem(item, lightColor, rotation, DifferentGlowMaskOnVanillaTruffleWeapons_Hammush.HammushTexture!.Value);
                ItemUtils.DrawItem(item, Color.White * 0.9f, rotation, DifferentGlowMaskOnVanillaTruffleWeapons_Hammush.HammushGlowMaskTexture!.Value);
            }
            else {
                ItemUtils.DrawItem(item, lightColor, rotation, _mushroomSpearTexture.Value);
                ItemUtils.DrawItem(item, Color.White * 0.9f, rotation, _mushroomSpearGlowMaskTexture.Value);
            }

            return false;
        }

        if (item.type == ItemID.TerraBlade) {
            if (_terraBladeGlowMaskTexture?.IsLoaded != true) {
                return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
            }

            ItemUtils.DrawItem(item, lightColor, rotation, TextureAssets.Item[ItemID.TerraBlade].Value);
            ItemUtils.DrawItem(item, Color.White * 0.75f, rotation, _terraBladeGlowMaskTexture.Value);
            return false;
        }

        return base.PreDrawInWorld(item, spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
    }

    private void LoadMushroomSpearTextures() {
        if (Main.dedServ) {
            return;
        }

        if (!ModContent.GetInstance<RoAClientConfig>().VanillaResprites) {
            return;
        }

        string texturePath = ResourceManager.MeleeWeaponTextures + "MushroomSpear";
        _mushroomSpearTexture = ModContent.Request<Texture2D>(texturePath);
        _mushroomSpearGlowMaskTexture = ModContent.Request<Texture2D>(texturePath + "_Glow");
    }
}

sealed class DifferentGlowMaskOnVanillaTruffleWeapons_Hammush : IInitializer {
    public static Asset<Texture2D>? HammushTexture, HammushGlowMaskTexture;

    void ILoadable.Load(Mod mod) {
        LoadHammushTextures();

        On_PlayerDrawLayers.DrawPlayer_27_HeldItem += On_PlayerDrawLayers_DrawPlayer_27_HeldItem;
    }

    private void LoadHammushTextures() {
        if (Main.dedServ) {
            return;
        }

        if (!ModContent.GetInstance<RoAClientConfig>().VanillaResprites) {
            return;
        }

        string texturePath = ResourceManager.MeleeWeaponTextures + "Hammush";
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

        if (num == ItemID.TerraBlade) {
            if (DifferentGlowMaskOnVanillaWeapons_GlowMaskInWorld._terraBladeGlowMaskTexture?.IsLoaded != true) {
                orig(ref drawinfo);

                return;
            }

            ItemUtils.DrawHeldItem(heldItem, ref drawinfo, glowMaskTexture: DifferentGlowMaskOnVanillaWeapons_GlowMaskInWorld._terraBladeGlowMaskTexture.Value);
            return;
        }

        orig(ref drawinfo);
    }
}

sealed class DifferentGlowMaskOnVanillaTruffleWeapons_MushroomSpear : GlobalProjectile {
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

        if (!ModContent.GetInstance<RoAClientConfig>().VanillaResprites) {
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

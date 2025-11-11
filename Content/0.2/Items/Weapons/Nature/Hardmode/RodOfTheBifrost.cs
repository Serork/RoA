using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid;
using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Content.Projectiles.Friendly.Nature.MagicalBifrostBlock;
using static Terraria.Player;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

[AutoloadGlowMask(127, 127, 127, 127)]
sealed class RodOfTheBifrost : NatureItem {
    private static Asset<Texture2D> _outlineBlockTexture = null!;

    public override void SetStaticDefaults() {
        Item.staff[Type] = true;

        if (!Main.dedServ) {
            _outlineBlockTexture = ModContent.Request<Texture2D>(ProjectileLoader.GetProjectile(ModContent.ProjectileType<MagicalBifrostBlock>()).Texture + "_Outline");
        }
    }

    public override void Load() {
        On_Main.DrawInterface_4_Ruler += On_Main_DrawInterface_4_Ruler;
    }

    private void On_Main_DrawInterface_4_Ruler(On_Main.orig_DrawInterface_4_Ruler orig) {
        orig();

        Player player = Main.LocalPlayer;
        if (!player.IsHolding<RodOfTheBifrost>()) {
            return;
        }
        SpriteBatch batch = Main.spriteBatch;
        MagicalBifrostBlockInfo[]? blockData = player.GetCommon().ActiveMagicalBlockData;
        if (blockData is null) {
            return;
        }
        Point16 blockPosition = player.GetWorldMousePosition().ToTileCoordinates16();
        foreach (var blockInfo in blockData) {
            Texture2D texture = TextureAssets.Projectile[ModContent.ProjectileType<MagicalBifrostBlock>()].Value;
            Color color = Color.White;
            color *= 0.5f;
            Vector2 position = GetBlockPosition(blockPosition.ToWorldCoordinates(), blockInfo, tiled: false);
            Point16 frameCoords = blockInfo.FrameCoords;
            Rectangle clip = new(frameCoords.X * 18, frameCoords.Y * 18, 16, 16);
            Vector2 origin = clip.Centered();
            batch.Draw(texture, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Scale = Vector2.One
            });
            for (float num5 = 0f; num5 < 1f; num5 += 0.25f) {
                Vector2 vector2 = (num5 * ((float)Math.PI * 2f)).ToRotationVector2() * 1f;
                texture = _outlineBlockTexture.Value;
                color = Color.White;
                color *= 1f;
                color.A /= 2;
                batch.Draw(texture, position + vector2 * Helper.Wave(-1f, 1f, 5f, 0f), DrawInfo.Default with {
                    Clip = clip,
                    Origin = origin,
                    Color = color,
                    Scale = Vector2.One
                });
            }
        }
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(36, 36);
        Item.SetWeaponValues(60, 4f);
        Item.SetUsableValues(ItemUseStyleID.HiddenAnimation, 20, useSound: SoundID.Item7);
        Item.SetShopValues(ItemRarityColor.Yellow8, Item.sellPrice());
        Item.SetShootableValues();

        NatureWeaponHandler.SetPotentialDamage(Item, 100);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.5f);
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame) {
        player.itemLocation = player.Center + player.MovementOffset() + new Vector2(player.direction * 4f, 6f * player.gravDir);
        float maxRotation = -0.15f;
        player.itemRotation = player.DirectionTo(player.Center + new Vector2(player.direction * 100f, -10f)).ToRotation() 
            + Helper.Wave(-maxRotation, maxRotation, 10f, player.whoAmI) * player.direction
            + MathHelper.Pi * (!player.FacedRight()).ToInt();

        CompositeArmStretchAmount compositeArmStretchAmount2 = CompositeArmStretchAmount.Full;
        float rotation = player.itemRotation * 0.375f * -player.direction;
        rotation += MathHelper.PiOver4 * 1f;
        rotation *= -player.direction;
        if (player.FacedRight()) {
            rotation += -0.1f;
        }
        rotation += -0.5f * player.direction;
        player.SetCompositeArmFront(enabled: true, compositeArmStretchAmount2, rotation);
        player.SetCompositeArmBack(enabled: true, CompositeArmStretchAmount.Full, rotation);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        player.GetCommon().ChooseNextBifrostPattern();
        Projectile magicalBlock = ProjectileUtils.SpawnPlayerOwnedProjectile<MagicalBifrostBlock>(new ProjectileUtils.SpawnProjectileArgs(player, player.GetSource_ItemUse(Item)) {
            Damage = damage,
            KnockBack = knockback,
            AI1 = player.miscCounterNormalized * 12f % 1f
        });

        return false;
    }
}

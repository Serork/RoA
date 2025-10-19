using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Druid.Wreath;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Common.VisualEffects;
using RoA.Content.Buffs;
using RoA.Content.Forms;
using RoA.Content.VisualEffects;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using static RoA.Common.Druid.Forms.BaseForm;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

[Tracked]
sealed class HallowWard : FormProjectile_NoTextureLoad {
    private static ushort TIMELEFT => 300;
    private static float AREASIZE => 200f;

    public ref float AreaSize => ref Projectile.ai[0];
    public ref float State => ref Projectile.ai[1];

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(0);

        Projectile.friendly = true;

        Projectile.timeLeft = TIMELEFT;

        ShouldApplyAttachedNatureWeaponCurrentDamage = false;

        Projectile.Opacity = 0f;
    }

    public override void AI() {
        void givePlayersBuff() {
            float neededDistanceInPixels = AreaSize;
            foreach (Player player in Main.ActivePlayers) {
                if (player.Distance(Projectile.Center) < neededDistanceInPixels) {
                    player.AddBuff<HallowBlessing>(2);
                }
            }
        }
        givePlayersBuff();

        Player owner = Projectile.GetOwnerAsPlayer();

        if (!owner.IsAlive() || !owner.GetFormHandler().IsConsideredAs<HallowEnt>() || Projectile.timeLeft < 40) {
            State = 1f;
        }

        Projectile.Center = owner.Bottom - Vector2.UnitY * 40f;
        Projectile.Center = Utils.Floor(Projectile.Center) + Vector2.UnitY * 10f + Vector2.UnitY * owner.gfxOffY;

        Projectile.localAI[0]++;
        AreaSize = AREASIZE + AREASIZE * 0.115f * Helper.Wave(-1f, 1f, 2f, Projectile.whoAmI);
        AreaSize *= Ease.SineInOut(Utils.Remap(Projectile.Opacity, 0f, 1f, 0.75f, 1f));

        if (State == 1f) {
            Projectile.Opacity -= 0.05f;
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }
        else {
            Projectile.Opacity += 0.05f;
            if (Projectile.Opacity >= 1f) {
                Projectile.Opacity = 1f;
            }
        }

        if (Main.rand.Next(4) == 0) {
            float num4 = AreaSize * 0.9f;
            Vector2 vector = new(Main.rand.Next(-10, 11), Main.rand.Next(-10, 11));
            float num5 = Main.rand.Next(3, 9);
            vector.Normalize();
            float scale = 1f;
            Vector2 velocity;
            if (Main.rand.Next(8) == 0) {
                velocity = vector * (0f - num5) * 3f;
                scale += 0.5f;
            }
            else {
                velocity = vector * (0f - num5);
            }
            velocity *= Main.rand.NextFloat(0.75f);
            Leaf? leafParticle = VisualEffectSystem.New<Leaf>(VisualEffectLayer.ABOVEPLAYERS)?.Setup(Projectile.Center + vector * num4, velocity);
            if (leafParticle != null) {
                leafParticle.CustomData = owner;
                leafParticle.Scale *= 0.85f;
                leafParticle.AI0 = -1f;
            }
        }
    }

    public override bool? CanDamage() => false;

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<HallowLeaf>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D leafTexture = TextureAssets.Projectile[ModContent.ProjectileType<HallowLeaf>()].Value,
                  glowMaskLeafTexture = indexedTextureAssets[(byte)HallowLeaf.HallowLeafRequstedTextureType.Glow].Value;
        SpriteFrame frame = new(1, HallowLeaf.FRAMECOUNT);
        Rectangle clip = frame.GetSourceRectangle(leafTexture);
        int count = 21;
        Player owner = Projectile.GetOwnerAsPlayer();
        Color color;
        for (int i = 0; i < count; i++) {
            Color pickedColor = HallowLeaf.GetColor(HallowLeaf.PickIndex(i));
            Texture2D texture = ResourceManager.DefaultSparkle;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            SpriteEffects effects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float circleFactor = MathHelper.TwoPi / count * i - MathHelper.Pi;
            float visualTimer = (float)Main.timeForVisualEffects * 0.1f;
            float maxOffset = 8f;
            float progress = BaseFormDataStorage.GetAttackCharge(owner);
            Vector2 position = Projectile.Center;
            Vector2 drawPos = position + Utils.ToRotationVector2(circleFactor) * AreaSize * 0.95f;
            color = Color.Lerp(new Color(pickedColor.R, pickedColor.G, pickedColor.B, 50), Color.White, HallowLeaf.EXTRABRIGHTNESSMODIFIER);
            float rotation = circleFactor + MathHelper.PiOver2;
            float areaFactor0 = AreaSize / AREASIZE * 1.5f;
            Vector2 areaFactor = new(1f, areaFactor0);
            rotation += MathF.Sin(circleFactor + visualTimer) * 0.3f;
            position += Utils.ToRotationVector2(circleFactor) * (AreaSize + MathF.Sin(circleFactor * 5f + visualTimer) * maxOffset);
            float rotation1 = rotation;
            for (int i2 = 0; i2 < 1; i2++) {
                Main.spriteBatch.Draw(glowMaskLeafTexture, drawPos - Main.screenPosition, clip, Color.Lerp(color.MultiplyRGB(Lighting.GetColor(drawPos.ToTileCoordinates())) * 0.25f, Color.White, HallowLeaf.EXTRABRIGHTNESSMODIFIER * progress * 0.75f) * areaFactor0, rotation1 + (float)MathHelper.Pi / 2 + MathF.Sin(circleFactor + visualTimer) * 0.05f, drawOrigin, Projectile.scale, effects, 0f);
                rotation1 += MathHelper.PiOver2;
            }
            //spriteBatch.DrawSelf(texture, drawPos - Projectile.oldPos[k] * 0.5f + Projectile.oldPos[k + 1] * 0.5f, null, color * 0.45f, Projectile.oldRot[k] * 0.5f + Projectile.oldRot[k + 1] * 0.5f + (float)Math.PI / 2, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length, effects, 0f);
            Color baseColor = Color.Lerp(Lighting.GetColor(position.ToTileCoordinates()), Color.White, HallowLeaf.EXTRABRIGHTNESSMODIFIER * progress).MultiplyRGB(pickedColor);
            color = baseColor * Projectile.Opacity;
            Vector2 origin = clip.Centered();
            Main.spriteBatch.Draw(leafTexture, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Rotation = rotation,
                Color = color
            });
            color = WreathHandler.GetArmorGlowColor_HallowEnt(owner, baseColor, progress) * Projectile.Opacity;
            Main.spriteBatch.Draw(glowMaskLeafTexture, position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Rotation = rotation,
                Color = color
            });
        }
    }
}

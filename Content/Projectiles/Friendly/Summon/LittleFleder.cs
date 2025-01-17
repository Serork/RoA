using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Miscellaneous;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class LittleFleder : ModProjectile {
    private const float ATTACKRATE = 40f;

    private float _canChangeDirectionAgainTimer;

    private ref float AttackTimer => ref Projectile.ai[2];

    private float AcornOpacity => Utils.GetLerpValue(ATTACKRATE / 4f, ATTACKRATE / 2f, AttackTimer, true);

    public override void SetStaticDefaults() {
        Main.projFrames[Projectile.type] = 4;
        Main.projPet[Projectile.type] = true;

        ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
    }

    private void ChangeDirection(int dir, float time) {
        if (--_canChangeDirectionAgainTimer > 0f) {
            return;
        }

        Projectile.direction = dir;

        _canChangeDirectionAgainTimer = time;
    }

    public override void SetDefaults() {
        int width = 30; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;

        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.minionSlots = 1;

        Projectile.netImportant = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override bool PreAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 4) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame > 3)
            Projectile.frame = 0;
        return true;
    }

    private static int GetGrabRange(Player player, Item item) => player.GetItemGrabRange(item);

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        SpriteEffects spriteEffects = (SpriteEffects)(Projectile.spriteDirection != 1).ToInt();
        int height = texture.Height / Main.projFrames[Projectile.type];
        Rectangle sourceRectangle = new(0, height * Projectile.frame, texture.Width, height);
        Vector2 origin = sourceRectangle.Size() / 2f;
        Vector2 position = Projectile.Center - Main.screenPosition;
        texture = ModContent.Request<Texture2D>(Texture + "_Acorn").Value;
        float progress = Math.Abs(Projectile.rotation) / MathHelper.PiOver2 * Projectile.spriteDirection;
        sourceRectangle = new(0, 0, texture.Width, texture.Height);
        spriteBatch.Draw(texture,
            position +
            new Vector2(-8f - (Projectile.spriteDirection == -1 ? 4f : 0f), 10f + (Projectile.spriteDirection == 1 ? 16f * progress : 0f)), sourceRectangle,
            lightColor * AcornOpacity, Projectile.rotation * 0.5f + MathHelper.Pi, origin / 2f, Projectile.scale, spriteEffects, 0);
        texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        position = Projectile.Center - Main.screenPosition;
        sourceRectangle = new(0, height * Projectile.frame, texture.Width, height);
        Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects);

        if (Projectile.ai[1] == 1f) {
            texture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");
            spriteBatch.BeginBlendState(BlendState.Additive);
            float lifeProgress = 1f;
            for (float i = -MathHelper.Pi; i <= MathHelper.Pi; i += MathHelper.PiOver2) {
                spriteBatch.Draw(texture, position +
                    Utils.RotatedBy(Utils.ToRotationVector2(i), Main.GlobalTimeWrappedHourly * 10.0, new Vector2())
                    * Helper.Wave(0f, 3f, 12f, 0.5f) * lifeProgress,
                   sourceRectangle, Color.White.MultiplyAlpha(Helper.Wave(0.5f, 0.75f, 12f, 0.5f)) * lifeProgress, Projectile.rotation + Main.rand.NextFloatRange(0.05f) * lifeProgress, origin, Projectile.scale, spriteEffects, 0f);
            }
            spriteBatch.EndBlendState();
        }

        return false;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        if (player.dead || !player.active)
            player.ClearBuff(ModContent.BuffType<Buffs.LittleFleder>());
        if (player.HasBuff(ModContent.BuffType<Buffs.LittleFleder>()))
            Projectile.timeLeft = 2;

        if (AttackTimer < ATTACKRATE) {
            AttackTimer += 1f;
        }
    }

    private void AI_GetMyGroupIndexAndFillBlackList(out int index, out int totalIndexesInGroup) {
        index = 0;
        totalIndexesInGroup = 0;
        for (int i = 0; i < 1000; i++) {
            Projectile projectile = Main.projectile[i];
            if (projectile.active && projectile.owner == Projectile.owner && projectile.type == Projectile.type) {
                if (Projectile.whoAmI > i)
                    index++;

                totalIndexesInGroup++;
            }
        }
    }

    private void AI_156_GetIdlePosition(Vector2 destination, int stackedIndex, int totalIndexes, out Vector2 idleSpot, out float idleRotation) {
        bool num = true;
        idleRotation = 0f;
        idleSpot = Vector2.Zero;
        if (num) {
            float num2 = ((float)totalIndexes - 1f) / 2f;
            idleSpot = destination + -Vector2.UnitY.RotatedBy(2f / (float)totalIndexes * ((float)stackedIndex - num2)) * 100f;
            idleRotation = 0f;
        }
    }

    public override bool? CanDamage() => false;

    public override bool? CanCutTiles() => false;

    public override bool MinionContactDamage() => true;
}

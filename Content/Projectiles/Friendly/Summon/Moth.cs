using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Items.Weapons.Summon;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class Moth : ModProjectile {
    private float mothRotation, mothCount;
    private int dashTimer;
    private int dashTrailTimer;
    private int summonSwitchTimer;
    private int summonTimer;
    private bool summonAttack = false;

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Moth");

        Main.projFrames[Projectile.type] = 8;
        Main.projPet[Projectile.type] = true;

        ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;

        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
    }

    public override void SetDefaults() {
        int width = 36; int height = 24;
        Projectile.Size = new Vector2(width, height);
        DrawOriginOffsetY = -16;

        Projectile.penetrate = -1;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;

        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.minionSlots = 1;
    }

    public override bool PreAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 2) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame > 7)
            Projectile.frame = 0;
        return true;
    }

    public override void AI() { // basically modified ExampleMinion with some additional code for dashing and summoning small moths  UPD: база UPD2: база
        Player player = Main.player[Projectile.owner];

        #region Active check
        if (player.dead || !player.active)
            player.ClearBuff(ModContent.BuffType<Buffs.Moth>());
        if (player.HasBuff(ModContent.BuffType<Buffs.Moth>()))
            Projectile.timeLeft = 2;

        #endregion

        #region General behavior

        Vector2 idlePosition = player.Center;
        mothCount = player.ownedProjectileCounts[ModContent.ProjectileType<Moth>()];
        if (mothCount == 0) mothCount++;
        if (Main.mouseLeft && Main.mouseLeftRelease && player.inventory[Main.LocalPlayer.selectedItem].type == ModContent.ItemType<MothStaff>())
            mothRotation = 0;

        if (mothRotation < 360) mothRotation += 1f;
        else mothRotation = 0;

        if (Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].type == ModContent.ItemType<MothStaff>()) {
            idlePosition += new Vector2(32.5f * player.direction, -12.5f);
            Vector2 minionPositionOffset = new Vector2(0f, 18f + mothCount * 4f);
            float deg = mothRotation * 2f * player.direction + 360 / mothCount * Projectile.minionPos;
            idlePosition += minionPositionOffset.RotatedBy(MathHelper.ToRadians(deg));
        }
        else {
            idlePosition.X -= 2f;
            idlePosition.Y += 4f;
            Vector2 minionPositionOffset = new Vector2(0f, 50f + mothCount * 4f);
            float deg = mothRotation * player.direction + 360 / mothCount * Projectile.minionPos;
            idlePosition += minionPositionOffset.RotatedBy(MathHelper.ToRadians(deg));
        }

        // All of this code below this line is adapted from Spazmamini code (ID 388, aiStyle 66)

        // Teleport to player if distance is too big
        Vector2 vectorToIdlePosition = idlePosition - Projectile.Center;
        float distanceToIdlePosition = vectorToIdlePosition.Length();
        if (Main.myPlayer == player.whoAmI && distanceToIdlePosition > 2000f) {
            // Whenever you deal with non-regular events that change the behavior or position drastically, make sure to only run the code on the owner of the projectile,
            // and then set netUpdate to true
            Projectile.position = idlePosition;
            Projectile.velocity *= 0.1f;
            Projectile.netUpdate = true;
        }

        // If your minion is flying, you want to do this independently of any conditions
        float overlapVelocity = 0.04f;
        for (int i = 0; i < Main.maxProjectiles; i++) {
            // Fix overlap with other minions
            Projectile other = Main.projectile[i];
            if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width / 1.5f) {
                if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
                else Projectile.velocity.X += overlapVelocity;

                if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
                else Projectile.velocity.Y += overlapVelocity;
            }
        }
        #endregion

        #region Find target
        // Starting search distance
        float distanceFromTarget = 600f;
        Vector2 targetCenter = Projectile.position;
        bool foundTarget = false;

        // This code is required if your minion weapon has the targeting feature
        if (player.HasMinionAttackTargetNPC) {
            NPC npc = Main.npc[player.MinionAttackTargetNPC];
            float between = Vector2.Distance(npc.Center, Projectile.Center);
            // Reasonable distance away so it doesn't target across multiple screens
            if (between < 2000f) {
                distanceFromTarget = between;
                targetCenter = npc.Center;
                foundTarget = true;
            }
        }
        if (!foundTarget) {
            // This code is required either way, used for finding a target
            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy()) {
                    float between = Vector2.Distance(npc.Center, Projectile.Center);
                    bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                    bool inRange = between < distanceFromTarget;
                    bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                    // Additional check for this specific minion behavior, otherwise it will stop attacking once it dashed through an enemy while flying though tiles afterwards
                    // The number depends on various parameters seen in the movement code below. Test different ones out until it works alright
                    bool closeThroughWall = between < 100f;
                    if ((closest && inRange || !foundTarget) && (lineOfSight || closeThroughWall)) {
                        distanceFromTarget = between;
                        targetCenter = npc.Center;
                        foundTarget = true;
                    }
                }
            }
        }

        // friendly needs to be set to true so the minion can deal contact damage
        // friendly needs to be set to false so it doesn't damage things like target dummies while idling
        // Both things depend on if it has a target or not, so it's just one assignment here
        // You don't need this assignment if your minion is shooting things instead of dealing contact damage
        Projectile.friendly = foundTarget;
        #endregion

        #region Movement

        float speed = 10f;
        float inertia = 40f;

        if (foundTarget) {
            if (Projectile.ai[1] > 0f) {
                Projectile.ai[1]--;
            }
            // if summon attack is active, slow down and wait, then spawn small moths and reset attack
            if (summonAttack) {
                Projectile.velocity *= 0.9f;
                summonTimer++;
                if (summonTimer == 40) {
                    for (int k = 0; k < Main.rand.Next(3, 5); k++) {
                        Vector2 summonPos = Projectile.Center + new Vector2(0f, -16f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), summonPos, Vector2.Normalize(new Vector2(0, 16f).RotatedByRandom(MathHelper.ToRadians(360))), ModContent.ProjectileType<SmallMoth>(), (int)(Projectile.damage * 0.5f), 0, player.whoAmI);
                    }
                    SoundEngine.PlaySound(SoundID.NPCHit32, Projectile.Center);
                    summonTimer = 0;
                    summonAttack = false;
                }
            }
            // otherwise attack normally with dashes
            else {
                Vector2 direction = targetCenter - Projectile.Center;
                direction.Normalize();
                direction *= speed;
                if (distanceFromTarget >= 40f && dashTimer < 40)
                    Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
                else
                    Projectile.velocity = Projectile.velocity * (inertia - 1) / inertia;

                if (distanceFromTarget < 600f) {
                    dashTimer++;
                    if (dashTimer == 40) {
                        Projectile.velocity += direction * 0.9f;
                        Projectile.ai[1] = 20f;
                        Projectile.netUpdate = true;
                    }
                    if (dashTimer == 45) {
                        Projectile.velocity *= 0.9f;
                        dashTimer = 0;
                    }
                }
                // chance to activate summoning attack, increases over time and resets upon activation
                if (Main.rand.Next(500 - summonSwitchTimer) == 0 && dashTimer < 40) {
                    summonAttack = true;
                    summonSwitchTimer = 0;
                    dashTimer = 0;
                }
                else summonSwitchTimer++;
            }
        }
        else {
            summonSwitchTimer = 0;
            dashTimer = 0;
            // Minion doesn't have a target: return to player and idle
            if (distanceToIdlePosition > 600f) {
                // Speed up the minion if it's away from the player
                speed = 16f;
                inertia = 60f;
            }
            else if (distanceToIdlePosition < 10f) {
                // Slow down the minion if close to the player
                speed = 0.5f;
                inertia = 2f;
            }
            else {
                // Medium speed for medium distance to the player
                speed = 4f;
                inertia = 20f;
            }
            if (distanceToIdlePosition > 5f) {
                // The immediate range around the player (when it passively floats about)

                // This is a simple movement formula using the two parameters and its desired direction to create a "homing" movement
                vectorToIdlePosition.Normalize();
                vectorToIdlePosition *= speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
            }
            else if (Projectile.velocity == Vector2.Zero) {
                // If there is a case where it's not moving at all, give it a little "poke"
                Projectile.velocity.X = -0.15f;
                Projectile.velocity.Y = -0.05f;
            }
        }
        #endregion

        Projectile.rotation = Projectile.velocity.X * 0.05f;
        Projectile.spriteDirection = -Projectile.direction;
    }

    public override void PostDraw(Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        if (dashTimer >= 40 && dashTrailTimer < Projectile.oldPos.Length)
            dashTrailTimer++;
        else if (dashTrailTimer > 0)
            dashTrailTimer--;
        Texture2D projectileTexture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Texture2D glowTexture = (Texture2D)ModContent.Request<Texture2D>(Texture + "_Glow");
        int frameHeight = glowTexture.Height / Main.projFrames[Projectile.type];
        Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, glowTexture.Width, frameHeight);
        Vector2 drawOrigin = new Vector2(projectileTexture.Width * 0.5f, Projectile.height * 0.5f);
        for (int k = 0; k < dashTrailTimer; k++) {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, -16f);
            Color color = new Color(255, 140, 0, 0) * ((dashTrailTimer - k) / (float)dashTrailTimer);
            spriteBatch.Draw(glowTexture, drawPos, frameRect, color, Projectile.rotation, drawOrigin, Projectile.scale * 1.1f, Projectile.direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        }
        if (dashTimer >= 40) {
            if (Projectile.direction < 0) {
                int dust2 = Dust.NewDust(new Vector2(Projectile.position.X + 8, Projectile.position.Y + 5), 6, 6, 6, 0f, 0f, 0, new Color(), Main.rand.NextFloat(0.85f, 1.1f));
                Main.dust[dust2].velocity *= 0.25f;
                Main.dust[dust2].noGravity = true;
            }
            else {
                int dust2 = Dust.NewDust(new Vector2(Projectile.position.X - 13 + Projectile.width, Projectile.position.Y + 5), 6, 6, 6, 0f, 0f, 0, new Color(), Main.rand.NextFloat(0.85f, 1.1f));
                Main.dust[dust2].velocity *= 0.25f;
                Main.dust[dust2].noGravity = true;
            }
        }
    }

    public override bool? CanDamage() => Projectile.velocity.Length() > 5f && Projectile.ai[1] > 0f;

    public override bool? CanCutTiles() => false;

    public override bool MinionContactDamage() => true;
}

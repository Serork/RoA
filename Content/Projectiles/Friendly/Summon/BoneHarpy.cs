using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Projectiles;
using RoA.Content.Items;
using RoA.Content.Items.Equipables.Armor.Summon;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class BoneHarpy : InteractableProjectile {
    private static Asset<Texture2D> _hoverTexture = null!;

    protected override Asset<Texture2D> HoverTexture => _hoverTexture;


    private const float DIST = 500f;

    public static readonly Color TrailColor = new(153, 134, 128);

    private static WorshipperBonehelm.BoneHarpyOptions GetHandler(Player player) => player.GetModPlayer<WorshipperBonehelm.BoneHarpyOptions>();
    private static bool IsInAttackMode(Player player) => !GetHandler(player).IsInIdle;
    private static bool IsBeingControlled(Player player) => GetHandler(player).RodeHarpy;

    private ref float TrailOpacity => ref Projectile.localAI[2];

    protected override void OnInteraction(Player player) {
        GetHandler(player).ToggleState(Projectile.whoAmI);
        Projectile.velocity = player.velocity;
        Projectile.velocity *= 0.5f;
        Projectile.ai[0] = Projectile.ai[1] = Projectile.ai[2] = 0f;
        Projectile.netUpdate = true;
    }

    protected override void OnHover(Player player) {
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<WingIcon>();
    }

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 6;

        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.TrailingMode[Type] = 0;

        ProjectileID.Sets.MinionSacrificable[Type] = false;

        if (Main.dedServ) {
            return;
        }

        _hoverTexture = ModContent.Request<Texture2D>(Texture + "_Hover");
    }

    public override void SetDefaults() {
        int width = 100; int height = 86;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = -1;
        Projectile.minion = false;
        Projectile.DamageType = DamageClass.Summon;

        Projectile.aiStyle = -1;

        Projectile.friendly = true;
        Projectile.tileCollide = false;
    }

    public override bool? CanDamage() => false;

    public override bool? CanCutTiles() => false;

    public override void SafeAI() {
        Player player = Main.player[Projectile.owner];

        if (player.Distance(Projectile.Center) > 1000f) {
            Projectile.Center = player.GetPlayerCorePoint();
        }

        if (IsBeingControlled(player)) {
            Vector2 center = player.Center - Vector2.UnitY * (player.height - 15);
            Projectile.Center = new Vector2((int)center.X, (int)center.Y) + new Vector2(0f, player.gfxOffY);

            RemoveTrails();

            WhileBeingActive(false, true, false);

            if (Math.Abs(player.velocity.X) > 0.05f) {
                Projectile.direction = player.velocity.X.GetDirection();
                Projectile.spriteDirection = -Projectile.direction;
            }

            return;
        }

        AttackNearTarget(out bool foundTarget);
        WhileBeingActive(foundTarget);
        if (foundTarget) {
            return;
        }
        HandleMovement(player);
    }

    private void RemoveTrails() {
        if (TrailOpacity > 0f) {
            TrailOpacity -= 0.2f;
        }
        else {
            TrailOpacity = 0f;
        }
    }

    private void AttackNearTarget(out bool foundTarget) {
        Player player = Main.player[Projectile.owner];
        bool flag = IsInAttackMode(player);
        if (!flag) {
            foundTarget = false;
            RemoveTrails();
            return;
        }
        if (TrailOpacity < 1f) {
            TrailOpacity += 0.1f;
        }
        else {
            TrailOpacity = 1f;
        }

        float neededDistance = DIST;
        float distanceFromTarget = neededDistance;
        Vector2 targetCenter = Projectile.position;
        foundTarget = false;
        NPC target = null;
        if (player.HasMinionAttackTargetNPC) {
            NPC npc = Main.npc[player.MinionAttackTargetNPC];
            float between = Vector2.Distance(npc.Center, Projectile.Center);
            if (between < 2000f) {
                distanceFromTarget = between;
                targetCenter = npc.Center;
                target = npc;
                foundTarget = true;
            }
        }
        if (!foundTarget) {
            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy()) {
                    float between = Vector2.Distance(npc.Center, Projectile.Center);
                    bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                    bool inRange = between < distanceFromTarget;
                    bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                    bool closeThroughWall = between < 100f;
                    if ((closest && inRange || !foundTarget) && (lineOfSight || closeThroughWall)) {
                        distanceFromTarget = between;
                        targetCenter = npc.Center;
                        foundTarget = true;
                        target = npc;
                    }
                }
            }
        }
        if (target == null) {
            foundTarget = false;
            return;
        }
        HandleMovement(target);

        if (target.Distance(Projectile.Center) > neededDistance / 2f) {
            return;
        }

        ref float attackTimer = ref Projectile.localAI[1];
        float attackTime = 40f;
        if (++attackTimer > attackTime) {
            if (attackTimer % 4f == 0f) {
                SoundEngine.PlaySound(SoundID.Item42, Projectile.Center);

                if (Projectile.owner == Main.myPlayer) {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(),
                        Projectile.Center,
                        Vector2.Normalize(target.Center - Projectile.Center) * 4.5f,
                        ModContent.ProjectileType<BoneHarpyFeather>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner);
                }
            }
            if (attackTimer >= attackTime + 12f) {
                attackTimer = 0f;
            }
        }
    }

    private void WhileBeingActive(bool foundTarget, bool applyPlayerRotation = false, bool applyDirection = true) {
        Player player = Main.player[Projectile.owner];
        bool flag = player.GetModPlayer<WorshipperBonehelm.BoneHarpyOptions>().ShouldBeActive();
        if (player.dead || !player.active || !flag) {
            Projectile.Kill();
        }
        if (flag) {
            Projectile.timeLeft = 2;
        }
        Projectile.rotation = (applyPlayerRotation ? player.velocity.X : Projectile.velocity.X) * 0.085f;
        float maxRotation = 0.2f;
        Projectile.rotation = MathHelper.Clamp(Projectile.rotation, -maxRotation, maxRotation);
        if (!foundTarget && applyDirection) {
            Projectile.spriteDirection = -Projectile.direction;
        }
    }

    private void HandleMovement(Entity target) {
        float baseSpeed = 2f;
        bool far = target.Distance(Projectile.Center) > baseSpeed * 100f;
        if (target.Distance(Projectile.Center) > baseSpeed * 100f) {
            float length = target.velocity.Length() * 0.5f;
            if (length > baseSpeed) {
                Projectile.localAI[0] = length;
            }
            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], Projectile.localAI[0], 1f);
        }
        else {
            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 0f, 0.035f);
        }
        float speed = baseSpeed + Projectile.ai[2];
        bool flag = target is Player;
        float speedOffsetX = flag ? 0.5f : 1f;
        float acceleration = 0.015f;
        float maxSpeed = speed;
        ref float velocityX = ref Projectile.ai[0];
        if (Projectile.Center.X >= target.Center.X && velocityX >= -maxSpeed - speedOffsetX) {
            velocityX -= speed;
        }
        else if (Projectile.Center.X <= target.Center.X && velocityX <= maxSpeed + speedOffsetX) {
            velocityX += speed;
        }
        Projectile.velocity.X += velocityX * acceleration;
        Projectile.velocity.X = MathHelper.Clamp(Projectile.velocity.X, -maxSpeed - speedOffsetX, maxSpeed + speedOffsetX);
        float speedOffsetY = 0f;
        float offsetY = flag ? target.height / 4f : 75f;
        if (!flag && !far) {
            Projectile.direction = Math.Sign(Projectile.Center.X - target.Center.X);
            Projectile.spriteDirection = Projectile.direction;
        }
        else {
            Projectile.direction = Projectile.velocity.X.GetDirection();
            Projectile.spriteDirection = -Projectile.direction;
        }
        ref float velocityY = ref Projectile.ai[1];
        if (Projectile.Center.Y >= target.Center.Y - offsetY && velocityY >= -maxSpeed * 2f - speedOffsetY) {
            velocityY -= speed;
        }
        else if (Projectile.Center.Y <= target.Center.Y - offsetY && velocityY <= maxSpeed + speedOffsetY) {
            velocityY += speed;
        }
        Projectile.velocity.Y += velocityY * acceleration;
        Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y, -maxSpeed - speedOffsetY, maxSpeed + speedOffsetY);
    }

    public override bool PreDraw(ref Color lightColor) {
        if (TrailOpacity > 0f) {
            Texture2D texture2D = Projectile.GetTexture();
            Vector2 origin = new Vector2(texture2D.Width * 0.5f, Projectile.height * 0.5f);
            int length = (int)(Projectile.oldPos.Length * TrailOpacity);
            for (int i = 1; i < length; i += 3) {
                Vector2 position = Projectile.oldPos[i] - Main.screenPosition + origin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor.MultiplyRGB(TrailColor)) * ((float)(Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                color *= TrailOpacity;
                Main.EntitySpriteDraw(texture2D, position, new Rectangle?(new Rectangle(0, Math.Max(0, (Projectile.frame - 1) * Projectile.height), Projectile.width, Projectile.height)), color, Projectile.rotation, origin, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
            }
        }
        return true;
    }

    public override void PostAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 4) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame >= 6) {
            Projectile.frame = 0;
        }
    }
}

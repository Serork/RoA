using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.NPCs;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility.Vanilla;

static class NPCUtils {
    public static void ResetTarget(this NPC npc) => npc.target = -1;

    public static Texture2D GetTexture(this NPC npc) => TextureAssets.Npc[npc.type].Value;

    public static Rectangle GetFrameBox(this NPC npc, int xFrame) {
        Texture2D tex = npc.GetTexture();
        return tex.Frame(xFrame, Main.npcFrameCount[npc.type], npc.frame.X, npc.frame.Y);
    }

    public static void QuickDraw(this NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor, Rectangle? frameBox = null, float scale = 1f, SpriteEffects? effect = null, float exRot = 0, float yOffset = 0f, float xOffset = 0f, Texture2D? texture = null) {
        Texture2D tex = texture ?? npc.GetTexture();
        Rectangle sourceRectangle = frameBox ?? npc.frame;
        spriteBatch.Draw(tex, npc.Center + Vector2.UnitY * yOffset + Vector2.UnitX * xOffset - screenPos, sourceRectangle, lightColor,
            npc.rotation + exRot, sourceRectangle.Size() / 2, npc.scale * scale, effect ?? npc.spriteDirection.ToSpriteEffects(), 0);
    }

    public static DrawData QuickDrawAsDrawData(this NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor, Rectangle? frameBox = null, float scale = 1f, SpriteEffects? effect = null, float exRot = 0, float yOffset = 0f, float xOffset = 0f, Texture2D? texture = null) {
        Texture2D tex = texture ?? npc.GetTexture();
        Rectangle sourceRectangle = frameBox ?? npc.frame;
        return new DrawData(tex, npc.Center + Vector2.UnitY * yOffset + Vector2.UnitX * xOffset - screenPos, sourceRectangle, lightColor,
            npc.rotation + exRot, sourceRectangle.Size() / 2, npc.scale * scale, effect ?? npc.spriteDirection.ToSpriteEffects(), 0);
    }

    public static NPCCommon GetCommon(this NPC npc) => npc.GetGlobalNPC<NPCCommon>();

    public static bool TryGetCommon(this NPC npc, out NPCCommon npcCommon) => npc.TryGetGlobalNPC(out npcCommon);

    public static bool CanBeChasedBy(NPC npc, object attacker = null, bool ignoreDontTakeDamage = false, bool checkForImmortals = true) {
        if (npc.active && npc.chaseable && npc.lifeMax > 5 && (!npc.dontTakeDamage || ignoreDontTakeDamage) && !npc.friendly)
            return !checkForImmortals || !npc.immortal;

        return false;
    }

    public static NPC? FindClosestNPC(Vector2 checkPosition, int checkDistance, bool shouldCheckForCollisions = true, bool shouldCheckForImmortals = true) {
        NPC? target = null;
        int neededDistance = checkDistance;
        foreach (NPC checkNPC in Main.ActiveNPCs) {
            if (!CanBeChasedBy(checkNPC, checkForImmortals: shouldCheckForImmortals)) {
                continue;
            }
            float distance = (checkPosition - checkNPC.Center).Length();
            if (distance < neededDistance && (!shouldCheckForCollisions || Collision.CanHitLine(checkPosition, 1, 1, checkNPC.Center, 1, 1))) {
                target = checkNPC;
            }
        }
        return target;
    }

    public static bool DamageNPCWithPlayerOwnedProjectile(NPC target, Projectile damageSourceAsProjectile, ref ushort immuneTime, ushort immuneTimeAfterHit = 10, Rectangle? damageSourceHitbox = null, int? direction = null) {
        if (target.dontTakeDamage || damageSourceAsProjectile.owner < 0) {
            return false;
        }
        bool canDamage = damageSourceAsProjectile.friendly && !target.friendly;
        if (!canDamage) {
            return false;
        }
        target.position += target.netOffset;
        int npcId = target.whoAmI;
        direction ??= damageSourceAsProjectile.direction;
        damageSourceHitbox ??= damageSourceAsProjectile.getRect();
        Player owner = damageSourceAsProjectile.GetOwnerAsPlayer();
        if (immuneTime == 0 && damageSourceHitbox.Value.Intersects(target.getRect())) {
            var modifiers = target.GetIncomingStrikeModifiers(damageSourceAsProjectile.DamageType, direction.Value);
            modifiers.ArmorPenetration += damageSourceAsProjectile.ArmorPenetration;
            bool crit = false;
            if (damageSourceAsProjectile.DamageType.UseStandardCritCalcs && Main.rand.Next(100) < damageSourceAsProjectile.CritChance) {
                crit = true;
            }

            int num26 = Item.NPCtoBanner(target.BannerID());
            if (num26 >= 0)
                Main.player[Main.myPlayer].lastCreatureHit = num26;
            if (Main.netMode != NetmodeID.Server) {
                owner.ApplyBannerOffenseBuff(target, ref modifiers);
            }
            damageSourceAsProjectile.StatusNPC(npcId);
            if (target.life > 5)
                owner.OnHit(target.Center.X, target.Center.Y, target);

            if (ProjectileID.Sets.ImmediatelyUpdatesNPCBuffFlags[damageSourceAsProjectile.type])
                target.UpdateNPC_BuffSetFlags(lowerBuffTime: false);

            float knockBack = damageSourceAsProjectile.knockBack;
            int? num34 = null;//direction;
            float num3 = knockBack;

            float num19 = 1000; // to reduce patches, set to 1000, and then turn it into a multiplier later
            float armorPenetrationPercent = 0f;
            modifiers.ScalingArmorPenetration += armorPenetrationPercent;
            modifiers.Knockback *= num3 / knockBack;
            modifiers.TargetDamageMultiplier *= num19 / 1000;
            if (num34 != null)
                modifiers.HitDirectionOverride = num34;
            var strike = modifiers.ToHitInfo(damageSourceAsProjectile.damage, crit, knockBack, damageVariation: true, luck: owner.luck);
            NPCKillAttempt attempt = new NPCKillAttempt(target);
            /*
            int num35 = ((!flag) ? ((int)nPC.StrikeNPCNoInteraction(num19, num3, num34, flag12)) : ((int)nPC.StrikeNPC(num19, num3, num34, flag12)));
            */
            int num35 = target.StrikeNPC(strike, noPlayerInteraction: false);
            if (attempt.DidNPCDie())
                owner.OnKillNPC(ref attempt, damageSourceAsProjectile);

            if (owner.accDreamCatcher && !target.HideStrikeDamage)
                owner.addDPS(num35);

            CombinedHooks.OnHitNPCWithProj(damageSourceAsProjectile, target, strike, num35);

            if (damageSourceAsProjectile.penetrate > 0) {
                damageSourceAsProjectile.penetrate--;
                if (damageSourceAsProjectile.penetrate == 0) {
                    target.position -= target.netOffset;
                    if (damageSourceAsProjectile.stopsDealingDamageAfterPenetrateHits) {
                        damageSourceAsProjectile.penetrate = -1;
                        damageSourceAsProjectile.damage = 0;
                    }
                }
            }

            damageSourceAsProjectile.numHits++;

            immuneTime = immuneTimeAfterHit;

            if (Main.netMode != NetmodeID.SinglePlayer) {
                NetMessage.SendStrikeNPC(target, strike);
            }

            return true;
        }
        target.position -= target.netOffset;

        return false;
    }
}

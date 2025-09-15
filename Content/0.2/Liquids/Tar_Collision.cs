using Microsoft.Xna.Framework;

using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;

using RoA.Common.Players;
using RoA.Content.Buffs;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Liquids;
sealed partial class Tar : ModLiquid {
    public partial void CollisionLoad() {
        CommonHandler.PreUpdateEvent += RoAPlayer_PreUpdateEvent;
        On_NPC.Collision_MoveWhileWet += On_NPC_Collision_MoveWhileWet;
        On_NPC.UpdateNPC_UpdateGravity += On_NPC_UpdateNPC_UpdateGravity;
        On_NPC.DelBuff += On_NPC_DelBuff;
        On_Player.DelBuff += On_Player_DelBuff;
        On_Projectile.UpdatePosition += On_Projectile_UpdatePosition;
        On_Item.MoveInWorld += On_Item_MoveInWorld;
    }

    public override bool PlayerLiquidMovement(Player player, bool fallThrough, bool ignorePlats) {
        int num = ((!player.onTrack) ? player.height : (player.height - 20));
        Vector2 vector = player.velocity;
        player.velocity = Collision.TileCollision(player.position, player.velocity, player.width, num, fallThrough, ignorePlats, (int)player.gravDir);
        Vector2 vector2 = player.velocity * PlayerMovementMultiplier; //We reuse the PlayerMovementMultiplier here for it to serve the same purpose
        if (player.velocity.X != vector.X) {
            vector2.X = player.velocity.X;
        }
        if (player.velocity.Y != vector.Y) {
            vector2.Y = player.velocity.Y;
        }
        player.position += vector2;
        player.TryFloatingInFluid();
        player.AddBuff(ModContent.BuffType<TarDebuff>(), 420);
        return false; //We return false as we do not want the normal liquid movement to execute after this hook/method
    }

    private void On_Item_MoveInWorld(On_Item.orig_MoveInWorld orig, Item self, float gravity, float maxFallSpeed, ref Vector2 wetVelocity, int i) {
        if (self.GetModdedWetArray()[LiquidLoader.LiquidType<Liquids.Tar>() - LiquidID.Count]) {
            gravity = 0.05f;
            maxFallSpeed = 3f;
            wetVelocity = self.velocity * 0.175f;
        }

        orig(self, gravity, maxFallSpeed, ref wetVelocity, i);
    }

    private void On_Projectile_UpdatePosition(On_Projectile.orig_UpdatePosition orig, Projectile self, Vector2 wetVelocity) {
        if (self.GetModdedWetArray()[LiquidLoader.LiquidType<Liquids.Tar>() - LiquidID.Count]) {
            wetVelocity *= 0.425f;
        }

        orig(self, wetVelocity);
    }

    private void On_Player_DelBuff(On_Player.orig_DelBuff orig, Player self, int b) {
        if ((self.onFire || self.onFire3) && self.GetModdedWetArray()[LiquidLoader.LiquidType<Liquids.Tar>() - LiquidID.Count] && (self.buffType[b] == 24 || self.buffType[b] == 323)) {
            return;
        }
        orig(self, b);
    }

    private void On_NPC_DelBuff(On_NPC.orig_DelBuff orig, NPC self, int buffIndex) {
        if (self.onFire && self.GetModdedWetArray()[LiquidLoader.LiquidType<Liquids.Tar>() - LiquidID.Count] && self.buffType[buffIndex] == 24) {
            return;
        }
        orig(self, buffIndex);
    }

    private void On_NPC_UpdateNPC_UpdateGravity(On_NPC.orig_UpdateNPC_UpdateGravity orig, NPC self) {
        orig(self);

        if (self.GetModdedWetArray()[LiquidLoader.LiquidType<Liquids.Tar>() - LiquidID.Count]) {
            if (!self.GravityIgnoresLiquid) {
                self.GravityMultiplier *= 0.3f;
                self.MaxFallSpeedMultiplier *= 0.4f;
            }
        }
    }

    private void On_NPC_Collision_MoveWhileWet(On_NPC.orig_Collision_MoveWhileWet orig, NPC self, Vector2 oldDryVelocity, float Slowdown) {
        if (self.GetModdedWetArray()[LiquidLoader.LiquidType<Liquids.Tar>() - LiquidID.Count]) {
            CustomLiquidCollision(self, oldDryVelocity, 0.175f);
            return;
        }

        orig(self, oldDryVelocity, Slowdown);
    }

    private void CustomLiquidCollision(NPC self, Vector2 oldDryVelocity, float Slowdown = 0.5f) {
        if (Collision.up)
            self.velocity.Y = 0.01f;

        if (Slowdown == 0.15f && !self.noGravity) {
            if (self.velocity.Y > self.gravity * 5f) {
                self.velocity.Y = self.gravity * 5f;
            }
        }
        Vector2 vector = self.velocity * Slowdown;
        if (self.velocity.X != oldDryVelocity.X) {
            vector.X = self.velocity.X;
            self.collideX = true;
        }

        if (self.velocity.Y != oldDryVelocity.Y) {
            vector.Y = self.velocity.Y;
            self.collideY = true;
        }

        self.oldPosition = self.position;
        self.oldDirection = self.direction;
        self.position += vector;
    }

    private void RoAPlayer_PreUpdateEvent(Player player) {
        if (player.GetModdedWetArray()[LiquidLoader.LiquidType<Tar>() - LiquidID.Count]) {
            player.gravity = 0.1f;
            player.maxFallSpeed = 3f;
            /*if (Player.IsLocal()) */
            {
                Player.jumpHeight = (int)(Player.jumpHeight * 0.75f);
                Player.jumpSpeed *= 0.75f;
            }
        }
    }

}

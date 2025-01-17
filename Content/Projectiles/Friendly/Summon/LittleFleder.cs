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
    private float _speed;

    public Item PickUpIHave { get; private set; }
    public Item ItemIFound { get; private set; }

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

        //float overlapVelocity = 0.04f;
        //for (int i = 0; i < Main.maxProjectiles; i++) {
        //    // Fix overlap with other minions
        //    Projectile other = Main.projectile[i];
        //    if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width / 1.5f) {
        //        if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
        //        else Projectile.velocity.X += overlapVelocity;

        //        if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
        //        else Projectile.velocity.Y += overlapVelocity;
        //    }
        //}

        float distanceFromTarget = 1000f;
        NPC target = null;
        Vector2 targetCenter = Projectile.position;
        bool foundTarget = false;

        // This code is required if your minion weapon has the targeting feature
        if (player.HasMinionAttackTargetNPC) {
            NPC npc = Main.npc[player.MinionAttackTargetNPC];
            float between = Vector2.Distance(npc.Center, Projectile.Center);
            // Reasonable distance away so it doesn't target across multiple screens
            if (between < distanceFromTarget) {
                distanceFromTarget = between;
                targetCenter = npc.Center;
                target = npc;
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
                        target = npc;
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

        Vector2 spawnPosition = Projectile.Center + Vector2.UnitY * 15f;
        float distance = Vector2.Distance(Projectile.Center, player.Center);
        bool foundPickUp = false;
        Vector2 pickUpPosition = Vector2.Zero;

        AI_GetMyGroupIndexAndFillBlackList(out var index, out var totalIndexesInGroup);

        int[] validItems = [ItemID.Heart, ItemID.Star, ModContent.ItemType<MagicHerb1>(), ModContent.ItemType<MagicHerb2>(), ModContent.ItemType<MagicHerb3>()];

        void searchForPickUps() {
            if (index == 0) {
                for (int j = 0; j < 400; j++) {
                    Item item = Main.item[j];
                    Player player = Main.player[Projectile.owner];
                    if (!item.active || item.shimmerTime != 0f || item.noGrabDelay != 0 || item.playerIndexTheItemIsReservedFor != player.whoAmI || !player.CanAcceptItemIntoInventory(item) || (item.shimmered && !((double)item.velocity.Length() < 0.2)))
                        continue;

                    if (item.Distance(player.Center) > 600f) {
                        continue;
                    }

                    if (!validItems.Contains(item.type)) {
                        continue;
                    }

                    Rectangle hitbox = item.Hitbox;
                    if (!Projectile.Hitbox.Intersects(hitbox)) {
                        pickUpPosition = item.Center;
                        foundPickUp = true;
                    }
                    else {
                        PickUpIHave = item;
                    }
                    ItemIFound = item;
                    break;
                }
            }
        }
        void pickUp() {
            if (index == 0) {
                for (int j = 0; j < 400; j++) {
                    Item item = Main.item[j];
                    Player player = Main.player[Projectile.owner];
                    if (!item.active || item.shimmerTime != 0f || item.noGrabDelay != 0 || item.playerIndexTheItemIsReservedFor != player.whoAmI || !player.CanAcceptItemIntoInventory(item) || (item.shimmered && !((double)item.velocity.Length() < 0.2)))
                        continue;

                    if (item.Distance(Projectile.Center) > 1000f) {
                        continue;
                    }

                    if (!validItems.Contains(item.type) || item.beingGrabbed) {
                        continue;
                    }

                    int itemGrabRange = GetGrabRange(player, item);
                    Rectangle hitbox = item.Hitbox;
                    //if (Projectile.Hitbox.Intersects(hitbox)) {

                    //}
                    //else
                    //{
                    if (!new Rectangle((int)Projectile.position.X - itemGrabRange, (int)Projectile.position.Y - itemGrabRange, Projectile.width + itemGrabRange * 2, Projectile.height + itemGrabRange * 2).Intersects(hitbox))
                        continue;

                    if (ItemIFound == item) {
                        Player.ItemSpaceStatus status = player.ItemSpace(item);
                        if (player.CanPullItem(item, status)) {
                            item.shimmered = false;
                            item.beingGrabbed = true;

                            Item itemToPickUp = item;
                            if (itemToPickUp.Distance(spawnPosition) < 15f) {
                                itemToPickUp.Center = spawnPosition;
                                itemToPickUp.velocity = Vector2.Zero;
                            }
                            else {
                                float speed = 2.5f;
                                int acc = 2;
                                Vector2 vector = new Vector2(itemToPickUp.position.X + (float)(itemToPickUp.width / 2), itemToPickUp.position.Y + (float)(itemToPickUp.height / 2));
                                float num = spawnPosition.X - vector.X;
                                float num2 = spawnPosition.Y - vector.Y;
                                float num3 = (float)Math.Sqrt(num * num + num2 * num2);
                                num3 = speed / num3;
                                num *= num3;
                                num2 *= num3;
                                itemToPickUp.velocity.X = (itemToPickUp.velocity.X * (float)(acc - 1) + num) / (float)acc;
                                itemToPickUp.velocity.Y = (itemToPickUp.velocity.Y * (float)(acc - 1) + num2) / (float)acc;
                            }
                        }
                    }
                }
            }
        }
        bool flag1 = ItemIFound != null;
        bool flag2 = PickUpIHave != null;
        bool flag3 = !foundPickUp && !flag2 && !flag1;
        searchForPickUps();
        if (foundPickUp) {
            flyTo(destination2: pickUpPosition);
            Projectile.ai[1] = -1f;
        }
        pickUp();

        if (PickUpIHave != null && PickUpIHave.Distance(player.Center) < 35f) {
            PickUpIHave = null;
            ItemIFound = null;
        }
        if (flag2 && AttackTimer > 0f) {
            AttackTimer = 0f;
        }
        if (flag1 || foundPickUp) {
            if (AttackTimer > 0f) {
                AttackTimer -= 5f;
            }
        }
        else {
            if (AttackTimer < ATTACKRATE) {
                AttackTimer += 1f;
            }
        }

        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.X * 0.085f, 0.1f);
        Projectile.rotation = MathHelper.Clamp(Projectile.rotation, -0.2f, 0.2f);

        bool flag5 = flag2 || foundPickUp;
        float time = flag5 ? 10f : 0f;
        bool flag4 = false;
        if (Math.Abs(Projectile.velocity.X) > 1f || flag5) {
            if (foundPickUp) {
                ChangeDirection(Projectile.velocity.X.GetDirection(), time);
                flag4 = true;
            }
            else if (flag5) {
                ChangeDirection(-(Projectile.Center.X - player.Center.X).GetDirection(), time);
            }
        }
        if (flag3 && !flag4) {
            ChangeDirection(-(Projectile.Center.X - player.Center.X).GetDirection(), time);
        }
        if (foundTarget && !flag5) {
            ChangeDirection(-(Projectile.Center.X - target.Center.X).GetDirection(), time);
        }

        if (!flag2) {
            ItemIFound = null;
        }

        Projectile.spriteDirection = -Projectile.direction;

        void flyTo(Entity to = null, Vector2? destination2 = null) {
            Projectile.ai[0] += 1f;

            Vector2 destination = player.Center;
            bool flag = to != null;
            if (flag) {
                destination = to.Center;
            }
            bool flag2 = destination2 != null;
            if (flag2) {
                destination = destination2.Value;
            }
            int direction = 1;
            if (flag) {
                direction = to.direction;
            }
            if (foundTarget || flag2) {
                //Projectile.direction = -(Projectile.Center.X - destination.X).GetDirection();
                //Projectile.spriteDirection = -Projectile.direction;
            }
            Vector2 offset = new Vector2(-MathHelper.Lerp(5f, 15f, Utils.Clamp((float)Math.Sin(Projectile.ai[0] * 0.25f), 0, 1)) * Projectile.direction).RotatedBy(MathHelper.ToRadians(Projectile.ai[0] * Projectile.direction));
            Vector2 levitation = Vector2.UnitY * offset.Y + Vector2.UnitX * offset.X * 0.25f;
            Vector2 offset2 = new(50f * (Projectile.Center.X - player.Center.X).GetDirection(), -15f);
            Vector2 positionTo = destination + (!flag3 ? offset2 : new Vector2(-(35f + 50f * Projectile.minionPos) * direction, -25f)) + levitation;
            if (foundTarget && flag3) {
                AI_156_GetIdlePosition(destination, index, totalIndexesInGroup, out var idleSpot, out var idleRotation);
                positionTo = idleSpot + levitation;
            }

            float inertia = 15f;
            distance = Vector2.Distance(Projectile.Center, positionTo);
            Vector2 dif = positionTo - Projectile.Center;
            if (dif.Length() < 0.0001f) {
                dif = Vector2.Zero;
            }
            else {
                float speed = 35f;
                if (distance < 1000f) {
                    speed = MathHelper.Lerp(5f, 10f, distance / 1000f);
                }
                if (distance < 100f) {
                    speed = MathHelper.Lerp(0.1f, 5f, distance / 100f);
                }
                dif.Normalize();
                _speed = MathHelper.Lerp(_speed, speed, !flag3 ? 0.01f : 0.1f);
                dif *= _speed;
            }
            Projectile.velocity = (Projectile.velocity * (inertia - 1) + dif) / inertia;
            if (Projectile.velocity.Length() > 5f) {
            }
            else {
                Projectile.velocity *= (float)Math.Pow(0.99, inertia * 2.0 / inertia);
                if (distance > 50f) {
                    Projectile.velocity += Projectile.DirectionTo(destination) * distance / 100f * 0.1f;
                }
            }
        }
        if (!foundPickUp || flag2) {
            if (!foundTarget || flag2) {
                flyTo(player);
                Projectile.ai[1] = 0f;
            }
            else {
                Projectile.ai[1] = 1f;

                Lighting.AddLight(Projectile.Top + Vector2.UnitY * Projectile.height * 0.1f, new Vector3(1f, 0.2f, 0.2f) * 0.75f);

                flyTo(target);
                if (AttackTimer >= ATTACKRATE) {
                    AttackTimer = 0f;

                    if (Projectile.owner == Main.myPlayer) {
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPosition,
                            Helper.VelocityToPoint(spawnPosition, target.Center, 7.5f),
                            ModContent.ProjectileType<Acorn>(),
                            Projectile.damage,
                            Projectile.knockBack,
                            Projectile.owner,
                            ai2: target.whoAmI);
                    }
                }
            }
        }

        if (distance > 2000f) {
            Projectile.Center = player.Center;
            Projectile.velocity *= 0.95f;
            Projectile.netUpdate = true;
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Cache;
using RoA.Content.Items.Miscellaneous;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Summon;

sealed class LittleFleder : ModProjectile {
    private static Asset<Texture2D> _acornTexture = null!,
                                    _glowTexture = null!;

    private const float ATTACKRATE = 35f;

    private bool _hasTarget;
    private float _canChangeDirectionAgain = 20f;
    private bool _foundPickUp;
    private Vector2 _pickUpPosition;
    private Item _pickUpIFound;
    private bool _nowGoToPlayer;
    private bool _havePickUp;
    private float _havePickUpTimer;

    private ref float AttackTimer => ref Projectile.ai[1];

    private float AcornOpacity => _havePickUp ? 0f : Utils.GetLerpValue(ATTACKRATE / 4f, ATTACKRATE / 2f, AttackTimer, true);

    public override void SetStaticDefaults() {
        Main.projFrames[Projectile.type] = 4;
        Main.projPet[Projectile.type] = true;

        ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;

        if (Main.dedServ) {
            return;
        }

        _acornTexture = ModContent.Request<Texture2D>(Texture + "_Acorn");
        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
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

    private static int GetGrabRange(Player player, Item item) => player.GetItemGrabRange(item) * 2;

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        SpriteEffects spriteEffects = (SpriteEffects)(Projectile.spriteDirection != 1).ToInt();
        int height = texture.Height / Main.projFrames[Projectile.type];
        Rectangle sourceRectangle = new(0, height * Projectile.frame, texture.Width, height);
        Vector2 origin = sourceRectangle.Size() / 2f;
        Vector2 position = Projectile.Center - Main.screenPosition;
        texture = _acornTexture.Value;
        float progress = Math.Abs(Projectile.rotation) / MathHelper.PiOver2 * Projectile.spriteDirection;
        sourceRectangle = new(0, 0, texture.Width, texture.Height);
        spriteBatch.Draw(texture,
            position +
            new Vector2(-8f - (Projectile.spriteDirection == -1 ? 4f : 0f), 10f + (Projectile.spriteDirection == 1 ? 16f * progress : 0f)), sourceRectangle,
            lightColor * AcornOpacity, Projectile.rotation * 0.5f + MathHelper.Pi, origin / 2f, Projectile.scale, spriteEffects, 0);
        texture = Projectile.GetTexture();
        position = Projectile.Center - Main.screenPosition;
        sourceRectangle = new(0, height * Projectile.frame, texture.Width, height);
        Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects);

        if (_hasTarget) {
            texture = _glowTexture.Value;
            SpriteBatchSnapshot snapshot = spriteBatch.CaptureSnapshot();
            spriteBatch.Begin(snapshot with { blendState = BlendState.Additive }, true);
            float lifeProgress = 1f;
            for (float i = -MathHelper.Pi; i <= MathHelper.Pi; i += MathHelper.PiOver2) {
                spriteBatch.Draw(texture, position +
                    Utils.RotatedBy(Utils.ToRotationVector2(i), TimeSystem.TimeForVisualEffects * 10.0, new Vector2())
                    * Helper.Wave(0f, 3f, 12f, 0.5f) * lifeProgress,
                   sourceRectangle, Color.White.MultiplyAlpha(Helper.Wave(0.5f, 0.75f, 12f, 0.5f)) * lifeProgress, Projectile.rotation + Main.rand.NextFloatRange(0.05f) * lifeProgress, origin, Projectile.scale, spriteEffects, 0f);
            }
            spriteBatch.Begin(snapshot, true);
        }

        return false;
    }

    public override void AI() {
        Projectile.localAI[2] += TimeSystem.LogicDeltaTime;

        if (--_havePickUpTimer <= 0f) {
            _havePickUp = _foundPickUp || _nowGoToPlayer;
            _havePickUpTimer = 10f;
        }

        Player player = Main.player[Projectile.owner];
        if (player.dead || !player.active)
            player.ClearBuff(ModContent.BuffType<Buffs.LittleFleder>());
        if (player.HasBuff(ModContent.BuffType<Buffs.LittleFleder>()))
            Projectile.timeLeft = 2;

        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.X * 0.085f, 0.1f);
        Projectile.rotation = MathHelper.Clamp(Projectile.rotation, -0.2f, 0.2f);

        float overlapVelocity = 0.04f;
        for (int i = 0; i < Main.maxProjectiles; i++) {
            Projectile other = Main.projectile[i];
            if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width / 1.5f) {
                if (Projectile.position.X < other.position.X) Projectile.velocity.X -= overlapVelocity;
                else Projectile.velocity.X += overlapVelocity;

                if (Projectile.position.Y < other.position.Y) Projectile.velocity.Y -= overlapVelocity;
                else Projectile.velocity.Y += overlapVelocity;
            }
        }

        List<int> validItems = [];
        validItems.Add(ItemID.Heart);
        validItems.Add(ModContent.ItemType<MagicHerb1>());
        validItems.Add(ModContent.ItemType<MagicHerb2>());
        validItems.Add(ModContent.ItemType<MagicHerb3>());
        validItems.Add(ModContent.ItemType<AvengingSoul>());
        validItems.Add(ItemID.Star);
        bool validPickUp(Item item) {
            if ((item.type == validItems[0] ||
                item.type == validItems[1] ||
                item.type == validItems[2] ||
                item.type == validItems[3]) && player.statLife >= player.statLifeMax2 * 0.9f) {
                return false;
            }

            if (item.type == validItems[4] && player.statMana >= player.statManaMax2 * 0.9f) {
                return false;
            }

            return true;
        }

        void searchForPickUps() {
            float minDistance = float.MaxValue;
            Vector2 to = Projectile.Center;
            for (int j = 0; j < 400; j++) {
                Item item = Main.item[j];
                Player player = Main.player[Projectile.owner];
                if (!item.active || item.shimmerTime != 0f || item.noGrabDelay != 0 || item.playerIndexTheItemIsReservedFor != player.whoAmI || !player.CanAcceptItemIntoInventory(item) || (item.shimmered && !((double)item.velocity.Length() < 0.2)))
                    continue;

                if (item.Distance(Projectile.Center) > 600f) {
                    continue;
                }

                if (!validItems.Contains(item.type)) {
                    continue;
                }

                if (!validPickUp(item)) {
                    continue;
                }

                bool flag = false;
                foreach (Projectile projectile in Main.ActiveProjectiles) {
                    if (projectile.owner != Projectile.owner) {
                        continue;
                    }
                    if (projectile.type != Projectile.type) {
                        continue;
                    }
                    if (projectile.whoAmI != Projectile.whoAmI && projectile.As<LittleFleder>()._pickUpIFound == item) {
                        flag = true;
                        break;
                    }
                }
                if (flag) {
                    continue;
                }

                float distance = Vector2.Distance(item.Center, Projectile.Center);
                if (distance < minDistance) {
                    minDistance = distance;
                    to = item.Center - Vector2.UnitY * item.height * 3f;
                }

                _pickUpIFound = item;
            }

            if (_pickUpIFound != null) {
                _pickUpPosition = to;
                _foundPickUp = true;
            }
        }

        searchForPickUps();

        float num = 0f;
        float num2 = 0f;
        float num3 = 20f;
        float num4 = 40f;
        float num5 = 0.69f;

        Vector2 spawnPosition = Projectile.Bottom;
        if (_foundPickUp && _pickUpIFound != null && !_nowGoToPlayer) {
            Vector2 v2 = _pickUpPosition - Projectile.Center;
            float distanceBetweenTargetAndMe2 = v2.Length();
            v2 = v2.SafeNormalize(Vector2.Zero);
            if (distanceBetweenTargetAndMe2 > 25f) {
                float acceleration = 7.5f + num2 * num;
                v2 *= acceleration;
                float speed = num3;
                Projectile.velocity.X = (Projectile.velocity.X * speed + v2.X) / (speed + 1f);
                Projectile.velocity.Y = (Projectile.velocity.Y * speed + v2.Y) / (speed + 1f);

                if (++_canChangeDirectionAgain > 20f) {
                    if ((_pickUpIFound.Center - Projectile.Center).X > 0f)
                        Projectile.spriteDirection = (Projectile.direction = -1);
                    else if ((_pickUpIFound.Center - Projectile.Center).X < 0f)
                        Projectile.spriteDirection = (Projectile.direction = 1);
                    _canChangeDirectionAgain = 0f;
                }
            }
            else {
                Projectile.velocity *= 0.9f;
                pickUp();
                if (_pickUpIFound != null && _pickUpIFound.Distance(spawnPosition) < 15f) {
                    _nowGoToPlayer = true;
                }
            }

            bool flag5 = false;
            foreach (Item item in Main.ActiveItems) {
                if (_pickUpIFound == item) {
                    flag5 = true;
                }
            }
            if (!flag5 || !validPickUp(_pickUpIFound)) {
                _pickUpIFound = null;
                _foundPickUp = false;
            }

            return;
        }

        void pickUp() {
            Item item = _pickUpIFound;
            if (item != null) {
                int itemGrabRange = GetGrabRange(player, item);
                Rectangle hitbox = _pickUpIFound.Hitbox;
                if (new Rectangle((int)Projectile.position.X - itemGrabRange, (int)Projectile.position.Y - itemGrabRange, Projectile.width + itemGrabRange * 2, Projectile.height + itemGrabRange * 2).Intersects(hitbox)) {
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
                            float numm = spawnPosition.X - vector.X;
                            float numm2 = spawnPosition.Y - vector.Y;
                            float numm3 = (float)Math.Sqrt(numm * numm + numm2 * numm2);
                            numm3 = speed / numm3;
                            numm *= numm3;
                            numm2 *= numm3;
                            itemToPickUp.velocity.X = (itemToPickUp.velocity.X * (float)(acc - 1) + numm) / (float)acc;
                            itemToPickUp.velocity.Y = (itemToPickUp.velocity.Y * (float)(acc - 1) + numm2) / (float)acc;
                        }
                    }
                }
            }
        }

        if (_nowGoToPlayer) {
            pickUp();
            if (_pickUpIFound == null || !_pickUpIFound.active || _pickUpIFound.Distance(player.Center) < 50f || _pickUpIFound.Distance(Projectile.Center) > 25f) {
                _pickUpIFound = null;
                _nowGoToPlayer = false;
                _foundPickUp = false;
                _pickUpPosition = Projectile.Center;
            }
        }

        Vector2 targetCenter = Projectile.position;
        float distanceToTarget = 400f;

        distanceToTarget = 2000f;

        bool hasTarget = false;
        int targetWhoAmI = -1;
        Projectile.tileCollide = true;

        Vector2 center = Main.player[Projectile.owner].Center;
        Vector2 vector2 = new Vector2(0.5f);
        vector2.Y = 0f;

        NPC ownerMinionAttackTargetNPC = Projectile.OwnerMinionAttackTargetNPC;
        if (ownerMinionAttackTargetNPC != null && ownerMinionAttackTargetNPC.CanBeChasedBy(this)) {
            Vector2 vector3 = ownerMinionAttackTargetNPC.position + ownerMinionAttackTargetNPC.Size * vector2;
            float num14 = distanceToTarget * 3f;
            float num15 = Vector2.Distance(vector3, center);
            if (num15 < num14 && !hasTarget && Collision.CanHit(Projectile.Center, 1, 1, ownerMinionAttackTargetNPC.Center, 1, 1)) {
                distanceToTarget = num15;
                targetCenter = vector3;
                hasTarget = true;
                targetWhoAmI = ownerMinionAttackTargetNPC.whoAmI;
            }
        }

        AI_GetMyGroupIndexAndFillBlackList(out var index, out var totalIndexesInGroup);

        if (!hasTarget) {
            for (int n = 0; n < 200; n++) {
                NPC nPC = Main.npc[n];
                if (nPC.CanBeChasedBy(this)) {
                    Vector2 vector4 = nPC.position + nPC.Size * vector2;
                    float num16 = Vector2.Distance(vector4, center);
                    if (!(num16 >= distanceToTarget) && Collision.CanHit(Projectile.Center, 1, 1, nPC.Center, 1, 1)) {
                        distanceToTarget = num16;
                        targetCenter = vector4;
                        hasTarget = true;
                        targetWhoAmI = n;
                    }
                }
            }
        }
        //else {
        //    AI_156_GetIdlePosition(targetCenter, index, totalIndexesInGroup, out var idleSpot, out var idleRotation, offset: 20f);
        //    targetCenter = idleSpot;
        //}

        if (_nowGoToPlayer || _havePickUp) {
            hasTarget = false;
        }

        _hasTarget = hasTarget;

        Projectile.position.Y += Helper.Wave(Projectile.localAI[2], -1f, 1f, 2.5f, Projectile.identity) * 0.25f;

        int num21 = 500;
        if (hasTarget)
            num21 = 1000;

        Vector2 v = targetCenter - Projectile.Center;
        float distanceBetweenTargetAndMe = v.Length();

        if (Vector2.Distance(player.Center, Projectile.Center) > (float)num21) {
            Projectile.ai[0] = 1f;
            Projectile.netUpdate = true;
        }

        if (Projectile.ai[0] == 1f)
            Projectile.tileCollide = false;

        if (Projectile.ai[0] >= 2f) {
            Projectile.ai[0] += 1f;

            if (!hasTarget)
                Projectile.ai[0] += 1f;

            if (Projectile.ai[0] > num4) {
                Projectile.ai[0] = 0f;
                Projectile.netUpdate = true;
            }

            Projectile.velocity *= num5;
        }
        else if (hasTarget && Projectile.ai[0] == 0f) {
            AI_156_GetIdlePosition(targetCenter, index, totalIndexesInGroup, out var idleSpot, out var idleRotation, offset: 75f);
            idleSpot.Y += 160f;
            v = idleSpot - Projectile.Center;
            distanceBetweenTargetAndMe = v.Length();
            v = v.SafeNormalize(Vector2.Zero);

            if ((targetCenter - Projectile.Center).Length() < 100f) {
                Projectile.velocity.Y -= 0.075f;
            }
            else {
                Projectile.velocity.Y *= 0.95f;
            }

            if (Projectile.Bottom.Y > targetCenter.Y) {
                Projectile.velocity.Y -= 0.125f;
            }

            if (distanceBetweenTargetAndMe > 200f) {
                float acceleration = 7.5f + num2 * num;
                v *= acceleration;
                float speed = num3;
                Projectile.velocity.X = (Projectile.velocity.X * speed + v.X) / (speed + 1f);
                Projectile.velocity.Y = (Projectile.velocity.Y * speed + v.Y) / (speed + 1f);
            }
            else {
                if (distanceBetweenTargetAndMe < 150f) {
                    float acceleration = 5f;
                    v *= 0f - acceleration;
                    float speed = 40f;
                    Projectile.velocity.X = (Projectile.velocity.X * speed + v.X) / (speed + 1f);
                    Projectile.velocity.Y = (Projectile.velocity.Y * speed + v.Y) / (speed + 1f);
                }
                else {
                    Projectile.velocity *= 0.97f;
                }
            }
        }
        else {
            if (!Collision.CanHitLine(Projectile.Center, 1, 1, Main.player[Projectile.owner].Center, 1, 1))
                Projectile.ai[0] = 1f;

            float num31 = 6f;
            if (Projectile.ai[0] == 1f)
                num31 = 15f;

            Vector2 center2 = Projectile.Center;
            AI_156_GetIdlePosition(player.GetPlayerCorePoint(), index, totalIndexesInGroup, out var idleSpot, out var idleRotation, offset: 50f);
            Vector2 v2 = idleSpot - center2;

            Projectile.ai[1] = ATTACKRATE;

            Projectile.netUpdate = true;
            v2 = idleSpot - center2;
            int num32 = 1;
            for (int num33 = 0; num33 < Projectile.whoAmI; num33++) {
                if (Main.projectile[num33].active && Main.projectile[num33].owner == Projectile.owner && Main.projectile[num33].type == Projectile.type)
                    num32++;
            }

            if (++_canChangeDirectionAgain > 20f) {
                if ((player.Center - Projectile.Center).X > 0f)
                    Projectile.spriteDirection = (Projectile.direction = -1);
                else if ((player.Center - Projectile.Center).X < 0f)
                    Projectile.spriteDirection = (Projectile.direction = 1);
                _canChangeDirectionAgain = 0f;
            }

            float num34 = v2.Length();
            if (num34 > 200f && num31 < 9f)
                num31 = 9f;

            num31 = (int)((double)num31 * 0.85);

            if (num34 < 100f && Projectile.ai[0] == 1f && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)) {
                Projectile.ai[0] = 0f;
                Projectile.netUpdate = true;
            }

            if (num34 > 2000f) {
                Projectile.position.X = Main.player[Projectile.owner].Center.X - (float)(Projectile.width / 2);
                Projectile.position.Y = Main.player[Projectile.owner].Center.Y - (float)(Projectile.width / 2);
            }

            if (num34 > 10f) {
                v2 = v2.SafeNormalize(Vector2.Zero);
                if (num34 < 50f)
                    num31 /= 2f;

                v2 *= num31;
                Projectile.velocity = (Projectile.velocity * 20f + v2) / 21f;
            }
            else {
                Projectile.direction = Main.player[Projectile.owner].direction;
                Projectile.velocity *= 0.9f;
            }
        }

        bool canAttack = AttackTimer > ATTACKRATE;
        if (AttackTimer > 0f && !canAttack) {
            AttackTimer += 1f;
        }
        if (distanceBetweenTargetAndMe < num21 / 2f) {
            if (canAttack) {
                AttackTimer = 0f;
                Projectile.netUpdate = true;
            }
        }

        if (Projectile.ai[0] != 0f)
            return;

        float num45 = 0f;
        int num46 = 0;

        num45 = 8f;
        num46 = ModContent.ProjectileType<Acorn>();

        if (!hasTarget)
            return;

        if ((targetCenter - Projectile.Center).X > 0f)
            Projectile.spriteDirection = (Projectile.direction = -1);
        else if ((targetCenter - Projectile.Center).X < 0f)
            Projectile.spriteDirection = (Projectile.direction = 1);

        if (AttackTimer == 0f) {
            Vector2 v5 = targetCenter - Projectile.Center;
            AttackTimer += 1f;
            Vector2 position2 = Projectile.Bottom;
            if (Main.myPlayer == Projectile.owner) {
                v5 = v5.SafeNormalize(Vector2.Zero);
                v5 *= num45;
                int num51 = Projectile.NewProjectile(Projectile.GetSource_FromThis(), position2.X, position2.Y, v5.X, v5.Y, num46, Projectile.damage, Projectile.knockBack, Main.myPlayer
                    , ai2: targetWhoAmI);
                Projectile.netUpdate = true;
            }
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

    private void AI_156_GetIdlePosition(Vector2 destination, int stackedIndex, int totalIndexes, out Vector2 idleSpot, out float idleRotation, float offset = 20f) {
        bool num = true;
        idleRotation = 0f;
        idleSpot = Vector2.Zero;
        if (num) {
            float num2 = ((float)totalIndexes - 1f) / 2f;
            idleSpot = destination + -Vector2.UnitY.RotatedBy(2f / (float)totalIndexes * ((float)stackedIndex - num2)) * offset;
            idleRotation = 0f;
        }
    }

    public override bool? CanDamage() => false;

    public override bool? CanCutTiles() => false;

    public override bool MinionContactDamage() => true;
}

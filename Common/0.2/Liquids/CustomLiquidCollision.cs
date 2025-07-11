using Microsoft.Xna.Framework;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Liquids;

sealed class CustomLiquidCollision_Item : GlobalItem {
    public bool permafrostWet, tarWet;
    public bool wet;
    public byte wetCount;

    public override void Load() {
        On_Item.MoveInWorld += On_Item_MoveInWorld;
    }

    private void On_Item_MoveInWorld(On_Item.orig_MoveInWorld orig, Item self, float gravity, float maxFallSpeed, ref Vector2 wetVelocity, int i) {
        var handler = self.GetGlobalItem<CustomLiquidCollision_Item>();
        bool num4 = Collision.WetCollision(self.position, self.width, self.height);
        if (CustomLiquidCollision_Player.tarCollision)
            handler.tarWet = true;

        if (Collision.shimmer)
            handler.permafrostWet = true;

        ushort tarDustType = (ushort)ModContent.DustType<Content.Dusts.Tar>(),
               permafrostDustType = (ushort)ModContent.DustType<Content.Dusts.Permafrost>();

        if (num4) {
            if (!handler.wet) {
                if (handler.wetCount == 0) {
                    handler.wetCount = 20;

                    if (handler.tarWet) {
                        for (int n = 0; n < 5; n++) {
                            int num8 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, tarDustType);
                            Main.dust[num8].velocity.Y -= 1.5f;
                            Main.dust[num8].velocity.X *= 2.5f;
                            Main.dust[num8].scale = 1.3f;
                            Main.dust[num8].alpha = 100;
                            Main.dust[num8].noGravity = true;
                        }

                        SoundEngine.PlaySound(SoundID.Splash, self.position);
                    }
                    else if (handler.permafrostWet) {
                        for (int n = 0; n < 5; n++) {
                            int num8 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, permafrostDustType);
                            Main.dust[num8].velocity.Y -= 1.5f;
                            Main.dust[num8].velocity.X *= 2.5f;
                            Main.dust[num8].scale = 1.3f;
                            Main.dust[num8].alpha = 100;
                            Main.dust[num8].noGravity = true;
                        }

                        SoundEngine.PlaySound(SoundID.Splash, self.position);
                    }
                }

                handler.wet = true;
            }
        }
        else if (handler.wet) {
            handler.wet = false;
            if (handler.wetCount == 0) {
                handler.wetCount = 20;

                if (handler.tarWet) {
                    for (int num15 = 0; num15 < 5; num15++) {
                        int num16 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, tarDustType);
                        Main.dust[num16].velocity.Y -= 1.5f;
                        Main.dust[num16].velocity.X *= 2.5f;
                        Main.dust[num16].scale = 1.3f;
                        Main.dust[num16].alpha = 100;
                        Main.dust[num16].noGravity = true;
                    }

                    SoundEngine.PlaySound(SoundID.Splash, self.position);
                }
                else if (handler.permafrostWet) {
                    for (int num15 = 0; num15 < 5; num15++) {
                        int num16 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, permafrostDustType);
                        Main.dust[num16].velocity.Y -= 1.5f;
                        Main.dust[num16].velocity.X *= 2.5f;
                        Main.dust[num16].scale = 1.3f;
                        Main.dust[num16].alpha = 100;
                        Main.dust[num16].noGravity = true;
                    }

                    SoundEngine.PlaySound(SoundID.Splash, self.position);
                }
            }
        }

        if (!handler.wet) {
            handler.permafrostWet = false;
            handler.tarWet = false;
        }

        if (handler.tarWet || handler.permafrostWet) {
            if (self.wetCount == 0) {
                self.wetCount = 1;
            }

            self.lavaWet = false;
            self.honeyWet = false;
            self.shimmerWet = false;
        }

        if (handler.wetCount > 0) {
            handler.wetCount--;
        }

        if (handler.tarWet || handler.permafrostWet) {
            if (self.wetCount == 0) {
                self.wetCount = 1;
            }
        }

        if (handler.tarWet) {
            gravity = 0.05f;
            maxFallSpeed = 3f;
            wetVelocity = self.velocity * 0.15f;
        }

        orig(self, gravity, maxFallSpeed, ref wetVelocity, i);
    }

    public override bool InstancePerEntity => true;

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed) {

    }
}

sealed class CustomLiquidCollision_Projectile : GlobalProjectile {
    public bool permafrostWet, tarWet;
    public bool wet;
    public byte wetCount;

    public override void Load() {
        On_Projectile.UpdatePosition += On_Projectile_UpdatePosition;
    }

    private void On_Projectile_UpdatePosition(On_Projectile.orig_UpdatePosition orig, Projectile self, Vector2 wetVelocity) {
        var handler = self.GetGlobalProjectile<CustomLiquidCollision_Projectile>();
        if (handler.tarWet) {
            wetVelocity *= 0.4f;
        }
        else if (handler.permafrostWet) {

        }

        orig(self, wetVelocity);
    }

    public override bool InstancePerEntity => true;

    public override void AI(Projectile projectile) {
        var handler = projectile.GetGlobalProjectile<CustomLiquidCollision_Projectile>();
        if (!projectile.ignoreWater) {
            bool flag2;
            try {
                flag2 = Collision.WetCollision(projectile.position, projectile.width, projectile.height);

                if (CustomLiquidCollision_Player.tarCollision)
                    handler.tarWet = true;

                if (CustomLiquidCollision_Player.permafrostCollision)
                    handler.permafrostWet = true;
            }
            catch {
                projectile.active = false;
                return;
            }

            ushort tarDustType = (ushort)ModContent.DustType<Content.Dusts.Tar>(),
                   permafrostDustType = (ushort)ModContent.DustType<Content.Dusts.Permafrost>();

            if (flag2) {
                if (handler.wetCount == 0 && !handler.wet) {
                    if (handler.tarWet) {
                        for (int num7 = 0; num7 < 10; num7++) {
                            int num8 = Dust.NewDust(new Vector2(projectile.position.X - 6f, projectile.position.Y + (float)(projectile.height / 2) - 8f), projectile.width + 12, 24, tarDustType);
                            Main.dust[num8].velocity.Y -= 1.5f;
                            Main.dust[num8].velocity.X *= 2.5f;
                            Main.dust[num8].scale = 1.3f;
                            Main.dust[num8].alpha = 100;
                            Main.dust[num8].noGravity = true;
                        }

                        SoundEngine.PlaySound(SoundID.Splash, projectile.position);
                    }
                }

                handler.wet = true;
            }
            else if (handler.wet) {
                handler.wet = false;
                if (projectile.type == 155) {
                }
                else if (handler.wetCount == 0) {
                    handler.wetCount = 10;
                    if (handler.tarWet) {
                        for (int num15 = 0; num15 < 10; num15++) {
                            int num16 = Dust.NewDust(new Vector2(projectile.position.X - 6f, projectile.position.Y + (float)(projectile.height / 2) - 8f), projectile.width + 12, 24, tarDustType);
                            Main.dust[num16].velocity.Y -= 1.5f;
                            Main.dust[num16].velocity.X *= 2.5f;
                            Main.dust[num16].scale = 1.3f;
                            Main.dust[num16].alpha = 100;
                            Main.dust[num16].noGravity = true;
                        }

                        SoundEngine.PlaySound(SoundID.Splash, projectile.position);
                    }
                    else if (handler.permafrostWet) {
                        for (int num15 = 0; num15 < 10; num15++) {
                            int num16 = Dust.NewDust(new Vector2(projectile.position.X - 6f, projectile.position.Y + (float)(projectile.height / 2) - 8f), projectile.width + 12, 24, permafrostDustType);
                            Main.dust[num16].velocity.Y -= 1.5f;
                            Main.dust[num16].velocity.X *= 2.5f;
                            Main.dust[num16].scale = 1.3f;
                            Main.dust[num16].alpha = 100;
                            Main.dust[num16].noGravity = true;
                        }

                        SoundEngine.PlaySound(SoundID.Splash, projectile.position);
                    }
                }
            }

            if (!handler.wet) {
                handler.permafrostWet = false;
                handler.tarWet = false;
            }

            if (projectile.wetCount == 0 || handler.wetCount > 0) {
                projectile.wetCount = 1;
            }

            if (handler.tarWet || handler.permafrostWet) {

                projectile.lavaWet = false;
                projectile.honeyWet = false;
                projectile.shimmerWet = false;
            }

            if (handler.wetCount > 0) {
                handler.wetCount--;
            }
        }
    }
}

sealed class CustomLiquidCollision_NPC : GlobalNPC {
    public bool permafrostWet, tarWet;
    public bool wet;
    public byte wetCount;

    public override void Load() {
        On_NPC.UpdateCollision += On_NPC_UpdateCollision;
        On_NPC.Collision_WaterCollision += On_NPC_Collision_WaterCollision;
        On_NPC.Collision_MoveWhileWet += On_NPC_Collision_MoveWhileWet;
        On_NPC.UpdateNPC_UpdateGravity += On_NPC_UpdateNPC_UpdateGravity;
    }

    private void On_NPC_UpdateNPC_UpdateGravity(On_NPC.orig_UpdateNPC_UpdateGravity orig, NPC self) {
        orig(self);

        var handler = self.GetGlobalNPC<CustomLiquidCollision_NPC>();
        if (handler.tarWet || CustomLiquidCollision_Player.tarCollision) {
            if (!self.GravityIgnoresLiquid && handler.tarWet) {
                self.GravityMultiplier *= 0.3f;
                self.MaxFallSpeedMultiplier *= 0.4f;
            }
        }
    }

    private void On_NPC_Collision_MoveWhileWet(On_NPC.orig_Collision_MoveWhileWet orig, NPC self, Vector2 oldDryVelocity, float Slowdown) {
        var handler = self.GetGlobalNPC<CustomLiquidCollision_NPC>();
        if (Slowdown == self.waterMovementSpeed) {
            if (handler.tarWet || CustomLiquidCollision_Player.tarCollision) {
                CustomLiquidCollision(self, oldDryVelocity, 0.15f);
                return;
            }
            else if (handler.permafrostWet || CustomLiquidCollision_Player.permafrostCollision) {
                CustomLiquidCollision(self, oldDryVelocity);
                return;
            }
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

    public override bool InstancePerEntity => true;

    private bool On_NPC_Collision_WaterCollision(On_NPC.orig_Collision_WaterCollision orig, NPC self, bool lava) {
        var handler = self.GetGlobalNPC<CustomLiquidCollision_NPC>();
        if (handler.tarWet || handler.permafrostWet) {
            if (self.wetCount == 0) {
                self.wetCount = 1;
            }
        }
        int type = self.type;
        self.type = 617;

        bool flag = false;
        if (self.type == 72 || self.aiStyle == 21 || self.aiStyle == 67 || self.type == 376 || self.type == 579 || self.type == 541 || (self.aiStyle == 7 && self.ai[0] == 25f)) {
            flag = false;
            wetCount = 0;
            lava = false;
        }
        else {
            flag = Collision.WetCollision(self.position, self.width, self.height);
            if (CustomLiquidCollision_Player.tarCollision)
                handler.tarWet = true;

            if (CustomLiquidCollision_Player.permafrostCollision)
                handler.permafrostWet = true;
        }
        ushort tarDustType = (ushort)ModContent.DustType<Content.Dusts.Tar>(),
               permafrostDustType = (ushort)ModContent.DustType<Content.Dusts.Permafrost>();
        if (flag) {
            //if (onFire && !lavaWet && Main.netMode != 1) {
            //    for (int i = 0; i < maxBuffs; i++) {
            //        if (buffType[i] == 24)
            //            DelBuff(i);
            //    }
            //}

            if (!handler.wet && handler.wetCount == 0) {
                handler.wetCount = 10;
                if (!lava) {
                    if (CustomLiquidCollision_Player.tarCollision) {
                        for (int m = 0; m < 10; m++) {
                            int num4 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, tarDustType);
                            Main.dust[num4].velocity.Y -= 1.5f;
                            Main.dust[num4].velocity.X *= 2.5f;
                            Main.dust[num4].scale = 1.3f;
                            Main.dust[num4].alpha = 100;
                            Main.dust[num4].noGravity = true;
                        }

                        if (self.aiStyle != 1 && self.type != 1 && self.type != 16 && self.type != 147 && self.type != 59 && self.type != 300 && self.aiStyle != 39 && !self.noGravity)
                            SoundEngine.PlaySound(SoundID.Splash, self.position);
                    }
                    else if (CustomLiquidCollision_Player.permafrostCollision) {
                        for (int m = 0; m < 10; m++) {
                            int num4 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, permafrostDustType);
                            Main.dust[num4].velocity.Y -= 1.5f;
                            Main.dust[num4].velocity.X *= 2.5f;
                            Main.dust[num4].scale = 1.3f;
                            Main.dust[num4].alpha = 100;
                            Main.dust[num4].noGravity = true;
                        }

                        if (self.aiStyle != 1 && self.type != 1 && self.type != 16 && self.type != 147 && self.type != 59 && self.type != 300 && self.aiStyle != 39 && !self.noGravity)
                            SoundEngine.PlaySound(SoundID.Splash, self.position);
                    }
                }
            }

            handler.wet = true;
        }
        else if (handler.wet) {
            self.velocity.X *= 0.5f;
            handler.wet = false;
            if (self.type == 620 && self.GetTargetData().Center.Y < self.Center.Y)
                self.velocity.Y -= 8f;

            if (handler.wetCount == 0) {
                handler.wetCount = 10;
                if (!self.lavaWet) {
                    if (handler.tarWet) {
                        for (int num10 = 0; num10 < 10; num10++) {
                            int num11 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, tarDustType);
                            Main.dust[num11].velocity.Y -= 1.5f;
                            Main.dust[num11].velocity.X *= 2.5f;
                            Main.dust[num11].scale = 1.3f;
                            Main.dust[num11].alpha = 100;
                            Main.dust[num11].noGravity = true;
                        }

                        if (self.aiStyle != 1 && self.type != 1 && self.type != 16 && self.type != 59 && self.type != 300 && self.aiStyle != 39 && !self.noGravity)
                            SoundEngine.PlaySound(SoundID.Splash, self.position);
                    }
                    else if (handler.permafrostWet) {
                        for (int num10 = 0; num10 < 10; num10++) {
                            int num11 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, permafrostDustType);
                            Main.dust[num11].velocity.Y -= 1.5f;
                            Main.dust[num11].velocity.X *= 2.5f;
                            Main.dust[num11].scale = 1.3f;
                            Main.dust[num11].alpha = 100;
                            Main.dust[num11].noGravity = true;
                        }

                        if (self.aiStyle != 1 && self.type != 1 && self.type != 16 && self.type != 59 && self.type != 300 && self.aiStyle != 39 && !self.noGravity)
                            SoundEngine.PlaySound(SoundID.Splash, self.position);
                    }
                }
                else {
                }
            }
        }

        bool result = orig(self, lava);
        self.type = type;
        return result;
    }

    private void On_NPC_UpdateCollision(On_NPC.orig_UpdateCollision orig, NPC self) {
        orig(self);

        var handler = self.GetGlobalNPC<CustomLiquidCollision_NPC>();
        if (!handler.wet) {
            handler.permafrostWet = false;
            handler.tarWet = false;
        }

        if (handler.tarWet || handler.permafrostWet) {
            self.lavaWet = false;
            self.honeyWet = false;
            self.shimmerWet = false;
        }

        if (handler.wetCount > 0) {
            handler.wetCount--;
        }
    }

    public override void SetDefaults(NPC entity) {
        permafrostWet = tarWet = false;
        wet = false;

        wetCount = 0;
    }
}

sealed class CustomLiquidCollision_Player : ModPlayer {
    public bool permafrostWet, tarWet;
    public bool wet;
    public byte wetCount;

    public bool waterWalk, waterWalk2;

    public static bool permafrostCollision;
    public static bool tarCollision;

    public static bool updatingNewLiquidCollision;

    public override void Load() {
        On_Collision.WetCollision += On_Collision_WetCollision;
        On_Player.DryCollision += On_Player_DryCollision;
        On_Dust.NewDust += On_Dust_NewDust;
        On_Player.WaterCollision += On_Player_WaterCollision;
    }

    private void On_Player_WaterCollision(On_Player.orig_WaterCollision orig, Player self, bool fallThrough, bool ignorePlats) {
        var handler = self.GetModPlayer<CustomLiquidCollision_Player>();
        if (handler.tarWet || tarCollision) {
            TarCollision(self, fallThrough, ignorePlats);
            return;
        }
        else if (handler.permafrostWet || permafrostCollision) {
            PermafrostCollision(self, fallThrough, ignorePlats);
            return;
        }

        orig(self, fallThrough, ignorePlats);
    }

    private void On_Player_DryCollision(On_Player.orig_DryCollision orig, Player self, bool fallThrough, bool ignorePlats) {
        if (permafrostCollision || tarCollision) {
            return;
        }

        orig(self, fallThrough, ignorePlats);
    }

    private int On_Dust_NewDust(On_Dust.orig_NewDust orig, Vector2 Position, int Width, int Height, int Type, float SpeedX, float SpeedY, int Alpha, Color newColor, float Scale) {
        int whoAmI = orig(Position, Width, Height, Type, SpeedX, SpeedY, Alpha, newColor, Scale);
        return whoAmI;
    }

    private bool On_Collision_WetCollision(On_Collision.orig_WetCollision orig, Microsoft.Xna.Framework.Vector2 Position, int Width, int Height) {
        Collision.honey = false;
        Collision.shimmer = false;
        permafrostCollision = false;
        tarCollision = false;
        Vector2 vector = new Vector2(Position.X + (float)(Width / 2), Position.Y + (float)(Height / 2));
        int num = 10;
        int num2 = Height / 2;
        if (num > Width)
            num = Width;

        if (num2 > Height)
            num2 = Height;

        vector = new Vector2(vector.X - (float)(num / 2), vector.Y - (float)(num2 / 2));
        int value = (int)(Position.X / 16f) - 1;
        int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
        int value3 = (int)(Position.Y / 16f) - 1;
        int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
        int num3 = Utils.Clamp(value, 0, Main.maxTilesX - 1);
        value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
        value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
        value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);
        Vector2 vector2 = default(Vector2);
        for (int i = num3; i < value2; i++) {
            for (int j = value3; j < value4; j++) {
                if (Main.tile[i, j] == null)
                    continue;

                if (Main.tile[i, j].LiquidAmount > 0) {
                    vector2.X = i * 16;
                    vector2.Y = j * 16;
                    int num4 = 16;
                    float num5 = 256 - Main.tile[i, j].LiquidAmount;
                    num5 /= 32f;
                    vector2.Y += num5 * 2f;
                    num4 -= (int)(num5 * 2f);
                    if (vector.X + (float)num > vector2.X && vector.X < vector2.X + 16f && vector.Y + (float)num2 > vector2.Y && vector.Y < vector2.Y + (float)num4) {
                        if (Main.tile[i, j].LiquidType == LiquidID.Honey)
                            Collision.honey = true;

                        if (Main.tile[i, j].LiquidType == LiquidID.Shimmer)
                            Collision.shimmer = true;

                        if (Main.tile[i, j].LiquidType == 4)
                            permafrostCollision = true;

                        if (Main.tile[i, j].LiquidType == 5)
                            tarCollision = true;

                        return true;
                    }
                }
                else {
                    if (!Main.tile[i, j].HasTile || Main.tile[i, j].Slope == 0 || j <= 0 || Main.tile[i, j - 1] == null || Main.tile[i, j - 1].LiquidAmount <= 0)
                        continue;

                    vector2.X = i * 16;
                    vector2.Y = j * 16;
                    int num6 = 16;
                    if (vector.X + (float)num > vector2.X && vector.X < vector2.X + 16f && vector.Y + (float)num2 > vector2.Y && vector.Y < vector2.Y + (float)num6) {
                        if (Main.tile[i, j - 1].LiquidType == LiquidID.Honey)
                            Collision.honey = true;
                        else if (Main.tile[i, j - 1].LiquidType == LiquidID.Shimmer)
                            Collision.shimmer = true;
                        else if (Main.tile[i, j - 1].LiquidType == 4)
                            permafrostCollision = true;
                        else if (Main.tile[i, j - 1].LiquidType == 5)
                            tarCollision = true;

                        return true;
                    }
                }
            }
        }

        return false;
    }

    public override void PostUpdateRunSpeeds() {
        int num82 = Player.height;
        if (waterWalk)
            num82 -= 6;

        if (waterWalk2 && !waterWalk)
            num82 -= 6;

        bool num85 = Collision.WetCollision(Player.position, Player.width, Player.height);
        bool permafrost = permafrostCollision;
        bool tar = tarCollision;

        if (tar) {
            tarWet = true;
        }

        if (permafrost) {
            permafrostWet = true;
        }

        if (tarWet || permafrostWet) {
            if (Player.wetCount == 0) {
                Player.wetCount = 1;
            }
        }

        ushort tarDustType = (ushort)ModContent.DustType<Content.Dusts.Tar>(),
               permafrostDustType = (ushort)ModContent.DustType<Content.Dusts.Permafrost>();
        if (num85) {
            //if ((onFire || onFire3) && !lavaWet) {
            //    for (int num88 = 0; num88 < maxBuffs; num88++) {
            //        int num89 = buffType[num88];
            //        if (num89 == 24 || num89 == 323)
            //            DelBuff(num88);
            //    }
            //}

            if (!wet) {
                if (wetCount == 0) {
                    wetCount = 10;
                    if (!Player.shimmering) {
                        if (tar) { 
                            for (int num96 = 0; num96 < 20; num96++) {
                                int num97 = Dust.NewDust(new Vector2(Player.position.X - 6f, Player.position.Y + (float)(Player.height / 2) - 8f), Player.width + 12, 24, tarDustType);
                                Main.dust[num97].velocity.Y -= 1.5f;
                                Main.dust[num97].velocity.X *= 2.5f;
                                Main.dust[num97].scale = 1.3f;
                                Main.dust[num97].alpha = 100;
                                Main.dust[num97].noGravity = true;
                            }

                            SoundEngine.PlaySound(SoundID.Splash, Player.position);
                        }
                        else if (permafrost) {
                            for (int num96 = 0; num96 < 20; num96++) {
                                int num97 = Dust.NewDust(new Vector2(Player.position.X - 6f, Player.position.Y + (float)(Player.height / 2) - 8f), Player.width + 12, 24, permafrostDustType);
                                Main.dust[num97].velocity.Y -= 1.5f;
                                Main.dust[num97].velocity.X *= 2.5f;
                                Main.dust[num97].scale = 1.3f;
                                Main.dust[num97].alpha = 100;
                                Main.dust[num97].noGravity = true;
                            }

                            SoundEngine.PlaySound(SoundID.Splash, Player.position);
                        }
                    }
                }

                wet = true;
                //if (ShouldFloatInWater) {
                //    velocity.Y /= 2f;
                //    if (velocity.Y > 3f)
                //        velocity.Y = 3f;
                //}
            }
        }
        else if (wet) {
            wet = false;
            //if (jump > jumpHeight / 5 && wetSlime == 0)
            //    jump = jumpHeight / 5;

            if (wetCount == 0) {
                wetCount = 10;
                if (!Player.shimmering) {
                    if (tarWet) {
                        for (int num104 = 0; num104 < 20; num104++) {
                            int num105 = Dust.NewDust(new Vector2(Player.position.X - 6f, Player.position.Y + (float)(Player.height / 2) - 8f), Player.width + 12, 24, tarDustType);
                            Main.dust[num105].velocity.Y -= 1.5f;
                            Main.dust[num105].velocity.X *= 2.5f;
                            Main.dust[num105].scale = 1.3f;
                            Main.dust[num105].alpha = 100;
                            Main.dust[num105].noGravity = true;
                        }

                        SoundEngine.PlaySound(SoundID.Splash, Player.position);
                    }
                    else if (permafrostWet) {
                        for (int num104 = 0; num104 < 20; num104++) {
                            int num105 = Dust.NewDust(new Vector2(Player.position.X - 6f, Player.position.Y + (float)(Player.height / 2) - 8f), Player.width + 12, 24, permafrostDustType);
                            Main.dust[num105].velocity.Y -= 1.5f;
                            Main.dust[num105].velocity.X *= 2.5f;
                            Main.dust[num105].scale = 1.3f;
                            Main.dust[num105].alpha = 100;
                            Main.dust[num105].noGravity = true;
                        }

                        SoundEngine.PlaySound(SoundID.Splash, Player.position);
                    }
                }
            }
        }

        if (!permafrost)
            permafrostWet = false;

        if (!tar)
            tarWet = false;

        if (tarWet) {
            Player.gravity = 0.1f;
            Player.maxFallSpeed = 3f;
        }
    }

    public override void PostUpdate() {
        if (!wet) {
            permafrostWet = false;
            tarWet = false;
        }

        if (wetCount > 0)
            wetCount--;

        if (tarWet || permafrostWet) {
            Player.lavaWet = false;
            Player.honeyWet = false;
            Player.shimmerWet = false;
        }
    }

    public void TarCollision(Player self, bool fallThrough, bool ignorePlats) {
        int num = ((!self.onTrack) ? self.height : (self.height - 20));
        Vector2 vector = self.velocity;
        if (self.velocity.Y > self.gravity * 5f) {
            self.velocity.Y = self.gravity * 5f;
        }
        self.velocity = Collision.TileCollision(self.position, self.velocity, self.width, num, fallThrough, ignorePlats, (int)self.gravDir);
        Vector2 vector2 = self.velocity * 0.15f;
        if (self.velocity.X != vector.X)
            vector2.X = self.velocity.X;

        if (self.velocity.Y != vector.Y)
            vector2.Y = self.velocity.Y;

        self.position += vector2;
        //self.TryFloatingInFluid();
    }

    public void PermafrostCollision(Player self, bool fallThrough, bool ignorePlats) {
        int num = ((!self.onTrack) ? self.height : (self.height - 20));
        Vector2 vector = self.velocity;
        self.velocity = Collision.TileCollision(self.position, self.velocity, self.width, num, fallThrough, ignorePlats, (int)self.gravDir);
        Vector2 vector2 = self.velocity * 0.5f;
        if (self.velocity.X != vector.X)
            vector2.X = self.velocity.X;

        if (self.velocity.Y != vector.Y)
            vector2.Y = self.velocity.Y;

        self.position += vector2;
        //self.TryFloatingInFluid();
    }
}

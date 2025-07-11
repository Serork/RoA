using Microsoft.Xna.Framework;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Liquids;

sealed class CustomLiquidCollision_Player : ModPlayer {
    public int lavaTime, lavaMax;
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
        if (Type == Dust.dustWater() && Width == Player.defaultWidth + 12 && Height == 24) {
            if (permafrostCollision || tarCollision) {
                return 6000;
            }
        }

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

        if (lavaTime > lavaMax)
            lavaTime = lavaMax;

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
        Vector2 vector2 = self.velocity * 0.25f;
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

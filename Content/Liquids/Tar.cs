using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ModLiquidLib.ModLoader;
using ModLiquidLib.Utils;
using ModLiquidLib.Utils.Structs;

using RoA.Content.Buffs;
using RoA.Content.Projectiles.LiquidsSpecific;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Liquid;
using Terraria.Graphics;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Liquids;

sealed partial class Tar : ModLiquid {
    private static int _animationFrame, _animationFrame2;
    private static float _frameState, _frameState2;

    public static Color LiquidColor => new(46, 34, 47);

    private class UpdateAnimation : IInitializer {
        void ILoadable.Load(Mod mod) {
            On_LiquidRenderer.Update += On_LiquidRenderer_Update;
        }

        private void On_LiquidRenderer_Update(On_LiquidRenderer.orig_Update orig, LiquidRenderer self, GameTime gameTime) {
            orig(self, gameTime);

            if (!Main.gamePaused && Main.hasFocus) {
                float num = Main.windSpeedCurrent * 25f;

                num = Main.windSpeedCurrent * 15f;
                num = ((!(num < 0f)) ? (num + 5f) : (num - 5f));
                num = MathF.Abs(num);
                //num *= 0.5f;
                _frameState += num * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_frameState < 0f)
                    _frameState += 16f;

                _frameState %= 16f;

                _animationFrame = (int)_frameState;

                num = 1f;
                num = ((!(num < 0f)) ? (num + 5f) : (num - 5f));
                num = MathF.Abs(num);
                _frameState2 += num * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_frameState2 < 0f)
                    _frameState2 += 16f;

                _frameState2 %= 16f;

                _animationFrame2 = (int)_frameState2;
            }
        }
    }

    public override void SetStaticDefaults() {
        //This is the viscosity of the liquid, only used visually.
        //Lava usually has this set to 200, while honey has this set to 240. All other liquids set this to 0 by default.
        //In Vanilla this property can be found at "Terraria.GameContent.Liquid.LiquidRenderer.VISCOSITY_MASK"
        LiquidRenderer.VISCOSITY_MASK[Type] = 240;

        //This is the length the liquid will visually have when flowing/falling downwards or if there is a slope underneath.
        //In Vanilla This property can be found at "Terraria.GameContent.Liquid.LiquidRenderer.WATERFALL_LENGTH"
        LiquidRenderer.WATERFALL_LENGTH[Type] = 2;

        //This is the opacity of the liquid. How well you can see objects in the liquid.
        //The SlopeOpacity property is different, as slopes do not render the same as a normal liquid tile
        //DefaultOpacity in vanilla, can be found at "Terraria.GameContent.Liquid.LiquidRenderer.DEFAULT_OPACITY"
        LiquidRenderer.DEFAULT_OPACITY[Type] = 0.975f;
        SlopeOpacity = 1f;
        //To change the old liquid rendering opacity, please see the RetroDrawEffects override.

        //For the Waves Quality setting, when set to Medium, waves are set to be the same distance no matter the liquid type.
        //To do this, the game applied a multiplier to make them all consistant between liquids. Here we set our own multiplier to make the waves the same distance.
        WaterRippleMultiplier = 0.3f;

        //This is used to specify what dust is used when splashing in this liquid.
        //Normally, when returning false in each OnSplash hook/method, this property is used in the mod liquid's default splash code
        //It returns -1 normally, which prevents the liquid from doing any splash dust
        //Here we set it, as we use the property in our OnSplash hooks to have one central variable that controls which dust ID is used in our custom splash
        SplashDustType = ModContent.DustType<Dusts.Tar>();

        //This is used to specify what sound is played when an entity enters a liquid
        //Normally this property is used in the mod liquid's default splash code and returns null as no sound is played normally.
        //Similarly to SplashDustType, we use this to have 1 central place for the splash sound used accross each OnSplash hooks.
        SplashSound = SoundID.SplashWeak;

        FallDelay = 13; //The delay when liquids are falling. Liquids will wait this extra amount of frames before falling again.

        ChecksForDrowning = true; //If the player can drown in this liquid
        AllowEmitBreathBubbles = false;  //Bubbles will come out of the player's mouth normally when drowning, here we can stop that by setting it to false.

        FishingPoolSizeMultiplier = 2f; //The multiplier used for calculating the size of a fishing pool of this liquid. Here, each liquid tile counts as 2 for every tile in a fished pool.

        //We can add a map entry to our liquid, by doing so we can show where our liquid is on the map.
        //Unlike vanilla, we can also add a map entry name, which will display a name if the liquid is being selected on the map.
        AddMapEntry(LiquidColor);

        PlayerMovementMultiplier = 0.175f;
        StopWatchMPHMultiplier = PlayerMovementMultiplier; //We set stopwatch to the same multiplier as we don't want a different between whats felt and what the player can read their movement as.

        LiquidfallOpacityMultiplier = 0.5f;
    }

    public override void PlayerRippleModifier(Player player, ref float rippleStrength, ref float rippleOffset) {
        rippleStrength *= 0.75f;
        rippleOffset *= 0.75f;
    }

    public override void NPCRippleModifier(NPC npc, ref float rippleStrength, ref float rippleOffset) {
        rippleStrength *= 0.75f;
        rippleOffset *= 0.75f;
    }

    public override int ChooseWaterfallStyle(int i, int j) => ModContent.GetInstance<TarFall>().Slot;

    public override void Load() {
        CollisionLoad();

        On_Player.SpawnFastRunParticles += On_Player_SpawnFastRunParticles;
    }

    private void On_Player_SpawnFastRunParticles(On_Player.orig_SpawnFastRunParticles orig, Player self) {
        if (self.GetModdedWetArray()[LiquidLoader.LiquidType<Liquids.Tar>() - LiquidID.Count]) {
            return;
        }

        orig(self);
    }

    public partial void CollisionLoad();

    public override int LiquidMerge(int i, int j, int otherLiquid) {
        ushort tarTile = (ushort)ModContent.TileType<Tiles.LiquidsSpecific.SolidifiedTar>();
        if (otherLiquid == LiquidID.Water) {
            return tarTile; //When the liquid collides with water. Blue team block is created
        }
        else if (otherLiquid == LiquidID.Lava) {
            return -1; //When the liquid collides with lava. Red team block is created
        }
        else if (otherLiquid == LiquidID.Honey) {
            return tarTile; //When the liquid collides with honey. Yellow team block is created
        }
        else if (otherLiquid == LiquidID.Shimmer) {
            return TileID.ShimmerBlock; //When the liquid collides with shimmer. Pink team block is created
        }
        //The base return is what
        return tarTile;
    }

    public override bool PreLiquidMerge(int liquidX, int liquidY, int tileX, int tileY, int otherLiquid) {
        if (otherLiquid == LiquidID.Lava) {
            int x = tileX, y = tileY;
            Tile tile = Main.tile[x - 1, y];
            Tile tile2 = Main.tile[x + 1, y];
            Tile tile3 = Main.tile[x, y - 1];
            Tile tile4 = Main.tile[x, y + 1];
            Tile tile5 = Main.tile[x, y];
            tile.LiquidAmount = tile2.LiquidAmount = tile3.LiquidAmount = 0;
            if (Helper.SinglePlayerOrServer) {
                Projectile.NewProjectile(null, new Point16(tileX, tileY).ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<TarExplosion>(), 100, 0f, Main.myPlayer);
            }
            WorldGen.SquareTileFrame(x, y);

            return false;
        }

        return base.PreLiquidMerge(liquidX, liquidY, tileX, tileY, otherLiquid);
    }

    public override LightMaskMode LiquidLightMaskMode(int i, int j) => LightMaskMode.None;

    public override void ModifyLightMaskMode(int index, ref float r, ref float g, ref float b) {
        Vector3 lightDecayThroughTar = new Vector3(0.88f, 0.96f, 1.015f) * 0.6f;
        r *= lightDecayThroughTar.X;
        g *= lightDecayThroughTar.Y;
        b *= lightDecayThroughTar.Z;
    }

    public override bool OnPlayerSplash(Player player, bool isEnter) {
        Player Player = player;
        ushort tarDustType = (ushort)ModContent.DustType<Dusts.Tar>();
        if (isEnter) {
            for (int num96 = 0; num96 < 20; num96++) {
                int num97 = Dust.NewDust(new Vector2(Player.position.X - 6f, Player.position.Y + (float)(Player.height / 2) - 8f), Player.width + 12, 24, tarDustType);
                Main.dust[num97].velocity.Y -= 1.5f;
                Main.dust[num97].velocity.X *= 2.5f;
                Main.dust[num97].scale = 1f;
                Main.dust[num97].alpha = Main.rand.Next(50, 100);
                if (Main.rand.Next(2) == 0)
                    Main.dust[num97].alpha += 25;

                if (Main.rand.Next(2) == 0)
                    Main.dust[num97].alpha += 25;
                Main.dust[num97].noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Splash, Player.position);
        }
        else {
            for (int num104 = 0; num104 < 20; num104++) {
                int num105 = Dust.NewDust(new Vector2(Player.position.X - 6f, Player.position.Y + (float)(Player.height / 2) - 8f), Player.width + 12, 24, tarDustType);
                Main.dust[num105].velocity.Y -= 1.5f;
                Main.dust[num105].velocity.X *= 2.5f;
                Main.dust[num105].scale = 1f;
                Main.dust[num105].alpha = Main.rand.Next(50, 100);
                if (Main.rand.Next(2) == 0)
                    Main.dust[num105].alpha += 25;

                if (Main.rand.Next(2) == 0)
                    Main.dust[num105].alpha += 25;
                Main.dust[num105].noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Splash, Player.position);
        }
        
        return false;
    }

    public override bool OnNPCSplash(NPC npc, bool isEnter) {
        NPC self = npc;
        ushort tarDustType = (ushort)ModContent.DustType<Dusts.Tar>();
        if (isEnter) {
            for (int m = 0; m < 10; m++) {
                int num4 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, tarDustType);
                Main.dust[num4].velocity.Y -= 1.5f;
                Main.dust[num4].velocity.X *= 2.5f;
                Main.dust[num4].scale = 1f;
                Main.dust[num4].alpha = Main.rand.Next(50, 100);
                if (Main.rand.Next(2) == 0)
                    Main.dust[num4].alpha += 25;

                if (Main.rand.Next(2) == 0)
                    Main.dust[num4].alpha += 25;
                Main.dust[num4].noGravity = true;
            }

            if (self.aiStyle != 1 && self.type != 1 && self.type != 16 && self.type != 147 && self.type != 59 && self.type != 300 && self.aiStyle != 39 && !self.noGravity)
                SoundEngine.PlaySound(SoundID.Splash, self.position);
        }
        else {
            for (int num10 = 0; num10 < 10; num10++) {
                int num11 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, tarDustType);
                Main.dust[num11].velocity.Y -= 1.5f;
                Main.dust[num11].velocity.X *= 2.5f;
                Main.dust[num11].scale = 1f;
                Main.dust[num11].alpha = Main.rand.Next(50, 100);
                if (Main.rand.Next(2) == 0)
                    Main.dust[num11].alpha += 25;

                if (Main.rand.Next(2) == 0)
                    Main.dust[num11].alpha += 25;
                Main.dust[num11].noGravity = true;
            }

            if (self.aiStyle != 1 && self.type != 1 && self.type != 16 && self.type != 59 && self.type != 300 && self.aiStyle != 39 && !self.noGravity)
                SoundEngine.PlaySound(SoundID.Splash, self.position);
        }

        return false;
    }

    public override bool OnProjectileSplash(Projectile proj, bool isEnter) {
        Projectile projectile = proj;
        ushort tarDustType = (ushort)ModContent.DustType<Dusts.Tar>();
        if (isEnter) {
            for (int num7 = 0; num7 < 10; num7++) {
                int num8 = Dust.NewDust(new Vector2(projectile.position.X - 6f, projectile.position.Y + (float)(projectile.height / 2) - 8f), projectile.width + 12, 24, tarDustType);
                Main.dust[num8].velocity.Y -= 1.5f;
                Main.dust[num8].velocity.X *= 2.5f;
                Main.dust[num8].scale = 1f;
                Main.dust[num8].alpha = Main.rand.Next(50, 100);
                if (Main.rand.Next(2) == 0)
                    Main.dust[num8].alpha += 25;

                if (Main.rand.Next(2) == 0)
                    Main.dust[num8].alpha += 25;
                Main.dust[num8].noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Splash, projectile.position);
        }
        else {
            for (int num15 = 0; num15 < 10; num15++) {
                int num16 = Dust.NewDust(new Vector2(projectile.position.X - 6f, projectile.position.Y + (float)(projectile.height / 2) - 8f), projectile.width + 12, 24, tarDustType);
                Main.dust[num16].velocity.Y -= 1.5f;
                Main.dust[num16].velocity.X *= 2.5f;
                Main.dust[num16].scale = 1f;
                Main.dust[num16].alpha = 100;
                Main.dust[num16].noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Splash, projectile.position);
        }

        return false;
    }

    public override bool OnItemSplash(Item item, bool isEnter) {
        Item self = item;
        ushort tarDustType = (ushort)ModContent.DustType<Dusts.Tar>();
        if (isEnter) {
            for (int n = 0; n < 5; n++) {
                int num8 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, tarDustType);
                Main.dust[num8].velocity.Y -= 1.5f;
                Main.dust[num8].velocity.X *= 2.5f;
                Main.dust[num8].scale = 1f;
                Main.dust[num8].alpha = Main.rand.Next(50, 100);
                if (Main.rand.Next(2) == 0)
                    Main.dust[num8].alpha += 25;

                if (Main.rand.Next(2) == 0)
                    Main.dust[num8].alpha += 25;
                Main.dust[num8].noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Splash, self.position);
        }
        else {
            for (int num15 = 0; num15 < 5; num15++) {
                int num16 = Dust.NewDust(new Vector2(self.position.X - 6f, self.position.Y + (float)(self.height / 2) - 8f), self.width + 12, 24, tarDustType);
                Main.dust[num16].velocity.Y -= 1.5f;
                Main.dust[num16].velocity.X *= 2.5f;
                Main.dust[num16].scale = 1f;
                Main.dust[num16].alpha = 100;
                Main.dust[num16].noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Splash, self.position);
        }

        return false;
    }

    public override bool PreDraw(int i, int j, LiquidRenderer.LiquidDrawCache liquidDrawCache, Vector2 drawOffset, bool isBackgroundDraw) {
        Lighting.GetCornerColors(i, j, out VertexColors vertices);
        Texture2D texture = LiquidLoader.LiquidAssets[Type].Value;
        Rectangle sourceRectangle = liquidDrawCache.SourceRectangle;
        if (j < Main.worldSurface - 40.0) {
            sourceRectangle.Y += _animationFrame * 80;
        }
        else {
            sourceRectangle.Y += _animationFrame2 * 80;
        }

        drawOffset += liquidDrawCache.LiquidOffset;

        SetTarVertexColors(ref vertices, 1f, i, j);

        float liquidOpacity = liquidDrawCache.Opacity * (isBackgroundDraw ? 1f : LiquidRenderer.DEFAULT_OPACITY[Type]);
        liquidOpacity = Math.Min(1f, liquidOpacity);
        vertices.BottomLeftColor *= liquidOpacity;
        vertices.BottomRightColor *= liquidOpacity;
        vertices.TopLeftColor *= liquidOpacity;
        vertices.TopRightColor *= liquidOpacity;

        if (Main.LocalPlayer.GetCommon().IsClarityEffectActive) {
            float num = Clarity.APPLIEDLIQUIDOPACITY;
            vertices.BottomLeftColor *= num;
            vertices.BottomRightColor *= num;
            vertices.TopLeftColor *= num;
            vertices.TopRightColor *= num;
        }

        Main.DrawTileInWater(drawOffset, i, j);
        Main.tileBatch.Draw(texture, new Vector2(i << 4, j << 4) + drawOffset, sourceRectangle, vertices, Vector2.Zero, 1f, SpriteEffects.None);

        if (Dust.lavaBubbles < 200) {
            if (Main.rand.Next(600) == 0)
                Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, ModContent.DustType<Dusts.Tar>(), 0f, 0f, 0, Color.White);

            if (Main.rand.Next(300) == 0) {
                int num27 = Dust.NewDust(new Vector2(i * 16, j * 16), 16, 8, ModContent.DustType<Dusts.Tar>(), 0f, 0f, 50, Color.White, 1.5f);
                Main.dust[num27].velocity *= 0.8f;
                Main.dust[num27].velocity.X *= 2f;
                Main.dust[num27].velocity.Y -= (float)Main.rand.Next(1, 7) * 0.1f;
                if (Main.rand.Next(5) == 0)
                    Main.dust[num27].velocity.Y *= Main.rand.Next(2, 5);

                Main.dust[num27].noGravity = true;
            }
        }

        return false;
    }

    public static float GetTarWave(ref float worldPositionX, ref float worldPositionY) => (float)Math.Sin(((double)((Math.Cos(worldPositionX + Main.timeForVisualEffects / 180) + Math.Sin(worldPositionY + Main.timeForVisualEffects / 180))) - Main.timeForVisualEffects / 360) * 6.2831854820251465);

    public static Vector4 GetTarBaseColor(float worldPositionX, float worldPositionY) {
        float shimmerWave = GetTarWave(ref worldPositionX, ref worldPositionY);
        float brightness = Lighting.Brightness((int)worldPositionX, (int)worldPositionY);
        Vector4 brightColor1 = new(0.3f, 0.3f, 0.3f, 1f),
                brightColor2 = new(1f, 1f, 1f, 1f);
        Vector4 darkColor1 = new(0.2f, 0.2f, 0.2f, 1f),
                darkColor2 = new(0.9f, 0.9f, 0.9f, 1f);
        var output = Vector4.Lerp(Vector4.Lerp(darkColor1, darkColor2, brightness), Vector4.Lerp(brightColor1, brightColor2, brightness), shimmerWave);
        return output * 1f;
    }

    public static void SetTarVertexColors(ref VertexColors colors, float opacity, int x, int y) {
        float brightness = Lighting.Brightness(x, y) * 1f;
        brightness = MathF.Pow(brightness, 0.1f);
        brightness = MathHelper.Clamp(brightness, 0f, 1f);
        colors.BottomLeftColor = Color.Lerp(colors.BottomLeftColor, Color.White, brightness);
        colors.BottomRightColor = Color.Lerp(colors.BottomRightColor, Color.White, brightness);
        colors.TopLeftColor = Color.Lerp(colors.TopLeftColor, Color.White, brightness);
        colors.TopRightColor = Color.Lerp(colors.TopRightColor, Color.White, brightness);
        colors.BottomLeftColor *= opacity;
        colors.BottomRightColor *= opacity;
        colors.TopLeftColor *= opacity;
        colors.TopRightColor *= opacity;
        colors.BottomLeftColor = new Color(colors.BottomLeftColor.ToVector4() * GetTarBaseColor(x, y + 1));
        colors.BottomRightColor = new Color(colors.BottomRightColor.ToVector4() * GetTarBaseColor(x + 1, y + 1));
        colors.TopLeftColor = new Color(colors.TopLeftColor.ToVector4() * GetTarBaseColor(x, y));
        colors.TopRightColor = new Color(colors.TopRightColor.ToVector4() * GetTarBaseColor(x + 1, y));
    }
}

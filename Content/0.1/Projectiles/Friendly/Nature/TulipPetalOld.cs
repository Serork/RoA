using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class TulipPetalOld : NatureProjectile {
    private float _rotationTimer = MathHelper.Pi;
    private float _rotationSpeed = 2.5f;
    private bool _initialize = false;
    private float _beeDrawRotation = 0;
    private float[] _beeDrawAlpha = new float[3]; // alpha for three bees separately
    private float[] _beeDrawOffset = new float[3]; // offset for three bees separately
    private int _beeCounter = 0; // this is not the actual number of bees
    private int _allBees = 0; // this bullshittery is to add the third bee projectile when using beeCounter breaks other functions
    private int _flowerRarity = 3; // the lower the more flowers
    private int _explosionMultiplier = 0; // affects size of explosion and dust rays
    private bool[] _largeBee = new bool[3];

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 3;

        ProjectileID.Sets.TrailCacheLength[Type] = 3;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        base.SafeSendExtraAI(writer);

        for (int i = 0; i < _largeBee.Length - 1; i++) {
            writer.Write(_largeBee[i]);
        }
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        base.SafeReceiveExtraAI(reader);

        for (int i = 0; i < _largeBee.Length - 1; i++) {
            _largeBee[i] = reader.ReadBoolean();
        }
    }

    protected override void SafeSetDefaults() {
        //Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);

        int width = 16; int height = width;
        Projectile.Size = new Vector2(width, height);

        //DrawOffsetX = -4;
        //DrawOriginOffsetY = -4;

        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;

        AIType = -1;

        Projectile.timeLeft = 120;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.friendly = true;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Collision.CheckAABBvAABBCollision(Projectile.position - Vector2.One * 2f, Projectile.Size + Vector2.One * 4f, targetHitbox.Location.ToVector2(), targetHitbox.Size());

    public override void AI() {
        if (!_initialize) {
            if (Projectile.ai[0] == 0 || Projectile.ai[0] == 3) Projectile.penetrate = 1;
            if (Projectile.ai[0] == 1 || Projectile.ai[0] == 3) {
                Projectile.penetrate = 1;
                _beeDrawOffset[0] = _beeDrawOffset[1] = _beeDrawOffset[2] = 50;
                _beeDrawAlpha[0] = _beeDrawAlpha[1] = _beeDrawAlpha[2] = 0f;

                if (Main.player[Projectile.owner].strongBees) {
                    if (Projectile.owner == Main.myPlayer) {
                        int max = _largeBee.Length;
                        for (int i = 0; i < max; i++) {
                            if (Main.rand.NextBool()) {
                                _largeBee[i] = true;
                            }
                        }
                        Projectile.netUpdate = true;
                    }
                }
            }
            Projectile.rotation = Main.rand.Next(360);
            if (Projectile.ai[0] < 3) Projectile.frame = (int)Projectile.ai[0];
            else Projectile.frame = Main.rand.Next(0, 3);
            _initialize = true;
        }
        Projectile.rotation += _rotationSpeed / _rotationTimer * Projectile.direction;
        _rotationTimer += 0.01f;
        _rotationSpeed *= 0.96f;

        Projectile.ai[1]++;
        if (Projectile.ai[0] == 0 || Projectile.ai[0] == 3) {
            if (Projectile.ai[1] > 4 && Projectile.ai[1] % _flowerRarity == 0 && Projectile.ai[1] < 46) {
                if (Projectile.owner == Main.myPlayer)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                        Projectile.Center.X + Main.rand.NextFloat(-2f, 2f), Projectile.Center.Y + Main.rand.NextFloat(-2f, 2f), 0, 0, ModContent.ProjectileType<TulipTrailOld>(), (int)(Projectile.damage * 0.3f), 0, Projectile.owner, Projectile.frame, Projectile.ai[1] + 4);
            }
        }
        if (Projectile.ai[0] == 1 || Projectile.ai[0] == 3) {
            if (_beeDrawRotation < 360) _beeDrawRotation += 2f;
            else _beeDrawRotation = 0;
            if (Projectile.ai[1] > 10) {
                if (_beeDrawAlpha[_beeCounter] < 1f) _beeDrawAlpha[_beeCounter] += 0.05f;
                if (_beeDrawOffset[_beeCounter] > 15) _beeDrawOffset[_beeCounter] -= 2.5f;
                else if (_beeCounter < 2) _beeCounter++;
                else _allBees = 1;
            }
        }
        if (Projectile.ai[0] == 2 || Projectile.ai[0] == 3) {
            if (Projectile.ai[1] % 5 == 0 && _explosionMultiplier < 10)
                _explosionMultiplier++;
            if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3) {
                Projectile.penetrate = -1;
                Projectile.tileCollide = false;
                Projectile.alpha = 255;
                Projectile.position = Projectile.Center;
                Projectile.width = 6 + (int)(8f * _explosionMultiplier);
                Projectile.height = 6 + (int)(8f * _explosionMultiplier);
                Projectile.Center = Projectile.position;
            }
            if (_explosionMultiplier == 10 && Projectile.ai[1] % 3 == 0) {
                if (Projectile.frame == 2) {
                    int _dust = Dust.NewDust(new Vector2(Projectile.position.X - 4, Projectile.position.Y - 4), 8, 8, DustID.BlueTorch, 0f, 0f, 40, new Color(170, 170, 255), 2f);
                    Main.dust[_dust].noGravity = true;
                    Main.dust[_dust].velocity *= 0.9f;
                }
                else if (Projectile.frame == 1) {
                    int _dust2 = Dust.NewDust(new Vector2(Projectile.position.X - 4, Projectile.position.Y - 4), 8, 8, DustID.YellowTorch, 0f, 0f, 40, new Color(255, 170, 110), 2f);
                    Main.dust[_dust2].noGravity = true;
                    Main.dust[_dust2].velocity *= 0.9f;
                }
                else {
                    int _dust3 = Dust.NewDust(new Vector2(Projectile.position.X - 4, Projectile.position.Y - 4), 8, 8, DustID.PurpleTorch, 0f, 0f, 40, new Color(255, 130, 170), 2f);
                    Main.dust[_dust3].noGravity = true;
                    Main.dust[_dust3].velocity *= 0.9f;
                }
            }
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        if (Projectile.ai[0] == 1 || Projectile.ai[0] == 3) {
            for (int k = 0; k < _beeCounter + 1; k++) {
                Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.NatureProjectileTextures + "Bee");
                if (_largeBee[k]) {
                    texture = (Texture2D)ModContent.Request<Texture2D>(ResourceManager.NatureProjectileTextures + "LargeBee");
                }
                Texture2D projectileTexture = (Texture2D)ModContent.Request<Texture2D>(Texture);
                Rectangle frameRect = new Rectangle(0, texture.Height / 4 * beeFrame, texture.Width, texture.Height / 4);
                Vector2 drawOrigin = new Vector2(projectileTexture.Width * 0.5f, Projectile.height * 0.5f);
                Vector2 drawPos = Projectile.oldPos[0] - Main.screenPosition + drawOrigin + new Vector2(0, _beeDrawOffset[k]).RotatedBy(MathHelper.ToRadians(_beeDrawRotation * Projectile.direction + k * 120));
                Color color = Projectile.GetAlpha(lightColor) * _beeDrawAlpha[k];
                if (Projectile.velocity.X > 0)
                    spriteBatch.Draw(texture, drawPos, frameRect, color, 0, drawOrigin, 1f, SpriteEffects.None, 0f);
                else
                    spriteBatch.Draw(texture, drawPos, frameRect, color, 0, drawOrigin, 1f, SpriteEffects.FlipHorizontally, 0f); // flopping
            }
        }
        return true;
    }

    private int beeFrameCounter, beeFrame;

    public override void SafePostAI() {
        if (Projectile.ai[0] == 1 || Projectile.ai[0] == 3) {
            beeFrameCounter++;
            if (beeFrameCounter >= 3) {
                beeFrame++;
                beeFrameCounter = 0;
            }

            if (beeFrame >= 3)
                beeFrame = 0;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        if (Projectile.timeLeft > 4 && (Projectile.ai[0] == 2 || Projectile.ai[0] == 3)) {
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 4;
            return false;
        }
        else return true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Projectile.timeLeft > 4 && (Projectile.ai[0] == 2 || Projectile.ai[0] == 3)) {
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 4;
        }
    }

    public int beeDamage(int dmg, bool flag) {
        if (flag)
            return dmg + Main.rand.Next(1, 4);

        return dmg + Main.rand.Next(2);
    }

    public float beeKB(float KB, bool flag) {
        if (flag)
            return 0.5f + KB * 1.1f;

        return KB;
    }

    public override void OnKill(int timeLeft) {
        Player player = Main.player[Projectile.owner];
        Texture2D projectileTexture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        if (Projectile.ai[0] == 1 || Projectile.ai[0] == 3) {
            if (Projectile.owner == Main.myPlayer) {
                Vector2 spawnOrigin = new Vector2(projectileTexture.Width * 0.5f, Projectile.height * 0.5f);
                for (int k = 0; k < _beeCounter + _allBees; k++) {
                    Vector2 spawnPos = Projectile.oldPos[0] + spawnOrigin + new Vector2(0, _beeDrawOffset[k]).RotatedBy(MathHelper.ToRadians(_beeDrawRotation * Projectile.direction + k * 120));
                    Projectile.NewProjectile(Projectile.GetSource_Death(), spawnPos, Vector2.Normalize(new Vector2(0, _beeDrawOffset[k]).RotatedBy(MathHelper.ToRadians(_beeDrawRotation + k * 120))),
                        _largeBee[k] ? ModContent.ProjectileType<LargeBee>() : ModContent.ProjectileType<Bee>(),
                        beeDamage((int)(Projectile.damage * 0.333f),
                        _largeBee[k]),
                        beeKB(0f, _largeBee[k]),
                        player.whoAmI);
                }
            }
        }
        if (Projectile.ai[0] == 2 || Projectile.ai[0] == 3) {
            SoundEngine.PlaySound(SoundID.Item45, Projectile.position);
            Projectile.position.X = Projectile.position.X + (Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y + (Projectile.height / 2);
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.position.X = Projectile.position.X - (Projectile.width / 2);
            Projectile.position.Y = Projectile.position.Y - (Projectile.height / 2);
            if (Projectile.owner == Main.myPlayer) {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X, Projectile.position.Y, 0, 0, ModContent.ProjectileType<WeepingTulipExplosion>(),
                    (int)(Projectile.damage * 0.5f * _explosionMultiplier * 0.25f), 0, Projectile.owner, ai2: _explosionMultiplier);
            }
            int dustCount = 8 + 8 * _explosionMultiplier;
            int currentDustCount = 0;
            int num5 = 0;
            while (currentDustCount < dustCount) {
                Vector2 vector = Vector2.UnitX * 0f;
                vector += -Vector2.UnitY.RotatedBy(currentDustCount * (6.18f / dustCount), default) * new Vector2(6f * _explosionMultiplier, 6f * _explosionMultiplier) * (num5 % 8) / 7;
                vector = vector.RotatedBy(Projectile.velocity.ToRotation(), default);
                if (Main.netMode != NetmodeID.Server) {
                    if (Projectile.frame == 2) {
                        int _dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.BlueTorch, 0f, 0f, 40, new Color(170, 170, 255), 2f);
                        Main.dust[_dust].noGravity = true;
                        Main.dust[_dust].position = Projectile.Center + vector;
                        Main.dust[_dust].velocity *= 0.95f;
                    }
                    else if (Projectile.frame == 1) {
                        int _dust2 = Dust.NewDust(Projectile.Center, 0, 0, DustID.YellowTorch, 0f, 0f, 40, new Color(255, 170, 110), 2f);
                        Main.dust[_dust2].noGravity = true;
                        Main.dust[_dust2].position = Projectile.Center + vector;
                        Main.dust[_dust2].velocity *= 0.95f;
                    }
                    else {
                        int _dust3 = Dust.NewDust(Projectile.Center, 0, 0, DustID.PurpleTorch, 0f, 0f, 40, new Color(255, 130, 170), 2f);
                        Main.dust[_dust3].noGravity = true;
                        Main.dust[_dust3].position = Projectile.Center + vector;
                        Main.dust[_dust3].velocity *= 0.95f;
                    }
                }
                int num4 = currentDustCount;
                currentDustCount = num4 + 1;
                num5++;
            }
        }
        if (Projectile.frame == 0 && Projectile.ai[0] != 3) {
            if (Main.netMode != NetmodeID.Server) {
                for (int i = 0; i < 5; i++) {
                    int ind4 = Dust.NewDust(Projectile.Center - Vector2.One * 4, 8, 8, ModContent.DustType<Dusts.ExoticTulip>(), 0f, 0f, 0, default, 1f + Main.rand.NextFloatRange(0.1f));
                    Main.dust[ind4].velocity *= 0.95f;
                    Main.dust[ind4].noGravity = true;
                }
            }
            SoundEngine.PlaySound(SoundID.NPCHit7, Projectile.position);
        }
        if (Projectile.frame == 1 && Projectile.ai[0] != 3) {
            if (Main.netMode != NetmodeID.Server) {
                for (int i = 0; i < 5; i++) {
                    int ind4 = Dust.NewDust(Projectile.Center - Vector2.One * 4, 8, 8, ModContent.DustType<Dusts.SweetTulip>(), 0f, 0f, 0, default, 1f + Main.rand.NextFloatRange(0.1f));
                    Main.dust[ind4].velocity *= 0.95f;
                    Main.dust[ind4].noGravity = true;
                }
            }
            SoundEngine.PlaySound(SoundID.NPCHit7, Projectile.position);
        }
        if (Projectile.frame == 2 && Projectile.ai[0] != 3) {
            if (Main.netMode != NetmodeID.Server) {
                for (int i = 0; i < 5; i++) {
                    int ind4 = Dust.NewDust(Projectile.Center - Vector2.One * 4, 8, 8, ModContent.DustType<Dusts.WeepingTulip>(), 0f, 0f, 0, default, 1f + Main.rand.NextFloatRange(0.1f));
                    Main.dust[ind4].velocity *= 0.95f;
                    Main.dust[ind4].noGravity = true;
                }
            }
            SoundEngine.PlaySound(SoundID.NPCHit7, Projectile.position);
        }
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    private class WeepingTulipExplosion : NatureProjectile {
        public override string Texture => ResourceManager.EmptyTexture;

        public override bool PreDraw(ref Color lightColor) => false;

        protected override void SafeSetDefaults() {
            //Projectile.Size = new Vector2(125, 75);
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;

            Projectile.friendly = true;
            Projectile.tileCollide = false;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.timeLeft = 10;

            ShouldApplyAttachedNatureWeaponCurrentDamage = false;
        }

        protected override void SafeOnSpawn(IEntitySource source) {
            Projectile.width = 6 + (int)(8f * Projectile.ai[2]);
            Projectile.height = 6 + (int)(8f * Projectile.ai[2]);
        }

        public override void AI() {
            Projectile.width = 6 + (int)(8f * Projectile.ai[2]);
            Projectile.height = 6 + (int)(8f * Projectile.ai[2]);
        }
    }
}
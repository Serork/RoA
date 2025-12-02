using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class AcalyphaTulip : NatureProjectile {
    private static ushort MAXTIMELEFT => 280;

    private static Asset<Texture2D>? _acalyphaTulipTexture2;

    private Color _tulipColor, _tulipColor2;

    public override Color? GetAlpha(Color lightColor) => new Color(255 - Projectile.alpha, 255 - Projectile.alpha, 255 - Projectile.alpha, 63 - Projectile.alpha / 4) * 0.9f;

    public override void Load() {
        if (Main.dedServ) {
            return;
        }

        _acalyphaTulipTexture2 = ModContent.Request<Texture2D>(Texture + "2");
    }

    public override void SetStaticDefaults() {
        Projectile.SetTrail(2, 18);
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: true);

        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = 2;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox.Inflate(4, 4);
    }

    protected override void SafeSendExtraAI(BinaryWriter writer) {
        writer.WriteRGB(_tulipColor);
        writer.WriteRGB(_tulipColor2);
    }

    protected override void SafeReceiveExtraAI(BinaryReader reader) {
        _tulipColor = reader.ReadRGB();
        _tulipColor2 = reader.ReadRGB();
    }

    public override void AI() {
        if (Projectile.localAI[1] == 0f) {
            //Projectile.frame = Main.rand.NextBool().ToInt();
            if (Projectile.IsOwnerLocal()) {
                _tulipColor = Color.Lerp(new Color(71, 167, 208), new Color(165, 18, 68), Main.rand.NextFloat()) * Main.rand.NextFloat(1f, 1.25f);
                _tulipColor2 = new Color(246, 73, 112) * Main.rand.NextFloat(0.75f, 1f);
                Projectile.ai[0] = Main.rand.NextFloat(1f, 2f);
                Projectile.netUpdate = true;
            }
            Projectile.localAI[1] = 10f;
        }
        float num829 = Projectile.localAI[1];
        Projectile.localAI[0] += 1f * Projectile.ai[0];
        if (Projectile.localAI[0] >= num829) {
            if (Projectile.ai[1] == 0f) {
                Projectile.ai[1] = 105 + 2.5f * Main.rand.NextFloat();
                if (Projectile.IsOwnerLocal()) {
                    Projectile.ai[2] = Main.rand.NextBool().ToDirectionInt();
                    Projectile.netUpdate = true;
                }
            }
            //Projectile.localAI[0] += Projectile.localAI[0] / Projectile.localAI[1];
            float factor = Utils.Remap(Projectile.localAI[0] - num829, 0f, Projectile.ai[1], 1f, 0f);
            if (Projectile.timeLeft <= MAXTIMELEFT / 2) {
                Projectile.tileCollide = true;
            }
            if (factor <= 0.03f && Projectile.localAI[2] == 0f) {
                for (int i = 0; i < 3; i++) {
                    MakeTulipDust();
                }
                Projectile.localAI[2] = 1f;
            }
            float angle = MathHelper.WrapAngle((Projectile.localAI[0] - num829) / MathHelper.TwoPi * -Projectile.direction * Projectile.ai[2] * factor);
            Projectile.position -= Projectile.velocity;
            Vector2 newVelocity = Projectile.velocity.RotatedBy(angle).SafeNormalize() * Projectile.velocity.Length();
            Projectile.position += newVelocity;
            Projectile.rotation = newVelocity.ToRotation() + (float)Math.PI / 2f;
        }
        else {
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2f;
        }
        Projectile.alpha = 100;
        int num830 = Projectile.owner;

        if (Main.rand.NextChance(0.05)) {
            MakeTulipDust();
        }

        //Projectile.position -= Projectile.velocity;
        //Projectile.position += Main.player[Projectile.owner].position - Main.player[Projectile.owner].oldPosition;
        //if (Math.Sign(Projectile.velocity.X) != Math.Sign(Main.player[num830].velocity.X) && Main.player[num830].velocity.X != 0f) {
        //    Projectile.Kill();
        //    return;
        //}
    }

    private void MakeTulipDust() {
        float offset2 = 10f;
        Vector2 randomOffset = Main.rand.RandomPointInArea(offset2, offset2),
                spawnPosition = Projectile.Center - randomOffset / 2f + randomOffset;

        //ushort dustType = CoreDustType();
        Dust dust = Dust.NewDustPerfect(Projectile.Center,
                                        ModContent.DustType<Dusts.Tulip>(),
                                        (spawnPosition - Projectile.Center).SafeNormalize(Vector2.Zero) * 2.5f * Main.rand.NextFloat(1.25f, 1.5f) - Projectile.velocity * 0.1f,
                                        Scale: Main.rand.NextFloat(0.5f, 0.8f) * Main.rand.NextFloat(1.25f, 1.5f) * 1.5f,
                                        Alpha: 4);
        dust.customData = Main.rand.NextFloatRange(50f);
    }

    public override bool PreDraw(ref Color lightColor) {
        if (_acalyphaTulipTexture2?.IsLoaded != true) {
            return false;
        }

        Projectile proj = Projectile;
        SpriteEffects dir = SpriteEffects.None;
        if (proj.spriteDirection == -1)
            dir = SpriteEffects.FlipHorizontally;

        Texture2D value48 = TextureAssets.Projectile[proj.type].Value;
        Vector2 position13 = proj.position + new Vector2(proj.width, proj.height) / 2f + Vector2.UnitY * proj.gfxOffY - Main.screenPosition;
        Vector2 scale4 = new Vector2(1f, proj.velocity.Length() / (float)value48.Height);
        //Main.instance.LoadNPC(139);
        Texture2D value49 = TextureAssets.Npc[139].Value;
        bool num244 = proj.velocity.X >= 0f;
        float rotation25 = proj.velocity.ToRotation() + (float)Math.PI;
        SpriteEffects effects3 = (num244 ? SpriteEffects.FlipVertically : SpriteEffects.None);
        float fromValue = 1f - proj.Opacity;
        float num245 = Utils.Remap(fromValue, 0f, 0.2f, 0f, 1f) * Utils.Remap(fromValue, 0.2f, 1f, 1f, 0f);
        //num245 = 1f;
        //Main.EntitySpriteDraw(value49, position13, null, lightColor * num245, rotation25, value49.Size() / 2f, 0.65f, effects3);
        Microsoft.Xna.Framework.Color color65 = new Microsoft.Xna.Framework.Color(255, 189, 163, 127) * num245;
        Microsoft.Xna.Framework.Color color66 = new Microsoft.Xna.Framework.Color(255, 21, 21, 127) * num245;
        Microsoft.Xna.Framework.Rectangle rectangle11 = value48.Frame(2);
        Vector2 origin16 = rectangle11.Bottom();
        var frame = _acalyphaTulipTexture2.Value.Frame(1);

        for (int num354 = proj.oldPos.Length - 1; num354 > 0; num354--) {
            Vector2 vector85 = proj.oldPos[num354] + new Vector2(proj.width, proj.height) / 2f + Vector2.UnitY * proj.gfxOffY - Main.screenPosition;
            Vector2 value91 = proj.oldPos[num354 - 1] + new Vector2(proj.width, proj.height) / 2f + Vector2.UnitY * proj.gfxOffY - Main.screenPosition;
            float num355 = proj.oldRot[num354];
            Vector2 scale14 = new Vector2(Vector2.Distance(vector85, value91) / (float)value48.Frame(2).Width * 1.5f, 0.1f);
            float lerpValue = (1f - (float)num354 / (float)proj.oldPos.Length);
            Microsoft.Xna.Framework.Color color98 = Color.Lerp(_tulipColor, _tulipColor2, MathUtils.Clamp01(MathF.Pow(lerpValue, 1.5f)));
            Main.EntitySpriteDraw(value48, vector85 - proj.oldPos[num354].DirectionTo(proj.oldPos[num354 - 1]) * 10f, value48.Frame(2), Lighting.GetColor((vector85 + Main.screenPosition).ToTileCoordinates()).MultiplyRGB(color98) * (1f - (float)num354 / (float)proj.oldPos.Length), num355, origin16, scale14, dir);
        }

        Main.EntitySpriteDraw(_acalyphaTulipTexture2.Value, Projectile.Center - Main.screenPosition, frame, lightColor, proj.rotation - MathHelper.PiOver4, frame.Size() / 2f, 1f, dir);
        Main.EntitySpriteDraw(value48, position13, rectangle11, lightColor.MultiplyRGBA(color65), proj.rotation, origin16, scale4, dir);
        rectangle11 = value48.Frame(2, 1, 1);
        Main.EntitySpriteDraw(value48, position13, rectangle11, lightColor.MultiplyRGBA(color66), proj.rotation, origin16, scale4, dir);

        return false;
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 5; i++) {
            int whoAmI = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, Projectile.velocity.X / 2f, Projectile.velocity.Y / 2f, 0, Color.Lerp(_tulipColor, _tulipColor2, Main.rand.NextFloat()), 1f);
            Main.dust[whoAmI].velocity *= Main.rand.NextFloat(0.8f, 1.1f);
            Main.dust[whoAmI].noGravity = true;

            MakeTulipDust();
        }
        SoundEngine.PlaySound(SoundID.NPCHit7, Projectile.position);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.Druid.Wreath;
using RoA.Common.VisualEffects;
using RoA.Content.Projectiles.Friendly.Nature.Forms;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

using static RoA.Common.Druid.Forms.BaseForm;

namespace RoA.Content.VisualEffects;

sealed class Leaf : VisualEffect<Leaf> {
    private static float BASECHANGEVALUE => 10f;

    private static Asset<Texture2D>? _glowTexture = null!;

    private Vector2 _newVelocity;

    public bool OnSet;
    public bool OnDismount;
    public float DisappearValue;

    protected override void SetDefaults() {
        TimeLeft = 300;

        SetFramedTexture(3, Main.rand.Next(3));

        AI0 = BASECHANGEVALUE;

        OnSet = OnDismount = false;

        _newVelocity = Velocity;

        DrawColor = HallowLeaf.GetColor(HallowLeaf.PickIndex());
        Scale *= Main.rand.NextFloat(0.5f, 0.75f);
        DisappearValue = 0f;

        DontEmitLight = true;
    }

    public override void OnLoad(Mod mod) {
        if (Main.dedServ) {
            return;
        }

        _glowTexture = ModContent.Request<Texture2D>(TexturePath + "_Glow");
    }

    public override void Update(ref ParticleRendererSettings settings) {
        bool onKill = AI0 == -2f;
        bool noGravity = AI0 != 0f || OnSet;
        if (onKill) {
            if (TimeLeft > MaxTimeLeft * 0.75f) {
                Velocity *= 0.85f;
            }
            else {
                Velocity.Y += 0.1f;
                Velocity.Y = MathF.Min(1f, Velocity.Y);
                _newVelocity = Velocity.RotatedByRandom(MathHelper.PiOver4);
                Velocity = Vector2.Lerp(Velocity, _newVelocity, 0.1f);
            }
        }
        if (noGravity) {
            Helper.ApplyWindPhysics(Position, ref Velocity);
            if (AI0-- <= 0f) {
                _newVelocity = Velocity.RotatedByRandom(MathHelper.PiOver4);

                AI0 = BASECHANGEVALUE;
            }
            else {
                Velocity = Vector2.Lerp(Velocity, _newVelocity, 0.1f);
            }
        }
        else {
            Scale *= 0.95f;
            Velocity *= 0.95f;
        }

        bool flag2 = (Collision.SolidCollision(Position - Vector2.One * 2, 4, 4) && noGravity && !onKill);
        bool flag3 = false;
        if (flag2 || DisappearValue > 0f) {
            if (flag2) {
                DisappearValue++;
            }
            if (DisappearValue <= 0f) {
                Scale *= 0.9f;
                Velocity *= 0.25f;
            }
            else {
                Velocity *= 0.25f;
            }
            flag3 = flag2;
        }
        if (!noGravity) {
            flag3 = true;
        }

        if (Scale <= 0.01f || --TimeLeft <= 0 || DisappearValue > 100f) {
            RestInPool();
        }

        if (!flag3) {
            Velocity.Y += 0.1f;
            Velocity.Y = MathF.Min(1f, Velocity.Y);
        }

        Position += Velocity;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        if (_glowTexture?.IsLoaded != true) {
            return;
        }
        if (CustomData is Player owner) {
            float progress = OnDismount ? -1f : BaseFormDataStorage.GetAttackCharge(owner);
            Color color = Color.Lerp(Lighting.GetColor(Position.ToTileCoordinates()), Color.White, MathF.Max(0f, HallowLeaf.EXTRABRIGHTNESSMODIFIER * progress)).MultiplyRGB(DrawColor);
            float opacity = 1f - Utils.GetLerpValue(50f, 100f, DisappearValue, true);
            opacity *= 1f - Utils.GetLerpValue(50f, 0f, TimeLeft, true);
            Draw_Inner(spritebatch, color: color * opacity);
            color = WreathHandler.GetArmorGlowColor_HallowEnt(owner, color, progress) * opacity;
            Draw_Inner(spritebatch, _glowTexture.Value, color);
        }
    }
}

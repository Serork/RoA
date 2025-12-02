using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Common.WorldEvents;
using RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;
using RoA.Content.Tiles.Ambient;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Content.VisualEffects;

sealed class SoulPart : VisualEffect<SoulPart> {
    public int Type { get; private set; }
    public Vector2 MoveTo { get; private set; }
    public Vector2[] Positions { get; private set; }
    public float Opacity { get; private set; }
    public Vector2 Size { get; private set; }

    public override Texture2D Texture => (Texture2D)ModContent.Request<Texture2D>(ModContent.GetModNPC(ModContent.NPCType<DruidSoul>()).Texture + "_Part");

    public SoulPart SetupPart(int type, Vector2 velocity, Vector2 position, Vector2 moveTo, float scale, float opacity) {
        Type = type;
        Velocity = velocity;
        Position = position;
        MoveTo = moveTo;
        Scale = scale;
        Opacity = opacity;

        Size = Texture.Size();

        SetDefaults();

        return this;
    }

    protected override void SetDefaults() {
        Positions = new Vector2[5];
    }

    public override void Update(ref ParticleRendererSettings settings) {
        TimeLeft = 2;

        bool flag = false/*Helper.EaseInOut3(OvergrownCoords.Strength) > 0.75f*/;
        if (Type == 0) {
            Position += Velocity * (flag ? Vector2.One : (Main.rand.NextVector2Circular(1f, 1f) + Vector2.One) * 0.5f);
            Vector2 velocity = Position - MoveTo;
            float speed = 125f - velocity.Length();
            if (speed > 0f) {
                Scale -= speed * (flag ? 0.0001f : 0.000085f);
            }
            velocity.Normalize();
            float counting = (float)((1.0 - (double)Scale) * 20.0);
            Vector2 velocity2 = velocity * -counting;
            Velocity = (Velocity * 4f + velocity2) / 5f;
            Velocity -= Main.rand.NextVector2Circular(1f, 1f);
            Velocity *= (Main.rand.NextVector2Circular(1f, 1f) + Vector2.One) * 0.5f;
            Scale += 0.01f;
            Opacity -= 0.0045f;
            float altarStrength = AltarHandler.GetAltarStrength();
            bool flag2 = Helper.EaseInOut3(altarStrength) > 0.65f;

            if (Vector2.Distance(Position + Velocity, MoveTo) < 40f) {
                Opacity -= 0.1f;
            }

            if (Opacity <= 0f || (!flag2 && Scale > 1.5f) || ShouldBeRemovedFromRenderer) {
                Opacity = 0f;
                TimeLeft = 0;
                RestInPool();
            }
        }
        else if (Type == 1) {
            Position += Velocity;
            Scale += 0.007f;
            Vector2 velocity = Position - MoveTo;
            float speed = 90f - velocity.Length();
            if (speed > 0f) {
                Scale -= speed * 0.0015f;
            }
            velocity.Normalize();
            float counting = (float)((double)Scale * 2.0);
            Vector2 velocity2 = velocity * -counting;
            Velocity = (Velocity * 4f + velocity2) / 5f;
            Opacity -= 0.0075f;

            float dif = Position.X - MoveTo.X;
            Position.X += MathF.Abs(dif) * 0.05f * -dif.GetDirection();

            if (Vector2.Distance(Position + Velocity, MoveTo) < 60f) {
                Opacity -= 0.2f;
            }
            if (Opacity <= 0f || ShouldBeRemovedFromRenderer) {
                Opacity = 0f;
                TimeLeft = 0;
                RestInPool();
            }
        }
        Array.Copy(Positions, 0, Positions, 1, Positions.Length - 1);
        Positions[0] = Position;
        for (int i = 0; i < Positions.Length; i++) {
            Positions[i] = Position;
            Positions[i] += Main.rand.Random2(Helper.Wave(0.25f, 1.5f, Main.rand.NextFloat(1f, 3f), 0.5f));
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spriteBatch) {
        float altarStrength = AltarHandler.GetAltarStrength();
        bool flag2 = Helper.EaseInOut3(altarStrength) > 0.65f;
        if (Type == 0 && !flag2) {
            Opacity *= 1f - Utils.GetLerpValue(1.4f, 1.7f, Scale, true);
        }
        Color color = Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16).MultiplyRGB(new Color(241, 53, 84, 200)) * Opacity;
        float distanceProgress = MathUtils.Clamp01(1f - (Vector2.Distance(Position, MoveTo) - 75f) / 50f);
        for (int index = 0; index < Positions.Length; index++) {
            float factor = (Positions.Length - (float)index) / Positions.Length;
            for (double i = -Math.PI; i <= Math.PI; i += Math.PI / 2.0) {
                Vector2 extraPosition = ((float)i).ToRotationVector2().RotatedBy(Main.GlobalTimeWrappedHourly * 2.0, new Vector2()) * Helper.Wave(0f, 3f, speed: 12f) - Main.screenPosition;
                Vector2 position = Positions[index] + extraPosition + Size / 2;
                Color baseColor = color.MultiplyAlpha(Opacity).MultiplyAlpha((float)i / Positions.Length) * factor;
                Color color2 = baseColor;
                color2.A = (byte)Utils.Lerp(color2.A, 0, distanceProgress);
                SpriteEffects effect = SpriteEffects.None;
                spriteBatch.Draw(Texture, position, null, color2 * (Opacity + 0.5f), 0f, Size / 2, Helper.Wave(Scale + 0.05f, Scale + 0.15f, 1f, 0f) * 0.9f * factor, effect, 0f);
            }
        }
    }
}

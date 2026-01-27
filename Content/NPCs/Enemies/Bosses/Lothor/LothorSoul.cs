using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed class LothorSoul : RoANPC {
    private static Asset<Texture2D> _eyeTexture = null!;

    private static float FirstFrameTime => 4f;
    private static float AnimationSpeed => 27.5f;

    private readonly Color _color = new(241, 53, 84, 200), _color2 = new(114, 216, 102, 200);

    public enum States {
        Spawned,
        Disappeared
    }

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Lothor's Soul");

        Main.npcFrameCount[Type] = 4;

        NPCID.Sets.TrailCacheLength[Type] = 6;
        NPCID.Sets.TrailingMode[Type] = 1;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Hide = true
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        if (Main.dedServ) {
            return;
        }

        _eyeTexture = ModContent.Request<Texture2D>(Texture + "_Eye");
    }

    public override void SetDefaults() {
        base.SetDefaults();

        NPC.lifeMax = 10;

        int width = 54; int height = 58;
        NPC.Size = new Vector2(width, height);

        NPC.noTileCollide = NPC.friendly = true;

        NPC.friendly = true;
        NPC.noGravity = true;

        NPC.immortal = NPC.dontTakeDamage = true;

        NPC.aiStyle = AIType = -1;

        NPC.Opacity = 0f;
    }

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = -NPC.direction;

        bool flag = State == (float)States.Disappeared;
        NPC.frame = new Rectangle(flag ? NPC.width + 2 : 0, NPC.frame.Y, NPC.width, NPC.height);

        double maxCounter = 6.0;
        if (flag) {
            if (++NPC.frameCounter >= maxCounter * (Main.npcFrameCount[Type] - 1)) {
                NPC.frameCounter = 0.0;
            }
            int currentFrame = (int)(NPC.frameCounter / maxCounter);
            NPC.frame.Y = currentFrame * frameHeight;
        }
        else if (StateTimer > FirstFrameTime) {
            int currentFrame = (int)(StateTimer - FirstFrameTime);
            NPC.frame.Y = currentFrame * frameHeight;

            NPC.frameCounter = 0.0;
        }
    }

    public override void AI() {
        Vector2 velocity = NPC.velocity;
        float rotation = velocity.X * 0.05f;
        NPC.rotation = rotation;

        for (int i = 0; i < NPC.oldPos.Length; i++) {
            float randomOffset = Helper.Wave(0.25f, 1.5f, Main.rand.NextFloat(1f, 3f), 0.5f);
            Vector2 randomness = Main.rand.Random2(randomOffset);
            NPC.oldPos[i] += randomness;
        }

        NPC.TargetClosest();

        switch (State) {
            case (float)States.Spawned:
                Appearance();
                break;
            case (float)States.Disappeared:
                Disappear();
                break;
        }
    }

    private void Appearance() {
        StateTimer += AnimationSpeed / 250f;
        if (StateTimer > 3f + FirstFrameTime) {
            ChangeState((int)States.Disappeared);
            StateTimer = 0f;
        }
        else {
            //NPC.velocity.Y -= 0.0075f;
            //NPC.velocity.Y *= 1f + 0.02f * (1f - MathHelper.Clamp(StateTimer * 2f / 3f, 0f, 1f));
        }

        NPC.Opacity = 0.35f;
    }

    private void Disappear() {
        if (NPC.velocity.Y > -10f) {
            NPC.velocity.Y = Helper.Approach(NPC.velocity.Y, NPC.velocity.Y - 0.07f, 0.035f);
        }

        if (++StateTimer > 35f) {
            NPC.Opacity -= 0.005f;

            if (NPC.Opacity <= 0f) {
                NPC.KillNPC();
            }
        }
        else {
            NPC.Opacity = 0.35f;
            NPC.velocity *= 0.925f;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Texture2D texture = NPC.GetTexture();
        spriteBatch.Draw(texture, NPC.position + NPC.Size / 2 - screenPos - Vector2.UnitY * 6f, NPC.frame, drawColor * NPC.Opacity, NPC.rotation, NPC.Size / 2, 1f, NPC.direction != 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        Color color = drawColor.MultiplyRGB(_color);
        DrawTextureUnderCustomSoulEffect(spriteBatch, texture, color);
        return false;
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        DrawTextureUnderCustomSoulEffect(spriteBatch, _eyeTexture.Value, drawColor);
    }

    private void DrawTextureUnderCustomSoulEffect(SpriteBatch spriteBatch, Texture2D texture, Color drawColor) {
        Color color = drawColor * NPC.Opacity;
        for (int index = 0; index < NPC.oldPos.Length; index++) {
            float factor = (NPC.oldPos.Length - (float)index) / NPC.oldPos.Length;
            for (double i = -Math.PI; i <= Math.PI; i += Math.PI / 2.0) {
                Vector2 position = NPC.oldPos[index] - Vector2.UnitY * 6f + NPC.Size / 2 + Utils.RotatedBy(Utils.ToRotationVector2((float)i), TimeSystem.TimeForVisualEffects * 2.0, new Vector2()) * Helper.Wave(0f, 3f, speed: 12f) - Main.screenPosition;
                Color color2 = color.MultiplyAlpha(NPC.Opacity).MultiplyAlpha((float)i / NPC.oldPos.Length) * factor;
                spriteBatch.Draw(texture, position + Utils.RotatedBy(Utils.ToRotationVector2((float)i), TimeSystem.TimeForVisualEffects * 5.0, new Vector2()) * Helper.Wave(0f, 2f, speed: 20f), NPC.frame, color2 * (NPC.Opacity + 0.25f) * 0.75f, NPC.rotation + Main.rand.NextFloat(0.075f), NPC.Size / 2, Helper.Wave(NPC.scale + 0.05f, NPC.scale + 0.15f, 1f, 0f) * 0.9f * factor, NPC.direction != 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
            }
        }
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Biomes.Backwoods;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class DeerSkull : RoANPC {
	public override void SetDefaults() {
		NPC.lifeMax = 200;
		NPC.damage = 44;
		NPC.defense = 8;
		NPC.knockBackResist = 0.1f;

		int width = 50; int height = 50;
		NPC.Size = new Vector2(width, height);

		NPC.aiStyle = -1;

		NPC.npcSlots = 1.25f;
		NPC.value = Item.buyPrice(0, 0, 25, 5);

		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;

		NPC.noTileCollide = true;
		NPC.noGravity = true;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
    }

    public override void AI() {
		if (NPC.ai[0] == 0f) {
			NPC.ai[0] = Main.rand.Next(5, 11);
			NPC.netUpdate = true;
		}
		NPC.direction = NPC.velocity.X.GetDirection();
		NPC.velocity = Helper.VelocityToPoint(NPC.Center, Main.MouseWorld, 1f);
		NPC.rotation = Helper.VelocityAngle(NPC.velocity) - MathHelper.PiOver2 * NPC.direction;
		NPC.position -= NPC.velocity;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.DeerSkull")
        ]);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		Vector2 drawPosition = NPC.Center - screenPos;
		Texture2D texture = TextureAssets.Npc[Type].Value;
		Rectangle sourceRectangle = new(56, 0, 26, 60);
		SpriteEffects effects = (SpriteEffects)(NPC.velocity.X < 0f).ToInt();
		Vector2 moveDirection = NPC.rotation.ToRotationVector2() * NPC.direction;
		int index = 0;
		int length = (int)NPC.ai[0];
		int bodyHeight = 16;
		float startOriginX = 10f;
		Vector2 origin = new(NPC.direction == -1 ? sourceRectangle.Width - startOriginX : startOriginX, sourceRectangle.Height / 2f + 4f);
        Vector2 start = drawPosition + Vector2.UnitY * bodyHeight / 2f;
		Vector2 bodyOrigin = new Vector2(16, 38) / 2f;
		Vector2 offsetY = Vector2.UnitY * origin.Y / 4f;
        while (index < length) {
			int bodyWidth = 16;
			float value = (float)(index + 1) / length;
			float max = 10f * value;
			float sin = Helper.Wave(-max, max, 5f, index / 2f);
            Vector2 wave = moveDirection * sin;
            wave = new(-wave.Y, wave.X);
            Vector2 offset2 = moveDirection * 10f + new Vector2(0f, 10f).RotatedBy(NPC.rotation);
            if (index == length - 1) {
				Vector2 move = moveDirection * (bodyWidth - 1);
                start -= move;
				float rotation = Helper.VelocityAngle(move) - MathHelper.PiOver2 * NPC.direction;
                Rectangle bodySourceRectangle = new(0, 24, bodyWidth, bodyHeight);
                Main.EntitySpriteDraw(texture, start - offsetY + wave + offset2, bodySourceRectangle, drawColor, rotation, bodyOrigin, NPC.scale, effects);
            }
			else if (index == 0) {
				bodyWidth = 16;
				bodyHeight = 38;
                Vector2 move = moveDirection * bodyWidth * 1.5f;
                start -= move;
				float rotation = Helper.VelocityAngle(move) - MathHelper.PiOver2 * NPC.direction;
                Rectangle bodySourceRectangle = new(38, 22, bodyWidth, bodyHeight);
                Main.EntitySpriteDraw(texture, start - offsetY + wave * 0.75f + offset2, bodySourceRectangle, drawColor, rotation, bodyOrigin, NPC.scale, effects);
                Main.EntitySpriteDraw(texture, drawPosition + wave, sourceRectangle, drawColor, rotation, origin, NPC.scale, effects);
            }
			else {
                Vector2 move = moveDirection * (bodyWidth - 2);
                start -= move;
				float rotation = Helper.VelocityAngle(move) - MathHelper.PiOver2 * NPC.direction;
                Rectangle bodySourceRectangle = new(20, 22, bodyWidth, bodyHeight);
                Main.EntitySpriteDraw(texture, start - offsetY + wave + offset2, bodySourceRectangle, drawColor, rotation, bodyOrigin, NPC.scale, effects);
			}
            index += 1;
        }

		return false;
    }
}
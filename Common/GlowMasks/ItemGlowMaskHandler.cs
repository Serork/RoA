using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;
using ReLogic.Utilities;

using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.GlowMasks;

sealed class ItemGlowMaskHandler : PlayerDrawLayer {
    private const string REQUIREMENT = "_Glow";
    
	private static Dictionary<int, Asset<Texture2D>> _glowMasks;

	private class ItemGlowMaskWorld : GlobalItem {
        public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
            if (item.type >= ItemID.Count && _glowMasks.TryGetValue(item.type, out Asset<Texture2D> glowMask)) {
                Texture2D glowMaskTexture = glowMask.Value;
                Vector2 origin = glowMaskTexture.Size() / 2f;
				Color color = item.GetAlpha(Color.White);
				spriteBatch.Draw(glowMaskTexture, item.Center - Main.screenPosition, null, color, rotation, origin, 1f, SpriteEffects.None, 0f);
			}
        }
    }

    public static void AddGlowMask(ushort type, Asset<Texture2D> texture) => _glowMasks[type] = texture;
    public static void AddGlowMask(ModItem modItem) => AddGlowMask((ushort)modItem.Type, ModContent.Request<Texture2D>(modItem.Texture + REQUIREMENT));

    public override void Load() {
        if (Main.dedServ) {
            return;
        }

        _glowMasks = [];
	}

    public override void SetStaticDefaults() {
        if (Main.dedServ) {
            return;
        }

		LoadGlowMasks();
    }

    private static void LoadGlowMasks() {
        foreach (ModItem item in RoA.Instance.GetContent<ModItem>()) {
            AutoloadGlowMaskAttribute attribute = item?.GetType().GetAttribute<AutoloadGlowMaskAttribute>();
            if (attribute != null) {
                string modItemTexture = item.Texture;
                Asset<Texture2D> texture = ModContent.Request<Texture2D>(modItemTexture + REQUIREMENT, AssetRequestMode.ImmediateLoad);
                texture.Value.Name = modItemTexture;
				AddGlowMask((ushort)item.Type, texture);
            }
        }
    }

    public override void Unload() => _glowMasks.Clear();

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => _glowMasks != null;

	public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ArmOverItem);

	protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (drawInfo.shadow != 0f || !player.active) {
			return;
		}

        Item item = player.HeldItem;
		if (player.frozen || ((drawInfo.drawPlayer.itemAnimation <= 0 || item.useStyle == ItemUseStyleID.None) && (item.holdStyle <= 0 || drawInfo.drawPlayer.pulley)) || drawInfo.drawPlayer.dead || item.noUseGraphic || (drawInfo.drawPlayer.wet && item.noWet)) {
			return;
		}

		if (item.type >= ItemID.Count && _glowMasks.TryGetValue(item.type, out Asset<Texture2D> glowMask)) {
			Texture2D texture = glowMask.Value;
			Vector2 offset = new();
			float rotOffset = 0f;
			Vector2 origin = new();
			bool gravReversed = player.gravDir == -1f,
				 leftDirection = player.direction == -1;
            if (item.useStyle == ItemUseStyleID.Shoot) {
				if (Item.staff[item.type]) {
					rotOffset = MathHelper.PiOver4 * player.direction;
					if (gravReversed) {
						rotOffset -= MathHelper.PiOver2 * player.direction;
					}
					origin = new Vector2(texture.Width * 0.5f * (1f - player.direction), gravReversed ? 0 : texture.Height);
					int x = -(int)origin.X;
					ItemLoader.HoldoutOrigin(player, ref origin);
					offset = new Vector2(origin.X + x, 0f);
				}
				else {
					offset = new Vector2(10f, texture.Height / 2f);
					ItemLoader.HoldoutOffset(player.gravDir, item.type, ref offset);
					origin = new Vector2(-offset.X, texture.Height / 2);
					if (leftDirection) {
						origin.X = texture.Width + offset.X;
					}
					offset = new Vector2(texture.Width / 2f, offset.Y);
				}
			}
			else {
				origin = new Vector2(texture.Width * 0.5f * (1 - player.direction), gravReversed ? 0 : texture.Height);
			}

			drawInfo.DrawDataCache.Add(new DrawData(texture, (player.itemLocation - Main.screenPosition + offset + new Vector2(0f, player.gfxOffY)).Floor(), texture.Bounds,
                                                    item.GetAlpha(Color.White), player.itemRotation + rotOffset, origin, item.scale, drawInfo.itemEffect, 0));
		}
	}
}
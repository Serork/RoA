using Microsoft.Build.Tasks;
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
	public readonly struct GlowMaskInfo(Asset<Texture2D> glowMask, Color glowColor) {
		public readonly Asset<Texture2D> GlowMask = glowMask;
        public readonly Color GlowColor = glowColor;
    }

    private const string REQUIREMENT = "_Glow";
    
	private static Dictionary<int, GlowMaskInfo> _glowMasks;

	private class ItemGlowMaskWorld : GlobalItem {
        public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
            if (item.type >= ItemID.Count && _glowMasks.TryGetValue(item.type, out GlowMaskInfo glowMaskInfo)) {
                Texture2D glowMaskTexture = glowMaskInfo.GlowMask.Value;
				Vector2 origin = glowMaskTexture.Size() / 2f;
                Color color = Color.Lerp(glowMaskInfo.GlowColor, Lighting.GetColor((int)item.Center.X / 16, (int)item.Center.Y / 16), Lighting.Brightness((int)item.Center.X / 16, (int)item.Center.Y / 16));
				spriteBatch.Draw(glowMaskTexture, item.Center - Main.screenPosition, null, item.GetAlpha(color), rotation, origin, 1f, SpriteEffects.None, 0f);
			}
        }
    }

    //ModContent.Request<Texture2D>(modItem.Texture + REQUIREMENT)
    public static void AddGlowMask(ushort type, GlowMaskInfo glowMaskInfo) => _glowMasks[type] = glowMaskInfo;
    public static void AddGlowMask(ModItem modItem, GlowMaskInfo glowMaskInfo) => AddGlowMask((ushort)modItem.Type, glowMaskInfo);

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
				GlowMaskInfo glowMaskInfo = new(texture, attribute.GlowColor);
                AddGlowMask((ushort)item.Type, glowMaskInfo);
            }
        }
    }

	public override void Unload() {
		_glowMasks.Clear();
		_glowMasks = null;
    }

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => _glowMasks != null;

	public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ArmOverItem);

	protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (drawInfo.shadow != 0f || !player.active) {
			return;
		}

        Item item = player.HeldItem;
		if (player.frozen || ((player.itemAnimation <= 0 || item.useStyle == ItemUseStyleID.None) && (item.holdStyle <= 0 || player.pulley)) || player.dead || item.noUseGraphic || (player.wet && item.noWet)) {
			return;
		}

		if (item.type >= ItemID.Count && _glowMasks.TryGetValue(item.type, out GlowMaskInfo glowMaskInfo)) {
			Texture2D texture = glowMaskInfo.GlowMask.Value;
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

			Vector2 position = (player.itemLocation + offset + new Vector2(0f, player.gfxOffY)).Floor();
            Color color = Color.Lerp(glowMaskInfo.GlowColor, drawInfo.itemColor, Lighting.Brightness((int)position.X / 16, (int)position.Y / 16));
			drawInfo.DrawDataCache.Add(new DrawData(texture, position - Main.screenPosition, texture.Bounds,
                                                    item.GetAlpha(color), player.itemRotation + rotOffset, origin, item.scale, drawInfo.itemEffect, 0));
		}
	}
}
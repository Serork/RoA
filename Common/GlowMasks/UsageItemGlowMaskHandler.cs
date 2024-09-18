using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RiseofAges;

sealed class UsageItemGlowMaskHandler : PlayerDrawLayer {
    private const string REQUIREMENT = "_Glow";
    
	private static Dictionary<int, Asset<Texture2D>> _glowMasks;

    public static void AddGlowMask(ModItem modItem) => _glowMasks[modItem.Type] = ModContent.Request<Texture2D>(modItem.Texture + REQUIREMENT);

    public override void Load() {
        if (Main.dedServ) {
            return;
        }

        _glowMasks = [];
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

		if (player.HeldItem.type >= ItemID.Count && _glowMasks.TryGetValue(player.HeldItem.type, out Asset<Texture2D> textureItem)) {
			Texture2D texture = textureItem.Value;
			Vector2 offset = new();
			float rotOffset = 0f;
			Vector2 origin = new();
			bool flag = player.gravDir == -1f,
				 flag2 = player.direction == -1;
            if (item.useStyle == ItemUseStyleID.Shoot) {
				if (Item.staff[item.type]) {
					rotOffset = MathHelper.PiOver4 * player.direction;
					if (flag) {
						rotOffset -= MathHelper.PiOver2 * player.direction;
					}
					origin = new Vector2(texture.Width * 0.5f * (1f - player.direction), flag ? 0 : texture.Height);
					int x = -(int)origin.X;
					ItemLoader.HoldoutOrigin(player, ref origin);
					offset = new Vector2(origin.X + x, 0f);
				}
				else {
					offset = new Vector2(10f, texture.Height / 2f);
					ItemLoader.HoldoutOffset(player.gravDir, item.type, ref offset);
					origin = new Vector2(-offset.X, texture.Height / 2);
					if (flag2) {
						origin.X = texture.Width + offset.X;
					}
					offset = new Vector2(texture.Width / 2f, offset.Y);
				}
			}
			else {
				origin = new Vector2(texture.Width * 0.5f * (1 - player.direction), flag ? 0 : texture.Height);
			}
			drawInfo.DrawDataCache.Add(new DrawData(texture, player.itemLocation - Main.screenPosition + offset + new Vector2(0f, player.gfxOffY), texture.Bounds,
													new Color(250, 250, 250, item.alpha), player.itemRotation + rotOffset, origin, item.scale, drawInfo.itemEffect, 0));
		}
	}
}
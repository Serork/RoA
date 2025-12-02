using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Buffs;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Pets;

sealed class MoonFlower : ModItem {
    public static Asset<Texture2D> GlowTexture { get; private set; } = null!;

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Moon Flower");
        // Tooltip.SetDefault("Summons a small moon to provide light");

        if (Main.dedServ) {
            return;
        }

        GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    public override void SetDefaults() {
        int width = 26; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.useTime = 22;
        Item.useAnimation = 22;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = false;
        Item.UseSound = SoundID.Item8;

        Item.value = Item.sellPrice(0, 5, 0, 0);
        Item.rare = ItemRarityID.Orange;

        Item.buffType = ModContent.BuffType<SmallMoon>();
        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Pets.SmallMoon>();

        Item.shootSpeed = 3.5f;
        Item.master = true;
        //Item.glowMask = RoAGlowMask.Get(nameof(MoonFlower));
    }

    public override bool? UseItem(Player player) {
        if (player.whoAmI == Main.myPlayer && player.itemTime == 0) player.AddBuff(Item.buffType, 3600, true);
        return null;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<SmallMoonPlayer>().smallMoon = true;
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
        Texture2D glowmask = GlowTexture.Value;
        Player player = Main.player[Main.myPlayer];
        ushort type = (ushort)ModContent.ItemType<MoonFlower>();
        Color color = player.GetModPlayer<SmallMoonPlayer>().smallMoonColor;

        float globalTimeWrappedHourly2 = Main.GlobalTimeWrappedHourly;
        globalTimeWrappedHourly2 %= 5f;
        globalTimeWrappedHourly2 /= 2.5f;
        if (globalTimeWrappedHourly2 >= 1f)
            globalTimeWrappedHourly2 = 2f - globalTimeWrappedHourly2;
        color.A = (byte)(60 + 100 * globalTimeWrappedHourly2);
        Vector2 drawPosition = position;
        float scale_ = Main.inventoryScale;
        spriteBatch.Draw(glowmask, position, null,
                        color, 0f, glowmask.Size() / 2f, new Vector2(scale_ *= 1f), SpriteEffects.None, 0);

        spriteBatch.Draw(glowmask, position,
                         new Rectangle(0, 0, glowmask.Width, glowmask.Height),
                         color,
                         0f,
                         glowmask.Size() * 0.5f,
                         scale,
                         SpriteEffects.None,
                         0f);
        //foreach (Item item in player.miscEquips) {
        //    if (item.stack > 0 && item.type == type) {
        //        float scale_ = Main.inventoryScale;
        //        spriteBatch.DrawSelf(glowmask, position, null,
        //        color, 0f, glowmask.Size() / 2f, new Vector2(scale_ *= 1f), SpriteEffects.None, 0);
        //    }
        //}
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
        Player player = Main.player[Main.myPlayer];
        Color color = player.GetModPlayer<SmallMoonPlayer>().smallMoonColor;
        color.A = 80;
        Lighting.AddLight(Item.position, player.GetModPlayer<SmallMoonPlayer>().smallMoonColor.ToVector3());
        Texture2D texture = TextureAssets.Item[Type].Value;
        Texture2D glowmask = GlowTexture.Value;
        spriteBatch.Draw(glowmask, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
            new Rectangle(0, 0, texture.Width, texture.Height),
            color,
            rotation,
            texture.Size() * 0.5f,
            scale,
            SpriteEffects.None,
            0f
        );

        var shader = GameShaders.Armor.GetSecondaryShader(player.cLight, player);
        ArmorShaderData armorShaderData = null;
        if (shader != armorShaderData) {
            spriteBatch.End();
            armorShaderData = shader;
            if (armorShaderData == null) {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
            }
            else {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);
                shader.Apply(null);
            }
        }

        spriteBatch.EndBlendState();
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
    }
}
sealed class MoonFlowerUseGlow : PlayerDrawLayer {
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        => drawInfo.drawPlayer.inventory[drawInfo.drawPlayer.selectedItem].type == ModContent.ItemType<MoonFlower>();

    public override Position GetDefaultPosition()
        => new BeforeParent(PlayerDrawLayers.ArmOverItem);

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        if (drawInfo.hideEntirePlayer) {
            return;
        }

        Player player = drawInfo.drawPlayer;
        Texture2D texture = MoonFlower.GlowTexture.Value;
        if (drawInfo.shadow != 0f || !player.IsAliveAndFree() || player.itemAnimation <= 0)
            return;
        Color color = player.GetModPlayer<SmallMoonPlayer>().smallMoonColor;
        color.A = 80;
        Vector2 position = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y));
        Vector2 offset = new Vector2(player.direction == -1 ? texture.Width : 0, player.gravDir == -1f ? 0 : texture.Height);
        DrawData drawData = new DrawData(texture, position, null,
                                        color, player.itemRotation, offset,
                                        player.HeldItem.scale, SpriteEffects.None, 0);
        drawInfo.DrawDataCache.Add(drawData);
    }
}
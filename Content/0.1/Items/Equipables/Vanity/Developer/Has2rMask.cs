using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common.GlowMasks;
using RoA.Common.Players;
using RoA.Core;
using RoA.Core.Graphics.Data;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity.Developer;

[AutoloadEquip(EquipType.Head)]
[AutoloadGlowMask2("_Head_Glow")]
sealed class Has2rMask : ModItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
        if (drawPlayer.active && drawPlayer.hair == 26) {
            glowMask = VanillaGlowMaskHandler.GetID(Texture + "_Head_Glow");
            glowMaskColor = Color.White * 0.9f;
        }
    }

    public override void SetDefaults() {
        int width = 18; int height = 24;
        Item.Size = new Vector2(width, height);

        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.buyPrice(gold: 5);
        Item.vanity = true;
    }

    public override bool IsVanitySet(int head, int body, int legs)
       => (head == EquipLoader.GetEquipSlot(Mod, nameof(Has2rMask), EquipType.Head) || head == EquipLoader.GetEquipSlot(Mod, nameof(Has2rShades), EquipType.Head)) &&
          body == EquipLoader.GetEquipSlot(Mod, nameof(Has2rJacket), EquipType.Body) &&
          legs == EquipLoader.GetEquipSlot(Mod, nameof(Has2rPants), EquipType.Legs);

    private class TentaclesDrawLayer : ILoadable {
        private static Asset<Texture2D> _tentacleTexture = null!;

        private const int MAX = 3;

        private Vector2 tentaclePosition;

        private readonly bool[] pulseBack = new bool[MAX];
        private readonly bool[] rotationBack = new bool[MAX];

        private readonly float[] scale = new float[MAX];
        private readonly float[] pulse = new float[MAX];
        private readonly float[] rotation = new float[MAX];
        private readonly float[] randomMovement = new float[MAX];

        void ILoadable.Load(Mod mod) {
            On_PlayerDrawLayers.DrawPlayer_08_Backpacks += On_PlayerDrawLayers_DrawPlayer_08_Backpacks;

            if (!Main.dedServ) {
                _tentacleTexture = ModContent.Request<Texture2D>(ResourceManager.DeveloperEquipableTextures + "Tentacle");
            }
        }

        private void On_PlayerDrawLayers_DrawPlayer_08_Backpacks(On_PlayerDrawLayers.orig_DrawPlayer_08_Backpacks orig, ref PlayerDrawSet drawInfo) {
            Player player = drawInfo.drawPlayer;
            bool flag2 = false;
            if (drawInfo.shadow == 0) {
                int itemType = ModContent.ItemType<Has2rMask>();
                bool flag = ItemLoader.GetItem(itemType).IsVanitySet(player.head, player.body, player.legs);
                if (flag) {
                    Player _player = drawInfo.drawPlayer;
                    for (int i = 0; i < MAX; i++) {
                        if (!Main.gamePaused)
                            randomMovement[i] = Main.rand.NextFloat(-0.1f, 0.1f);
                        Vector2 _position = new(drawInfo.Center.X - (float)((i == 0 ? -1 : (i - 1f)) * 6) - 13f * _player.direction, drawInfo.Center.Y - 10f);
                        if (_player.gravDir == -1.0) _position.Y += 50f;
                        tentaclePosition = new((float)(int)_position.X, (float)(int)_position.Y);

                        if (_player.gravDir == -1.0)
                            tentaclePosition.Y += 50f;

                        if (!Main.gamePaused) {
                            Pulsation(i);
                            Rotation(i);
                        }

                        int shader = 0;
                        shader = drawInfo.cBody;

                        if (i == 0) {
                            shader = drawInfo.cHead;
                        }
                        if (i == 2) {
                            shader = drawInfo.cLegs;
                        }

                        var asset = _tentacleTexture;
                        Texture2D _texture = asset.Value;
                        Vector2 _position2 = tentaclePosition - Main.screenPosition;
                        Vector2 _origin = new(_texture.Width * 0.5f, _texture.Height * 0.5f);
                        Color _color = new(255, 215, 0, 140);
                        bool _flag = (i == 0 ? -1f : (float)(i - 1f)) != 0 ? _player.direction * (int)_player.gravDir < 0 : _player.direction * (int)_player.gravDir > 0;
                        SpriteEffects _effect = _flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        DrawData _drawData = new(_texture, _position2, null, _color, rotation[i], _origin, scale[i], _effect, 0);
                        _drawData.shader = shader;
                        drawInfo.DrawDataCache.Add(_drawData);
                    }

                    flag2 = true;

                    //return;
                }
            }

            ExtraDrawLayerSupport.DrawBackpacks(ref drawInfo);
            if (flag2) {
                return;
            }


            orig(ref drawInfo);
        }

        void ILoadable.Unload() { }

        private void Pulsation(int index) {
            scale[index] = 1f + pulse[index];
            float _speed = 0.0025f;
            pulse[index] += _speed * (pulseBack[index] ? -1f : 1f);
            if (pulseBack[index]) {
                if (pulse[index] <= randomMovement[index] - 0.2) pulseBack[index] = false;
            }
            else { if (pulse[index] >= randomMovement[index] + 0.2) pulseBack[index] = true; }
        }

        private void Rotation(int index) {
            float _speed = 0.0025f;
            rotation[index] += _speed * (rotationBack[index] ? -1f : 1f);
            if (rotationBack[index]) {
                if (rotation[index] <= randomMovement[index] - 0.18f) rotationBack[index] = false;
            }
            else { if (rotation[index] >= randomMovement[index] + 0.18f) rotationBack[index] = true; }
        }
    }
}

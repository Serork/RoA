using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using RoA.Content.NPCs.Enemies.Backwoods;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;

sealed partial class DruidSoul : RoANPC {
    private const float MAXDISTANCETOALTAR = 300f;

    public override void SetDefaults() {
        NPC.lifeMax = 10;

        int width = 28; int height = 50;
        NPC.Size = new Vector2(width, height);

        NPC.noTileCollide = NPC.friendly = true;

        NPC.friendly = true;
        NPC.noGravity = true;

        NPC.immortal = NPC.dontTakeDamage = true;

        NPC.aiStyle = AIType = -1;

        NPC.npcSlots = 5f;
    }

    // separate
    public override void Load() {
        On_Main.HoverOverNPCs += On_Main_HoverOverNPCs;
    }

    private static bool TryFreeingElderSlime(int npcIndex) {
        Player player = Main.player[Main.myPlayer];
        short type = 327;
        bool inVoidBag = false;
        int num = player.FindItemInInventoryOrOpenVoidBag(type, out inVoidBag);
        if (num == -1)
            return false;

        Item item = null;
        item = ((!inVoidBag) ? player.inventory[num] : player.bank4.item[num]);
        if (--item.stack <= 0)
            item.TurnToAir();

        Recipe.FindRecipes();
        return true;
    }

    private void On_Main_HoverOverNPCs(On_Main.orig_HoverOverNPCs orig, Main self, Rectangle mouseRectangle) {
        Player player = Main.player[Main.myPlayer];
        for (int i = 0; i < 200; i++) {
            NPC nPC = Main.npc[i];
            if (!(nPC.active & (nPC.shimmerTransparency == 0f || nPC.CanApplyHunterPotionEffects())))
                continue;

            int type = nPC.type;
            if (TextureAssets.Npc[type].State == AssetState.NotLoaded)
                Main.Assets.Request<Texture2D>(TextureAssets.Npc[type].Name);
            nPC.position += nPC.netOffset;
            if (type == ModContent.NPCType<DruidSoul>() && nPC.Opacity < 0.38f) {
                continue;
            }
            //if (type == Type && _isAngry) {
            //    continue;
            //}
            Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle((int)nPC.Bottom.X - nPC.frame.Width / 2, (int)nPC.Bottom.Y - nPC.frame.Height, nPC.frame.Width, nPC.frame.Height);
            if (nPC.type >= 87 && nPC.type <= 92)
                value = new Microsoft.Xna.Framework.Rectangle((int)((double)nPC.position.X + (double)nPC.width * 0.5 - 32.0), (int)((double)nPC.position.Y + (double)nPC.height * 0.5 - 32.0), 64, 64);

            bool flag = mouseRectangle.Intersects(value);
            bool flag2 = flag || (Main.SmartInteractShowingGenuine && Main.SmartInteractNPC == i);
            if (flag2 && ((nPC.type != 85 && nPC.type != 341 && nPC.type != 629 && nPC.aiStyle != 87) || nPC.ai[0] != 0f) && nPC.type != 488) {
                if (nPC.type == 685) {
                    player.cursorItemIconEnabled = true;
                    player.cursorItemIconID = 327;
                    player.cursorItemIconText = "";
                    player.noThrow = 2;
                    if (!player.dead) {
                        PlayerInput.SetZoom_MouseInWorld();
                        if (Main.mouseRight && Main.npcChatRelease) {
                            Main.npcChatRelease = false;
                            if (PlayerInput.UsingGamepad)
                                player.releaseInventory = false;

                            if (player.talkNPC != i && !player.tileInteractionHappened && TryFreeingElderSlime(i)) {
                                NPC.TransformElderSlime(i);
                                SoundEngine.PlaySound(SoundID.Unlock);
                            }
                        }
                    }
                }
                else {
                    bool flag3 = Main.SmartInteractShowingGenuine && Main.SmartInteractNPC == i;
                    if (nPC.townNPC || nPC.type == 105 || nPC.type == 106 || nPC.type == 123 || nPC.type == 354 || nPC.type == 376 || nPC.type == 579 || nPC.type == 453 || nPC.type == 589) {
                        Microsoft.Xna.Framework.Rectangle rectangle = new Microsoft.Xna.Framework.Rectangle((int)(player.position.X + (float)(player.width / 2) - (float)(Player.tileRangeX * 16)), (int)(player.position.Y + (float)(player.height / 2) - (float)(Player.tileRangeY * 16)), Player.tileRangeX * 16 * 2, Player.tileRangeY * 16 * 2);
                        Microsoft.Xna.Framework.Rectangle value2 = new Microsoft.Xna.Framework.Rectangle((int)nPC.position.X, (int)nPC.position.Y, nPC.width, nPC.height);
                        if (rectangle.Intersects(value2))
                            flag3 = true;
                    }

                    if (player.ownedProjectileCounts[651] > 0)
                        flag3 = false;

                    if (flag3 && !player.dead) {
                        PlayerInput.SetZoom_MouseInWorld();
                        Main.HoveringOverAnNPC = true;
                        Main.instance.currentNPCShowingChatBubble = i;
                        if (Main.mouseRight && Main.npcChatRelease) {
                            Main.npcChatRelease = false;
                            if (PlayerInput.UsingGamepad)
                                player.releaseInventory = false;

                            if (player.talkNPC != i && !player.tileInteractionHappened) {
                                Main.CancelHairWindow();
                                Main.SetNPCShopIndex(0);
                                Main.InGuideCraftMenu = false;
                                player.dropItemCheck();
                                Main.npcChatCornerItem = 0;
                                player.sign = -1;
                                Main.editSign = false;
                                player.SetTalkNPC(i);
                                Main.playerInventory = false;
                                player.chest = -1;
                                Recipe.FindRecipes();
                                Main.npcChatText = nPC.GetChat();
                                SoundEngine.PlaySound(SoundID.Chat);
                            }
                        }
                    }

                    if (flag && !player.mouseInterface) {
                        player.cursorItemIconEnabled = false;
                        string text = nPC.GivenOrTypeName;
                        int num = i;
                        if (nPC.realLife >= 0)
                            num = nPC.realLife;

                        if (Main.npc[num].lifeMax > 1 && !Main.npc[num].dontTakeDamage)
                            text = text + ": " + Main.npc[num].life + "/" + Main.npc[num].lifeMax;

                        Main.instance.MouseTextHackZoom(text);
                        Main.mouseText = true;
                        nPC.position -= nPC.netOffset;
                        break;
                    }

                    if (flag2) {
                        nPC.position -= nPC.netOffset;
                        break;
                    }
                }
            }

            nPC.position -= nPC.netOffset;
        }
    }
}
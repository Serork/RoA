using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.PopupTexts;
using RoA.Common.Projectiles;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class PettyBag : InteractableProjectile {
    private sealed class PettyBagPopupText : CustomPopupText {

    }

    private sealed class PettyBagItemExtra : GlobalItem {
        public bool WasCollectedByPettyBag;

        public override bool InstancePerEntity => true;
    }

    private sealed class PettyBagHandler : ModPlayer {
        public HashSet<Item> BagItems { get; private set; } = [];

        public override void SaveData(TagCompound tag) {
            tag["bagitems"] = BagItems.ToList();
        }

        public override void LoadData(TagCompound tag) {
            var bagitems = tag.GetList<TagCompound>("bagitems").Select(ItemIO.Load).ToList();
            BagItems = [.. bagitems];
        }

        public override void Unload() {
            BagItems.Clear();
            BagItems = null;
        }

        public void AddItem(Item item, Projectile projectile) {
            Player player = Main.player[projectile.owner];
            bool flag = false;
            for (int i = 0; i < BagItems.Count; i++) {
                if (TryGetItem_FillIntoOccupiedSlot(player, item, i, projectile)) {
                    flag = true;
                }
            }
            if (!flag) {
                if (item.IsACoin)
                    SoundEngine.PlaySound(SoundID.CoinPickup, projectile.Center);
                else
                    SoundEngine.PlaySound(SoundID.Grab, projectile.Center);

                item.shimmered = false;
                BagItems.Add(item);
                CustomPopupText.NewText(CustomPopupTextContext.PettyBag, item, item.stack, noStack: false, true);
            }
        }


        private bool TryGetItem_FillIntoOccupiedSlot(Player player, Item item, int index, Projectile projectile) {
            Item bagItem = BagItems.ElementAt(index);
            if (bagItem.type > 0 && bagItem.stack < bagItem.maxStack && item.type == bagItem.type) {
                if (item.IsACoin)
                    SoundEngine.PlaySound(SoundID.CoinPickup, projectile.Center);
                else
                    SoundEngine.PlaySound(SoundID.Grab, projectile.Center);

                if (item.stack + bagItem.stack <= bagItem.maxStack) {
                    bagItem.stack += item.stack;

                    CustomPopupText.NewText(CustomPopupTextContext.PettyBag, item, item.stack, noStack: false, true);

                    return true;
                }

                item.stack -= bagItem.maxStack - bagItem.stack;
                CustomPopupText.NewText(CustomPopupTextContext.PettyBag, item, bagItem.maxStack - bagItem.stack, noStack: false, true);

                bagItem.stack = bagItem.maxStack;
            }

            return false;
        }

        public void Collect(Projectile projectile) {
            for (int i = 0; i < BagItems.Count; i++) {
                Item item = BagItems.ElementAt(i);
                int num = Item.NewItem(Player.GetSource_Misc("pettybaginteraction"), (int)projectile.position.X, (int)projectile.position.Y, projectile.width, projectile.height, item.type);
                Main.item[num].netDefaults(item.netID);
                Main.item[num].Prefix(item.prefix);
                Main.item[num].stack = item.stack;
                Main.item[num].velocity.Y = (float)Main.rand.Next(-20, 1) * 0.2f;
                Main.item[num].velocity.X = (float)Main.rand.Next(-20, 21) * 0.2f;
                Main.item[num].noGrabDelay = 100;
                Main.item[num].favorited = false;
                Main.item[num].newAndShiny = false;
                Main.item[num].GetGlobalItem<PettyBagItemExtra>().WasCollectedByPettyBag = true;
                if (Main.netMode == 1)
                    NetMessage.SendData(21, -1, -1, null, num);
            }

            BagItems.Clear();
        }
    }

    public override void Load() {
        On_Player.ItemSpace += On_Player_ItemSpace;
        On_Player.PickupItem += On_Player_PickupItem;
    }

    private Item On_Player_PickupItem(On_Player.orig_PickupItem orig, Player self, int playerIndex, int worldItemArrayIndex, Item itemToPickUp) {
        ushort bagType = (ushort)ModContent.ProjectileType<PettyBag>();
        if (self.ownedProjectileCounts[bagType] > 0) {
            itemToPickUp.GetGlobalItem<PettyBagItemExtra>().WasCollectedByPettyBag = false;
        }

        return orig(self, playerIndex, worldItemArrayIndex, itemToPickUp);
    }

    private Player.ItemSpaceStatus On_Player_ItemSpace(On_Player.orig_ItemSpace orig, Player self, Item newItem) {
        ushort bagType = (ushort)ModContent.ProjectileType<PettyBag>();
        if (self.ownedProjectileCounts[bagType] > 0) {
            foreach (Projectile projectile in Main.ActiveProjectiles) {
                if (projectile.owner == self.whoAmI && projectile.type == bagType) {
                    int itemGrabRange = self.GetItemGrabRange(newItem) * 2;
                    Rectangle hitbox = newItem.Hitbox;
                    if (new Rectangle((int)Projectile.position.X - itemGrabRange, (int)Projectile.position.Y - itemGrabRange, Projectile.width + itemGrabRange * 2, Projectile.height + itemGrabRange * 2).Intersects(hitbox)) {
                        return new Player.ItemSpaceStatus(CanTakeItem: false);
                    }
                }
            }
        }

        return orig(self, newItem);
    }

    private void GrabNearbyItems() {
        for (int j = 0; j < 400; j++) {
            Item item = Main.item[j];
            Player player = Main.player[Projectile.owner];
            if (!item.active || item.shimmerTime != 0f || item.noGrabDelay != 0 || item.playerIndexTheItemIsReservedFor != player.whoAmI || !player.CanAcceptItemIntoInventory(item) || (item.shimmered && !((double)item.velocity.Length() < 0.2)))
                continue;

            if (item.GetGlobalItem<PettyBagItemExtra>().WasCollectedByPettyBag) {
                continue;
            }

            int itemGrabRange = player.GetItemGrabRange(item) * 2;
            Rectangle hitbox = item.Hitbox;
            if (Projectile.Hitbox.Intersects(hitbox)) {
                player.GetModPlayer<PettyBagHandler>().AddItem(item, Projectile);
                Main.item[j] = new Item();
            }
            else {
                if (!new Rectangle((int)Projectile.position.X - itemGrabRange, (int)Projectile.position.Y - itemGrabRange, Projectile.width + itemGrabRange * 2, Projectile.height + itemGrabRange * 2).Intersects(hitbox))
                    continue;

                Player.ItemSpaceStatus status = player.ItemSpace(item);
                if (player.CanPullItem(item, status)) {
                    item.shimmered = false;
                    item.beingGrabbed = true;

                    Item itemToPickUp = item;
                    float speed = 5f;
                    int acc = 2;
                    Vector2 vector = new Vector2(itemToPickUp.position.X + (float)(itemToPickUp.width / 2), itemToPickUp.position.Y + (float)(itemToPickUp.height / 2));
                    float num = Projectile.Center.X - vector.X;
                    float num2 = Projectile.Center.Y - vector.Y;
                    float num3 = (float)Math.Sqrt(num * num + num2 * num2);
                    num3 = speed / num3;
                    num *= num3;
                    num2 *= num3;
                    itemToPickUp.velocity.X = (itemToPickUp.velocity.X * (float)(acc - 1) + num) / (float)acc;
                    itemToPickUp.velocity.Y = (itemToPickUp.velocity.Y * (float)(acc - 1) + num2) / (float)acc;
                }
            }
        }
    }

    protected override Vector2 DrawOffset => Vector2.UnitY * 4f;

    protected override SpriteEffects SetSpriteEffects() => base.SetSpriteEffects();

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 6;
    }

    public override void SetDefaults() {
        Projectile.width = 34;
        Projectile.height = 34;

        Projectile.aiStyle = -1;

        Projectile.tileCollide = true;
        Projectile.timeLeft = 10800;
        Projectile.hide = false;
    }

    protected override void OnHover(Player player) {
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<Items.Miscellaneous.PettyBag>();
    }

    protected override void OnInteraction(Player player) {
        HashSet<Item> bagItems = player.GetModPlayer<PettyBagHandler>().BagItems;
        player.GetModPlayer<PettyBagHandler>().Collect(Projectile);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;

    public override void SafeAI() {
        KillSame();

        Projectile.velocity.X *= 0.925f;
        if ((double)Projectile.velocity.X < 0.1 && (double)Projectile.velocity.X > -0.1)
            Projectile.velocity.X = 0f;
        
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            if (Projectile.velocity.X < 0f)
                Projectile.direction = -1;
            else
                Projectile.direction = 1;

            Projectile.spriteDirection = Projectile.direction;
        }

        float gravity = 0.2f;
        float maxFallSpeed = 7f;
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            int num = (int)(Projectile.position.X + (float)(Projectile.width / 2)) / 16;
            int num2 = (int)(Projectile.position.Y + (float)(Projectile.height / 2)) / 16;
            if (num >= 0 && num2 >= 0 && num < Main.maxTilesX && num2 < Main.maxTilesY && Main.tile[num, num2] == null) {
                gravity = 0f;
                Projectile.velocity.X = 0f;
                Projectile.velocity.Y = 0f;
            }
        }

        Vector2 wetVelocity = Projectile.velocity * 0.5f;
        if (Projectile.shimmerWet) {
            gravity = 0.065f;
            maxFallSpeed = 4f;
            wetVelocity = Projectile.velocity * 0.375f;
        }
        else if (Projectile.honeyWet) {
            gravity = 0.05f;
            maxFallSpeed = 3f;
            wetVelocity = Projectile.velocity * 0.25f;
        }
        else if (Projectile.wet) {
            gravity = 0.08f;
            maxFallSpeed = 5f;
        }

        Projectile.velocity.Y += gravity;
        if (Projectile.velocity.Y > maxFallSpeed)
            Projectile.velocity.Y = maxFallSpeed;

        if (Projectile.wet) {
            Vector2 vector = Projectile.velocity;
            Projectile.velocity = Collision.TileCollision(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            if (Projectile.velocity.X != vector.X)
                wetVelocity.X = Projectile.velocity.X;

            if (Projectile.velocity.Y != vector.Y)
                wetVelocity.Y = Projectile.velocity.Y;
        }

        GrabNearbyItems();
    }

    private void KillSame() {
        if (Projectile.owner == Main.myPlayer) {
            for (int num825 = 0; num825 < 1000; num825++) {
                if (num825 != Projectile.whoAmI && Main.projectile[num825].active && Main.projectile[num825].owner == Projectile.owner && Main.projectile[num825].type == Projectile.type) {
                    if (Projectile.timeLeft >= Main.projectile[num825].timeLeft)
                        Main.projectile[num825].Kill();
                    else
                        Projectile.Kill();
                }
            }
        }
    }

    public override void PostAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 10) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame >= 6) {
            Projectile.frame = 0;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D projectileTexture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        int frameHeight = projectileTexture.Height / Main.projFrames[Type];
        Rectangle frameRect = new(0, Projectile.frame * frameHeight, projectileTexture.Width, frameHeight);
        Vector2 drawOrigin = new(projectileTexture.Width / 2f, projectileTexture.Height / Main.projFrames[Projectile.type] * 0.5f);
        Vector2 drawPos = Projectile.position + DrawOffset + drawOrigin - Main.screenPosition;
        Color color = Projectile.GetAlpha(lightColor);
        spriteBatch.Draw(projectileTexture, drawPos, frameRect, color, Projectile.rotation, drawOrigin, 1f, SetSpriteEffects(), 0f);

        return false;
    }
}

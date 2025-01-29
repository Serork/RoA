using Microsoft.Xna.Framework;

using RoA.Content.Items.Miscellaneous;
using RoA.Content.Projectiles.Enemies;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Renderers;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Terraria.Utilities;
using RoA.Utilities;
using RoA.Content.Items.Placeable.Banners;

namespace RoA.Content.NPCs.Enemies.Miscellaneous;

sealed class PettyGoblin : ModNPC {
    private const int FRAMES_COUNT = 16;
    private const int RUNNING_FRAMESCOUNT = 14;
    private const int STARTRUNNING_FRAME = 2;

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Collision_WaterCollision")]
    public extern static bool NPC_Collision_WaterCollision(NPC npc, bool lava);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Collision_LavaCollision")]
    public extern static bool NPC_Collision_LavaCollision(NPC npc);

    public int CurrentState {
        get => (int)NPC.ai[0];
        set => NPC.ai[0] = value;
    }

    public bool Stole {
        get => NPC.ai[1] == 1f;
        set => NPC.ai[1] = value ? 1f : 0f;
    }

    public int MovementDirection {
        get => (int)NPC.ai[2];
        set { NPC.ai[2] = value; }
    }

    public bool Attacking {
        get => NPC.ai[3] == 1f;
        set => NPC.ai[3] = value ? 1f : 0f;
    }

    public bool Invisible {
        get => NPC.localAI[0] == 1f;
        set => NPC.localAI[0] = value ? 1f : 0f;
    }

    private enum States {
        Walking,
        RunningAway,
        Attacking
    }

    private const int WALKING = (int)States.Walking;
    private const int ATTACKING = (int)States.Attacking;
    private const int AWAY = (int)States.RunningAway;

    private CoinGenerator Coins { get; set; }

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Petty Goblin");

        Main.npcFrameCount[Type] = FRAMES_COUNT;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Position = new Vector2(0f, 5f),

            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = 24f,

            Velocity = 1.5f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override void SetDefaults() {
        NPC.width = 20;
        NPC.height = 46;
        NPC.damage = 10;
        NPC.defense = 6;
        NPC.lifeMax = 40;
        NPC.HitSound = SoundID.DD2_GoblinBomberHurt;
        NPC.DeathSound = SoundID.DD2_GoblinBomberScream;
        NPC.value = Item.buyPrice(silver: 5, copper: 75);
        NPC.knockBackResist = 0.7f;
        NPC.rarity = 1;
        NPC.aiStyle = -1;
        NPC.noTileCollide = true;
        NPC.noGravity = true;

        DrawOffsetY = -2f;

        Coins = new CoinGenerator(NPC);

        NPC.rarity = 3;

        Banner = Type;
        BannerItem = ModContent.ItemType<PettyGoblinBanner>();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.PettyGoblin")
        });
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        Player player = spawnInfo.Player;
        float value = 0f;
        float gold = 10000f;
        int neededGoldCount = 3;
        int neededCount = (int)gold * neededGoldCount;
        for (int i = 0; i < Main.InventorySlotsTotal; i++) {
            if (value >= neededCount) {
                break;
            }
            Item item = player.inventory[i];
            if (item.type == ItemID.SilverCoin) {
                value += 100f * item.stack;
            }
            if (item.type == ItemID.GoldCoin) {
                value += 10000f * item.stack;
            }
            if (item.type == ItemID.PlatinumCoin) {
                value = neededCount;
            }
        }
        bool flag = value >= neededCount;
        return !flag || spawnInfo.SpawnTileY >= Main.worldSurface ? 0f : SpawnCondition.OverworldDay.Chance * 0.02f;
    }

    public override void OnKill()
        => Coins.SpawnCoins();

    public override void AI() {
        if (NPC.justHit) {
            if (NPC.life < NPC.lifeMax * 0.4) {
                CurrentState = AWAY;
                MakeALittleJump();
            }
            else {
                Offensive();
            }
            if (CurrentState == AWAY) {
                Coins.SpawnCoins(true);
            }
        }
        NPC.TargetClosest();
        Player player = Main.player[NPC.target];
        bool targetFound = !(NPC.target < 0 || NPC.target == 255 || player.dead || !player.active);
        if (!targetFound || Vector2.Distance(NPC.Center, player.Center) > 1000f) {
            if (NPC.Opacity < 1f) {
                NPC.Opacity += 0.1f;
            }
            NPC.TargetClosest();
            player = Main.player[NPC.target];
            if (player.dead) {
                NPC.spriteDirection = MovementDirection;
                Attacking = false;
                NPC.velocity.X *= 0.95f;
                if (Math.Abs(NPC.velocity.X) < 0.1f) {
                    NPC.velocity.X = 0f;
                }
                NPC.noTileCollide = false;
                if (NPC.velocity.Y == 0f) {
                    if (Main.netMode != NetmodeID.Server) {
                        SoundEngine.PlaySound(SoundID.DD2_DarkMageHurt, NPC.Center);
                    }
                    MovementDirection *= -1;
                    Coins.SpawnCoins(true);
                    DoJump();
                    NPC.netUpdate = true;
                }
                NPC.noGravity = false;
                return;
            }
        }
        if (MovementDirection == 0) {
            MovementDirection = 1;
        }
        NPC.Opacity = MathHelper.Clamp(NPC.Opacity += Invisible ? -0.05f : 0.05f, 0.3f, 1f);
        NPC.noTileCollide = NPC.noGravity = true;
        float distance = player.Center.X - NPC.Center.X;
        float distanceY = player.Center.Y - NPC.Center.Y;
        double direction = (double)Math.Abs(distance);
        if (NPC.velocity.X != 0f) {
            NPC.direction = NPC.spriteDirection = Math.Sign(NPC.velocity.X);
        }
        bool flag2 = (double)Math.Abs(distanceY) < 50;
        bool close = direction < 50.0 && flag2;
        bool close2 = direction < 150.0 && flag2;
        bool flag = NPC.position.X > player.position.X && NPC.direction != 1 || NPC.position.X < player.position.X && NPC.direction == 1;
        switch (CurrentState) {
            case WALKING:
                Invisible = flag;
                Walking(player, close, distance);
                break;
            case ATTACKING:
                Invisible = false;
                Attack();
                Walking(player, close, distance);
                if (Math.Abs(NPC.velocity.X) < 0.25f && close2) {
                    StealMoney();
                }
                break;
            default:
                if (flag) {
                    Attack();
                }
                else {
                    Attacking = false;
                    NPC.netUpdate = true;
                }
                Walking(player, close, distance, -1);
                break;
        }
        if (Stole || !close2) {
            return;
        }
        if (Math.Abs(NPC.velocity.X) < 0.25f) {
            StealMoney();
        }
    }

    private void MakeALittleJump() {
        if (NPC.velocity.Y != 0f) {
            return;
        }
        NPC.velocity.Y -= Main.rand.NextFloat(2f, 5f) * Main.rand.NextFloat(1.1f, 1.75f) * 0.75f;
    }

    private void Offensive() {
        if (CurrentState == ATTACKING) {
            return;
        }
        if (NPC.life < NPC.lifeMax - NPC.lifeMax / 4 && Main.rand.NextChance(0.333)) {
            goto attack;
        }
    attack:
        CurrentState = ATTACKING;
        MovementDirection = 1;
        NPC.netUpdate = true;
    }

    private void Attack() {
        if (!Attacking) {
            if (Main.netMode != NetmodeID.Server) {
                SoundEngine.PlaySound(SoundID.DD2_DarkMageHurt, NPC.Center);
            }
            Attacking = true;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GoblinsDagger>(), NPC.damage * 2, 2f, Main.myPlayer, NPC.whoAmI);
            }
            NPC.netUpdate = true;
        }
    }

    public override void FindFrame(int frameHeight) {
        float max = 0.25f;
        if (Math.Abs(NPC.velocity.Y) > max) {
            NPC.frame.Y = frameHeight;
            return;
        }
        float speedX = Math.Abs(NPC.velocity.X);
        if (NPC.frameCounter < RUNNING_FRAMESCOUNT * RUNNING_FRAMESCOUNT) {
            NPC.frameCounter += 0.2 * (double)MathHelper.Clamp(speedX, 0f, 3f);
        }
        else {
            NPC.frameCounter = 0.0;
        }
        int currentFrame = STARTRUNNING_FRAME + (int)(NPC.frameCounter % RUNNING_FRAMESCOUNT);
        NPC.frame.Y = currentFrame * frameHeight;
        if (speedX <= max) {
            NPC.frame.Y = 0;
        }
    }

    private void Walking(Player player, bool close, float distance, int direction = 1) {
        bool away = direction != 1;
        bool attacking = CurrentState == ATTACKING && NPC.life <= NPC.lifeMax / 3;
        float movementSpeed = (attacking ? 3.25f : !away ? 2.5f : 4f) * 0.65f;
        float acceleration = (attacking ? 13.5f : !away ? 10f : 15f) * 0.65f;
        float speedY = -0.4f;
        float min = -4f;
        if (CurrentState == AWAY && Math.Abs(player.Center.X - NPC.Center.X) < 200) {
            if (NPC.velocity.Y == 0f) {
                DoJump2();
            }
        }
        if (close) {
            if (away && Main.rand.NextChance(0.1)) {
                MovementDirection *= -1;
            }
            if (Stole && Main.rand.NextChance(0.05)) {
                Stole = false;
                NPC.netUpdate = true;
            }
            if (!away) {
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, 0f, 0.15f);
            }
        }
        else {
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, (float)Math.Sign(distance) * direction * MovementDirection * movementSpeed, 1f / acceleration);
        }
        int sizeX = 16;
        int sizeY = 6;
        Vector2 Position = new Vector2(NPC.Center.X - sizeX / 2, NPC.position.Y + NPC.height - sizeY);
        bool acceptTopSurfaces = NPC.Bottom.Y >= player.getRect().Top;
        bool flag = Collision.SolidCollision(Position, sizeX, sizeY, acceptTopSurfaces);
        bool flag2 = Collision.SolidCollision(Position, sizeX, sizeY - 2, acceptTopSurfaces);
        bool flag3 = !Collision.SolidCollision(Position + new Vector2(32 * NPC.direction, 0f), 16, 16, acceptTopSurfaces);
        bool flag4 = Collision.SolidCollision(NPC.Center + new Vector2(NPC.width * NPC.direction, -sizeY), 16, 16, true) && away;
        float jumpHeight = 5f;
        if (flag4) {
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
            //MovementDirection *= -1;
            //if (Math.Abs(NPC.velocity.X) <= 0.25f) {
            //    NPC.velocity.Y = -jumpHeight;
            //    NPC.netUpdate = true;
            //}
        }
        if (away) {
            Invisible = false;
        }
        if (flag && !flag2) {
            NPC.velocity.Y = 0f;
        }
        else if (flag) {
            NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y + speedY, min, 0f);
        }
        else if (NPC.velocity.Y == 0f & flag3) {
            NPC.velocity.Y = -jumpHeight;
            NPC.netUpdate = true;
        }
        else {
            NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y - speedY, -jumpHeight, 16f);
            if (away) {
                Invisible = true;
                NPC.netUpdate = true;
            }
        }
        if (NPC.velocity.Y == 0f) {
            int collisionWidth = 18;
            int collisionHeight = 40;
            int num20 = (int)((NPC.position.X + (float)(collisionWidth / 2) + (float)(15 * NPC.direction)) / 16f);
            int num21 = (int)((NPC.position.Y + (float)collisionHeight - 16f) / 16f);
            Tile tileSafely3 = Framing.GetTileSafely(num20, num21);
            Tile tileSafely4 = Framing.GetTileSafely(num20, num21 - 1);
            Tile tileSafely5 = Framing.GetTileSafely(num20, num21 - 2);
            bool flag21 = collisionHeight / 16 < 3;
            if ((NPC.velocity.X < 0f && NPC.direction == -1) || (NPC.velocity.X > 0f && NPC.direction == 1)) {
                bool flag22 = false;
                if (tileSafely5.HasUnactuatedTile && Main.tileSolid[tileSafely5.TileType] && !Main.tileSolidTop[tileSafely5.TileType] && (!flag21 || (tileSafely4.HasUnactuatedTile && Main.tileSolid[tileSafely4.TileType] && !Main.tileSolidTop[tileSafely4.TileType]))) {
                    if (!Collision.SolidTilesVersatile(num20 - NPC.direction * 2, num20 - NPC.direction, num21 - 5, num21 - 1) && !Collision.SolidTiles(num20, num20, num21 - 5, num21 - 3)) {
                        NPC.velocity.Y = -6f;
                        NPC.netUpdate = true;
                    }
                }
                else if (tileSafely4.HasUnactuatedTile && Main.tileSolid[tileSafely4.TileType] && !Main.tileSolidTop[tileSafely4.TileType]) {
                    if (!Collision.SolidTilesVersatile(num20 - NPC.direction * 2, num20 - NPC.direction, num21 - 4, num21 - 1) && !Collision.SolidTiles(num20, num20, num21 - 4, num21 - 2)) {
                        NPC.velocity.Y = -5f;
                        NPC.netUpdate = true;
                    }
                    else {
                        flag22 = true;
                    }
                }
                else if (NPC.position.Y + (float)collisionHeight - (float)(num21 * 16) > 20f && tileSafely3.HasUnactuatedTile && Main.tileSolid[tileSafely3.TileType] && !tileSafely3.TopSlope) {
                    if (!Collision.SolidTilesVersatile(num20 - NPC.direction * 2, num20, num21 - 3, num21 - 1)) {
                        NPC.velocity.Y = -4.4f;
                        NPC.netUpdate = true;
                    }
                    else {
                        flag22 = true;
                    }
                }

                if (flag22) {
                    NPC.direction *= -1;
                    NPC.velocity.X *= -1f;
                    NPC.netUpdate = true;
                }

                //if (NPC.velocity.Y < 0f) {
                //    NPC.velocity.Y *= 1.2f;
                //}
            }
        }
        bool lava = NPC_Collision_LavaCollision(NPC);
        lava = NPC_Collision_WaterCollision(NPC, lava);
        if (NPC.wet) {
            NPC.velocity *= 0.85f;
        }
    }

    private void DoJump() {
        NPC.velocity.Y -= Main.rand.NextFloat(2f, 5f) * Main.rand.NextFloat(1.25f, 1.75f) * 0.85f;
        NPC.velocity.X += Main.rand.NextFloat(2f, 5f) * 0.1f * MovementDirection;
        NPC.netUpdate = true;
    }

    private void DoJump2() {
        bool flag = NPC.velocity.X > 0f && NPC.Center.X < Main.player[NPC.target].Center.X;
        bool flag2 = NPC.velocity.X < 0f && NPC.Center.X > Main.player[NPC.target].Center.X;
        if (flag || flag2) {
            NPC.velocity.Y -= Main.rand.NextFloat(2f, 5f) * Main.rand.NextFloat(1.25f, 1.75f) * 0.85f;
            NPC.velocity.X -= Main.rand.NextFloat(2f, 5f) * 0.025f * (flag ? -1 : 1);
            NPC.netUpdate = true;
        }
    }

    private void StealMoney() {
        bool flag3 = CurrentState != AWAY;
        bool flag4 = CurrentState != ATTACKING;
        bool flag = flag3 && NPC.extraValue >= 10000f;
        bool flag2 = flag4 && flag3;
        for (int iPlayer = 0; iPlayer < Main.player.Length; iPlayer++) {
            Player player = Main.player[iPlayer];
            for (int i = 0; i < 58; i++) {
                Item item = player.inventory[i];
                float value = 1f;
                if (item.type == ItemID.SilverCoin) {
                    value = 100f;
                }
                if (item.type == ItemID.GoldCoin) {
                    value = 10000f;
                }
                if (item.type == ItemID.PlatinumCoin) {
                    value = 1000000f;
                }
                if (value <= 1f) {
                    continue;
                }
                float value2 = Main.rand.Next(50, 76) * 0.01f;
                if (item.type == ItemID.CopperCoin) {
                    value2 += Main.rand.Next(51) * 0.01f;
                }
                if (item.type == ItemID.SilverCoin) {
                    value2 += Main.rand.Next(26) * 0.01f;
                }
                if ((double)value2 > 1.0) {
                    value2 = 1f;
                }
                int stack = (int)(item.stack * (double)value2);
                if (stack < 1) {
                    stack = 1;
                }
                if (stack > item.stack) {
                    stack = item.stack;
                }
                if (item.type == ItemID.SilverCoin || item.type == ItemID.GoldCoin || item.type == ItemID.PlatinumCoin) {
                    item.stack -= stack;
                }
                float coins = stack * value;
                NPC.extraValue += (int)coins;
                if (Main.netMode != NetmodeID.Server) {
                    SoundEngine.PlaySound(SoundID.CoinPickup, NPC.Center);
                    SoundEngine.PlaySound(SoundID.DD2_DarkMageHurt, NPC.Center);
                }
                Stole = true;
                if (flag) {
                    MakeALittleJump();
                    CurrentState = AWAY;
                    return;
                }
            }
            if (flag2) {
                MakeALittleJump();
                CurrentState = ATTACKING;
            }
        }
    }

    //public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PettyBag>(), 10));

    public override void HitEffect(NPC.HitInfo hit) {
        if (NPC.life > 0) {
            for (int num828 = 0; (double)num828 < hit.Damage / (double)NPC.lifeMax * 100.0; num828++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f);
            }

            return;
        }

        for (int num829 = 0; num829 < 50; num829++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2.5f * (float)hit.HitDirection, -2.5f);
        }

        Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "PettyGoblinGore1".GetGoreType(), Scale: NPC.scale);
        Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "PettyGoblinGore2".GetGoreType(), Scale: NPC.scale);
        Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "PettyGoblinGore3".GetGoreType(), Scale: NPC.scale);
        Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "PettyGoblinGore4".GetGoreType(), Scale: NPC.scale);
    }

    private class CoinGenerator {
        private NPC NPC { get; set; }

        private readonly List<Coin> coins;

        public int Sum {
            get {
                int sum = 0;
                foreach (Coin coin in coins) {
                    sum += coin.Amount * (int)(coin.Type + 1);
                }
                return sum;
            }
        }

        public CoinGenerator(NPC npc) {
            NPC = npc;
            coins = new List<Coin>();
            UnifiedRandom rand = Main.rand;
            coins.Add(new Coin(CoinTypes.Copper, rand.Next(35, 60)));
            if (rand.NextChance(0.75)) {
                coins.Add(new Coin(CoinTypes.Silver, rand.Next(15, 25)));
            }
            if (rand.NextChance(0.4)) {
                coins.Add(new Coin(CoinTypes.Gold, rand.Next(5, 10)));
            }
            if (rand.NextChance(0.05)) {
                coins.Add(new Coin(CoinTypes.Platinum, 1));
            }
        }

        public void SpawnCoins(bool justHit = false, bool deathPlayers = false) {
            for (int i = 0; i < coins.Count; i++) {
                Coin coin = coins[i];
                if (coin.Amount == 0) {
                    continue;
                }
                int count = coin.Amount;
                for (int i2 = 0; i2 < (deathPlayers ? count / 15 : coin.Type == CoinTypes.Copper ? (int)(count / 7.5f) : coin.Type == CoinTypes.Gold || coin.Type == CoinTypes.Platinum ? 1 : count / (justHit ? (int)(count / 2f) : 1)); i2++) {
                    DropCoin(coin);
                    if (coin.Amount > 0) {
                        coins[i].Amount--;
                    }
                }
            }
            if (justHit) {
                return;
            }
            coins.Clear();
        }

        private void DropCoin(Coin coin) {
            int item = Item.NewItem(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height, coin.ItemType, 1, false, 0, false, false);
            if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0) {
                NetMessage.SendData(21, -1, -1, null, item, 1f, 0f, 0f, 0, 0, 0);
            }
        }
    }

    private class Coin {
        public CoinTypes Type { get; set; }
        public int Amount { get; set; }
        public int ItemType { get; set; }

        public Coin(CoinTypes type, int amount = 0) {
            Type = type;
            Amount = amount;
            ItemType = Type == CoinTypes.Platinum ? ItemID.PlatinumCoin : Type == CoinTypes.Gold ? ItemID.GoldCoin : Type == CoinTypes.Silver ? ItemID.SilverCoin : ItemID.CopperCoin;
        }
    }

    private enum CoinTypes {
        Platinum,
        Gold,
        Silver,
        Copper
    }
}

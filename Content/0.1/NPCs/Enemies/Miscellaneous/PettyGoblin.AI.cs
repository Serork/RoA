using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Content.Projectiles.Enemies;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Miscellaneous;

sealed partial class PettyGoblin : ModNPC {
    private enum GoblinAIState : byte {
        InvinsibleWalking,
        IntensiveWalking,
        RunAway,
        Attack,
        Victory,
        InvinsibleRunAway,
        SuperInvinsibleRunAway,
        SuperRunAway
    }

    private GoblinAIState State {
        get => (GoblinAIState)NPC.ai[1];
        set => NPC.ai[1] = (int)value;
    }

    private bool IsInvinsible {
        get => NPC.localAI[0] == 1f;
        set => NPC.localAI[0] = value.ToInt();
    }

    private bool StoleMoney {
        get => NPC.localAI[1] == 1f;
        set => NPC.localAI[1] = value.ToInt();
    }

    public override bool CheckActive() => State != GoblinAIState.Victory;

    public override void AI() {
        float opacity = 0.05f;
        NPC.Opacity = MathHelper.Clamp(NPC.Opacity += IsInvinsible ? -opacity : opacity, 0.3f, 1f);

        NPC.chaseable = NPC.Opacity >= 0.9f;

        switch (State) {
            case GoblinAIState.InvinsibleWalking:
                WalkingToPlayer1();
                break;
            case GoblinAIState.IntensiveWalking:
                WalkingToPlayer2();
                break;
            case GoblinAIState.RunAway:
                RunAway();
                break;
            case GoblinAIState.Attack:
                Attack();
                break;
            case GoblinAIState.Victory:
                Victory();
                break;
            case GoblinAIState.InvinsibleRunAway:
                InvinsibleRunAway();
                break;
            case GoblinAIState.SuperInvinsibleRunAway:
                RunAway(true);
                break;
            case GoblinAIState.SuperRunAway:
                InvinsibleRunAway(true);
                break;
        }
    }

    private void InvinsibleRunAway(bool super = false) {
        IsInvinsible = true;

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            if (NPC.justHit && (!super || (super && Main.rand.NextBool()))) {
                NPC.TargetClosest();
                GoToAttackStage(Main.player[NPC.target]);
                NPC.netUpdate = true;
                return;
            }
        }

        SimpleMovement(super);
    }

    private void DoLittleJump() {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }
        NPC.direction = -NPC.direction;
        NPC.velocity.Y -= Main.rand.NextFloat(2f, 5f) * Main.rand.NextFloat(1.25f, 1.75f) * 0.85f;
        NPC.velocity.X -= Main.rand.NextFloat(2f, 5f) * 0.025f * NPC.direction;
        NPC.netUpdate = true;
    }

    private void Victory() {
        if (NPC.IsGrounded()) {
            NPC.velocity.X *= 0.8f;
            if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1) {
                NPC.velocity.X = 0f;
            }
        }
        void setValue() {
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                NPC.ai[0] = Main.rand.NextFloat(50f, 80f) / 2.5f;
                NPC.netUpdate = true;
            }
        }
        if (NPC.justHit) {
            NPC.TargetClosest(false);
            GoToAttackStage(Main.player[NPC.target]);
            return;
        }
        if (NPC.ai[0] > 0) {
            NPC.ai[0]--;
            NPC.velocity.X *= 0.95f;
            if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1) {
                NPC.velocity.X = 0f;
            }
        }
        else if (NPC.IsGrounded()) {
            NPC.velocity.X *= 0.8f;
            if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1) {
                NPC.velocity.X = 0f;
            }
            setValue();
            DoLittleJump();
            SoundEngine.PlaySound(SoundID.DD2_DarkMageHurt, NPC.Center);

            if (NPC.extraValue > 100) {
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    int value = Main.rand.Next(2, 10);
                    while (value > 0) {
                        int type = ItemID.CopperCoin;
                        bool flag = true;
                        if (NPC.extraValue > 200 && Main.rand.NextBool(3)) {
                            type = ItemID.SilverCoin;
                            NPC.extraValue -= 100;
                            flag = false;
                        }
                        if (NPC.extraValue > 20000 && Main.rand.NextBool(10)) {
                            type = ItemID.GoldCoin;
                            NPC.extraValue -= 10000;
                            flag = false;
                        }
                        if (flag) {
                            NPC.extraValue -= 1;
                        }
                        if (Main.netMode != NetmodeID.SinglePlayer) {
                            MultiplayerSystem.SendPacket(new NPCExtraValuePacket(NPC.whoAmI, NPC.extraValue));
                        }
                        value -= 1;
                        DropCoin(type, 1);
                    }
                }
            }
        }
    }

    private void DropCoin(int type, int stack) {
        int item = Item.NewItem(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height, type, stack, false, 0, false, false);
        if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0) {
            NetMessage.SendData(21, -1, -1, null, item, 1f, 0f, 0f, 0, 0, 0);
        }
    }

    private void TryGetTargetClosest(out int attempts) {
        attempts = Main.player.Count(x => x.active);
        Player player = Main.player[NPC.target];
        while ((player.dead || !player.active) && attempts > 0) {
            NPC.TargetClosest();
            attempts--;
        }
    }

    private void Attack() {
        IsInvinsible = false;

        Player player = Main.player[NPC.target];
        TryGetTargetClosest(out int attempts);
        if (attempts <= 0) {
            State = GoblinAIState.Victory;
            NPC.ai[0] = 0f;
            NPC.ai[2] = 0f;
            return;
        }
        bool flag = NPC.life < NPC.lifeMax / 2;
        if (StoleMoney || flag) ;
        {
            if (NPC.ai[0] == 1f && !flag) {
                if (CanRunAway(flag2: false)) {
                    return;
                }
            }
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                if (NPC.justHit && ((NPC.life > NPC.lifeMax / 3 && Main.rand.NextBool()) || flag)) {
                    if (CanRunAway(!flag)) {
                        NPC.netUpdate = true;
                        return;
                    }
                }
            }
        }
        if (NPC.Distance(player.Center) > 50f) {
            void basicMovement(float speed, NPC npc) {
                float num87 = 1f * speed;
                float num88 = 0.07f * speed;
                if (npc.velocity.X < 0f - num87 || npc.velocity.X > num87) {
                    if (NPC.IsGrounded())
                        npc.velocity *= 0.7f;
                }
                else if (npc.velocity.X < num87 && npc.direction == 1) {
                    npc.velocity.X += num88;
                    if (npc.velocity.X > num87)
                        npc.velocity.X = num87;
                }
                else if (npc.velocity.X > 0f - num87 && npc.direction == -1) {
                    npc.velocity.X -= num88;
                    if (npc.velocity.X < 0f - num87)
                        npc.velocity.X = 0f - num87;
                }
            }
            NPC.ApplyFighterAI(false, movementX: (npc) => {
                basicMovement(2f, npc);
            }, targetDelay: 20);
        }
        else {
            if (NPC.IsGrounded()) {
                NPC.velocity.X *= 0.8f;
                if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1) {
                    NPC.velocity.X = 0f;
                }
            }
        }
    }

    private void GoToAttackStage(Player player) {
        State = GoblinAIState.Attack;
        NPC.target = player.whoAmI;
        SoundEngine.PlaySound(SoundID.DD2_DarkMageHurt, NPC.Center);
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GoblinsDagger>(),
                30 / 2, 1.5f, Main.myPlayer, NPC.whoAmI);
        }
    }

    private void RunAway(bool super = false) {
        IsInvinsible = false;

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            if (NPC.justHit && (!super || (super && Main.rand.NextBool()))) {
                NPC.TargetClosest();
                GoToAttackStage(Main.player[NPC.target]);
                NPC.netUpdate = true;
                return;
            }
        }

        if (Main.netMode != NetmodeID.MultiplayerClient) {
            if (!super || (super && Main.rand.NextBool())) {
                foreach (Player player in Main.ActivePlayers) {
                    if (!Collision.CanHit(player, NPC)) {
                        continue;
                    }
                    float distance = NPC.Distance(player.Center);
                    if (NPC.justHit) {
                        bool flag = NPC.life > NPC.lifeMax * 0.75f;
                        if (distance > 300f && flag) {
                            if ((NPC.direction == 1 && player.Center.X > NPC.Center.X) || (NPC.direction == -1 && player.Center.X < NPC.Center.X)) {
                                NPC.direction *= -1;
                                NPC.netUpdate = true;
                                break;
                            }
                        }
                        else {
                            GoToAttackStage(player);
                            NPC.netUpdate = true;
                            return;
                        }
                    }
                }
            }
        }

        SimpleMovement(super);
    }

    private void SimpleMovement(bool super = false) {
        NPC npc = NPC;
        float speed = super ? 3f : 2f;
        float num87 = 1f * speed;
        float num88 = 0.07f * speed;
        if (npc.velocity.X < 0f - num87 || npc.velocity.X > num87) {
            if (NPC.IsGrounded())
                npc.velocity *= 0.7f;
        }
        else if (npc.velocity.X < num87 && npc.direction == 1) {
            npc.velocity.X += num88;
            if (npc.velocity.X > num87)
                npc.velocity.X = num87;
        }
        else if (npc.velocity.X > 0f - num87 && npc.direction == -1) {
            npc.velocity.X -= num88;
            if (npc.velocity.X < 0f - num87)
                npc.velocity.X = 0f - num87;
        }

        int targetDelay = 20;
        int npcTypeForSomeReason = NPC.type;

        bool flag7 = false;
        int num56 = targetDelay;
        if (NPC.IsGrounded() && ((npc.velocity.X > 0f && npc.direction < 0) || (npc.velocity.X < 0f && npc.direction > 0)))
            flag7 = true;

        if (npc.position.X == npc.oldPosition.X || npc.ai[3] >= (float)num56 || flag7)
            npc.ai[3] += 1f;
        else if ((double)Math.Abs(npc.velocity.X) > 0.9 && npc.ai[3] > 0f)
            npc.ai[3] -= 1f;

        if (npc.ai[3] > 60) {
            npc.ai[3] = 0f;
            npc.direction *= -1;
            npc.spriteDirection = npc.direction;
        }

        if (npc.justHit)
            npc.ai[3] = 0f;

        if (npc.ai[3] == (float)num56)
            npc.netUpdate = true;

        if (Main.player[npc.target].Hitbox.Intersects(npc.Hitbox))
            npc.ai[3] = 0f;

        bool tileChecks = false;
        if (NPC.IsGrounded()) {
            int num77 = (int)(NPC.position.Y + NPC.height + 7f) / 16;
            int num189 = (int)NPC.position.X / 16;
            int num79 = (int)(NPC.position.X + NPC.width) / 16;
            for (int num80 = num189; num80 <= num79; num80++) {
                if (Main.tile[num80, num77] == null) {
                    return;
                }

                if (Main.tile[num80, num77].HasUnactuatedTile && Main.tileSolid[Main.tile[num80, num77].TileType]) {
                    tileChecks = true;
                    break;
                }
            }
        }
        if (NPC.velocity.Y >= 0f) {
            int direction = Math.Sign(NPC.velocity.X);

            Vector2 position3 = NPC.position;
            position3.X += NPC.velocity.X;
            int num82 = (int)((position3.X + NPC.width / 2 + (NPC.width / 2 + 1) * direction) / 16f);
            int num83 = (int)((position3.Y + NPC.height - 1f) / 16f);
            if (num82 * 16 < position3.X + NPC.width && num82 * 16 + 16 > position3.X && (Main.tile[num82, num83].HasUnactuatedTile && !Main.tile[num82, num83].TopSlope && !Main.tile[num82, num83 - 1].TopSlope && Main.tileSolid[Main.tile[num82, num83].TileType] && !Main.tileSolidTop[Main.tile[num82, num83].TileType] || Main.tile[num82, num83 - 1].IsHalfBlock && Main.tile[num82, num83 - 1].HasUnactuatedTile) && (!Main.tile[num82, num83 - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 1].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 1].TileType] || Main.tile[num82, num83 - 1].IsHalfBlock && (!Main.tile[num82, num83 - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 4].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 4].TileType])) && (!Main.tile[num82, num83 - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 2].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 2].TileType]) && (!Main.tile[num82, num83 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 3].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 3].TileType]) && (!Main.tile[num82 - direction, num83 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82 - direction, num83 - 3].TileType])) {
                float num84 = num83 * 16;
                if (Main.tile[num82, num83].IsHalfBlock) {
                    num84 += 8f;
                }

                if (Main.tile[num82, num83 - 1].IsHalfBlock) {
                    num84 -= 8f;
                }
                if (num84 < position3.Y + NPC.height) {
                    float num85 = position3.Y + NPC.height - num84;
                    float num86 = 16.1f;
                    if (NPC.type == NPCID.BlackRecluse || NPC.type == NPCID.WallCreeper || NPC.type == NPCID.JungleCreeper || NPC.type == NPCID.BloodCrawler || NPC.type == NPCID.DesertScorpionWalk) {
                        num86 += 8f;
                    }

                    if (num85 <= num86) {
                        NPC.gfxOffY += NPC.position.Y + NPC.height - num84;
                        NPC.position.Y = num84 - NPC.height;
                        if (num85 < 9f) {
                            NPC.stepSpeed = 1f;
                        }
                        else {
                            NPC.stepSpeed = 2f;
                        }
                    }
                }
            }
        }
        if (tileChecks && !Main.tile[(int)(NPC.Center.X) / 16, (int)(NPC.Center.Y - 15f) / 16 - 1].HasUnactuatedTile) {
            int tileX = (int)((NPC.position.X + NPC.width / 2 + 15 * NPC.direction) / 16f);
            int tileY = (int)((NPC.position.Y + NPC.height - 15f) / 16f);
            if (NPC.velocity.X < 0f && NPC.direction == -1 || NPC.velocity.X > 0f && NPC.direction == 1) {
                void jumpIfPlayerAboveAndClose() {
                    if (NPC.IsGrounded() && Main.expertMode && Main.player[npc.target].Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) < (float)(Main.player[npc.target].width * 3) && Collision.CanHit(npc, Main.player[npc.target])) {
                        if (NPC.IsGrounded()) {
                            int num200 = 6;
                            if (Main.player[npc.target].Bottom.Y > npc.Top.Y - (float)(num200 * 16)) {
                                npc.velocity.Y = -7.9f;
                            }
                            else {
                                int num201 = (int)(npc.Center.X / 16f);
                                int num202 = (int)(npc.Bottom.Y / 16f) - 1;
                                for (int num203 = num202; num203 > num202 - num200; num203--) {
                                    if (Main.tile[num201, num203].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[num201, num203].TileType]) {
                                        npc.velocity.Y = -7.9f;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                jumpIfPlayerAboveAndClose();

                bool JumpCheck(int tileX, int tileY) {
                    if (NPC.height >= 32 && Main.tile[tileX, tileY - 2].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 2].TileType]) {
                        if (Main.tile[tileX, tileY - 3].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 3].TileType]) {
                            NPC.velocity.Y = -8f;
                            NPC.netUpdate = true;
                        }
                        else {
                            NPC.velocity.Y = -7f;
                            NPC.netUpdate = true;
                        }
                        return true;
                    }
                    else if (Main.tile[tileX, tileY - 1].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 1].TileType]) {
                        NPC.velocity.Y = -6f;
                        NPC.netUpdate = true;
                        return true;
                    }
                    else if (NPC.position.Y + NPC.height - tileY * 16 > 20f && Main.tile[tileX, tileY].HasUnactuatedTile && !Main.tile[tileX, tileY].TopSlope && Main.tileSolid[Main.tile[tileX, tileY].TileType]) {
                        NPC.velocity.Y = -5f;
                        NPC.netUpdate = true;
                        return true;
                    }
                    else if (NPC.directionY < 0 && (!Main.tile[tileX, tileY + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY + 1].TileType]) && (!Main.tile[tileX + NPC.direction, tileY + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX + NPC.direction, tileY + 1].TileType])) {
                        NPC.velocity.Y = -8f;
                        NPC.velocity.X *= 1.5f;
                        NPC.netUpdate = true;
                        return true;
                    }
                    return false;
                }
                if (!JumpCheck(tileX, tileY)) {

                }
            }
        }
        if (npc.IsGrounded()) {
            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        }
    }

    private void TryToStealMoney(Player target) {
        bool flag = false;
        if (Collision.CanHit(NPC, target) && NPC.Distance(target.Center) < NPC.width * NPC.scale * 3f) {
            flag = true;
        }
        if (!flag) {
            return;
        }
        Player player = target;
        ulong value2 = 0;
        for (int i = 0; i < 58; i++) {
            Item item = player.inventory[i];
            float value = 0f;
            if (item.type == ItemID.CopperCoin) {
                value += 1f * item.stack;
            }
            if (item.type == ItemID.SilverCoin) {
                value += 100f * item.stack;
            }
            if (item.type == ItemID.GoldCoin) {
                value += 10000f * item.stack;
            }
            if (item.type == ItemID.PlatinumCoin) {
                value += 1000000f * item.stack;
            }
            if (value < 1f) {
                continue;
            }
            value2 += (ulong)value;
        }
        for (int i = 0; i < 58; i++) {
            Item item = player.inventory[i];
            if (item.type == ItemID.CopperCoin) {
                item.TurnToAir();
            }
            if (item.type == ItemID.SilverCoin) {
                item.TurnToAir();
            }
            if (item.type == ItemID.GoldCoin) {
                item.TurnToAir();
            }
            if (item.type == ItemID.PlatinumCoin) {
                item.TurnToAir();
            }
        }
        if (value2 < 10000) {
            GoToAttackStage(player);
            return;
        }
        NPC.extraValue += (int)value2;
        NPC.moneyPing(NPC.Center + Helper.VelocityToPoint(NPC.Center, player.Center, 25f));
        SoundEngine.PlaySound(SoundID.DD2_DarkMageHurt, NPC.Center);
        if (Main.netMode != NetmodeID.SinglePlayer) {
            MultiplayerSystem.SendPacket(new NPCExtraValuePacket(NPC.whoAmI, NPC.extraValue));
        }
        StoleMoney = true;
    }

    private void WalkingToPlayer1() {
        IsInvinsible = true;

        void basicMovement(float speed, NPC npc) {
            float num87 = 1f * speed;
            float num88 = 0.07f * speed;
            if (npc.velocity.X < 0f - num87 || npc.velocity.X > num87) {
                if (NPC.IsGrounded())
                    npc.velocity *= 0.7f;
            }
            else if (npc.velocity.X < num87 && npc.direction == 1) {
                npc.velocity.X += num88;
                if (npc.velocity.X > num87)
                    npc.velocity.X = num87;
            }
            else if (npc.velocity.X > 0f - num87 && npc.direction == -1) {
                npc.velocity.X -= num88;
                if (npc.velocity.X < 0f - num87)
                    npc.velocity.X = 0f - num87;
            }
        }
        NPC.ApplyFighterAI(false, movementX: (npc) => {
            basicMovement(1f, npc);
        }, targetDelay: 60);

        if (Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
            CanRunAway(false);
            return;
        }

        if (NPC.ai[3] > 400) {
            NPC.ai[3] = 0;
        }

        if (NPC.justHit) {
            State = GoblinAIState.IntensiveWalking;
            NPC.ai[0] = 0f;
        }

        if (CanRunAway(flag2: false)) {
            return;
        }

        if (NPC.ai[0] > 30f) {
            TryToStealMoney(Main.player[NPC.target]);
        }
        else {
            NPC.ai[0]++;
        }
    }

    private bool CanRunAway(bool flag = true, bool flag2 = true) {
        if ((StoleMoney && flag) || !flag) {
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                if (NPC.life < NPC.lifeMax / 2 && Main.rand.NextChance(0.75)) {
                    State = Main.rand.NextBool() && flag2 ? GoblinAIState.SuperInvinsibleRunAway : GoblinAIState.SuperRunAway;
                }
                else {
                    State = Main.rand.NextBool() && flag2 ? GoblinAIState.InvinsibleRunAway : GoblinAIState.RunAway;
                }
                NPC.netUpdate = true;
            }
            NPC.ai[0] = 0f;
            NPC.direction = -NPC.direction;
            if (!flag) {
                NPC.ai[0] = 1f;
            }
            return true;
        }

        return false;
    }

    private void WalkingToPlayer2() {
        IsInvinsible = false;

        void basicMovement(float speed, NPC npc) {
            float num87 = 1f * speed;
            float num88 = 0.07f * speed;
            if (npc.velocity.X < 0f - num87 || npc.velocity.X > num87) {
                if (NPC.IsGrounded())
                    npc.velocity *= 0.7f;
            }
            else if (npc.velocity.X < num87 && npc.direction == 1) {
                npc.velocity.X += num88;
                if (npc.velocity.X > num87)
                    npc.velocity.X = num87;
            }
            else if (npc.velocity.X > 0f - num87 && npc.direction == -1) {
                npc.velocity.X -= num88;
                if (npc.velocity.X < 0f - num87)
                    npc.velocity.X = 0f - num87;
            }
        }
        NPC.ApplyFighterAI(false, movementX: (npc) => {
            basicMovement(2f, npc);
        }, targetDelay: 20);

        NPC.ai[3] = 0;

        if (StoleMoney) {
            State = GoblinAIState.RunAway;
            NPC.ai[0] = 0f;
            return;
        }

        TryToStealMoney(Main.player[NPC.target]);
    }
}

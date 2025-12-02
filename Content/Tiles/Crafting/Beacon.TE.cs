using Microsoft.Xna.Framework;

using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Crafting;

sealed class BeaconTE : ModTileEntity {
    private enum AnimationState : byte {
        Animation1,
        Animation2,
        Animation3
    }

    private AnimationState _state = AnimationState.Animation1;
    private float _animationTimer;
    private bool _sync;

    public bool IsUsed { get; private set; }
    public Vector2 Scale { get; private set; }
    public Vector2 OffsetPosition { get; private set; }

    public void UseAnimation() {
        NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            MultiplayerSystem.SendPacket(new BeaconUsePacket(Position.X, Position.Y));
        }

        if (IsUsed) {
            return;
        }

        _sync = true;

        ResetAnimation();

        IsUsed = true;
    }

    public void ResetAnimation() {
        _sync = true;

        IsUsed = false;
        Scale = new Vector2(0.2f, 1f);
        OffsetPosition = Vector2.Zero;
        _state = AnimationState.Animation1;
        _animationTimer = 0f;
    }

    public override void Update() {
        int i = Position.X;
        int j = Position.Y - 2;

        UpdateAnimation(() => {
            if (WorldGenHelper.GetTileSafely(i, j).ActiveTile(ModContent.TileType<Beacon>())) {
                int gemType = Beacon.GetGemDropID(i, j);
                bool flag =
                    (gemType == ItemID.Diamond || gemType == ItemID.Amber) ? Main.rand.NextBool(3) :
                    gemType == ItemID.Ruby ? Main.rand.NextChance(0.4) :
                    gemType == ItemID.Emerald ? Main.rand.NextChance(0.5) :
                    gemType == ItemID.Sapphire ? Main.rand.NextChance(0.6) :
                    gemType == ItemID.Topaz ? Main.rand.NextChance(0.7) :
                    gemType != ItemID.Amethyst || Main.rand.NextChance(0.8);
                if (RoA.TryGetThoriumMod(out Mod thoriumMod)) {
                    if (gemType == (short)thoriumMod.Find<ModItem>("Aquamarine").Type) {
                        flag = Main.rand.NextChance(0.65);
                    }
                    else if (gemType == (short)thoriumMod.Find<ModItem>("Opal").Type) {
                        flag = Main.rand.NextChance(0.65);
                    }
                }
                if (flag) {
                    Beacon.ActionWithGem(i, j, true, false, true);
                }
            }
        });

        if (Main.netMode == NetmodeID.Server) {
            if (_sync || IsUsed) {
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                _sync = false;
            }
        }
    }

    public void UpdateAnimation(Action onEnd) {
        if (!IsUsed) {
            return;
        }

        float num = 1f;
        _animationTimer += num;

        float time = num;
        if (_animationTimer < time) {
            return;
        }

        float animationTimer = _animationTimer - time;
        switch (_state) {
            case AnimationState.Animation1:
                time = 20f;
                if (animationTimer < time) {
                    Scale = Vector2.Lerp(Scale, new Vector2(1.25f, 0.5f), animationTimer / time);
                }
                else {
                    _state = AnimationState.Animation2;
                    _animationTimer -= time;
                }
                break;
            case AnimationState.Animation2:
                time = 10f;
                if (animationTimer < time) {
                    Scale = Vector2.Lerp(Scale, new Vector2(0.85f, 1.25f) * 1.1f, animationTimer / time);
                }
                else {
                    _state = AnimationState.Animation3;
                    _animationTimer -= time;
                }
                break;
            case AnimationState.Animation3:
                time = 30f;
                if (animationTimer < time) {
                    if (animationTimer < time / 2f + time / 3f) {
                        OffsetPosition = Vector2.Lerp(OffsetPosition, Main.rand.NextVector2(-5f, 0f, 5f, 10f), 0.75f);
                    }
                    else {
                        OffsetPosition = Vector2.Lerp(OffsetPosition, Vector2.Zero, 0.25f);
                    }
                }
                else {
                    OffsetPosition = Vector2.Zero;
                    float time2 = 15f;
                    Scale = Vector2.Lerp(Scale, new Vector2(0.2f, 1f), (animationTimer - time) / time2);
                    if (animationTimer >= time + time2) {
                        ResetAnimation();
                        NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);
                        if (Main.netMode == NetmodeID.MultiplayerClient) {
                            MultiplayerSystem.SendPacket(new BeaconResetPacket(Position.X, Position.Y));
                        }
                        onEnd();
                    }
                }
                break;
        }
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);

            return -1;
        }

        int id = Place(i, j);
        return id;
    }

    public override void OnNetPlace() => NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, ID, Position.X, Position.Y);

    public override bool IsTileValidForEntity(int x, int y) => true;/*WorldGenHelper.GetTileSafely(x, y).ActiveTile(ModContent.TileType<Beacon>());*/

    public override void NetSend(BinaryWriter writer) {
        writer.Write(IsUsed);
        writer.WriteVector2(Scale);
        writer.WriteVector2(OffsetPosition);
        writer.Write((byte)_state);
        writer.Write(_animationTimer);
    }

    public override void NetReceive(BinaryReader reader) {
        IsUsed = reader.ReadBoolean();
        Scale = reader.ReadVector2();
        OffsetPosition = reader.ReadVector2();
        _state = (AnimationState)reader.ReadByte();
        _animationTimer = reader.ReadSingle();
    }
}
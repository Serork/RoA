using System.Runtime.CompilerServices;

using Terraria;

namespace RoA.Common.Players;

sealed class PlaceCheckHooks {
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "PlaceThing_Tiles_CheckLavaBlocking")]
    public extern static bool Player_PlaceThing_Tiles_CheckLavaBlocking(Player player);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "PlaceThing_Tiles_CheckGamepadTorchUsability")]
    public extern static bool Player_PlaceThing_Tiles_CheckGamepadTorchUsability(Player player, bool canUse);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "PlaceThing_Tiles_CheckWandUsability")]
    public extern static bool Player_PlaceThing_Tiles_CheckWandUsability(Player player, bool canUse);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "PlaceThing_Tiles_CheckRopeUsability")]
    public extern static bool Player_PlaceThing_Tiles_CheckRopeUsability(Player player, bool canUse);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "PlaceThing_Tiles_CheckFlexibleWand")]
    public extern static bool Player_PlaceThing_Tiles_CheckFlexibleWand(Player player, bool canUse);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "PlaceThing_TryReplacingTiles")]
    public extern static bool Player_PlaceThing_TryReplacingTiles(Player player, bool canUse);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "FigureOutWhatToPlace")]
    public extern static void Player_FigureOutWhatToPlace(Player player, Tile targetTile, Item sItem, out int tileToCreate, out int previewPlaceStyle, out bool? overrideCanPlace, out int? forcedRandom);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "PlaceThing_Tiles_BlockPlacementIfOverPlayers")]
    public extern static void Player_PlaceThing_Tiles_BlockPlacementIfOverPlayers(Player player, ref bool canPlace, ref TileObject data);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "PlaceThing_Tiles_BlockPlacementForRepeatedPigronatas")]
    public extern static void Player_PlaceThing_Tiles_BlockPlacementForRepeatedPigronatas(Player player, ref bool canPlace, ref TileObject data);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "PlaceThing_Tiles_BlockPlacementForRepeatedPumpkins")]
    public extern static void Player_PlaceThing_Tiles_BlockPlacementForRepeatedPumpkins(Player player, ref bool canPlace, ref TileObject data);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "PlaceThing_Tiles_BlockPlacementForRepeatedCoralAndBeachPiles")]
    public extern static void Player_PlaceThing_Tiles_BlockPlacementForRepeatedCoralAndBeachPiless(Player player, ref bool canPlace, ref TileObject data);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "PlaceThing_Tiles_BlockPlacementForAssortedThings")]
    public extern static bool Player_PlaceThing_Tiles_BlockPlacementForAssortedThings(Player player, bool canUse);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "PlaceThing_Tiles_PlaceIt")]
    public extern static TileObject Player_PlaceThing_Tiles_PlaceIt(Player player, bool newObjectType, TileObject data, int tileToCreate);
}

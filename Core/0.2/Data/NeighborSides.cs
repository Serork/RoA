using System;

namespace RoA.Core.Data;

[Flags]
enum NeighborSides : byte {
    None = 0,
    Left = 1 << 0, 
    Right = 1 << 1,
    Top = 1 << 2, 
    Bottom = 1 << 3,
    TopLeft = 1 << 4, 
    TopRight = 1 << 5, 
    BottomLeft = 1 << 6,
    BottomRight = 1 << 7 
}

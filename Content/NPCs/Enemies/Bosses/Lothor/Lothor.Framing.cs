using System;
using System.Collections.Generic;

using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    private record struct SpriteSheetLineInfo(byte RowCount, ushort FrameWidth, ushort FrameHeight);

    public enum SpriteSheetColumn : byte {
        Stand,
        Flight,
        FlightScream,
        Death
    }

    private SpriteSheetColumn _currentColumn = SpriteSheetColumn.Stand;
    private List<SpriteSheetLineInfo> _spriteSheet = [new(19, 146, 106), new(6, 146, 88), new(4, 146, 92), new(7, 146, 58)];
    private byte _currentFrame;

    private byte CurrentFrame {
        get => _currentFrame;
        set {
            if (value - PreviousColumnsFrameCount() < 0) {
                _currentColumn--;
            }
            _currentFrame = value;
        }
    }

    private SpriteSheetLineInfo CurrentSpriteSheetLine => _spriteSheet[(int)_currentColumn];
    private byte CurrentFrameInSpriteSheetLine => (byte)Math.Max(0, Math.Min(CurrentSpriteSheetLine.RowCount, CurrentFrame - PreviousColumnsFrameCount() - 1));

    private byte PreviousColumnsFrameCount() {
        byte rowCountSum = 0;
        byte currentColumn = (byte)_currentColumn;
        for (int index = 0; index < currentColumn; index++) {
            rowCountSum += _spriteSheet[index].RowCount;
        }
        return rowCountSum;
    }

    private void GetFrameInfo(out ushort x, out ushort y, out ushort width, out ushort height) {
        byte currentColumn = (byte)_currentColumn;
        if (currentColumn >= 0 && currentColumn < _spriteSheet.Count - 1) {
            for (int index = currentColumn; index < currentColumn + 1; index++) {
                SpriteSheetLineInfo current = _spriteSheet[index];
                byte rowCount = current.RowCount;
                if (Math.Min(CurrentFrameInSpriteSheetLine, rowCount) / rowCount >= 1) {
                    currentColumn++;
                    _currentColumn = (SpriteSheetColumn)currentColumn;
                    break;
                }
            }
        }
        width = CurrentSpriteSheetLine.FrameWidth;
        height = CurrentSpriteSheetLine.FrameHeight;
        x = (ushort)(currentColumn * width);
        y = (ushort)(CurrentFrameInSpriteSheetLine * height + 2);
    }
}

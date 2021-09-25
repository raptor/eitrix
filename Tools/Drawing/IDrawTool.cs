using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eitrix
{
    public enum PrintStyle
    {
        Normal,
        Debug,
        Score,
        Info
    }

    public interface IDrawTool
    {
        int Height { get; }
        int Width { get; }
        void ClearScreen();
        void ClearScreen(int backgroundId);
        void EndDrawing();
        void DebugPrint(int x, int y, Color color, string text);
        void SafePrint(float x, float y, Color color, string text, PrintStyle style);
        void DrawTitleMenu(MenuChoice selectedOption);
        void DrawHelpScreen(int page);
        void ClearScreen(Color color);
        void CenterPrint(Color color, string p);
        void DrawRoundStats(int currentRound, int numRounds, List<Player> Players);
        void DrawPlayers(List<Player> players);
        void DrawPlayersGathering(List<Player> players);

        void DrawOptions(List<Tuple<string, VisualOptionValue>> options, int selectedOption, string title, string instructions);
        void DrawPauseScreen(List<Tuple<string, VisualOptionValue>> options, int selectedOption);
        void Reset();
    }
}

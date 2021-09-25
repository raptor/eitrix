using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eitrix
{
    /// ---------------------------------------------------------------
    /// <summary>
    /// ScreenTypes
    /// </summary>
    /// ---------------------------------------------------------------
    public enum ScreenType
    {
        None,
        TitleScreen,
        HelpScreen,
        GameScreen,
        OptionsScreen,
        ExitGame
    }

    /// ---------------------------------------------------------------
    /// <summary>
    /// Screens control the modes of the game
    /// </summary>
    /// ---------------------------------------------------------------
    public abstract class Screen
    {
        public int frame;
        public ScreenType nextScreen;
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
        public abstract void Initialize();
    }
}

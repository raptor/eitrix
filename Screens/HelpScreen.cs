using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Eitrix
{
    class HelpScreen : Screen
    {
        IDrawTool drawTool;
        IInputTool inputTool;
        IAudioTool audioTool;
        int page = 0;

        /// ---------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// ---------------------------------------------------------------
        public HelpScreen(IDrawTool drawTool, IInputTool inputTool, IAudioTool audioTool)
        {
            this.drawTool = drawTool;
            this.inputTool = inputTool;
            this.audioTool = audioTool;
        }

        /// ---------------------------------------------------------------
        /// <summary>
        /// Initialize the game screen
        /// </summary>
        /// ---------------------------------------------------------------
        public override void Initialize()
        {
            this.nextScreen = ScreenType.HelpScreen;
            page = 0;
        }

        /// ---------------------------------------------------------------
        /// <summary>
        /// Update stuff
        /// </summary>
        /// ---------------------------------------------------------------
        public override void Update(GameTime gameTime)
        {
            foreach (InputAction action in inputTool.ActionQueue)
            {
                if (action.ActionType == InputActionType.Exit) nextScreen = ScreenType.TitleScreen;
                Debug.WriteLine("Got action: " + action.ActionType + " (" + action.Click +")");
                if (action.Click)
                {
                    if (action.ActionType == InputActionType.MenuDown || action.ActionType == InputActionType.MenuRight)
                    {
                        page++;
                        if (page >= Globals.HelpPages) page = 0;
                    }
                    if (action.ActionType == InputActionType.MenuUp || action.ActionType == InputActionType.MenuLeft)
                    {
                        page--;
                        if (page < 0) page = Globals.HelpPages-1;
                    }
                }
            }

            audioTool.ResolveState();
        }


        /// ---------------------------------------------------------------
        /// <summary>
        /// Draw a frame in the game
        /// </summary>
        /// ---------------------------------------------------------------
        public override void Draw(GameTime gameTime)
        {
            drawTool.ClearScreen();
            drawTool.DrawHelpScreen(page);
            drawTool.EndDrawing();
        }

    }
}

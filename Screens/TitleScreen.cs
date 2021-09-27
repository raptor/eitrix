using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eitrix
{
    public enum MenuChoice
    {
        Play,
        Help,
        Options,
        Quit,
        NumberOfChoices
    }


    class TitleScreen : Screen
    {
        IDrawTool drawTool;
        IInputTool inputTool;
        IAudioTool audioTool;

        public MenuChoice MenuChoice;
        DateTime timeEnteredScreen;
        DateTime stopShowMarketplaceFail;
        //string marketplaceFailText;


        /// ---------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// ---------------------------------------------------------------
        public TitleScreen(IDrawTool drawTool, IInputTool inputTool, IAudioTool audioTool)
        {
            this.drawTool = drawTool;
            this.inputTool = inputTool;
            this.audioTool = audioTool;
            this.nextScreen = ScreenType.TitleScreen;
            MenuChoice = MenuChoice.Play;
        }

        /// ---------------------------------------------------------------
        /// <summary>
        /// Initialize the game screen
        /// </summary>
        /// ---------------------------------------------------------------
        public override void Initialize()
        {
            nextScreen = ScreenType.TitleScreen;
            timeEnteredScreen = DateTime.Now;
            stopShowMarketplaceFail = DateTime.Now.AddSeconds(-1);
            frame = 0;
        }

        /// ---------------------------------------------------------------
        /// <summary>
        /// Update stuff
        /// </summary>
        /// ---------------------------------------------------------------
        public override void Update(GameTime gameTime)
        {
            audioTool.StopMusic();

            foreach (InputAction action in inputTool.ActionQueue)
            {
                if (action.Click && frame != 0)
                {
                    switch (action.ActionType)
                    {
                        case InputActionType.Exit:
                            if ((DateTime.Now - timeEnteredScreen).TotalSeconds > 1)
                            {
                                nextScreen = ScreenType.ExitGame;
                            }
                            break;
                        case InputActionType.DropAndSlide:
                        case InputActionType.MenuSelect:
                            {
                                switch (MenuChoice)
                                {
                                    case MenuChoice.Play: nextScreen = ScreenType.GameScreen; break;
                                    case MenuChoice.Quit: nextScreen = ScreenType.ExitGame; break;
                                    case MenuChoice.Help: nextScreen = ScreenType.HelpScreen; break;
                                    case MenuChoice.Options: nextScreen = ScreenType.OptionsScreen; break;
                                    default: throw new Exception("Menu Index is illegal value");
                                }
                            }
                            break;
                        case InputActionType.MenuLeft:
                        case InputActionType.MenuUp:
                            MenuChoice--;
                            audioTool.PlaySound(SoundEffectType.Dot, 1, .2f, 0);
                            if (MenuChoice < 0) MenuChoice = MenuChoice.NumberOfChoices - 1;
                            //else if (MenuChoice == MenuChoice.Purchase
                            //    && !buyGameAllowed) MenuChoice--;
                            stopShowMarketplaceFail = DateTime.Now.AddSeconds(-1);
                            break;
                        case InputActionType.MenuRight:
                        case InputActionType.MenuDown:
                            MenuChoice++;
                            audioTool.PlaySound(SoundEffectType.Dot, 1, -.2f, 0);
                            if (MenuChoice >= MenuChoice.NumberOfChoices) MenuChoice = 0;
                            //else if (MenuChoice == MenuChoice.Purchase
                            //    && !buyGameAllowed) MenuChoice++;
                            stopShowMarketplaceFail = DateTime.Now.AddSeconds(-1);
                            break;
                        default:
                            break;
                    }
                }
            }

            audioTool.ResolveState();
            frame++;
        }

        /// ---------------------------------------------------------------
        /// <summary>
        /// Draw a frame in the game
        /// </summary>
        /// ---------------------------------------------------------------
        public override void Draw(GameTime gameTime)
        {
            drawTool.ClearScreen(1);
            drawTool.DrawTitleMenu(MenuChoice);
            drawTool.EndDrawing();
        }
 
    }
}

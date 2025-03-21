using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Eitrix
{
    /// ---------------------------------------------------------
    /// <summary>
    /// Main class for controlling the game
    /// </summary>
    /// ---------------------------------------------------------
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        IDrawTool drawTool;
        IInputTool inputTool;
        IAudioTool audioTool;
        bool optionsLoaded = false;
        GameScreen theGameScreen;
        TitleScreen theTitleScreen;
        HelpScreen theHelpScreen;
        OptionsScreen theOptionScreen;
        Screen currentScreen;

        Exception caughtException;
        string errorOutputFile;
        Exception CaughtException
        {
            get { return caughtException; }
            set
            {
                if (caughtException == null)
                {
                    errorOutputFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "EitrixError.txt");
                    File.WriteAllText(errorOutputFile, value.ToString());
                }

                caughtException = value;
            }
        }

        /// ---------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// ---------------------------------------------------------
        public Game1()
        {
            drawTool = new DefaultDrawTool(this);
            inputTool = new MultiInput();
            inputTool.ReevaluateMappings();
            
            audioTool = new XnaAudioTool();

            theGameScreen = new GameScreen(drawTool, inputTool, audioTool);
            theTitleScreen = new TitleScreen(drawTool, inputTool, audioTool);
            theHelpScreen = new HelpScreen(drawTool, inputTool, audioTool);
            theOptionScreen = new OptionsScreen(drawTool, inputTool, audioTool);
            currentScreen = theTitleScreen;
            Content.RootDirectory = "Content";

#if XBOX
            this.Components.Add(new GamerServicesComponent(this));
#endif
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            DefaultDrawTool defaultDrawTool = drawTool as DefaultDrawTool;
            defaultDrawTool.Initialize();
            defaultDrawTool.LoadContent(Content);
            audioTool.LoadContent(Content);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        Stopwatch stopWatchUpdate = new Stopwatch();
        Stopwatch stopWatchDraw = new Stopwatch();

        /// ---------------------------------------------------------
        /// <summary>
        /// Update
        /// </summary>
        /// ---------------------------------------------------------
        protected override void Update(GameTime gameTime)
        {
            try
            {
                if (this.IsActive)
                {
                    if (!DiskStorage.Available) DiskStorage.Initialize(Globals.StorageName);
                    if (DiskStorage.Available && !optionsLoaded)
                    {
                        inputTool.ReadFromDisk();
                        inputTool.ReevaluateMappings();
                        Globals.Options = EitrixOptions.ReadFromDisk();
                        drawTool.Reset();
                        optionsLoaded = true;
                    }

                    stopWatchUpdate.Reset();
                    stopWatchUpdate.Start();

                    inputTool.UpdateInput();

                    if (CaughtException != null)
                    {
                        foreach (InputAction action in inputTool.ActionQueue)
                        {
                            if (action.ActionType == InputActionType.Exit) Exit();   
                        }
                    }
                    else
                    {
                        currentScreen.Update(gameTime);
                    }

                    base.Update(gameTime);
                    stopWatchUpdate.Stop();
                }
                else
                {
                    base.Update(gameTime);
                }
            }
            catch (Exception e)
            {
                CaughtException = e;
            }
        }

        /// ---------------------------------------------------------
        /// <summary>
        /// Handle the logic to swapscreens
        /// </summary>
        /// ---------------------------------------------------------
        private void SwapScreen()
        {

            Screen nextScreen = null;
            switch (currentScreen.nextScreen)
            {
                case ScreenType.HelpScreen: nextScreen = theHelpScreen;  break;
                case ScreenType.TitleScreen: nextScreen = theTitleScreen; break;
                case ScreenType.OptionsScreen: nextScreen = theOptionScreen; break;
                case ScreenType.GameScreen: nextScreen = theGameScreen; break;
                case ScreenType.ExitGame: 
                default:
                    Exit();
                    Globals.Options.SaveToDisk();
                    inputTool.SaveToDisk();
                    return;
            }

            if (currentScreen == nextScreen) return;
            inputTool.Reset();
            inputTool.ScreenBeingControlled = currentScreen.nextScreen;

            currentScreen = nextScreen;
            currentScreen.Initialize();

        }

        /// ---------------------------------------------------------
        /// <summary>
        /// Draw
        /// </summary>
        /// ---------------------------------------------------------
        protected override void Draw(GameTime gameTime)
        {
            if (CaughtException != null)
            {
                drawTool.ClearScreen(Color.DarkRed);
                drawTool.DebugPrint(10, 10, Color.Yellow, "ARRRGHHH!   THere was a program error.");
                drawTool.DebugPrint(10, 30, Color.Yellow, "Please email this information to support@betafreak.com");
                drawTool.DebugPrint(10, 50, Color.Yellow, "Press the back button or Esc to exit.");

#if WINDOWS
                drawTool.DebugPrint(10, 90 , Color.Yellow, "The following information was written to " + errorOutputFile);

#endif
                drawTool.DebugPrint(10, 110, Color.Yellow, CaughtException.ToString());
                drawTool.EndDrawing();
                base.Draw(gameTime);
                return;
            }

            try
            {
                drawTool.ClearScreen(Color.DarkGray);

                if (this.IsActive)
                {
                    stopWatchDraw.Reset();
                    stopWatchDraw.Start();
                    SwapScreen();


                    currentScreen.Draw(gameTime);
                    stopWatchDraw.Stop();
#if DEBUG
                    drawTool.DebugPrint(100, drawTool.Height - 15, Color.Cyan, "U: " + stopWatchUpdate.Elapsed.TotalMilliseconds.ToString(".000"));
                    drawTool.DebugPrint(200, drawTool.Height - 15, Color.Cyan, "D: " + stopWatchDraw.Elapsed.TotalMilliseconds.ToString(".000"));
                    drawTool.DebugPrint(300, drawTool.Height - 15, Color.Black, "U: " + stopWatchUpdate.Elapsed.TotalMilliseconds.ToString(".000"));
                    drawTool.DebugPrint(400, drawTool.Height - 15, Color.Black, "D: " + stopWatchDraw.Elapsed.TotalMilliseconds.ToString(".000"));
#endif
                }
                else
                {
                    drawTool.ClearScreen();
                    //drawTool.PrintAnnouncement("PAUSED");
                }

                drawTool.EndDrawing();
                base.Draw(gameTime);
            }
            catch (Exception e)
            {
                CaughtException = e;
            }
        }
    }
}

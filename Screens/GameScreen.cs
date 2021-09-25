using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Eitrix
{
    class GameScreen : Screen
    {
        IDrawTool drawTool;
        IInputTool inputTool;
        IAudioTool audioTool;
        World theWorld;
        bool paused;
        int selectedPauseOption = 0;
        
        /// ---------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// ---------------------------------------------------------------
        public GameScreen(IDrawTool drawTool, IInputTool inputTool, IAudioTool audioTool)
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
            frame = 0;
            this.nextScreen = ScreenType.GameScreen;
            theWorld = new World(audioTool);
            GamePadInput.TwoPlayersPerController = Globals.Options.TwoPlayersPerController;
            inputTool.ReevaluateMappings();
            paused = false;
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
                if (frame == 0)
                {
                    // Drain the action queue in the first frame to
                    // prevent automatically starting up a player  
                    // on the first keypress from the menu
                }
                else
                {
                    if (paused && action.Click)
                    {
                        switch (action.ActionType)
                        {
                            case InputActionType.MenuUp: selectedPauseOption--; audioTool.PlaySound(SoundEffectType.Dot01, 1, .5f, 0); break;
                            case InputActionType.MoveDown: selectedPauseOption++; audioTool.PlaySound(SoundEffectType.Dot01, 1, -.5f, 0); break;
                            case InputActionType.MoveLeft:
                                switch (selectedPauseOption)
                                {
                                    case 0: audioTool.SkipToNextBackgroundMusic(); break;
                                    case 1: Globals.Options.MusicVolume.Decrement(); audioTool.SetMusicVolume(1); break;
                                    case 2: Globals.Options.SoundEffectVolume.Decrement(); audioTool.PlaySound(SoundEffectType.Strum); break;
                                    default: break;
                                }
                                break;
                            case InputActionType.MoveRight:
                                switch (selectedPauseOption)
                                {
                                    case 0: audioTool.SkipToNextBackgroundMusic(); break;
                                    case 1: Globals.Options.MusicVolume.Increment(); audioTool.SetMusicVolume(1); break;
                                    case 2: Globals.Options.SoundEffectVolume.Increment(); audioTool.PlaySound(SoundEffectType.Strum); break;
                                    default: break;
                                }
                                break;
                            case InputActionType.MenuSelect:
                                switch (selectedPauseOption)
                                {
                                    case 3: paused = false; break;
                                    case 4: nextScreen = ScreenType.TitleScreen; break;
                                    default: break;
                                }
                                break;
                        }
                        if (selectedPauseOption < 0) selectedPauseOption = 4;
                        if (selectedPauseOption > 4) selectedPauseOption = 0;

                    }

                    if (action.ActionType == InputActionType.Advance && action.Click)
                    {
                        paused = true;
                    }

                    if (action.ActionType == InputActionType.Exit && action.Click)
                    {
                        paused = !paused;
                    }
                    else if (action.ActionType == InputActionType.ToggleStress)
                    {
                        if (action.Click)
                        {
                            Globals.StressEnabled = !Globals.StressEnabled;
                        }
                    }
                    else
                    {
                        if (!paused)
                        {
                            theWorld.HandleAction(action);
                        }
                    }
                }

            }

            if (!paused)
            {
                theWorld.Update(gameTime, audioTool);
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
            drawTool.ClearScreen();
            theWorld.Draw(drawTool, gameTime);

            if (paused)
            {
                List<Tuple<string, VisualOptionValue>> options = new List<Tuple<string,VisualOptionValue>>();
                options.Add(new Tuple<string, VisualOptionValue>("Switch Music", new VisualOptionValue(audioTool.CurrentSongName)));
                options.Add(new Tuple<string, VisualOptionValue>("Music Volume", new VisualOptionValue(Globals.Options.MusicVolume.ValueString)));
                options.Add(new Tuple<string, VisualOptionValue>("Effects Volume", new VisualOptionValue(Globals.Options.SoundEffectVolume.ValueString)));
                options.Add(new Tuple<string, VisualOptionValue>("Resume Play", new VisualOptionValue()));
                options.Add(new Tuple<string, VisualOptionValue>("Quit", new VisualOptionValue()));

                drawTool.DrawPauseScreen(options, selectedPauseOption); 
            }
            drawTool.EndDrawing();
        }


    }
}

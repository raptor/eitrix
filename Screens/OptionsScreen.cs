using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Eitrix
{
    class OptionsScreen : Screen
    {
        IDrawTool drawTool;
        IInputTool inputTool;
        IAudioTool audioTool;

        static int selectedOption = 0;

        /// ---------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// ---------------------------------------------------------------
        public OptionsScreen(IDrawTool drawTool, IInputTool inputTool, IAudioTool audioTool)
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
            this.nextScreen = ScreenType.OptionsScreen;
            selectedOption = 0;
            
            keyboardOptionsToShow = new List<Tuple<string, VisualOptionValue>>();
            keyboardOptionsToShow.Add(new Tuple<string, VisualOptionValue>("Player", new VisualOptionValue()));

            for (int i = 1; i < inputActionOptionList.Length; i++)
            {
                keyboardOptionsToShow.Add(new Tuple<string, VisualOptionValue>(inputActionOptionList[i].ToString(), new VisualOptionValue()));
            }


            generalOptionsToShow = new List<Tuple<string, VisualOptionValue>>();
                 
            for (int i = 0; i < Globals.Options.OptionList.Count; i++)
            {
                generalOptionsToShow.Add(new Tuple<string, VisualOptionValue>(Globals.Options.OptionList[i].Name, new VisualOptionValue()));
            }
        }

        /// ---------------------------------------------------------------
        /// <summary>
        /// Update stuff
        /// </summary>
        /// ---------------------------------------------------------------
        public override void Update(GameTime gameTime)
        {
            if (CollectKeystroke)
            {
                KeyboardState state = Keyboard.GetState();

                Keys[] keys = state.GetPressedKeys();
                if (WaitingForKeyUp)
                {
                    if (keys.Length == 0)
                    {
                        WaitingForKeyUp = false;
                        if (FoundKeyStroke) CollectKeystroke = false;
                    }
                }

                else if (keys.Length > 0)
                {
                    KeyboardInput.AddCustomKeyMapping(keys[0], CurrentPlayer, inputActionOptionList[selectedOption]);
                    WaitingForKeyUp = true;
                    FoundKeyStroke = true;
                }
            }

            foreach (InputAction action in inputTool.ActionQueue)
            {
                if (!action.Click || CollectKeystroke) continue;
                switch (action.ActionType)
                {
                    case InputActionType.Exit:
                        if (Globals.ConfiguringKeyboard)
                        {
                            Globals.ConfiguringKeyboard = false;
                            selectedOption = 0;
                        }
                        else
                        {
                            nextScreen = ScreenType.TitleScreen;
                        }
                        break;
                    case InputActionType.MenuDown: selectedOption++; audioTool.PlaySound(SoundEffectType.Dot01, 1, .5f, 0); break;
                    case InputActionType.MenuUp: selectedOption--; audioTool.PlaySound(SoundEffectType.Dot01, 1, -.5f, 0); break;
                    case InputActionType.MenuLeft:
                        audioTool.PlaySound(SoundEffectType.Dot03, 1, -.5f, 0);
                        if (Globals.ConfiguringKeyboard)
                        {
                            CurrentPlayer--;
                            if (CurrentPlayer < 0) CurrentPlayer = Globals.MaxPlayers - 1;
                        }
                        else
                        {
                            Globals.Options.OptionList[selectedOption].Decrement();
                        }
                        break;
                    case InputActionType.MenuRight:
                        audioTool.PlaySound(SoundEffectType.Dot03, 1, .5f, 0);
                        if (Globals.ConfiguringKeyboard)
                        {
                            CurrentPlayer++;
                            if (CurrentPlayer >= Globals.MaxPlayers) CurrentPlayer = 0;
                        }
                        else
                        {
                            Globals.Options.OptionList[selectedOption].Increment();
                        }
                        break;
                    case InputActionType.MenuSelect:
                        if (Globals.ConfiguringKeyboard)
                        {
                            CollectKeystroke = true;
                            FoundKeyStroke = false;
                            WaitingForKeyUp = true;
                        }
                        else
                        {
                            Globals.Options.OptionList[selectedOption].Activate();
                        }
                        break;
                    case InputActionType.MenuDelete:
                        if (Globals.ConfiguringKeyboard)
                        {
                            KeyboardInput.ClearKeys(CurrentPlayer, inputActionOptionList[selectedOption]);
                        }
                        break;

                }


                int maxOptions = Globals.Options.OptionList.Count;
                if (Globals.ConfiguringKeyboard) maxOptions = inputActionOptionList.Length;

                if (selectedOption < 0) selectedOption = 0;
                if (selectedOption >= maxOptions) selectedOption = maxOptions - 1;
            }
            

            if (Globals.ConfiguringKeyboard)
            {
                keyboardOptionsToShow[0].SecondValue.Value = (CurrentPlayer+1).ToString();
                for (int i = 1; i < inputActionOptionList.Length; i++)
                {
                    keyboardOptionsToShow[i].SecondValue.HighlightMe = (i == selectedOption && CollectKeystroke);
                    
                    string optionValue = GetKeyMappingValue(inputActionOptionList[i]);

                    keyboardOptionsToShow[i].SecondValue.Value = optionValue;
                    keyboardOptionsToShow[i].SecondValue.Error = false;
                    if (optionValue.Contains("!>")) keyboardOptionsToShow[i].SecondValue.Error = true;
                    
                }

            }
            else
            {
                for (int i = 0; i < Globals.Options.OptionList.Count; i++)
                {
                    generalOptionsToShow[i].SecondValue.Value = Globals.Options.OptionList[i].ValueString;
                }
            }


            audioTool.ResolveState();
        }

        InputActionType[] inputActionOptionList = new InputActionType[]
        {
            InputActionType.None,
            InputActionType.MoveLeft,
            InputActionType.MoveRight,
            InputActionType.MoveDown,
            InputActionType.RotateLeft,
            InputActionType.RotateRight,
            InputActionType.DropAndSlide,
            InputActionType.DropAndStick,
            InputActionType.ApplyAntidote,
            InputActionType.ChangeVictim,
        };

        int CurrentPlayer = 0;
        bool CollectKeystroke = false;
        bool FoundKeyStroke = true;
        bool WaitingForKeyUp = false;
        List<Tuple<string, VisualOptionValue>> generalOptionsToShow = null;
        List<Tuple<string, VisualOptionValue>> keyboardOptionsToShow = null;
        /// ---------------------------------------------------------------
        /// <summary>
        /// Draw a frame in the game
        /// </summary>
        /// ---------------------------------------------------------------
        public override void Draw(GameTime gameTime)
        {
            drawTool.ClearScreen(2);
            string title = Globals.ConfiguringKeyboard ? "Configuring Keyboard" : "General Options";

            if (Globals.ConfiguringKeyboard)
            {
                string instructions = "Press Del to clear keys, <Enter> to add a key";
                if (CollectKeystroke) instructions = "Press a key to assign ...";
                drawTool.DrawOptions(keyboardOptionsToShow, selectedOption, title, instructions);
            }
            else
            {
                drawTool.DrawOptions(generalOptionsToShow, selectedOption, title, "Use arrows or stick to change values, <Enter> or A to select.");
            }
            drawTool.EndDrawing();
        }

        /// ---------------------------------------------------------------
        /// <summary>
        /// Get a string representation of keyboard mappings for the current player number
        /// </summary>
        /// ---------------------------------------------------------------
        string GetKeyMappingValue(InputActionType actionType)
        {
            List<Keys> keys = KeyboardInput.QueryMapping(CurrentPlayer, actionType);

            StringBuilder output = new StringBuilder();
            foreach (Keys key in keys)
            {
                if(output.Length > 0) output.Append(", ");
                string name = key.ToString();
                if (name.StartsWith("Oem")) name = name.Substring(3);
                if (name.StartsWith("NumPad")) name = "#" + name.Substring(6);
                if (name.Length == 2 && name.StartsWith("D")) name = name.Substring(1);
                output.Append(name);
                if (KeyboardInput.IsMultiMapped(key)) output.Append(" <Conflict!>");
            }
            return output.ToString();
        }


        /// ---------------------------------------------------------------
        /// <summary>
        /// Put the screen in a Configure keyboard state
        /// </summary>
        /// ---------------------------------------------------------------
        public static void ConfigureKeyboard()
        {
            Globals.ConfiguringKeyboard = true;
            selectedOption = 0;
        }

    }

    public class VisualOptionValue
    {
        public string Value;
        public bool HighlightMe;
        public TimeWatcher Timer;
        public bool Error;

        public VisualOptionValue()
        {
            Timer = new TimeWatcher();
            Value = "";
        }

        public VisualOptionValue(string value)
        {
            Timer = new TimeWatcher();
            Value = value;
        }

    }

}

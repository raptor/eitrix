using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Eitrix
{
    public class KeyMapping
    {
        public Keys Key;
        public InputActionType InputActionType;
        public int PlayerNumber;

        public KeyMapping() { }
        public KeyMapping(Keys key, int playerNumber, InputActionType type)
        {
            this.Key = key;
            this.InputActionType = type;
            this.PlayerNumber = playerNumber;
        }
    }

    
    public class KeyboardInput : CommonInput
    {
        Dictionary<Keys, InputAction> menuKeyMappings;
        Dictionary<Keys, List<InputAction>> allKeyMappings;
        public static List<KeyMapping> gameKeyMappings = new List<KeyMapping>();

        /// ---------------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public KeyboardInput()
        {
            menuKeyMappings = new Dictionary<Keys, InputAction>();

#if DEBUG
            menuKeyMappings.Add(Keys.Pause, new InputAction() { PlayerID = Globals.NonPlayer, ActionType = InputActionType.DebugAction });
#endif
            menuKeyMappings.Add(Keys.Scroll, new InputAction() { PlayerID = Globals.NonPlayer, ActionType = InputActionType.ToggleStress });
            menuKeyMappings.Add(Keys.Escape, new InputAction() { PlayerID = Globals.NonPlayer, ActionType = InputActionType.Exit });

            menuKeyMappings.Add(Keys.Enter, new InputAction() { PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuSelect });
            menuKeyMappings.Add(Keys.Up, new InputAction() { PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuUp });
            menuKeyMappings.Add(Keys.Down, new InputAction() { PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuDown });
            menuKeyMappings.Add(Keys.Left, new InputAction() { PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuLeft });
            menuKeyMappings.Add(Keys.Right, new InputAction() { PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuRight });
            menuKeyMappings.Add(Keys.Delete, new InputAction() { PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuDelete });
            menuKeyMappings.Add(Keys.Back, new InputAction() { PlayerID = Globals.NonPlayer, ActionType = InputActionType.MenuDelete });

            // Special function keys
            menuKeyMappings.Add(Keys.F1, new InputAction() { ControllerID = 0, PlayerID = Globals.NonPlayer, ActionType = InputActionType.InstantComputer, AnalogValue = 1 });
            menuKeyMappings.Add(Keys.F2, new InputAction() { ControllerID = 0, PlayerID = Globals.NonPlayer, ActionType = InputActionType.Advance, AnalogValue = 1 });

            // Player 1 (Arrow Keys + ZXCV)
            gameKeyMappings.Add(new KeyMapping(Keys.Left, 0, InputActionType.MoveLeft));
            gameKeyMappings.Add(new KeyMapping(Keys.Right, 0, InputActionType.MoveRight));
            gameKeyMappings.Add(new KeyMapping(Keys.Down, 0, InputActionType.MoveDown));
            gameKeyMappings.Add(new KeyMapping(Keys.Up, 0, InputActionType.DropAndStick));
            gameKeyMappings.Add(new KeyMapping(Keys.Z, 0, InputActionType.RotateLeft));
            gameKeyMappings.Add(new KeyMapping(Keys.X, 0, InputActionType.RotateRight));
            gameKeyMappings.Add(new KeyMapping(Keys.C, 0, InputActionType.ApplyAntidote));
            gameKeyMappings.Add(new KeyMapping(Keys.V, 0, InputActionType.ChangeVictim));

            // Player 2 (WASD + HJKL)
            gameKeyMappings.Add(new KeyMapping(Keys.A, 1, InputActionType.MoveLeft));
            gameKeyMappings.Add(new KeyMapping(Keys.D, 1, InputActionType.MoveRight));
            gameKeyMappings.Add(new KeyMapping(Keys.S, 1, InputActionType.MoveDown));
            gameKeyMappings.Add(new KeyMapping(Keys.W, 1, InputActionType.DropAndStick));
            gameKeyMappings.Add(new KeyMapping(Keys.H, 1, InputActionType.RotateLeft));
            gameKeyMappings.Add(new KeyMapping(Keys.J, 1, InputActionType.RotateRight));
            gameKeyMappings.Add(new KeyMapping(Keys.K, 1, InputActionType.ApplyAntidote));
            gameKeyMappings.Add(new KeyMapping(Keys.L, 1, InputActionType.ChangeVictim));

            // Player 3 (Numpad)
            gameKeyMappings.Add(new KeyMapping(Keys.NumPad1, 2, InputActionType.MoveLeft));
            gameKeyMappings.Add(new KeyMapping(Keys.NumPad3, 2, InputActionType.MoveRight));
            gameKeyMappings.Add(new KeyMapping(Keys.NumPad2, 2, InputActionType.MoveDown));
            gameKeyMappings.Add(new KeyMapping(Keys.NumPad5, 2, InputActionType.DropAndStick));
            gameKeyMappings.Add(new KeyMapping(Keys.NumPad4, 2, InputActionType.RotateLeft));
            gameKeyMappings.Add(new KeyMapping(Keys.NumPad6, 2, InputActionType.RotateRight));
            gameKeyMappings.Add(new KeyMapping(Keys.NumPad0, 2, InputActionType.ApplyAntidote));
            gameKeyMappings.Add(new KeyMapping(Keys.OemPlus, 2, InputActionType.ChangeVictim));
        }

        /// ---------------------------------------------------------------------------------
        /// <summary>
        /// Change allKeyMappings to reflect the game key mappings
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public override void ReevaluateMappings()
        {
            allKeyMappings = new Dictionary<Keys, List<InputAction>>();

            foreach (Keys key in menuKeyMappings.Keys)
            {
                AddKeyMapping(key, menuKeyMappings[key]);
            }

            foreach (KeyMapping mapping in gameKeyMappings)
            {
                AddKeyMapping(mapping.Key, 
                    new InputAction() { 
                        ControllerID = 0, 
                        PlayerID = Globals.KeyBoardPlayerBaseId + mapping.PlayerNumber,
                        ActionType = mapping.InputActionType,
                        AnalogValue = 1});
            }
        }

        /// ---------------------------------------------------------------------------------
        /// <summary>
        /// Add a general mapping to allKeyMappings for this key
        /// </summary>
        /// ---------------------------------------------------------------------------------
        private void AddKeyMapping(Keys key, InputAction inputAction)
        {
            if (!allKeyMappings.ContainsKey(key)) allKeyMappings.Add(key, new List<InputAction>());

            allKeyMappings[key].Add(inputAction);
        }

        /// ---------------------------------------------------------------------------------
        /// <summary>
        /// Add a general mapping to allKeyMappings for this key
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public static void AddCustomKeyMapping(Keys key, int playerNumber, InputActionType actionType)
        {
            for (int i = 0; i < gameKeyMappings.Count; )
            {
                if (gameKeyMappings[i].PlayerNumber == playerNumber && gameKeyMappings[i].InputActionType == actionType)
                {
                    gameKeyMappings.RemoveAt(i);
                    continue;
                }
                else i++;
            }

            gameKeyMappings.Add(new KeyMapping(key, playerNumber, actionType));
        }

        /// ---------------------------------------------------------------------------------
        /// <summary>
        /// Indicate if a key is mapped more than once
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public static bool IsMultiMapped(Keys key)
        {
            int count = 0;
            for (int i = 0; i < gameKeyMappings.Count; i++)
            {
                if (gameKeyMappings[i].Key == key) count++;
            }
            return (count > 1);
        }

        /// ---------------------------------------------------------------------------------
        /// <summary>
        ///  Refresh input state
        /// </summary>
        /// ---------------------------------------------------------------------------------
        public override void UpdateInput()
        {
            //Debug.WriteLine("Updating input"); 
            KeyboardState currentState = Keyboard.GetState();

            foreach (Keys key in currentState.GetPressedKeys())
            {
                List<InputAction> newKeyActions;
                if (allKeyMappings.TryGetValue(key, out newKeyActions))
                {
                    foreach (InputAction action in newKeyActions)
                    {
                        AddAction(action);
                    }
                }
                else
                {
#if DEBUG
                    Debug.WriteLine("Unhandled Key: " + key);
#endif
                }
            }

            DoneWithFrame();
        }


        internal static List<Keys> QueryMapping(int playerNumber, InputActionType inputActionType)
        {
            List<Keys> output = new List<Keys>();
            foreach (KeyMapping mapping in gameKeyMappings)
            {
                if (mapping.PlayerNumber == playerNumber && mapping.InputActionType == inputActionType)
                {
                    output.Add(mapping.Key);
                }
            }
            return output;
        }

        internal static void ClearKeys(int playerNumber, InputActionType inputActionType)
        {
            for (int i = 0; i < gameKeyMappings.Count;  )
            {
                KeyMapping mapping = gameKeyMappings[i]; 
                if (mapping.PlayerNumber == playerNumber && mapping.InputActionType == inputActionType)
                {
                    gameKeyMappings.RemoveAt(i);
                    continue;
                }

                i++;
            }
        }

        string saveFileName = "KeyboardMappings.dat";
        public override void SaveToDisk()
        {
            try
            {
                DiskStorage.SaveToFile(saveFileName, (stream) =>
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<KeyMapping>));
                    serializer.Serialize(stream, gameKeyMappings);
                });
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e.Message); 
            }

        }

        public override void ReadFromDisk()
        {
            try
            {
                DiskStorage.ReadFromFile(saveFileName, (stream) =>
                {
                    if (stream.Length > 0)
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<KeyMapping>));
                        gameKeyMappings = (List<KeyMapping>)serializer.Deserialize(stream);
                    }
                });

            }
            catch (Exception) { }
        }

    }
}

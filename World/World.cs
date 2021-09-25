using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eitrix
{
    /// --------------------------------------------------------------
    /// <summary>
    /// Game States
    /// </summary>
    /// --------------------------------------------------------------
    public enum GameState
    {
        Gathering,
        Playing,
        EndRound
    }

    /// --------------------------------------------------------------
    /// <summary>
    /// All of the game lives here
    /// </summary>
    /// --------------------------------------------------------------
    public class World
    {
        Dictionary<int, Player> players = new Dictionary<int, Player>();
        List<int> playerIds = new List<int>();
        public GameState GameState = GameState.Gathering;
        int numRounds = Globals.Options.RoundsPerTourney;
        int currentRound = 0;
        bool moveFast;

        public List<Player> Players 
        { 
            get 
            { 
                List<Player> returnMe = new List<Player>();
                for (int i = 0; i < playerIds.Count; i++)
                {
                    returnMe.Add(players[playerIds[i]]);
                }
                return returnMe;
            }
        }

        public IAudioTool AudioTool { get; set; }

        /// --------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// --------------------------------------------------------------
        public World(IAudioTool audioTool)
        {
            this.AudioTool = audioTool;
            audioTool.SkipToNextBackgroundMusic();

            if (Globals.StressEnabled)
            {
                numRounds = 10000;
            }
        }

        /// --------------------------------------------------------------
        /// <summary>
        /// returns true if the world has this player.
        /// </summary>
        /// --------------------------------------------------------------
        internal bool HasPlayer(int playerId)
        {
            return players.ContainsKey(playerId);
        }

        /// --------------------------------------------------------------
        /// <summary>
        /// Add a new player to the game
        /// </summary>
        /// --------------------------------------------------------------
        internal void AddPlayer(int playerId)
        {
            if (players.Count >= Globals.MaxPlayers) return;
            Player newPlayer = new Player();
            newPlayer.Initialize(this);
            newPlayer.Number = players.Count;
            newPlayer.State = PlayerState.Prepping;
            players.Add(playerId, newPlayer);
            playerIds.Add(playerId);

            if (playerId >= Globals.ComputerPlayerId)
            {
                newPlayer.ThinkType = ThinkType.ComputerNormal;
                newPlayer.State = PlayerState.Ready;
            }
        }


        /// --------------------------------------------------------------
        /// <summary>
        /// Draw
        /// </summary>
        /// --------------------------------------------------------------
        internal void Draw(IDrawTool drawTool, GameTime gameTime)
        {
            switch (GameState)
            {
                case GameState.Gathering: drawTool.DrawPlayersGathering(Players); break;
                case GameState.EndRound: drawTool.DrawRoundStats(currentRound, numRounds, Players);  break;
                case GameState.Playing: drawTool.DrawPlayers(Players); break;
                default: throw new ApplicationException("Unknown gamestate: " + GameState); 
            }
        }

        int[] WinPoints = new int[] { 8, 5, 3, 2, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        /// --------------------------------------------------------------
        /// <summary>
        /// Update
        /// </summary>
        /// --------------------------------------------------------------
        internal void Update(GameTime gameTime, IAudioTool audioTool)
        {

            switch (GameState)
            {
                case GameState.Gathering:
                    bool allComputerPlayers = true;
                    foreach (Player player in Players)
                    {
                        player.Update(gameTime);
                        if (player.ThinkType == ThinkType.Human) allComputerPlayers = false;
                    }

                    if ((AllPlayersReady() && !allComputerPlayers) || forcePlay)
                    {
                        StartPlaying();
                    }

                    break;
                case GameState.Playing:
                    int iterations = 1;
                    if (moveFast || Globals.StressEnabled) iterations = 10;
                    for (int iterationCount = 0; iterationCount < iterations; iterationCount++)
                    {
                        int deadPlayerCount = 0;
                        bool onlyComputersLeft = true;
                        foreach (Player player in Players)
                        {
                            player.Update(gameTime);
                            if (player.State == PlayerState.Dead) deadPlayerCount++;
                            if (player.State == PlayerState.Playing && !player.IsComputer) onlyComputersLeft = false;
                        }

                        if (!forcePlay && onlyComputersLeft)
                        {
                            moveFast = true;
                            foreach (Player player in players.Values)
                            {
                                if (player.IsComputer) player.MoveFast = true;
                            }
                        }

                        if (deadPlayerCount == Players.Count<Player>())
                        {
                            List<Player> sortedPlayers = Player.SortPlayers(players.Values, (p1, p2) => { return p1.DeathTimer > p2.DeathTimer; });

                            for (int i = 0; i < sortedPlayers.Count; i++)
                            {
                                Player player = sortedPlayers[i];
                                player.Points = WinPoints[i];
                                player.State = PlayerState.Waiting;
                                player.TotalPoints += player.Points;
                                player.TotalRows += player.Rows;
                                player.TotalScore += player.Score;
                            }

                            GameState = GameState.EndRound;
                            break;
                        }
                    }

                    break;
                case GameState.EndRound:
                    foreach (Player player in Players)
                    {
                        player.Update(gameTime);
                    }

                    if (AllPlayersReady() || Globals.StressEnabled || forcePlay)
                    {
                        if (currentRound < numRounds)
                        {
                            StartPlaying();
                        }
                    }

                    break;

            }


        }

        /// --------------------------------------------------------------
        /// <summary>
        /// Cranks up a new game
        /// </summary>
        /// --------------------------------------------------------------
        private void StartPlaying()
        {
            AudioTool.PlaySound(SoundEffectType.GameStart);
            List<Player> playersInSlots = Players;
            for (int i = 0; i < playersInSlots.Count; i++)
            {
                playersInSlots[i].VictimId = (i + 1) % playersInSlots.Count;
                playersInSlots[i].Initialize(this);
                playersInSlots[i].State = PlayerState.Playing;

            }
            moveFast = false;
            GameState = GameState.Playing;
            currentRound++;
        }

        /// --------------------------------------------------------------
        /// <summary>
        /// Checks if all the players are ready to start
        /// </summary>
        /// --------------------------------------------------------------
        private bool AllPlayersReady()
        {
            int playerReadyCount = 0;
            foreach (Player player in Players)
            {
                if (player.State == PlayerState.Ready) playerReadyCount++;
            }

            bool allPlayersReady = playerReadyCount > 0 && playerReadyCount == Players.Count<Player>();
            return allPlayersReady;
        }

        bool forcePlay = false;
        /// --------------------------------------------------------------
        /// <summary>
        /// Handle input actions
        /// </summary>
        /// --------------------------------------------------------------
        internal void HandleAction(InputAction action)
        {
            if (action.ActionType == InputActionType.DebugAction) DoDebugAction();
            if (action.ActionType == InputActionType.InstantComputer && action.Click && GameState == GameState.Gathering)
            {
                if (players.Count == Globals.MaxPlayers) forcePlay = true;
                AudioTool.PlaySound(SoundEffectType.Wooahh00);
                AddPlayer(Globals.ComputerPlayerId);
                AssignAsComputer(Globals.ComputerPlayerId);
                return;
            }

            if (action.PlayerID == Globals.NonPlayer) return;
            if (!HasPlayer(action.PlayerID) &&
                action.Click &&
                (action.ActionType == InputActionType.DropAndStick
                 || action.ActionType == InputActionType.DropAndStick
                 || action.ActionType == InputActionType.DropAndSlide
                 || action.ActionType == InputActionType.ChangeVictim
                 || action.ActionType == InputActionType.ApplyAntidote))
            {
                if (GameState == GameState.Gathering)
                {
                    AudioTool.PlaySound(SoundEffectType.Wooahh00);
                    AddPlayer(action.PlayerID);
                    return;
                }
            }

            if (!HasPlayer(action.PlayerID)) return;

            players[action.PlayerID].HandleAction(action);
        }

        TimeWatcher debugActionTimer = new TimeWatcher(.5);
        ///------------------------------------------------------------------------------
        /// <summary>
        /// Do something
        /// </summary>
        ///------------------------------------------------------------------------------
        private void DoDebugAction()
        {
            if (!debugActionTimer.Expired) return;
            debugActionTimer.Reset();

            foreach (Player player in Players)
            {
                if (player.ThinkType != ThinkType.Human)
                {
                    player.State = PlayerState.Dying;
                    //player.RepelledAttack = true;
                    //AudioTool.PlaySound(SoundEffectType.Attack02);
                }

                //Special evilPieces = new Special.EvilPieces(player, Players[player.VictimId], this);
                //evilPieces.AddToPlayer();
            }

            //Player player = Players[0];
            //for (int j = 0; j < player.Grid.Height; j++)
            //{
            //    for (int i = 0; i < player.Grid.Width; i++)
            //    {
            //        if (player.Grid[i, j] == null) continue;
            //        player.Grid[i, j].AnimationType = AnimationType.ExplodeMe;
            //        return;
            //    }
            //}
        }


        ///------------------------------------------------------------------------------
        /// <summary>
        /// Move the player down in the list of players
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void MovePlayerDown(Player player)
        {
            if (player.Number == playerIds.Count - 1) return;

            int tempId = playerIds[player.Number];
            playerIds[player.Number] = playerIds[player.Number + 1];
            playerIds[player.Number + 1] = tempId;
            players[playerIds[player.Number]].Number--;
            player.Number++;
        }
        ///------------------------------------------------------------------------------
        /// <summary>
        /// Move the player up in the list of players
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void MovePlayerUp(Player player)
        {
            if (player.Number == 0) return;

            int tempId = playerIds[player.Number];
            playerIds[player.Number] = playerIds[player.Number - 1];
            playerIds[player.Number - 1] = tempId;
            players[playerIds[player.Number]].Number++;
            player.Number--;
        }

        int computerPlayerId = Globals.ComputerPlayerId + 100;
        ///------------------------------------------------------------------------------
        /// <summary>
        /// Turn this player into an autonomous computer player
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void AssignAsComputer(int playerId)
        {
            if (!HasPlayer(playerId)) return;
            Player player = players[playerId];
            if (player.ThinkType == ThinkType.Human) player.ThinkType = ThinkType.ComputerNormal;
            players.Remove(playerId);  
            computerPlayerId++;
            players.Add(computerPlayerId, player);
            playerIds[player.Number] = computerPlayerId;
        }
    }
}
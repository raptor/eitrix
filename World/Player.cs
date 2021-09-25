using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Eitrix
{

    public enum PlayerState
    {
        Prepping,
        ChooseBrains,
        ChooseOrder, 
        ChooseName,
        ChoosePractice,
        ChooseReadiness,
        Ready,
        Playing,
        Dying,
        Dead,
        Waiting,
    }

    public enum ThinkType
    {
        Human,
        ComputerEasy,
        ComputerNormal,
        ComputerHard,
        Dmitri,
        NumberOfTypes
    }

    /// <summary>
    /// All the player data goes here
    /// </summary>
    public class Player
    {
        World world;
        Grid grid;
        TimeWatcher nextDropTickTimer;
        double dropTickIntervalSeconds;
        double computerMoveIntervalSeconds;
        bool computerHasAMove;
        bool needsToRethink;
        int computerRotation = 0;
        int computerShift = 0;
        int specialBlockCount = 0;
        InputActionType lastPracticePress = InputActionType.None;
        public List<Piece> NextPieces { get; set; }
        public List<Special> Attacks { get; set; }
        public List<Special> Afflictions { get; set; }
        public List<Special> Powers { get; set; }
        public List<Special> Weapons { get; set; }
        public List<object> PsychoColors { get; set; }
        public int[,] PsychoOverlayGrid { get; set; }

        public List<PracticeAction> PracticeActions;
        public double DropTickIntervalSeconds { get { return dropTickIntervalSeconds; } set { dropTickIntervalSeconds = value; } }
        public Grid Grid { get { return grid; } }
        public TextInputWidget Name { get; set; }
        public Piece CurrentPiece { get; set; }
        public int Number { get; set; }
        public int BackGround { get; set; }
        public int Points { get; set; }
        public int Rows { get; set; }
        public int Score { get; set; }
        public int TotalScore { get; set; }
        public int TotalPoints { get; set; }
        public int TotalRows { get; set; }
        public int ShadowY { get; set; }
        public int VictimId { get; set; }
        public bool IsComputer { get { return this.ThinkType != ThinkType.Human; } }
        public ThinkType ThinkType { get; set; }
        public PlayerState State { get; set; }
        public PlayerState lastState;
        public TimeWatcher StateTimer { get; set; }
        public TimeWatcher DeathTimer { get; set; }
        public TimeWatcher LastAttackTimer { get; set; }
        public TimeWatcher LastComputerMoveTimer { get; set; }
        public TimeWatcher NextSpecialTimer { get; set; }
        TimeWatcher LastPieceSet { get; set; }
        public bool MoveFast { get; set; }
        public bool SeeShadows { get; set; }
        public bool EvilPieces { get; set; }
        public bool CrazyIvan { get; set; }
        public bool FreezeDried { get; set; }
        public bool Transparency { get; set; }
        public bool RepelledAttack { get; set; }

        static string[] randomPlayerNames = new string[]
        {
            "Alice",
            "Anastasia",
            "Argos",
            "Aristotle",
            "Athena",
            "Babyface",
            "Bernie",
            "Bilbo",
            "Bubba",
            "Cassius",
            "Cerberus",
            "Charlotte",
            "Daisy",
            "Desdemona",
            "Earnest",
            "Eddie2",
            "Estella",
            "George",
            "Grumpy",
            "Hal 9000",
            "Happy",
            "Heathcliff",
            "Holden",
            "Homer",
            "Hungry",
            "Lenny",
            "Mars",
            "Mork",
            "Mowgli",
            "Olive",
            "Pioneer",
            "Romeo",
            "Shadowfax",
            "Sleepy",
            "Smokey",
            "Snoopy",
            "Sputnik",
            "Tess",
            "Wilma",
            "Winnie",
            "XOLOTL",
            "Zeb",
        };
        ///------------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        ///------------------------------------------------------------------------------
        public Player()
        {
            StateTimer = new TimeWatcher();
            DeathTimer = new TimeWatcher();
            LastPieceSet = new TimeWatcher(.1);
            LastAttackTimer  = new TimeWatcher(0);
            LastComputerMoveTimer = new TimeWatcher(computerMoveIntervalSeconds);
            Name = new TextInputWidget();
            Name.MyValue = randomPlayerNames[Globals.rand.Next(randomPlayerNames.Length)];
            this.State = PlayerState.Prepping;
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Set up for a new game
        /// </summary>
        ///------------------------------------------------------------------------------
        public void Initialize(World world)
        {
            BackGround = -1;
            this.world = world;
            dropTickIntervalSeconds = 1;
            Score = Rows = Points = 0;
            NextPieces = new List<Piece>();
            MoveFast = false;
            NextSpecialTimer = new TimeWatcher(0);
            Attacks = new List<Special>();
            Afflictions = new List<Special>();
            Powers = new List<Special>();
            Weapons = new List<Special>();
            PsychoColors = null;
            PsychoOverlayGrid = null;
            lastState = PlayerState.Prepping;
            SeeShadows = false;
            EvilPieces = false;
            CrazyIvan = false;
            FreezeDried = false;
            Transparency = false;
            PracticeActions = new List<PracticeAction>();
            PracticeActions.Add(new PracticeAction(InputActionType.MoveLeft, "Left"));
            PracticeActions.Add(new PracticeAction(InputActionType.MoveRight, "Right"));
            PracticeActions.Add(new PracticeAction(InputActionType.MoveDown, "Down"));
            PracticeActions.Add(new PracticeAction(InputActionType.RotateLeft, "Rotate Left"));
            PracticeActions.Add(new PracticeAction(InputActionType.RotateRight, "Rotate Right"));
            PracticeActions.Add(new PracticeAction(InputActionType.DropAndSlide, "Drop & Slide"));
            PracticeActions.Add(new PracticeAction(InputActionType.DropAndStick, "Drop & Stick"));
            PracticeActions.Add(new PracticeAction(InputActionType.ChangeVictim, "Victim"));
            PracticeActions.Add(new PracticeAction(InputActionType.ApplyAntidote, "Antidote"));

            



            grid = new Grid(Globals.GridWidth, Globals.GridHeight);
            SelectNextPiece();
            nextDropTickTimer = new TimeWatcher(dropTickIntervalSeconds);

            for (int i = 0; i < Globals.Options.AntidotesAtStart; i++)
            {
                AddWeapon(new Special.Antidote(this, world));
            }

            switch (ThinkType)
            {
                case ThinkType.ComputerEasy: computerMoveIntervalSeconds = .8; break;
                case ThinkType.ComputerNormal: computerMoveIntervalSeconds = .5; break;
                case ThinkType.ComputerHard: computerMoveIntervalSeconds = .1; break;
                case ThinkType.Dmitri: computerMoveIntervalSeconds = 0; break;
            }
            
        }


        ///------------------------------------------------------------------------------
        /// <summary>
        /// Choose the next peice
        /// </summary>
        ///------------------------------------------------------------------------------
        private void SelectNextPiece() { SelectNextPiece(false); }
        private void SelectNextPiece(bool forceSelect)
        {
            CurrentPiece = null;
            if (!forceSelect && (BlocksClearing || !LastPieceSet.Expired)) return;
            while (NextPieces.Count < 4)
            {
                Piece nextPiece = new Piece(EvilPieces);//Globals.rand.Next(Piece.NumberOfTypes));
                nextPiece.X = grid.Width / 2;
                nextPiece.Y = 0;
                NextPieces.Add(nextPiece);

            }

            CurrentPiece = NextPieces[0];
            NextPieces.RemoveAt(0);
            CalculateShadowPosition();


            if (PieceCollides(CurrentPiece))
            {
                this.State = PlayerState.Dying;
                this.DeathTimer = new TimeWatcher();
                this.StateTimer = new TimeWatcher(1);
                world.AudioTool.PlaySound(SoundEffectType.CrowdAww);
                CurrentPiece.Y = -10;
                return;
            }

        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Update the player
        /// </summary>
        ///------------------------------------------------------------------------------
        public void Update(GameTime gameTime)
        {


            switch (this.State)
            {
                case PlayerState.Playing:
                    if (CurrentPiece == null) SelectNextPiece();
                    Player victim = world.Players[VictimId];
                    if (victim.State != PlayerState.Playing)
                    {
                        ChangeVictim();
                    }

                    LoopThroughSpecials(Powers);
                    LoopThroughSpecials(Afflictions);
                    LoopThroughSpecials(Attacks);

                    double intervalChange = dropTickIntervalSeconds * 0.0001 * Globals.Options.SpeedupRate;
                    if (intervalChange < 0.00005 * Globals.Options.SpeedupRate)
                        intervalChange = 0.00005 * Globals.Options.SpeedupRate;
                    if (IsComputer && MoveFast && intervalChange < 0.001) intervalChange = 0.001;

                    dropTickIntervalSeconds -= intervalChange;

                    if (IsComputer) Think();

                    if (nextDropTickTimer.Expired || (CurrentPiece != null && CurrentPiece.Dropping))
                    {
                        if (CurrentPiece == null) SelectNextPiece(true);
                        HandleFallingPieces();
                    }
                    HandleClearing();

                    if(NextSpecialTimer.Expired) 
                    {
                        AddSpecialBlockToGrid();
                        NextSpecialTimer.Reset(Globals.Options.SecondsBetweenSpecials);
                    }

                    break;
                case PlayerState.Dying:
                    if (StateTimer.Expired) State = PlayerState.Dead; 
                    break;
                case PlayerState.Prepping:
                    State = PlayerState.ChooseBrains; 
                    break;
                case PlayerState.Waiting:
                    if (IsComputer) State = PlayerState.Ready;
                    break;
                case PlayerState.Ready:
                    if (lastState != PlayerState.Ready)
                    {
                        world.AudioTool.PlaySound(SoundEffectType.Strum,.3f,0,0);
                    }
                    break;
            }

            lastState = State;

        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Loop through a list of specials and remove the ones that are done
        /// </summary>
        ///------------------------------------------------------------------------------
        private void LoopThroughSpecials(List<Special> specialList)
        {
            for (int i = 0; i < specialList.Count; )
            {
                if (specialList[i].Finished)
                {
                    specialList[i].RemoveFromPlayer();
                    specialList.RemoveAt(i);
                }
                else
                {
                    specialList[i].Update();
                    i++;
                }
            }
        }

        struct MyPoint
        {
            public int X, Y;
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Add a special to the grid
        /// </summary>
        ///------------------------------------------------------------------------------
        private void AddSpecialBlockToGrid()
        {

            List<MyPoint> blockLocations = new List<MyPoint>();

            for (int i = 0; i < grid.Width; i++)
            {
                for (int j = 0; j < grid.Height; j++)
                {
                    if(grid[i,j] == null) continue;

                    blockLocations.Add(new MyPoint() { X = i, Y = j }); 
                }
            }

            if (blockLocations.Count == 0) return;
            int pick = Globals.rand.Next(blockLocations.Count);

            if (Globals.RandomDouble(0, 1) < Globals.Options.AntidoteFrequency)
            {
                grid[blockLocations[pick].X, blockLocations[pick].Y].SpecialType = SpecialType.Antidote;
            }
            else
            {
                grid[blockLocations[pick].X, blockLocations[pick].Y].SpecialType = (SpecialType)Globals.rand.Next((int)SpecialType.NumberOfSpecials);
#if DEBUG
                //grid[blockLocations[pick].X, blockLocations[pick].Y].SpecialType = SpecialType.Transparency;
#endif
            }

#if DEBUG
            //grid[blockLocations[pick].X, blockLocations[pick].Y].SpecialType = SpecialType.None;
#endif
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Hanlde computer logic
        /// </summary>
        ///------------------------------------------------------------------------------
        private void Think()
        {
            int bestRotation = 0;
            int bestShift = 0;
            int bestScore = -1000;

            // don't "think" while there are blocks clearing
            if (CurrentPiece == null || BlocksClearing) return;

            bool antidoteFired = false;
            if (ThinkType == ThinkType.ComputerNormal || ThinkType == ThinkType.ComputerHard || ThinkType == ThinkType.Dmitri)
            {
                if (Afflictions.Count > 0 && Weapons.Count > 0)
                {
                    FireWeapon();
                    antidoteFired = true;
                }
            }

            if (ThinkType == ThinkType.ComputerHard || ThinkType == ThinkType.Dmitri)
            {
                if (Attacks.Count > 0 && Weapons.Count > 0 && !antidoteFired)
                {
                    FireWeapon();
                    antidoteFired = true;
                }
            }

            if (computerHasAMove)
            {
                if (Attacks.Count > 0 && (ThinkType == ThinkType.Dmitri || ThinkType == ThinkType.ComputerHard))
                {
                    needsToRethink = true;
                }

                if (Attacks.Count > 0 && needsToRethink)
                {
                    computerHasAMove = false;
                    return;
                }

                if (!MoveFast && !LastComputerMoveTimer.Expired) return;
                LastComputerMoveTimer = new TimeWatcher(computerMoveIntervalSeconds);

                if (computerRotation > 0)
                {
                    RotateRight();
                    computerRotation--;
                }
                else if (computerShift != 0)
                {
                    if (computerShift < 0)
                    {
                        MoveLeft();
                        computerShift++;
                    }
                    else
                    {
                        MoveRight();
                        computerShift--;
                    }
                }
                else if (!CurrentPiece.Dropping)
                {
                    Drop(true);
                }
                return;
            }


            Debug.WriteLine("--- Thinking ... ---");
            Grid testGrid = new Grid(grid.Width, grid.Height);
            grid.DummyCopyTo(testGrid);
            int originalVoids = testGrid.GetVoidCount();

            // Find an ideal move
            for (int rotation = 0; rotation < 4; rotation++)
            {
                for (int shift = -testGrid.Width; shift < testGrid.Width; shift++)
                {
                    Piece testPiece = CurrentPiece.Clone();
                    grid.DummyCopyTo(testGrid);
                    bool invalidMove = false;

                    // rotate the piece
                    for (int i = 0; i < rotation; i++)
                    {
                        testPiece.RotateRight();
                        if (PieceCollides(testPiece))
                        {
                            invalidMove = true;
                            break;
                        }
                    }

                    if (invalidMove) continue;

                    // move the piece
                    for( int i = 0; i != shift; i += shift < 0 ? -1: 1)
                    {
                        if (shift < 0) testPiece.X--;
                        else testPiece.X++;
                        if (PieceCollides(testPiece))
                        {
                            invalidMove = true;
                            break;
                        }
                    }

                    if (invalidMove) continue;

                    // Drop the piece
                    for (int i = 0; i < testGrid.Height; i++)
                    {
                        testPiece.Y++;
                        if (PieceCollides(testPiece))
                        {
                            testPiece.Y--;
                            break;
                        }
                    }

                    // Count the touchingFaces
                    int touchingFaces = 0;
                    testPiece.ProcessBlocks(
                        (block, realx, realy) =>
                        {
                            if (realy >= 0)
                            {
                                if (realx <= 0 || testGrid[realx - 1, realy] != null) touchingFaces++;
                                if (realx >= testGrid.Width - 1 || testGrid[realx + 1, realy] != null) touchingFaces++;
                                if (realy <= 0 || testGrid[realx, realy - 1] != null) touchingFaces++;
                                if (realy >= testGrid.Height - 1 || testGrid[realx, realy + 1] != null) touchingFaces++;
                            }
                        });

                    // put the piece on
                    testPiece.ProcessBlocks(
                        (block, realx, realy) =>
                        {
                            if (realy >= 0 && realx >=0 && realy < testGrid.Height && realx < testGrid.Width)
                            {
                                testGrid[realx, realy] = block;
                            }
                        });

                    // removeFilledRows
                    int clearedRows = 0;
                    for (int j = 0; j < testGrid.Height; j++)
                    {
                        bool hasEmpty = false;
                        for (int i = 0; i < testGrid.Width; i++)
                        {
                            if (testGrid[i, j] == null)
                            {
                                hasEmpty = true;
                                break;
                            }

                        }

                        if (!hasEmpty)
                        {
                            // Collapse this row
                            clearedRows++;
                            for (int i = 0; i < testGrid.Width; i++)
                            {
                                for (int y = j; y> 0; y--)
                                {
                                    testGrid[i,y] = testGrid[i,y-1];
                                }
                                testGrid[i,0] = null;

                            }
                        }
                    }

                    int voids = testGrid.GetVoidCount() - originalVoids;


                    int thinkScore = touchingFaces + testPiece.Y  - voids * 2 + clearedRows * 2;

                    if (thinkScore > bestScore)
                    {
                        Debug.WriteLine(string.Format("Picked: f{0}, y{1}, v{2} ({3})", touchingFaces, testPiece.Y, voids, thinkScore));
                        bestScore = thinkScore;
                        bestShift = shift;
                        bestRotation = rotation;
                    }
                }
            }

            computerHasAMove = true;
            needsToRethink = false;
            computerRotation = bestRotation;
            computerShift = bestShift;

            //  computer "mistakes"
            if (Afflictions.Count > 0)
            {
                float errorProbability = 0.1f;
                errorProbability *= Afflictions.Count;
                switch (ThinkType)
                {
                    case ThinkType.ComputerEasy: errorProbability *= 4; break;
                    case ThinkType.ComputerNormal: errorProbability *= 2; break;
                    case ThinkType.ComputerHard: errorProbability *= 1; break;
                    case ThinkType.Dmitri:
                        errorProbability = 0.01f;
                        break;
                }

                if (Globals.RandomDouble(0, 1) < errorProbability)
                {
                    if (Globals.rand.Next(2) == 0) computerRotation++;
                    else 
                    {
                        computerShift += Globals.rand.Next(2) == 0 ? -1 : 1;
                    }
                }
            }
        }

        /// <summary>
        /// Check if any blocks are clearing
        /// </summary>
        private bool BlocksClearing
        {
            get
            {
                bool blocksClearing = false;
                for (int y = 0; y < grid.Height; y++)
                {
                    if (grid[0, y] != null && grid[0, y].Clearing)
                    {
                        blocksClearing = true;
                        break;
                    }
                }
                return blocksClearing;
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Handle ClearingRows
        /// </summary>
        ///------------------------------------------------------------------------------
        private void HandleClearing()
        {
            int rowsCleared = 0;
            int columnsDropped = 0;
            specialBlockCount = 0;

            for (int j = 0; j < grid.Height; j++)
            {
                int blocksInThisRow = 0;
                for (int i = 0; i < grid.Width; i++)
                {
                    if (grid[i, j] == null) continue;
                    if (grid[i, j].SpecialType != SpecialType.None)
                    {
                        specialBlockCount++;
                    }



                    if (grid[i, j].Clearing)
                    {
                        // If the block is done clearing, then we can drop down the pieces above it
                        if (grid[i, j].FractionLeft <= 0)
                        {
                            grid[i, j].ActivateSpecial(this, world);
                            for (int y = j; y > 0; y--)
                            {
                                grid[i, y] = grid[i, y - 1];
                            }
                            grid[i, 0] = null;
                            columnsDropped++;
                        }
                    }
                    else if (grid[i, j].AnimationType == AnimationType.ExplodeMe)
                    {
                        if(grid[i, j].AnimationStarted) grid[i, j] = null;
                    }
                    else
                    {
                        blocksInThisRow++;
                    }
                }

                if (blocksInThisRow == grid.Width)
                {
                    for (int i = 0; i < grid.Width; i++)
                    {
                        grid[i, j].Clearing = true;
                    }
                    rowsCleared++;
                }

            }

            
            if (columnsDropped > 0) CalculateShadowPosition();

            this.Rows += rowsCleared;
            this.Score += rowsCleared * rowsCleared * 1000;
            switch (rowsCleared)
            {
                case 0: break;
                case 1: world.AudioTool.PlaySound(SoundEffectType.Clear1Line, 1f, 0, 0); break;
                case 2: world.AudioTool.PlaySound(SoundEffectType.Clear2Lines, 1f, 0, 0); break;
                case 3: world.AudioTool.PlaySound(SoundEffectType.Clear3Lines, 1f, 0, 0); break;
                default:
                    Special.ActivateSpecial(SpecialType.Bridge, this, world);
                    world.AudioTool.PlaySound(SoundEffectType.Clear4Lines, 1f, 0, 0); 
                    break;
            }
        }



        ///------------------------------------------------------------------------------
        /// <summary>
        /// Handle falling pieces
        /// </summary>
        ///------------------------------------------------------------------------------
        private void HandleFallingPieces()
        {
            if (CurrentPiece == null) return;
            nextDropTickTimer = new TimeWatcher(dropTickIntervalSeconds);
            CurrentPiece.Y++;

            if(CurrentPiece.Dropping) this.Score += 10;

            if (PieceCollides(CurrentPiece))
            {
                CurrentPiece.Y--;
                if (CurrentPiece.Dropping)
                {
                    CurrentPiece.Dropping = false;
                    if (CurrentPiece.MakeItStick) SetPiece();
                }
                else
                {
                    SetPiece();
                }
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Returns true if the current piece goes off the edge of the grid or
        /// if it hits other blocks in the grid.
        /// </summary>
        ///------------------------------------------------------------------------------
        private bool PieceCollides(Piece piece)
        {
            bool collides = false;
            piece.ProcessBlocks(
                (block, realx, realy) =>
                {
                    if(realx < 0 || realx >= grid.Width || realy >= grid.Height) collides = true;
                    else if (realy >= 0 && grid[realx, realy] != null) collides = true;
                });
            return collides;
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Handle input from the controller
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void HandleAction(InputAction action)
        {
            if (!action.Click) return;
            switch (State)
            {
                case PlayerState.ChooseBrains:
                    int thinkNumber = (int)ThinkType;
                    switch (action.ActionType)
                    {
                        case InputActionType.MoveRight:
                            thinkNumber++;
                            if (thinkNumber >= (int)ThinkType.NumberOfTypes) thinkNumber = 0;
                            world.AudioTool.PlaySound(SoundEffectType.Dot01, 1, .5f, 0);
                            break;
                        case InputActionType.MoveLeft:
                            thinkNumber--;
                            if (thinkNumber < 0 ) thinkNumber = (int)ThinkType.NumberOfTypes  - 1;
                            world.AudioTool.PlaySound(SoundEffectType.Dot01, 1, -.5f, 0);
                            break;
                        case InputActionType.MoveDown:
                        case InputActionType.DropAndSlide:
                        case InputActionType.DropAndStick:
                        case InputActionType.ApplyAntidote: 
                            State = PlayerState.ChooseOrder;
                            world.AudioTool.PlaySound(SoundEffectType.Dot02);
                            break;
                    }
                    ThinkType = (ThinkType)thinkNumber;
                    break;
                case PlayerState.ChooseOrder:
                    switch (action.ActionType)
                    {
                        case InputActionType.MoveRight:
                            world.AudioTool.PlaySound(SoundEffectType.Dot01, 1, .5f, 0);
                            world.MovePlayerDown(this);
                            break;
                        case InputActionType.MoveLeft:
                            world.AudioTool.PlaySound(SoundEffectType.Dot01, 1, -.5f, 0);
                            world.MovePlayerUp(this);
                            break;
                        case InputActionType.MoveDown:
                        case InputActionType.DropAndSlide:
                        case InputActionType.DropAndStick:
                        case InputActionType.ApplyAntidote:
                            world.AudioTool.PlaySound(SoundEffectType.Dot02);
                            if (ThinkType == ThinkType.Human) State = PlayerState.ChooseReadiness;
                            else
                            {
                                world.AssignAsComputer(action.PlayerID);
                                State = PlayerState.Ready;
                            }
                            break;
                        case InputActionType.ChangeVictim:
                        case InputActionType.RotateRight:
                            State = PlayerState.ChooseBrains;
                            world.AudioTool.PlaySound(SoundEffectType.Dot02, 1, -.5f, 0);
                            break;
                    }
                    break;
                case PlayerState.ChooseReadiness:
                case PlayerState.Ready:
                    if (TotalScore == 0)
                    {
                        switch (action.ActionType)
                        {
                            case InputActionType.MoveRight:
                            case InputActionType.MoveLeft:
                                world.AudioTool.PlaySound(SoundEffectType.Dot01, 1, .5f, 0);
                                if (State == PlayerState.Ready) State = PlayerState.ChooseReadiness;
                                else
                                {
                                    State = PlayerState.Ready;
                                    if (ThinkType != ThinkType.Human)
                                    {
                                        world.AssignAsComputer(action.PlayerID);
                                    }
                                }
                                break;
                            case InputActionType.MoveDown:
                            case InputActionType.DropAndSlide:
                            case InputActionType.DropAndStick:
                            case InputActionType.ApplyAntidote:
                                world.AudioTool.PlaySound(SoundEffectType.Dot02);
                                if (ThinkType == ThinkType.Human)
                                {
                                    State = PlayerState.ChoosePractice;
                                    foreach (PracticeAction subAction in PracticeActions)
                                    {
                                        subAction.LastPress = null;
                                        subAction.Repeats = 0;
                                    }

                                }
                                break;
                            case InputActionType.ChangeVictim:
                            case InputActionType.RotateRight:
                                State = PlayerState.ChooseOrder;
                                world.AudioTool.PlaySound(SoundEffectType.Dot02, 1, -.5f, 0);
                                break;
                        }
                    }
                    break;
                case PlayerState.ChooseName:
                    State = PlayerState.Ready;
                    break;
                case PlayerState.ChoosePractice:
                    world.AudioTool.PlaySound(SoundEffectType.Dot, 1, .5f, 0);
                    switch (action.ActionType)
                    {
                        case InputActionType.MoveRight:
                        case InputActionType.MoveLeft:
                        case InputActionType.MoveDown:
                        case InputActionType.RotateRight:
                        case InputActionType.RotateLeft:
                        case InputActionType.DropAndSlide:
                        case InputActionType.DropAndStick:
                        case InputActionType.ApplyAntidote:
                        case InputActionType.ChangeVictim:

                            bool foundUnpracticedAction = false;
                            foreach (PracticeAction subAction in PracticeActions)
                            {
                                if (subAction.LastPress == null) foundUnpracticedAction = true;
                            }
                            if (!foundUnpracticedAction)
                            {
                                State = PlayerState.ChooseReadiness;
                                world.AudioTool.PlaySound(SoundEffectType.Dot, 1, -.5f, 0);
                            }

                            PracticeAction practiceAction = FindPracticeAction(action.ActionType);
                            if (practiceAction.ActionType == lastPracticePress)
                            {

                                practiceAction.Repeats++;
                                if (practiceAction.Repeats > 2)
                                {
                                    State = PlayerState.ChooseReadiness;
                                    world.AudioTool.PlaySound(SoundEffectType.Dot, 1, -.5f, 0);
                                }
                            }
                            else
                            {
                                practiceAction.Repeats = 0;
                            }
                            lastPracticePress = action.ActionType;
                            practiceAction.LastPress = new TimeWatcher(0.2);
                            break;
                    }
                    break;
                case PlayerState.Waiting:
                    switch (action.ActionType)
                    {
                        case InputActionType.DropAndStick: State = PlayerState.Ready; break;
                    }
                    break;
                case PlayerState.Playing:
                    switch (action.ActionType)
                    {
                        case InputActionType.MoveLeft:          if (CrazyIvan) MoveRight(); else MoveLeft(); break;
                        case InputActionType.MoveRight:         if (CrazyIvan) MoveLeft(); else MoveRight(); break;
                        case InputActionType.RotateRight:       if (CrazyIvan) RotateLeft(); else RotateRight(); break;
                        case InputActionType.MoveDown:          MoveDownOneAndStick(); break;
                        case InputActionType.RotateLeft:        if (CrazyIvan) RotateRight(); else RotateLeft(); break;
                        case InputActionType.DropAndStick:      Drop(true); break;
                        case InputActionType.DropAndSlide:      Drop(false); break;
                        case InputActionType.ApplyAntidote:    FireWeapon(); break; 
                        case InputActionType.ChangeVictim:      ChangeVictim();  break; // Change Victim
                    }
                    break;
                case PlayerState.Dead:
                    switch (action.ActionType)
                    {
                        default: break;  
                    }
                    break;
            }
        }


        ///------------------------------------------------------------------------------
        /// <summary>
        /// Find a corresponding practice action
        /// </summary>
        ///------------------------------------------------------------------------------
        private PracticeAction FindPracticeAction(InputActionType inputActionType)
        {
            for (int i = 0; i < PracticeActions.Count; i++)
            {
                if (PracticeActions[i].ActionType == inputActionType)
                {
                    return PracticeActions[i];
                }
            }
            return null;
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// If player has any weapons, fire the first one
        /// </summary>
        ///------------------------------------------------------------------------------
        private void FireWeapon()
        {
            if (Weapons.Count == 0)
            {

            }
            else
            {
                EmpowerWith(Weapons[0]);
                Weapons.RemoveAt(0);
            }

        }


        ///------------------------------------------------------------------------------
        /// <summary>
        /// Change the victim to the next living victim
        /// </summary>
        ///------------------------------------------------------------------------------
        private void ChangeVictim()
        {
            int tooManyTimes = 100;
            while (tooManyTimes > 0)
            {
                VictimId++;
                if (VictimId >= world.Players.Count) VictimId = 0;
                if (world.Players[VictimId].State == PlayerState.Playing) break;
                tooManyTimes--;
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Piece Movement
        /// </summary>
        ///------------------------------------------------------------------------------
        private void Drop(bool makeItStick)
        {
            if (CurrentPiece == null) return;
            if (CurrentPiece.Dropping) return;
            CurrentPiece.Dropping = true;
            CurrentPiece.MakeItStick = makeItStick;
            MoveDownOneAndStick();
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Piece Movement
        /// </summary>
        ///------------------------------------------------------------------------------
        private void RotateLeft()
        {
            if (CurrentPiece == null) return;
            CurrentPiece.RotateLeft();
            if (PieceCollides(CurrentPiece)) CurrentPiece.RotateRight();
            CalculateShadowPosition();
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Piece Movement
        /// </summary>
        ///------------------------------------------------------------------------------
        private void RotateRight()
        {
            if (CurrentPiece == null) return;
            CurrentPiece.RotateRight();
            if (PieceCollides(CurrentPiece)) CurrentPiece.RotateLeft();
            CalculateShadowPosition();
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Piece Movement
        /// </summary>
        ///------------------------------------------------------------------------------
        private void MoveRight()
        {
            if (CurrentPiece == null) return;
            CurrentPiece.X++;
            if (PieceCollides(CurrentPiece)) CurrentPiece.X--;
            CalculateShadowPosition();
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Piece Movement
        /// </summary>
        ///------------------------------------------------------------------------------
        private void MoveLeft()
        {
            if (CurrentPiece == null) return;
            CurrentPiece.X--;
            if (PieceCollides(CurrentPiece)) CurrentPiece.X++;
            CalculateShadowPosition();
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Figure out where the shadow piece goes
        /// </summary>
        ///------------------------------------------------------------------------------
        void CalculateShadowPosition()
        {
            if (CurrentPiece == null) return;
            int startY = CurrentPiece.Y;
            while (true)
            {
                CurrentPiece.Y++;
                if (PieceCollides(CurrentPiece))
                {
                    ShadowY = CurrentPiece.Y - 1;
                    break;
                }
            }
            CurrentPiece.Y = startY;
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Piece Movement
        /// </summary>
        ///------------------------------------------------------------------------------
        private void MoveDownOneAndStick()
        {
            if (CurrentPiece == null) return;
            CurrentPiece.Y++;
            if (PieceCollides(CurrentPiece))
            {
                CurrentPiece.Y--;
                SetPiece();
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Make the current piece stick where it is.
        /// </summary>
        ///------------------------------------------------------------------------------
        private void SetPiece()
        {
            if (CurrentPiece == null) return;
            LastPieceSet = new TimeWatcher(.1);
            Grid.SetPiece(CurrentPiece);
            CurrentPiece = null;
            computerHasAMove = false;
            world.AudioTool.PlaySound(SoundEffectType.Dot03, 1, Globals.RandomPitch(.1), 0);
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Utility for sorting players
        /// </summary>
        ///------------------------------------------------------------------------------
        public static List<Player> SortPlayers(IEnumerable<Player> players, ComparePlayers compare)
        {
            List<Player> sortedList = new List<Player>();

            foreach (Player player in players)
            {
                bool added = false;
                for (int i = 0; i < sortedList.Count; i++)
                {
                    if (compare(player, sortedList[i]))
                    {
                        sortedList.Insert(i, player);
                        added = true;
                        break;
                    }
                }
                if (!added) sortedList.Add(player);
            }
            return sortedList;
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Attack this player with something
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void AttackWith(Special attack)
        {
            foreach (Special power in Powers)
            {
                if (power is Special.Antidote)
                {
                    RepelledAttack = true;
                    world.AudioTool.PlaySound(SoundEffectType.Attack02);
                }
            }
            Attacks.Add(attack);
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Afflict this player with something
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void AfflictWith(Special affliction)
        {
            foreach (Special power in Powers)
            {
                if (power is Special.Antidote)
                {
                    RepelledAttack = true;
                    world.AudioTool.PlaySound(SoundEffectType.Attack02);
                }
            }

            foreach (Special myAffliction in Afflictions)
            {
                if (myAffliction.SpecialType == affliction.SpecialType) return;
            }

            Afflictions.Add(affliction);
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Attack this player with something
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void EmpowerWith(Special power)
        {
            foreach (Special myPower in Powers)
            {
                if (myPower.SpecialType == power.SpecialType) myPower.Finished = true;
            }
            Powers.Add(power);
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Attack this player with something
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void Cure(Special special)
        {
            foreach (Special affiction in Afflictions)
            {
                if (special == null || affiction.GetType() == special.GetType())
                {
                    affiction.Finished = true;
                    affiction.RemoveFromPlayer();
                }
            }

            foreach (Special attack in Attacks)
            {
                if (special == null || attack.GetType() == special.GetType())
                {
                    attack.Finished = true;
                    attack.RemoveFromPlayer();
                }
            }

        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Clean up the next piece buffer
        /// </summary>
        ///------------------------------------------------------------------------------
        public void ClearNextPieceBuffer()
        {
            NextPieces.Clear();
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Add to the list of weapons
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void AddWeapon(Special weapon)
        {
            Weapons.Add(weapon);
        }
    }

    public delegate bool ComparePlayers(Player player1, Player player2);

    public class PracticeAction
    {
        public string TranslatedName;
        public InputActionType ActionType;
        public TimeWatcher LastPress;
        public int Repeats;

        public PracticeAction(InputActionType actionType, string translatedName)
        {
            this.ActionType = actionType;
            this.TranslatedName = translatedName;
            LastPress = null;
            Repeats = 0;
        }

    }
}

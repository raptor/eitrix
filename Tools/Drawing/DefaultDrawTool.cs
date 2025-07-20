using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace Eitrix
{

    public enum Justification
    {
        Left,
        Centered,
        Right
    }

    /// -------------------------------------------------------------
    /// <summary>
    /// For drawing stuff that looks hand-drawn
    /// </summary>
    /// -------------------------------------------------------------
    public partial class DefaultDrawTool : IDrawTool
    {
        GraphicsDeviceManager graphics { get; set; }
        PresentationParameters presentationParameters { get; set; }
        SpriteBatch spriteBatch { get; set; }
        public int Width { get { return SafeArea.Width; } }
        public int Height { get { return SafeArea.Height; } }
        float screenSizeFactor = 1;
        bool batchStarted = false;
        delegate void DrawSnippet(Vector2 actualPosition);
        DateTime startTime = DateTime.Now;
        List<Animation> Animations = new List<Animation>();


        /// <summary>
        /// Cool team colors
        /// </summary>
        Color[] TeamColors = new Color[]{
            new Color(0x00,0xFF,0xFF),
            new Color(0xff,0x00,0x00),
            new Color(0x00,0xFF,0x00),
            new Color(0xFF,0x00,0xFF),
            new Color(0xFF,0x8E,0x00),
            new Color(0xFF,0xFF,0x00),
            new Color(0x00,0x00,0xFF),
            new Color(0x30,0x30,0x30),
            new Color(0x40,0x00,0x40),
            new Color(0x00,0x70,0x70),
            new Color(0x60,0x00,0x00),
            new Color(0x00,0x30,0x00),
            new Color(0x60,0x60,0x00),
            new Color(0x33,0x00,0xCC),
            new Color(0xFF,0x33,0x66),
            new Color(0xFF,0xFF,0x90),
            new Color(0x00,0x00,0x00),
            new Color(0xFF,0xFF,0xFF),
            Color.DarkGray,
            Color.Gold,
            Color.GreenYellow,
            Color.DarkKhaki,
            Color.DarkTurquoise,
            Color.DeepPink,
            Color.DeepSkyBlue,
            Color.MistyRose,
        };

        Color targetTint = Color.White;
        Color currentTint = Color.White;

        public string DebugText
        {
            get
            {
                return "";
            }
        }



        // The safe area for drawing 2D HUD items
        Rectangle safeArea;
        Rectangle adjustedSafeArea;
        double lastExpansionValue = double.MinValue;
        int borderWidth;
        int borderHeight;
        int totalWidth;
        int totalHeight;
        public Rectangle SafeArea 
        { 
            get 
            { 
                if(Globals.Options.ExpandGraphics != lastExpansionValue)
                {
                    lastExpansionValue = Globals.Options.ExpandGraphics;
                    adjustedSafeArea = new Rectangle((int)(borderWidth * (1-Globals.Options.ExpandGraphics) / 2),
                    (int)(borderHeight * (1-Globals.Options.ExpandGraphics) / 2),
                    (int)(safeArea.Width + borderWidth * Globals.Options.ExpandGraphics),
                    (int)(safeArea.Height + borderHeight * Globals.Options.ExpandGraphics));
                }
                return adjustedSafeArea;
            } 
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// -------------------------------------------------------------
        public DefaultDrawTool(Game game)
        {
            this.graphics = new GraphicsDeviceManager(game);
#if XBOX            
#else
            graphics.PreparingDeviceSettings +=
                new EventHandler<PreparingDeviceSettingsEventArgs>
                    (SetUpGraphics);
#endif
        }

        public void Reset()
        {
            graphics.GraphicsDevice.Reset(presentationParameters);
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Initializes the draw tool's view settings
        /// </summary>
        /// -------------------------------------------------------------
        public void Initialize()
        {
            // Create the Sprite Batch
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            safeArea = graphics.GraphicsDevice.Viewport.TitleSafeArea;
            borderWidth = safeArea.Left * 2;
            borderHeight = safeArea.Top * 2;
            totalWidth = borderWidth + safeArea.Width;
            totalHeight = borderHeight + safeArea.Height;
            if (borderWidth != 0) Globals.LowResolution = true;
        }

#if XBOX
#else
        /// -------------------------------------------------------------
        /// <summary>
        /// Sets the screen mode for this game
        /// </summary>
        /// -------------------------------------------------------------
        public void SetUpGraphics(object sender,
            PreparingDeviceSettingsEventArgs e)
        {
            bool forceSmall = false;
            int targetWidth = 1280;
            int targetHeight = 768;
            if (Globals.FullScreenMode)
            {
                targetWidth  = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                targetHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

                e.GraphicsDeviceInformation.PresentationParameters.IsFullScreen = true;
            }
            if(forceSmall)
            {
                targetWidth = 640;
                targetHeight = 480;
            }
            e.GraphicsDeviceInformation.PresentationParameters.BackBufferHeight = targetHeight;
            e.GraphicsDeviceInformation.PresentationParameters.BackBufferWidth  = targetWidth;

            e.GraphicsDeviceInformation.PresentationParameters.BackBufferFormat =
                GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Format;

            presentationParameters = e.GraphicsDeviceInformation.PresentationParameters;

            double aspectRatio = (double)targetWidth / targetHeight;  // unused?
        }
#endif // WINDOWS

        /// -------------------------------------------------------------
        /// <summary>
        /// Load content
        /// </summary>
        /// -------------------------------------------------------------
        public void LoadContent(ContentManager Content)
        {
            screenSizeFactor = Width / 1920f;
            float factor2 = Height / 1080f;
            if (factor2 > screenSizeFactor) screenSizeFactor = factor2;

            for (int i = 0; i < 3; i++)
            {
                Textures.Background.Add(SafeLoadTexture(Content, "Images/Background" + i.ToString("00")));
            }


            for (int i = 0; i < Globals.HelpPages; i++)
            {
                Textures.HelpPage.Add(SafeLoadTexture(Content, "Images/HelpPage" + i.ToString("00")));
            }

            for (int i = 0; i < 11; i++)
            {
                Textures.Grid.Add(SafeLoadTexture(Content, "Images/Grid" + i.ToString("00")));
            }

            Textures.BeveledBlock = SafeLoadTexture(Content, "Images/BeveledBlock");
            Textures.Brick = SafeLoadTexture(Content, "Images/Brick");
            Textures.BrickAndOverlay = SafeLoadTexture(Content, "Images/BrickAndOverlay");
            Textures.BrickOverlay1 = SafeLoadTexture(Content, "Images/BrickOverlay1");
            Textures.BrickOverlay2 = SafeLoadTexture(Content, "Images/BrickOverlay2");
            Textures.BrickOverlay3 = SafeLoadTexture(Content, "Images/BrickOverlay3");
            Textures.WhitePixel = SafeLoadTexture(Content, "Images/WhitePixel");
            Textures.PlayerOverlay = SafeLoadTexture(Content, "Images/PlayerOverlay");
            Textures.DownBevel = SafeLoadTexture(Content, "Images/DownBevel");
            Textures.AttackArrow = SafeLoadTexture(Content, "Images/AttackArrow");
            Textures.Logo = SafeLoadTexture(Content, "Images/Logo");
            Textures.LogoShadow = SafeLoadTexture(Content, "Images/LogoShadow");

            Fonts.DebugFont = Content.Load<SpriteFont>("Fonts/DebugFont");
            Fonts.ScoreFont = Content.Load<SpriteFont>("Fonts/ScoreFont");
            Fonts.InfoFont = Content.Load<SpriteFont>("Fonts/InfoFont");
            Fonts.MenuFont = Content.Load<SpriteFont>("Fonts/MenuFont");
            Fonts.PlayerFont = Content.Load<SpriteFont>("Fonts/PlayerFont");
            Fonts.PlayerFontBold = Content.Load<SpriteFont>("Fonts/PlayerFontBold");
        }

        Texture2D defaultTexture;
        /// -------------------------------------------------------------
        /// <summary>
        /// Try to load a texture and return a default texture if it fails
        /// </summary>
        /// -------------------------------------------------------------
        private Texture2D SafeLoadTexture(ContentManager Content, string contentName)
        {
            try
            {
                return Content.Load<Texture2D>(contentName);
            }
            catch(Exception)
            {
                if (defaultTexture == null) defaultTexture = Content.Load<Texture2D>("Images/DefaultImage");
                return defaultTexture;
            }
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Clear Screen
        /// </summary>
        /// -------------------------------------------------------------
        public void ClearScreen()
        {

            ClearScreen(0);
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Clear Screen
        /// </summary>
        /// -------------------------------------------------------------
        public void ClearScreen(Color color)
        {

            graphics.GraphicsDevice.Clear(color);  
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Clear Screen
        /// </summary>
        /// -------------------------------------------------------------
        public void ClearScreen(int backgroundId)
        {
            if (!batchStarted)
            {
                spriteBatch.Begin(blendState: BlendState.AlphaBlend);
                batchStarted = true;
            }

            Texture2D background = Textures.Background[backgroundId];
            float scalex = SafeArea.Width / (float)background.Width;
            float scaley = SafeArea.Height / (float)background.Height;
            spriteBatch.Draw(background,
               new Vector2(SafeArea.Width / 2 + SafeArea.Left, SafeArea.Height / 2 + SafeArea.Top),
               null,
               currentTint,
               0,
               new Vector2(background.Width / 2, background.Height / 2),
               new Vector2(scalex, scaley),
               SpriteEffects.None,
               0);
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Draw the the statistics at the end of the round
        /// </summary>
        /// -------------------------------------------------------------
        public void DrawRoundStats(int currentRound, int numRounds, List<Player> players)
        {
            // Draw Round info
            // Draw Round Stats
            // Draw Total Stats

            DrawPlayers(players);

            SafeDrawPlainRectangle(new Vector2(0, 0), Width, Height, new Color(0,0,0,.7f));


            float statScale = screenSizeFactor * 2f;
            SpriteFont labelFont = Fonts.PlayerFontBold;
            SpriteFont dataFont = Fonts.PlayerFontBold;
            Vector2 letterSize = labelFont.MeasureString("a") * statScale;
            float statsWidth = SafeArea.Width * 0.9f;
            float leftMargin = (SafeArea.Width - statsWidth);
            float topMargin = SafeArea.Height * 0.05f;
            float labelScale = statScale * 1.2f;

            SafePrint(Width/2, topMargin, Color.White, labelScale, Justification.Centered, labelFont, "Standings for round " + currentRound + "/" + numRounds +
                (currentRound == numRounds ? " (FINAL)" : ""));


            List<Player> scoreList = Player.SortPlayers(players, (p1, p2) => { 
                if(p1.TotalPoints > p2.TotalPoints) return true;
                if(p1.TotalPoints == p2.TotalPoints) return p1.TotalScore > p2.TotalScore;
                return false;
            });



            float nameMargin = letterSize.X * 5;
            float[] columns = new float[]
            {
                leftMargin + statsWidth * 0.05f,
                leftMargin + statsWidth * 0.15f,
                leftMargin + statsWidth * 0.30f,
                leftMargin + statsWidth * 0.35f,
                leftMargin + statsWidth * 0.48f,
                leftMargin + statsWidth * 0.64f, // scorechange
                leftMargin + statsWidth * 0.80f,
                leftMargin + statsWidth * 0.85f,
            };

            float y = topMargin + letterSize.Y * 1.5f;
            SafePrint(columns[0], y, Color.Cyan, statScale, Justification.Centered, labelFont, "Rank");
            SafePrint(columns[1], y, Color.Cyan, statScale, Justification.Centered, labelFont, "Name");
            SafePrint(columns[2], y, Color.Cyan, statScale, Justification.Centered, labelFont, "Points");
            SafePrint(columns[3], y, Color.Cyan, statScale, Justification.Centered, labelFont, "");
            SafePrint(columns[4], y, Color.Cyan, statScale, Justification.Centered, labelFont, "Score");
            SafePrint(columns[5], y, Color.Cyan, statScale, Justification.Centered, labelFont, "");
            SafePrint(columns[6], y, Color.Cyan, statScale, Justification.Centered, labelFont, "Rows");
            SafePrint(columns[7], y, Color.Cyan, statScale, Justification.Centered, labelFont, "");

            for (int i = 0; i < scoreList.Count; i++)
            {
                y = topMargin + letterSize.Y * 3f + i * letterSize.Y;
                SafePrint(columns[0], y, Color.Cyan, statScale, Justification.Centered, labelFont, (i+1).ToString() + ")");
                SafePrint(columns[1], y, Color.LimeGreen, statScale, Justification.Centered, dataFont, scoreList[i].Name);
                SafePrint(columns[2], y, Color.LimeGreen, statScale, Justification.Centered, dataFont, scoreList[i].TotalPoints.ToString());
                SafePrint(columns[3], y, Color.LimeGreen, statScale, Justification.Centered, dataFont, "+" + scoreList[i].Points.ToString());
                SafePrint(columns[4], y, Color.LimeGreen, statScale, Justification.Centered, dataFont, scoreList[i].TotalScore.ToString("#,#"));
                SafePrint(columns[5], y, Color.LimeGreen, statScale, Justification.Centered, dataFont, "+" + scoreList[i].Score.ToString("#,#"));
                SafePrint(columns[6], y, Color.LimeGreen, statScale, Justification.Centered, dataFont, scoreList[i].TotalRows.ToString());
                SafePrint(columns[7], y, Color.LimeGreen, statScale, Justification.Centered, dataFont, "+" + scoreList[i].Rows.ToString());
            }

            y += letterSize.Y * 2;
            if (currentRound == numRounds)
            {
                SafePrint(Width / 2, y, Color.White, labelScale, Justification.Centered, labelFont, "Press Back or Esc to return to the main menu.");
            }
            else
            {
                SafePrint(Width / 2, y, Color.White, labelScale, Justification.Centered, labelFont, "Press drop button when you are ready to play!");
            }

            y += letterSize.Y * 2f;
            float width = SafeArea.Width * .9f;
            float nameSpace = width / players.Count;

            for (int i = 0; i < scoreList.Count; i++)
            {
                float x = SafeArea.Width *.05f +  (i +.5f) * nameSpace;
                SafePrint(x, y,
                    players[i].State == PlayerState.Ready ? Color.Yellow : Color.DarkGray,
                    statScale, Justification.Centered, labelFont,
                    players[i].Name);
            }

        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Draw the players as they are getting ready to play!
        /// </summary>
        /// -------------------------------------------------------------
        public void DrawPlayersGathering(List<Player> playerList)
        {
            double workingWidth = Width * .95;
            int minPlayerWidth = (int)(workingWidth / Globals.MaxPlayers);
            int maxPlayerWidth = (int)(workingWidth / 4);
            int playerWidth = (int)(workingWidth / playerList.Count);
            if (playerWidth > maxPlayerWidth) playerWidth = maxPlayerWidth;
            if (playerWidth < minPlayerWidth) playerWidth = minPlayerWidth;
            int shortWidth = (int)(playerWidth * 0.95);

            int setWidth = playerList.Count * playerWidth;
            int leftX = Width / 2 - setWidth / 2;
            

            float pixelsPerCharacter = playerWidth / 20f;
            Vector2 letterSize = Fonts.InfoFont.MeasureString("A");
            float fontScale = pixelsPerCharacter / letterSize.X;
            letterSize *= fontScale;
            int rowSize = (int)(letterSize.Y * 1.2);

            int topY = (int)(Height * 0.1);

            SafePrint(Width / 2, Height * .03f, Color.Cyan, screenSizeFactor, Justification.Centered, Fonts.MenuFont,
                "Press a drop key to add yourself as a player.");

            Vector2 safeOffset = new Vector2(SafeArea.Left, SafeArea.Top);


            for (int i = 0; i < playerList.Count; i++)
            {
                SpriteFont font = Fonts.InfoFont;


                Player player = playerList[i];
                int x = leftX + i * playerWidth;
                int y = (int)(topY + Height * 0.1f);
                float colorValue =  (float)((Math.Sin((DateTime.Now - startTime).TotalSeconds * 3) + 1)/2);
                Color selectionHighlightColor = new Color(colorValue, 0, 0, .5f);
                Color highlightColor = new Color(.5f, .5f, .5f, .5f);
                if (player.State == PlayerState.Ready) highlightColor = new Color(0, .5f, 0, .7f);

                int depth = 6;
                if (player.State == PlayerState.ChoosePractice) depth = 15;
                DrawPlainRectangle(safeOffset + new Vector2(x, y), shortWidth, rowSize * depth, highlightColor);

                switch (player.State)
                {
                    case PlayerState.ChooseBrains:  
                        DrawPlainRectangle(safeOffset + new Vector2(x, y + rowSize * 2), shortWidth, rowSize, selectionHighlightColor); break;
                    case PlayerState.ChooseOrder: 
                        DrawPlainRectangle(safeOffset + new Vector2(x, y + rowSize * 3), shortWidth, rowSize, selectionHighlightColor); break;
                    case PlayerState.ChooseReadiness:
                    case PlayerState.Ready:
                        DrawPlainRectangle(safeOffset + new Vector2(x, y + rowSize * 4), shortWidth, rowSize, selectionHighlightColor); break;
                    case PlayerState.ChoosePractice:
                        DrawPlainRectangle(safeOffset + new Vector2(x, y + rowSize * 5), shortWidth, rowSize, selectionHighlightColor); break;
                }

                SafePrint(x + shortWidth / 2, y + rowSize * 0, Color.White, fontScale, Justification.Centered, font, (i + 1).ToString());
                SafePrint(x + shortWidth / 2, y + rowSize * 1, Color.White, fontScale, Justification.Centered, font, player.Name);
                SafePrint(x + shortWidth / 2, y + rowSize * 2, Color.White, fontScale, Justification.Centered, font, player.ThinkType.ToString());

                spriteBatch.Draw(Textures.BrickAndOverlay,
                    new Vector2(x + shortWidth / 2, y + rowSize * 3.5f) + safeOffset,
                    GetSpriteArea(Textures.BrickAndOverlay, 10, 4),
                    Color.White,
                    MathHelper.PiOver2,
                    new Vector2(50f, 50f),
                    screenSizeFactor * 0.4f,
                    SpriteEffects.None,
                    0);

                SafePrint(x + shortWidth / 2, y + rowSize * 4, Color.White, fontScale, Justification.Centered, font, player.State == PlayerState.Ready ? "Ready": "Not Ready");
                SafePrint(x + shortWidth / 2, y + rowSize * 5, Color.White, fontScale, Justification.Centered, font, "Practice");

                if (player.State == PlayerState.ChoosePractice)
                {
                    for (int j = 0; j < player.PracticeActions.Count; j++)
                    {
                        int py = y + rowSize * 6 + j * rowSize;
                        PracticeAction action = player.PracticeActions[j];
                        Color practiceHighlight = new Color(0, 0, 0, .6f);

                        if (action.LastPress == null)
                        {
                            DrawPlainRectangle(safeOffset + new Vector2(x, py), shortWidth, rowSize, practiceHighlight);
                        }
                        else if (action.LastPress != null && !action.LastPress.Expired)
                        {
                            DrawPlainRectangle(safeOffset + new Vector2(x, py), shortWidth, rowSize, new Color(1, 1, 1, (float)action.LastPress.FractionLeft));
                        }

                        SafePrint(x + shortWidth / 2, py, Color.White, fontScale, Justification.Centered, font, action.TranslatedName);
                    }
                }

                //SafePrint(columnCenterX[2] + x, printy, Color.White, fontScale, Justification.Centered, font, "U/D");
                //SafePrint(columnLeftX[3] + x, printy, Color.White, fontScale, Justification.Left, font, player.Name);
                //SafePrint(columnCenterX[4] + x, printy, Color.White, fontScale, Justification.Centered, font, "<practice>");
                //SafePrint(columnCenterX[5] + x, printy, Color.White, fontScale, Justification.Centered, font, player.State == PlayerState.Ready ? "READY!":"");

            }
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Draw the main gameplay action
        /// </summary>
        /// -------------------------------------------------------------
        public void DrawPlayers(List<Player> playerList)
        {
            float playerHeight = Textures.PlayerOverlay.Height * screenSizeFactor;
            float playerWidth = Textures.PlayerOverlay.Width * screenSizeFactor;
            float squeezeFactor = 1f;
            float requiredWidth = playerList.Count * playerWidth;
            if (requiredWidth > Width)
            {
                squeezeFactor = Width / requiredWidth * .95f;
                playerHeight *= squeezeFactor;
                playerWidth *= squeezeFactor;
            }

            float drawY = (Height - playerHeight) * .8f;
            float squareSize = playerWidth * .94f / 10f;
            float blockScaleFactor = squareSize / 100f;


            for (int playerIndex = 0; playerIndex < playerList.Count; playerIndex++)
            {
                Player player = playerList[playerIndex];
                float playerSectionWidth = Width / (float)playerList.Count;
                float drawX = playerIndex * playerSectionWidth + playerSectionWidth / 2 - playerWidth / 2;

                if (player.BackGround == -1)
                {
                    player.BackGround = Globals.rand.Next(7);
                }

                DrawObject(drawX, drawY, (actualPosition) =>
                {
                    Color color = new Color(TeamColors[player.Number].ToVector3() * .5f);
                    float gridPixelWidth = squareSize * player.Grid.Width;

                    DrawPlainRectangle(actualPosition, playerWidth, playerHeight, color);
                    Vector2 gridOffset = new Vector2(playerWidth * .03f, playerHeight * .05f);
                    Vector2 gridScale = new Vector2(
                        squareSize * player.Grid.Width / Textures.Grid[player.BackGround].Width,
                        squareSize * player.Grid.Height / Textures.Grid[player.BackGround].Height);

                    // Draw the grid
                    if (player.State == PlayerState.Prepping)
                    {
                        DrawPlainRectangle(actualPosition + gridOffset, squareSize * player.Grid.Width, squareSize * player.Grid.Height, new Color(.1f, .1f, .1f));

                        UnsafeMarginPrint(Fonts.PlayerFont, actualPosition + gridOffset, squareSize * player.Grid.Width, Color.White, 
                            screenSizeFactor * squeezeFactor, "Press drop button when ready to start the game.");
                    }
                    else
                    {
                        spriteBatch.Draw(Textures.Grid[player.BackGround],
                          actualPosition + gridOffset,
                          null,
                          Color.White,
                          0,
                          new Vector2(0f, 0f),
                          gridScale, // * squeezeFactor,
                          SpriteEffects.None,
                          0);

                        if (player.PsychoColors != null)
                        {
                            if (player.PsychoColors.Count == 0)
                            {
                                for (int colorCount = 0; colorCount < Globals.PsychoColorCount; colorCount++)
                                {
                                    float r = (float)Globals.rand.NextDouble();
                                    float g = (float)Globals.rand.NextDouble();
                                    float b = (float)Globals.rand.NextDouble();
                                    player.PsychoColors.Add(new Color(r, g, b));
                                }
                            }
                        }

                        for (int i = 0; i < Globals.GridWidth; i++)
                        {
                            for (int j = 0; j < Globals.GridHeight; j++)
                            {
                                Vector2 blockOffset = new Vector2(i * squareSize, j * squareSize);
                                Block thisBlock = player.Grid[i, j];
                                if (thisBlock == null)
                                {
                                    if (player.PsychoColors != null)
                                    {
                                        Color overlayColor = new Color((Color)player.PsychoColors[player.PsychoOverlayGrid[i, j]], 0.4f);
                                        DrawPlainRectangle(actualPosition + gridOffset + blockOffset, blockScaleFactor * 100, blockScaleFactor * 100, overlayColor);
                                    }
                                    continue;
                                }


                                Color squareColor = TeamColors[thisBlock.ColorIndex];
                                if (player.Transparency) squareColor = new Color(0, 0, 0, 0);
                                if(player.PsychoColors != null) squareColor = (Color)player.PsychoColors[thisBlock.ColorIndex];

                                if (thisBlock.Clearing)
                                {
                                    float fraction = (float)(thisBlock.FractionLeft);
                                    if (fraction < 0) fraction = 0;
                                    squareColor = new Color(fraction, fraction, fraction, fraction);
                                }

                                float realBlockScaleFactor = blockScaleFactor;
                                if (player.FreezeDried)
                                {
                                    realBlockScaleFactor *= .4f;
                                    blockOffset += new Vector2(squareSize, squareSize) * thisBlock.FreezeDriedOffset;
                                }
                                
                                DrawBrick(actualPosition + gridOffset + blockOffset, realBlockScaleFactor, squareColor, thisBlock);
                            }
                        }

                        if (player.State == PlayerState.Playing && player.CurrentPiece != null)
                        {
                            if (player.SeeShadows)
                            {
                                // Draw the shadow piece
                                int originalY = player.CurrentPiece.Y;
                                player.CurrentPiece.Y = player.ShadowY;

                                // Draw main block
                                player.CurrentPiece.ProcessBlocks(
                                    (block, realx, realy) =>
                                    {
                                        if (realx >= 0 && realy >= 0 && realx < player.Grid.Width && realy < player.Grid.Height)
                                        {
                                            Vector2 blockOffset = new Vector2(realx * squareSize, realy * squareSize);

                                            // Draw the solid color
                                            spriteBatch.Draw(Textures.BrickAndOverlay,
                                                actualPosition + gridOffset + blockOffset,
                                                GetSpriteArea(Textures.BrickAndOverlay, 10, 0),
                                                new Color(1, 1, 1, .2f),
                                                0,
                                                new Vector2(0f, 0f),
                                                blockScaleFactor,
                                                SpriteEffects.None,
                                                0);
                                        }
                                    });

                                // Draw highlight
                                player.CurrentPiece.ProcessBlocks(
                                    (block, realx, realy) =>
                                    {
                                        if (realx >= 0 && realy >= 0 && realx < player.Grid.Width && realy < player.Grid.Height)
                                        {
                                            Vector2 blockOffset = new Vector2(realx * squareSize, realy * squareSize);

                                            // Draw the solid color
                                            spriteBatch.Draw(Textures.BrickAndOverlay,
                                                actualPosition + gridOffset + blockOffset + new Vector2(-(blockScaleFactor * 100 * .1f), -(blockScaleFactor * 100 * .1f)),
                                                GetSpriteArea(Textures.BrickAndOverlay, 10, 2),
                                                new Color(1, 1, 1, .4f),
                                                0,
                                                new Vector2(0, 0),
                                                blockScaleFactor * 1.2f,
                                                SpriteEffects.None,
                                                0);
                                        }
                                    });
                                player.CurrentPiece.Y = originalY;
                            }


                            // Draw the player piece highlight
                            player.CurrentPiece.ProcessBlocks(
                                (block, realx, realy) =>
                                {
                                    if (realx >= 0 && realy >= 0 && realx < player.Grid.Width && realy < player.Grid.Height)
                                    {
                                        Vector2 blockOffset = new Vector2(realx * squareSize, realy * squareSize) - new Vector2(.3f * squareSize, .3f * squareSize);
                                        spriteBatch.Draw(Textures.BrickAndOverlay,
                                            actualPosition + gridOffset + blockOffset,
                                            GetSpriteArea(Textures.BrickAndOverlay, 10, 2),
                                            Color.White,
                                            0,
                                            new Vector2(0f, 0f),
                                            blockScaleFactor * 1.6f,
                                            SpriteEffects.None,
                                            0);
                                    }
                                });

                            // Draw the player piece
                            player.CurrentPiece.ProcessBlocks(
                                (block, realx, realy) =>
                                {
                                    if (realx >= 0 && realy >= 0 && realx < player.Grid.Width && realy < player.Grid.Height)
                                    {
                                        Vector2 blockOffset = new Vector2(realx * squareSize, realy * squareSize);

                                        Color squareColor = TeamColors[block.ColorIndex];
                                        DrawBrick(actualPosition + gridOffset + blockOffset, blockScaleFactor, squareColor, block);
                                    }
                                });
                        }

                        float scorex = playerWidth * .05f;
                        float scorey = playerHeight * .91f;
                        float fontScale = 1.7f;
                        spriteBatch.DrawString(
                            Fonts.PlayerFont,
                            "R: " + player.Rows.ToString("000"),
                            actualPosition + new Vector2(scorex, scorey),
                            Color.White,
                            0,
                            Vector2.Zero,
                            screenSizeFactor * squeezeFactor * fontScale,
                            SpriteEffects.None, 0);


                        spriteBatch.DrawString(
                            Fonts.PlayerFont,
                            "S: " + player.Score.ToString("#,#"),
                            actualPosition + new Vector2(scorex, scorey + screenSizeFactor * squeezeFactor * 34),
                            Color.White,
                            0,
                            Vector2.Zero,
                            screenSizeFactor * squeezeFactor * fontScale,
                            SpriteEffects.None, 0);                    
                    }

                    // Draw Future pieces
                    float futurePieceAreaHeight = playerHeight * .1f;
                    float futurePieceAreaWidth =  gridPixelWidth * .55f;
                    float futurePieceAreaY = playerHeight * 0.81f;
                    Vector2 futurePieceAreaLocation = new Vector2(gridOffset.X, futurePieceAreaY) + actualPosition;
                    DrawBeveledRectangle(futurePieceAreaLocation, futurePieceAreaWidth, futurePieceAreaHeight, new Color(0, 0, 0, .6f));
                    float miniSquareSize = futurePieceAreaHeight / 6;
                    float miniBlockScaleFactor = miniSquareSize / 100f;


                    for (int i = 0; i < 2; i++)
                    {
                        if (i >= player.NextPieces.Count) break;
                        Piece thisPiece = player.NextPieces[i];
                        if (thisPiece == null) break;
                        // Find Verticle and horizontal size
                        int minx = int.MaxValue, miny = int.MaxValue, maxx = int.MinValue, maxy = int.MinValue;
                        thisPiece.ProcessBlocks(
                            (block, realx, realy) =>
                            {
                                
                                int blockx = realx - thisPiece.X;
                                int blocky = realy - thisPiece.Y;

                                if (blockx < minx) minx = blockx;
                                if (blocky < miny) miny = blocky;
                                if (blockx > maxx) maxx = blockx;
                                if (blocky > maxy) maxy = blocky;
                            });

                        int width = (int)((maxx - minx + 1) * miniSquareSize);
                        int height = (int)((maxy - miny + 1) * miniSquareSize);
                        int centerx = (int)(futurePieceAreaWidth / 4 + i * futurePieceAreaWidth / 2);
                        int centery = (int)(futurePieceAreaHeight / 2);
                        int offsetx = -(int)(minx * miniSquareSize);
                        int offsety = -(int)(miny * miniSquareSize);

                        // Draw the player piece
                        thisPiece.ProcessBlocks(
                            (block, realx, realy) =>
                            {
                                int blockx = realx - thisPiece.X;
                                int blocky = realy - thisPiece.Y;

                                Vector2 blockOffset = futurePieceAreaLocation 
                                    + new Vector2(centerx, centery) 
                                    + new Vector2(offsetx, offsety) 
                                    - new Vector2(width /2, height/2) 
                                    + new Vector2(blockx * miniSquareSize, blocky * miniSquareSize);


                                Color squareColor = TeamColors[block.ColorIndex];
                                DrawBrick(blockOffset, miniBlockScaleFactor, squareColor, block);
                            });
                    }

                    // Draw Weapons
                    int weaponx = (int)(playerWidth * .95);
                    int weapony = (int)(futurePieceAreaY);
                    Vector2 weaponPostion = actualPosition + new Vector2(weaponx, weapony);
                    for (int i = 0; i < player.Weapons.Count && i < 4; i++)
                    {
                        SpecialType weaponType = SpecialType.Antidote;

                        spriteBatch.Draw(Textures.BrickAndOverlay,
                            weaponPostion + new Vector2(-(i + 1) * squareSize, 0),
                            GetSpriteArea(Textures.BrickAndOverlay, 10, (int)weaponType + 10),
                            Color.White,
                            0,
                            new Vector2(0f, 0f),
                            blockScaleFactor,
                            SpriteEffects.None,
                            0);
                    }

                    // Draw Powers
                    int powerX = weaponx;
                    int powerY = (int)(futurePieceAreaY + squareSize * 1.5);
                    Vector2 powerPostion = actualPosition + new Vector2(powerX, powerY);
                    for (int i = 0; i < player.Powers.Count && i < 4; i++)
                    {
                        Block tempBlock = new Block(0, 0, 0);
                        tempBlock.SpecialType = player.Powers[i].SpecialType;
                        Vector2 blockPosition = powerPostion + new Vector2(-(i + 1) * squareSize, 0);
                        DrawBrick(blockPosition, blockScaleFactor, Color.Gray, tempBlock);

                        Color expiredColor = new Color(0, 0, 0, .5f);
                        float expiredWidth = (float)(squareSize * (1 - player.Powers[i].Time.FractionLeft));
                        DrawPlainRectangle(blockPosition + new Vector2(squareSize - expiredWidth,0) ,
                            expiredWidth,
                            squareSize, 
                            expiredColor);
                    }

                    // Draw Afflictions
                    int afflictionX = weaponx;
                    int afflictionY = (int)(futurePieceAreaY + squareSize * 3);
                    Vector2 afflictionPostion = actualPosition + new Vector2(afflictionX, afflictionY);
                    for (int i = 0; i < player.Afflictions.Count && i < 4; i++)
                    {
                        Block tempBlock = new Block(0, 0, 0);
                        tempBlock.SpecialType = player.Afflictions[i].SpecialType;
                        DrawBrick(afflictionPostion + new Vector2(-(i + 1) * squareSize, 0), blockScaleFactor, Color.Gray, tempBlock);
                    }


                    // Draw the text and status
                    spriteBatch.DrawString(
                        Fonts.PlayerFontBold,
                        player.Name,
                        actualPosition + new Vector2(playerWidth * .05f, playerHeight * .015f),
                        Color.White,
                        0,
                        Vector2.Zero,
                        screenSizeFactor * squeezeFactor,
                        SpriteEffects.None, 0);


                                                                                               
                    spriteBatch.Draw(Textures.PlayerOverlay,
                        actualPosition,
                        null,
                        Color.White,
                        0,
                        new Vector2(0f, 0f),
                        screenSizeFactor * squeezeFactor,
                        SpriteEffects.None,
                        0);

                    // Draw the attack indicator
                    float attackPointStartX = drawX + playerWidth * .9f;
                    float victimDrawX = player.VictimId * playerSectionWidth + playerSectionWidth / 2 - playerWidth / 2;
                    float attackPointEndX = victimDrawX + playerWidth * .1f + playerWidth * 0.05f * playerIndex;
                    Color attackColor = TeamColors[playerIndex];
                    if (Globals.rand.NextDouble() < (float)player.LastAttackTimer.FractionLeft)
                    {
                        attackColor = Globals.rand.Next(2) == 0 ? Color.White : Color.Black;
                    }
                    if (player.State == PlayerState.Playing && playerList.Count > 1)
                    {
                        if (playerList[player.VictimId].RepelledAttack)
                        {
                            playerList[player.VictimId].RepelledAttack = false;
                            Animations.Add(new Animation.SparkShower(this, new Vector2(attackPointEndX + SafeArea.Left, drawY + SafeArea.Top), squeezeFactor));
                        }

                        DrawAttackIndicator(
                            (int)attackPointStartX + SafeArea.Left, 
                            (int)drawY + SafeArea.Top, 
                            (int)attackPointEndX + SafeArea.Left, 
                            (int)drawY + SafeArea.Top, 
                            playerIndex, playerList.Count, attackColor);
                    }
                    
                    //  Fade out if Dying
                    if (player.State == PlayerState.Dying || player.State == PlayerState.Dead)
                    {
                        double fraction = player.StateTimer.FractionLeft;
                        if (fraction > 1) fraction = 1;

                        Color fadeColor = new Color(0, 0, 0, (float)(1-fraction)/2);
                        DrawPlainRectangle(actualPosition + gridOffset, squareSize * player.Grid.Width, squareSize * player.Grid.Height, fadeColor);
                    }
#if DEBUG
                    spriteBatch.DrawString(
                        Fonts.DebugFont,
                        "DI: " + player.DropTickIntervalSeconds.ToString(".000") + "\r\n" +
                        "A: " + player.Afflictions.Count,
                        actualPosition + new Vector2(playerWidth * .1f, playerHeight * .1f),
                        Color.Red,
                        0,
                        Vector2.Zero,
                        screenSizeFactor * squeezeFactor,
                        SpriteEffects.None, 0);
#endif
                });

            }
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Draw the arrow that indicates your victim
        /// </summary>
        /// -------------------------------------------------------------
        private void DrawAttackIndicator(int startX, int startY, int endx, int endy, int index, int playerCount, Color color)
        {
            float ySpace = startY;
            if (ySpace > 100) ySpace = 100;
            if (ySpace > 20 * playerCount) ySpace = 20 * playerCount;
            int spacePerPlayer = (int)(ySpace / (playerCount+1));
            int segmentSize = spacePerPlayer;

            int targetY = (int)(startY - spacePerPlayer * (index + 2));
            int subTargetY = targetY + segmentSize;
            Rectangle destinationRectangle;
            Rectangle sourceRectangle;

            // Draw the line up and away from player
            int y = startY;
            while (y > subTargetY)
            {
                y -= segmentSize;
                int destinationHeight = segmentSize;
                if (y < subTargetY)
                {
                    destinationHeight = segmentSize - (subTargetY - y);
                    y = subTargetY;
                }
                float bitmapPortion = (float)destinationHeight / segmentSize;

                destinationRectangle = new Rectangle(startX - segmentSize / 2, y, segmentSize, destinationHeight);
                sourceRectangle = GetSpriteArea(Textures.AttackArrow, 3, 3);
                sourceRectangle.Height = (int)(sourceRectangle.Height * bitmapPortion);

                spriteBatch.Draw(Textures.AttackArrow, destinationRectangle, sourceRectangle, color);
            }

            // Draw the first elbow
            int textureIndex = 0;
            if (endx < startX) textureIndex = 2;
            sourceRectangle = GetSpriteArea(Textures.AttackArrow, 3, textureIndex);
            destinationRectangle = new Rectangle(startX - segmentSize / 2, targetY, segmentSize, segmentSize);
            spriteBatch.Draw(Textures.AttackArrow, destinationRectangle, sourceRectangle, color);

            // Draw the line over
            int x = startX;
            int xm = Math.Sign(endx - startX);
            int lengthLeft = (int)Math.Abs(endx - startX) - segmentSize;
            while (lengthLeft > 0)
            {
                x += segmentSize * xm;
                int destinationWidth = segmentSize;
                lengthLeft -= segmentSize;
                if (lengthLeft < 0)
                {
                    destinationWidth += lengthLeft;
                    if(xm < 0) x += lengthLeft * xm;
                }
                float bitmapPortion = (float)destinationWidth / segmentSize;

                destinationRectangle = new Rectangle(x - segmentSize / 2, targetY, destinationWidth,segmentSize );
                sourceRectangle = GetSpriteArea(Textures.AttackArrow, 3, 1);
                sourceRectangle.Width = (int)(sourceRectangle.Width * bitmapPortion);
                spriteBatch.Draw(Textures.AttackArrow, destinationRectangle, sourceRectangle, color);
            }

            // Draw the second elbow
            textureIndex = 2;
            if (endx < startX) textureIndex = 0;
            sourceRectangle = GetSpriteArea(Textures.AttackArrow, 3, textureIndex);
            destinationRectangle = new Rectangle(endx - segmentSize / 2, targetY, segmentSize, segmentSize);
            spriteBatch.Draw(Textures.AttackArrow, destinationRectangle, sourceRectangle, color);

            // Draw the line down to the victom
            subTargetY = startY - segmentSize;
            while (y < subTargetY)
            {
                int destinationHeight = segmentSize;
                if (y > subTargetY)
                {
                    destinationHeight = segmentSize - (y - subTargetY);
                    y = subTargetY;
                }
                float bitmapPortion = (float)destinationHeight / segmentSize;

                destinationRectangle = new Rectangle(endx - segmentSize / 2, y, segmentSize, destinationHeight);
                sourceRectangle = GetSpriteArea(Textures.AttackArrow, 3, 3);
                sourceRectangle.Height = (int)(sourceRectangle.Height * bitmapPortion);

                spriteBatch.Draw(Textures.AttackArrow, destinationRectangle, sourceRectangle, color);
                y += segmentSize;
            }

            // Draw the arrow
            sourceRectangle = GetSpriteArea(Textures.AttackArrow, 3, 6);
            destinationRectangle = new Rectangle(endx - segmentSize, y, segmentSize * 2, segmentSize);
            spriteBatch.Draw(Textures.AttackArrow, destinationRectangle, sourceRectangle, color);

        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Draw a highlighted brick
        /// </summary>
        /// -------------------------------------------------------------
        private void DrawBrick(Vector2 position, float blockScaleFactor, Color color, Block block)
        {


            float size = 100 * blockScaleFactor;

            if (block.SpecialType != SpecialType.None)
            {
                float theta = (float)block.SpecialTimer.ElapsedSeconds;

                color = new Color(
                    (float)Math.Sin(theta * 3) * .3f + .3f,
                    (float)Math.Sin(theta * 7) * .3f + .3f,
                    (float)Math.Sin(theta * 13) * .3f + .3f);

            }

            // Draw the solid color
            spriteBatch.Draw(Textures.BrickAndOverlay,
                position,
                GetSpriteArea(Textures.BrickAndOverlay, 10, 0),
                color,
                0,
                new Vector2(0f, 0f),
                blockScaleFactor,
                SpriteEffects.None,
                0);

            // Draw the special if there is one
            if (block.SpecialType != SpecialType.None)
            {
                Vector2 offset = Vector2.Zero;
                if (block.SpecialType == SpecialType.Jumble)
                {
                    offset = new Vector2((float)Globals.RandomDouble(-1, 1), (float)Globals.RandomDouble(-1, 1)) * 20 * blockScaleFactor;
                }

                spriteBatch.Draw(Textures.BrickAndOverlay,
                    position + offset,
                    GetSpriteArea(Textures.BrickAndOverlay, 10, (int)block.SpecialType + 10),
                    Color.White,
                    0,
                    new Vector2(0f, 0f),
                    blockScaleFactor,
                    SpriteEffects.None,
                    0);
            }

            //  Draw the overlay
            spriteBatch.Draw(Textures.BrickAndOverlay,
                position,
                GetSpriteArea(Textures.BrickAndOverlay, 10, 1),
                Color.White,
                0,
                new Vector2(0f, 0f),
                blockScaleFactor,
                SpriteEffects.None,
                0);

            if (block.NeedsPuff)
            {
                for (int i = 0; i < 4; i++)
                {
                    Animations.Add(new Animation.Puff(this, position + new Vector2((size / 4) * (i + 0.5f), size + (size/8) * (float)Globals.RandomDouble(-1,1)), blockScaleFactor));
                }
                block.NeedsPuff = false;
            }

            if (block.AnimationType != AnimationType.None && !block.AnimationStarted)
            {
                block.AnimationStarted = true;
                switch (block.AnimationType)
                {
                    case AnimationType.Highlight:
                        Animations.Add(new Animation.Highlight(this, position + new Vector2(100 * blockScaleFactor / 2, 100 * blockScaleFactor / 2), blockScaleFactor));
                        break;
                    case AnimationType.Newbie:
                        Animations.Add(new Animation.NewbieGlow(this, position + new Vector2(100 * blockScaleFactor / 2, 100 * blockScaleFactor / 2)));
                        break;
                    case AnimationType.ExplodeMe:
                        Animations.Add(new Animation.ExplodeBrick(this, position + new Vector2(100 * blockScaleFactor / 2, 100 * blockScaleFactor / 2), blockScaleFactor, color));
                        break;
                    default: break;
                }
            }
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Draw a plain filled rectangle
        /// </summary>
        /// -------------------------------------------------------------
        private void DrawPlainRectangle(Vector2 actualPosition, float width, float height, Color color)
        {
            spriteBatch.Draw(Textures.WhitePixel,
                actualPosition,
                null,
                color,
                0,
                new Vector2(0f, 0f),
                new Vector2(width, height),
                SpriteEffects.None,
                0);
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Draw a pretty bevelled block
        /// </summary>
        /// -------------------------------------------------------------
        private void DrawBeveledBlock(Vector2 actualPosition, float width, float height, Color color, bool invert)
        {
            Vector2 embossSize = new Vector2(width * 0.04f, height * 0.04f);

            Rectangle destination = new Rectangle(
                (int)(actualPosition.X - embossSize.X), 
                (int)(actualPosition.Y - embossSize.Y), 
                (int)(width + embossSize.X * 2), (int)(height + embossSize.Y * 2));
            spriteBatch.Draw(Textures.BeveledBlock, destination, color);
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Draw a bevelled rectangle
        /// </summary>
        /// -------------------------------------------------------------
        private void DrawBeveledRectangle(Vector2 actualPosition, float width, float height, Color color)
        {
            Vector2 embossSize = new Vector2(width * 0.02f, height * 0.02f);
            spriteBatch.Draw(Textures.WhitePixel,
                actualPosition + embossSize,
                null,
                color,
                0,
                new Vector2(0f, 0f),
                new Vector2(width, height) - embossSize * 2,
                SpriteEffects.None,
                0);

            Rectangle destination = new Rectangle((int)actualPosition.X, (int)actualPosition.Y, (int)width, (int)height);
            spriteBatch.Draw(Textures.DownBevel, destination, Color.White);

        }


        private void SafeDrawPlainRectangle(Vector2 position, float width, float height, Color color)
        {
            if (!batchStarted)
            {
                spriteBatch.Begin();
                batchStarted = true;
            }
            Vector2 actualPosition = new Vector2(position.X + SafeArea.Left, position.Y + SafeArea.Top);

            DrawPlainRectangle(actualPosition, width, height, color);
        }


        /// -------------------------------------------------------------
        /// <summary>
        /// End drawing operations and render the frame
        /// </summary>
        /// -------------------------------------------------------------
        public void EndDrawing()
        {
            if (!batchStarted)
            {
                spriteBatch.Begin();
                batchStarted = true;
            }

            Debug.WriteLine(Animations.Count);
            for (int i = 0; i < Animations.Count; )
            {
                if (Animations[i].Finished)
                {
                    Animations.RemoveAt(i);
                    continue;
                }

                Animations[i].DrawMe();
                i++;
            }

            if (batchStarted)
            {
                spriteBatch.End();
                batchStarted = false;
            }
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Draw an object in a wraparound style
        /// </summary>
        /// -------------------------------------------------------------
        void DrawObject(float x, float y, DrawSnippet drawIt)
        {
            if (!batchStarted)
            {
                spriteBatch.Begin();
                batchStarted = true;
            }

            Vector2 actualPostion = new Vector2(
                x + SafeArea.Left,
                y + SafeArea.Top);

            drawIt(actualPostion);

        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Debug printing in a small font
        /// </summary>
        /// -------------------------------------------------------------
        public void DebugPrint(int x, int y, Color color, string text)
        {
            SafePrint(x, y, color, Fonts.DebugFont, text);
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Regular printing
        /// </summary>
        /// -------------------------------------------------------------
        public void Print(float x, float y, Color color, string text)
        {
            SafePrint(x, y, color, Fonts.InfoFont, text);
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Print Something in the center of the screen
        /// </summary>
        /// -------------------------------------------------------------
        public void CenterPrint(Color color, string text)
        {
            SpriteFont font = Fonts.PlayerFontBold;
            Vector2 size = font.MeasureString(text) * screenSizeFactor * 4;

            SafePrint(SafeArea.Width / 2 - size.X / 2, SafeArea.Height / 2 - size.Y / 2, color, screenSizeFactor * 4, Justification.Left, font, text);
        }

        public void SafePrint(float x, float y, Color color, string text, PrintStyle style)
        {
            SpriteFont font = Fonts.InfoFont;
            switch (style)
            {
                case PrintStyle.Normal:
                    font = Fonts.InfoFont;
                    break;
                case PrintStyle.Debug:
                    font = Fonts.DebugFont;
                    break;
                case PrintStyle.Score:
                    font = Fonts.ScoreFont;
                    break;
                case PrintStyle.Info:
                    font = Fonts.InfoFont;
                    break;
                default:
                    break;
            }

            SafePrint(x, y, color, font, text);
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Print From here
        /// </summary>
        /// -------------------------------------------------------------
        private void SafePrint(float x, float y, Color color, SpriteFont font, string text)
        {
            SafePrint(x, y, color, 1.0f, Justification.Left, font, text);
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Print From here
        /// </summary>
        /// -------------------------------------------------------------
        private void SafePrint(float x, float y, Color color, float scale, Justification justification, SpriteFont font, string text)
        {
            if (!batchStarted)
            {
                spriteBatch.Begin();
                batchStarted = true;
            }

            float drawx = x + SafeArea.Left;
            Vector2 size = font.MeasureString(text) * scale;
            switch (justification)
            {
                case Justification.Centered: drawx -= size.X / 2; break;
                case Justification.Right: drawx -= size.X; break;
            }

            spriteBatch.DrawString(font, text, new Vector2(drawx, y + SafeArea.Top), color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Print text on a margin
        /// </summary>
        /// -------------------------------------------------------------
        void UnsafeMarginPrint(SpriteFont font, Vector2 position, float rightMarginRelativeSize, Color color, float scale, string text)
        {
            string[] parts = text.Replace("\r", "").Replace("\n", " ~NEWLINE~ ").Split(' ');
            Vector2 currentPosition = position;
            Vector2 spaceSize = font.MeasureString(" ") * scale;
            float rightMargin = position.X + rightMarginRelativeSize;
            float leftMargin = position.X;

            foreach (string part in parts)
            {
                Vector2 size = font.MeasureString(part) * scale;
                if (currentPosition.X + size.X > rightMargin || text == "~NEWLINE~")
                {
                    currentPosition = new Vector2(leftMargin, currentPosition.Y + spaceSize.Y);   
                }

                if (text == "~NEWLINE~") continue;
                spriteBatch.DrawString(font, part, currentPosition, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                currentPosition = new Vector2(currentPosition.X + size.X + spaceSize.X, currentPosition.Y);
            }

        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Get the rectangle representing the sprite index
        /// </summary>
        /// -------------------------------------------------------------
        private Rectangle GetSpriteArea(Texture2D texture, int spritesPerRow, int index)
        {
            int spriteSize = texture.Width / spritesPerRow;
            int row = index / spritesPerRow;
            int column = index % spritesPerRow;
            return new Rectangle(column * spriteSize, row * spriteSize, spriteSize, spriteSize);
        }


        /// -------------------------------------------------------------
        /// <summary>
        /// Draw sprites here
        /// </summary>
        /// -------------------------------------------------------------
        private void DrawSprite(float x, float y, Texture2D tex, Color color, float rotation, float scale)
        {
            if (!batchStarted)
            {
                spriteBatch.Begin();
                batchStarted = true;
            }

            spriteBatch.Draw(tex,
               new Vector2(x + SafeArea.Left, y + SafeArea.Top),
               null,
               color,
               rotation,
               new Vector2(0, 0),
               scale,
               SpriteEffects.None,
               0);
        }


        /// -------------------------------------------------------------
        /// <summary>
        /// Draw sprites here
        /// </summary>
        /// -------------------------------------------------------------
        private void FillRectangleWithSprite(Texture2D tex, Rectangle rectangle, Color color)
        {
            if (!batchStarted)
            {
                spriteBatch.Begin();
                batchStarted = true;
            }

            spriteBatch.Draw(tex, rectangle, color);
        }



        /// -------------------------------------------------------------
        /// <summary>
        /// Print the logo
        /// </summary>
        /// -------------------------------------------------------------
        public void PrintLogo()
        {
            SafePrint(2, Height - 17, Color.LimeGreen, Fonts.InfoFont, "Eitrix by Betafreak");
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Print Announcement
        /// </summary>
        /// -------------------------------------------------------------
        public void PrintAnnouncement(string text)
        {
            Vector2 size = Fonts.ScoreFont.MeasureString(text) * 1.4f;
            SafePrint(Width / 2 - (int)(size.X / 2), Height / 2 + 10, Color.DarkRed, 1.4f, Justification.Left, Fonts.ScoreFont,
                text);
        }


        ///// -------------------------------------------------------------
        ///// <summary>
        ///// Draw the TitleMenu
        ///// </summary>
        ///// -------------------------------------------------------------
        public void DrawTitleMenu(MenuChoice selectedOption)
        {
            float drawX = (SafeArea.Width) * .12f;
            float drawY = (SafeArea.Height) * .2f;

            float menuScale = screenSizeFactor * .8f;
            float itemHeight = Fonts.MenuFont.LineSpacing * menuScale;

            //  Draw Logo and Version
            SafePrint(Width - 150, 5, Color.Black, Fonts.DebugFont, "ver " + AssemblyConstants.Version);
            DrawSprite(drawX / 2, drawY - Textures.Logo.Height * screenSizeFactor * 1.1f, Textures.LogoShadow, Color.Black, 0, screenSizeFactor);
            DrawSprite(drawX / 2, drawY - Textures.Logo.Height * screenSizeFactor * 1.1f, Textures.Logo, Color.White, 0, screenSizeFactor);

            float adjust = 0;

            for(int i=0; i < (int)MenuChoice.NumberOfChoices; i++)
            {
                MenuChoice choice = (MenuChoice)i;

                Color choiceColor = new Color(.7f, .7f, .7f);
                if (selectedOption == choice)
                {
                    adjust = (float)Math.Sin((DateTime.Now - startTime).TotalSeconds * 8) / 60f;
                    choiceColor = Color.DarkCyan;
                }
                else adjust = 0;
                SafePrint((int)drawX, (int)drawY, choiceColor, menuScale + adjust, Justification.Left, Fonts.MenuFont, choice.ToString());
                drawY += itemHeight;

            }

#if WINDOWS
            SafePrint(Width - 50, Height - 90 * screenSizeFactor, Color.Black, menuScale, Justification.Right, Fonts.MenuFont, "Coming soon to XBox!");
            SafePrint(Width - 53, Height - 93 * screenSizeFactor, Color.Red, menuScale, Justification.Right, Fonts.MenuFont, "Coming soon to XBox!");
#endif
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Draw a list of options
        /// </summary>
        /// -------------------------------------------------------------
        public void DrawOptions(List<Tuple<string, VisualOptionValue>> options, int selectedOption, string title, string instructions)
        {
            int x = (int)(Width * 0.1);
            int y = (int)(Height * 0.2);
            int bottomY = (int)(Height * 0.85);
            float optionScaleFactor = screenSizeFactor * .6f;
            Vector2 size = Fonts.MenuFont.MeasureString("A") * optionScaleFactor;
            int xtab1 = (int)(x + 25 * size.X);
            int optionsPerScreen = (int)( Height * 0.65 / size.Y);

            SafePrint(10, 10, Color.White, screenSizeFactor, Justification.Left, Fonts.MenuFont, "Eitrix Options");

            SafePrint(x, y + -1.5f * size.Y, Color.Yellow, optionScaleFactor, Justification.Left, Fonts.MenuFont, title);

            int optionOffset = 0;

            if (selectedOption >= optionsPerScreen - 1)
            {
                optionOffset = selectedOption - (optionsPerScreen - 1);
            }
            if (optionOffset < 0) optionOffset = 0;

            for (int i = 0; i < optionsPerScreen; i++)
            {
                int index = i + optionOffset;
                if (index >= options.Count) break;
                float optionY = y + (index - optionOffset) * size.Y;
                if (optionY > bottomY) break;
                Color color = Color.White;
                if (selectedOption == index) color = Color.Aqua;
                if (options[index].SecondValue.HighlightMe)
                {
                    float c = (float)(Math.Sin(options[index].SecondValue.Timer.ElapsedSeconds * 10) + 1)/4;
                    Color highlightColor = new Color(0, 1, 0, c);
                    DrawPlainRectangle(new Vector2(xtab1 + SafeArea.Left - 5, y + index * size.Y + SafeArea.Top), (Width - xtab1) * .95f, size.Y * .95f, highlightColor);
                }

                if (options[index].SecondValue.Error) color = Color.Red;
                SafePrintOptionPair(x, xtab1, (int)(optionY), color, optionScaleFactor, options[index].FirstValue, options[index].SecondValue.Value);
            }

            SafePrint(x, bottomY + 1 * size.Y, Color.Yellow, optionScaleFactor, Justification.Left, Fonts.MenuFont, instructions);

        }

        /// -------------------------------------------------------------
        /// <summary>
        /// Prints a pair of values
        /// </summary>
        /// -------------------------------------------------------------
        private void SafePrintOptionPair(int optionColumn0X, int optionColumn1X, int optionY, Color color, float optionScaleFactor, string name, string value)
        {
            SafePrint(optionColumn0X, optionY, color, optionScaleFactor, Justification.Left, Fonts.MenuFont, name);
            SafePrint(optionColumn1X, optionY, color, optionScaleFactor, Justification.Left, Fonts.MenuFont, value);
        }


        /// -------------------------------------------------------------
        /// <summary>
        /// Draw the pause screen
        /// </summary>
        /// -------------------------------------------------------------
        public void DrawPauseScreen(List<Tuple<string, VisualOptionValue>> options, int selectedOption)
        {
            float pauseWidth = Width * 0.7f;
            float pauseHeight = Height * 0.4f;
            int x = (int)(pauseWidth * 0.05 + (Width - pauseWidth)/2) ;
            int y = (int)(pauseHeight * 0.05 + (Height - pauseHeight)/2);
            int margin = (int)(pauseHeight * 0.05);
            float optionScaleFactor = screenSizeFactor * .6f;
            Vector2 size = Fonts.MenuFont.MeasureString("A") * optionScaleFactor;
            int xtab1 = (int)(x + 15 * size.X);

            DrawPlainRectangle(new Vector2(0, 0), 
                graphics.GraphicsDevice.DisplayMode.Width, 
                graphics.GraphicsDevice.DisplayMode.Height, 
                new Color(0, 0, 0, 0.5f));

            DrawBeveledBlock(new Vector2(x + safeArea.Left - margin,y + safeArea.Top - margin), pauseWidth + margin * 2, pauseHeight + margin * 2, Color.Gray, false);

          
            SafePrint(x + pauseWidth/2, y, Color.White, optionScaleFactor * 1.2f, Justification.Centered, Fonts.MenuFont, "Paused");


            for (int i = 0; i < options.Count; i++)
            {
                int index = i;
                float optionY = y + (index + 2f) * size.Y;
                Color color = Color.White;
                if (selectedOption == index) color = Color.Aqua;
                if (options[index].SecondValue.HighlightMe)
                {
                    float c = (float)(Math.Sin(options[index].SecondValue.Timer.ElapsedSeconds * 10) + 1) / 4;
                    Color highlightColor = new Color(0, 1, 0, c);
                    DrawPlainRectangle(new Vector2(xtab1 + SafeArea.Left - 5, y + index * size.Y + SafeArea.Top), (Width - xtab1) * .95f, size.Y * .95f, highlightColor);
                }
                SafePrintOptionPair(x, xtab1, (int)(optionY), color, optionScaleFactor, options[index].FirstValue, options[index].SecondValue.Value);
            }

        }

        Block[] HelpBlocks;
        /// -------------------------------------------------------------
        /// <summary>
        /// Draw the game help screen
        /// </summary>
        /// -------------------------------------------------------------
        public void DrawHelpScreen(int page)
        {
            FillRectangleWithSprite(Textures.HelpPage[page], new Rectangle(SafeArea.Left,SafeArea.Top,Width, Height), Color.White );
            if (page == 1)
            {
                if (HelpBlocks == null)
                {
                    HelpBlocks = new Block[(int)SpecialType.NumberOfSpecials];
                    for (int i = 0; i < (int)SpecialType.NumberOfSpecials; i++)
                    {
                        HelpBlocks[i] = new Block(0, 0, 0);
                        HelpBlocks[i].SpecialType = (SpecialType)i;
                        HelpBlocks[i].SpecialTimer = new TimeWatcher();
                    }
                }

                float blockSize = Height * .7f / 15;
                float blockScale = blockSize / 100 * .9f;
                float left = Width * 0.06f;
                float top = Height * 0.2f;
                Vector2 SafeCorner = new Vector2(SafeArea.Left, SafeArea.Top);
                for (int i = 0; i < (int)SpecialType.NumberOfSpecials; i++)
                {
                    float blockY = (i % 14) * blockSize;
                    float blockX = left + (i / 14) * Width * .3f;
                    DrawBrick(new Vector2(blockX, top + blockY) + SafeCorner, blockScale, Color.Green, HelpBlocks[i]);
                    SafePrint(blockX + blockSize * 1.1f, top + blockY, Color.Cyan, screenSizeFactor/2, Justification.Left, Fonts.MenuFont, ((SpecialType)i).ToString());
                }

            }
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// A class for tracking texture data
        /// </summary>
        /// -------------------------------------------------------------
        static class Textures
        {
            public static List<Texture2D> Background = new List<Texture2D>();
            public static List<Texture2D> Grid = new List<Texture2D>();
            public static List<Texture2D> HelpPage = new List<Texture2D>();
            public static Texture2D BeveledBlock;
            public static Texture2D Brick;
            public static Texture2D BrickAndOverlay;
            public static Texture2D BrickOverlay1;
            public static Texture2D BrickOverlay2;
            public static Texture2D BrickOverlay3;
            public static Texture2D WhitePixel;
            public static Texture2D PlayerOverlay;
            public static Texture2D DownBevel;
            public static Texture2D AttackArrow;
            public static Texture2D Logo;
            public static Texture2D LogoShadow;
        }

        /// -------------------------------------------------------------
        /// <summary>
        /// A class for tracking texture data
        /// </summary>
        /// -------------------------------------------------------------
        static class Fonts
        {
            public static SpriteFont DebugFont;
            public static SpriteFont ScoreFont;
            public static SpriteFont InfoFont;
            public static SpriteFont MenuFont;
            public static SpriteFont PlayerFont;
            public static SpriteFont PlayerFontBold;
        }
    }
}

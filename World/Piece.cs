using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Eitrix
{
    public delegate void BlockProcessor(Block block, int realx, int realy);

    public enum CenterType
    {
        Regular,
        UpperLeftCorner
    }

    public class Piece
    {
        CenterType centerType;
        int pieceType;
        List<Block> pieceParts;
        static string[] pieceData;
        static string[] extraPieceData;
        static string[] evilPieceData;

        public List<Block> PieceParts { get{ return pieceParts;}}

        public int PieceType { get { return pieceType; } }

        public int X { get; set; }
        public int Y { get; set; }
        public bool Dropping { get; set; }
        public bool MakeItStick { get; set; }

        public static int NumberOfTypes { get { return pieceData.Length; } }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        ///------------------------------------------------------------------------------
        static Piece()
        {
            pieceData = new string[]
            {
                "R|-1,0|0,0|1,0|0,-1",      // Pyramid

                "R|0,-1|0,0|0,1|0,2",       // I

                "R|0,-1|0,0|0,1|1,1",       // L
                "R|0,-1|0,0|0,1|-1,1",      // Reverse L   


                "R|0,-1|0,0|-1,-1|1,0",     // Z
                "R|0,-1|0,0|1,-1|-1,0",     // Reverse Z

                "C|0,0|1,0|0,1|1,1",        // Block
            };


            extraPieceData = new string[]
            {
               // Extra Pieces
                "R|0,-1|0,0|0,1",           // short I
                "R|0,-1|0,0|1,0",           // Short L
                "R|0,-1|0,0|-1,0",          // Reverse Short L
                "C|0,0|0,1"                 // TwoPiece Block
            };

            // R = regular center, C = lowerright corner center
            evilPieceData = new string[]
            {
                "R|0,-1|0,0|-1,-1|1,0",     // Z
                "R|0,-1|0,0|1,-1|-1,0",     // Reverse Z
                "C|0,0|1,0|0,1|1,1",        // Block
                "C|1,-1|1,0|0,1|0,2",    // Hyper Z
                "C|0,-1|0,0|1,1|1,2",    // Reverse Hyper Z
                "R|-1,-1|-1,0|-1,1|0,1|1,1",// Big L
                "R|1,-1|1,0|1,1|0,1|-1,1",  // Reverse Big L
                "R|0,-1|0,0|-1,1|0,1|1,1",  // Big T
            };


        }


        ///------------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        ///------------------------------------------------------------------------------
        public Piece(bool evilPieces)
        {
            List<string> pieceSource = new List<string>();

            if (evilPieces) pieceSource.AddRange(evilPieceData);
            else
            {
                pieceSource.AddRange(pieceData);
                if (Globals.Options.ExtraPieces) pieceSource.AddRange(extraPieceData);
            }

            this.pieceType = Globals.rand.Next(pieceSource.Count);
            int rotation = Globals.rand.Next(4);

            string[] parts = pieceSource[pieceType].Split('|');

            centerType = (parts[0][0] == 'R' ? CenterType.Regular : CenterType.UpperLeftCorner);

            pieceParts = new List<Block>();
            for (int i = 1; i < parts.Length; i++)
            {
                string[] offset = parts[i].Split(',');
                pieceParts.Add(new Block(int.Parse(offset[0]), int.Parse(offset[1]), pieceType));
            }

            while (rotation-- > 0) RotateLeft();

        }
        ///------------------------------------------------------------------------------
        /// <summary>
        /// Private constructor for cloning
        /// </summary>
        ///------------------------------------------------------------------------------
        private Piece()
        {
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// A generic way of handling the blocks of a piece because 
        /// of the weird logic for handling the center point
        /// </summary>
        ///------------------------------------------------------------------------------
        public void ProcessBlocks(BlockProcessor processor)
        {
            foreach (Block block in PieceParts)
            {
                int realx = X + block.OffsetX;
                int realy = Y + block.OffsetY;
                processor(block, realx, realy);
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Rotate the piece ClockWise
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void RotateRight()
        {
            int newx,newy;
            ProcessBlocks(
                (block, realx, realy) =>
                {
                    if (this.centerType == CenterType.Regular)
                    {
                        newx = -block.OffsetY;
                        newy = block.OffsetX;
                    }
                    else
                    {
                        newx = -block.OffsetY + 1;
                        newy = block.OffsetX;
                    }
                    block.OffsetX = newx;
                    block.OffsetY = newy;
                });
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// ROtate the piece CounterClockwise
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void RotateLeft()
        {
            int newx, newy;
            ProcessBlocks(
                (block, realx, realy) =>
                {
                    if (this.centerType == CenterType.Regular)
                    {
                        newx = block.OffsetY;
                        newy = -block.OffsetX;
                    }
                    else
                    {
                        newx = block.OffsetY;
                        newy = -block.OffsetX + 1;
                    }
                    block.OffsetX = newx;
                    block.OffsetY = newy;
                });
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Create a copy of this piece
        /// </summary>
        ///------------------------------------------------------------------------------
        internal Piece Clone()
        {
            Piece newPiece = new Piece();
            newPiece.centerType = this.centerType;
            newPiece.pieceParts = new List<Block>();
            newPiece.pieceType = this.pieceType;
            foreach(Block block in this.pieceParts)
            {
                newPiece.pieceParts.Add(new Block(block.OffsetX, block.OffsetY, block.ColorIndex));
            }

            newPiece.X = X;
            newPiece.Y = Y;
            return newPiece;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eitrix
{
    public class Grid
    {
        int width, height;
        Block[,] blockArray;

        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public Block this[int x, int y]
        {
            get { return blockArray[x, y]; }
            set { blockArray[x, y] = value; }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        ///------------------------------------------------------------------------------
        public Grid(int width, int height)
        {
            this.width = width;
            this.height = height;
            blockArray = new Block[width, height];
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Set the piece's blocks in the grid
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void SetPiece(Piece piece)
        {
            piece.ProcessBlocks(
                (block, realx, realy) =>
                {
                    if (realx >= 0 && realx < width && realy >= 0 && realy < Height)
                    {
                        if (realy == Height - 1 || blockArray[realx, realy + 1] != null)
                        {
                            block.NeedsPuff = true;
                        }
                    }
                });

            piece.ProcessBlocks(
                (block, realx, realy) =>
                {
                    if (realx >= 0 && realx < width && realy >= 0 && realy < Height)
                    {
                        blockArray[realx, realy] = block;
                    }
                });
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Make a quick copy of this grid to another Grid.
        /// </summary>
        ///------------------------------------------------------------------------------
        internal void DummyCopyTo(Grid targetGrid)
        {
            if (targetGrid.Width != Width || targetGrid.Height != Height) throw new ApplicationException("Grid sizes don't match");

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (this[i, j] != null) targetGrid[i, j] = new Block(0,0,0);
                    else targetGrid[i, j] = null;
                }
            }
        }

        ///------------------------------------------------------------------------------
        /// <summary>
        /// Return the number of voids in the grid (voids are spaces underneath blocks)
        /// </summary>
        ///------------------------------------------------------------------------------
        public int GetVoidCount()
        {
            int voidCount = 0;
            for (int i = 0; i < Width; i++)
            {
                bool foundBlock = false;
                for (int j = 0; j < Height; j++)
                {
                    if (this[i, j] == null)
                    {
                        if(foundBlock) voidCount++;
                    }
                    else foundBlock = true;
                }
            }
            return voidCount;
        }
    }
}

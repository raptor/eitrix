using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Eitrix
{
    public class IntPoint
    {
        public int X;
        public int Y;

        public IntPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Globals
    {
        public const int PsychoColorCount = 32;
        public const int MaxPlayers = 8;
        public const int GridWidth = 10;
        public const int GridHeight = 21;
        public const int NonPlayer = -1;
        public const int ComputerPlayerId = 20000;
        public const int KeyBoardPlayerBaseId = 1000; 
        public const int HelpPages = 3;


        public static Random rand = new Random();
        public static bool StressEnabled = false;
        public static EitrixOptions Options = new EitrixOptions();
        public static string StorageName = "EitrixGameData";
        public static bool LowResolution = false;
        public static bool ConfiguringKeyboard = false;
        public static bool FullScreenMode;

        public static bool IsTrialMode
        {
            get
            {

                // Turn off upselling in December 2010, and June 2011
                if (DateTime.Today < new DateTime(2010, 12, 14)
                    && DateTime.Today > new DateTime(2010, 12, 7))
                    return false;
                if (DateTime.Today < new DateTime(2011, 6, 14)
                    && DateTime.Today > new DateTime(2011, 6, 7))
                    return false;
                else return Microsoft.Xna.Framework.GamerServices.Guide.IsTrialMode;
            }
        }

        /// --------------------------------------------------------------
        /// <summary>
        /// Static Constructor
        /// </summary>
        /// --------------------------------------------------------------
        static Globals()
        {

            rand = new Random();
        }



        /// --------------------------------------------------------------
        /// <summary>
        /// Generate a Random offset from 1 with a maximum excursion
        /// </summary>
        /// --------------------------------------------------------------
        internal static float RandomPitch(double maxexcursion)
        {
            return (float)(rand.NextDouble() * maxexcursion * 2 - maxexcursion);
        }

        /// --------------------------------------------------------------
        /// <summary>
        /// Generate a Random double with the specified range
        /// </summary>
        /// --------------------------------------------------------------
        internal static double RandomDouble(double min, double max)
        {
            double range = max - min;
            return rand.NextDouble() * range + min;
        }


    }

}


namespace Eitrix
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                foreach (string arg in args)
                {
                    switch (arg.ToLower())
                    {
                        case "/fullscreen": Globals.FullScreenMode = true; break;
                    }
                }

                using (Game1 game = new Game1())
                {
                    game.Run();
                }
            }
#if !DEBUG
            catch (Exception e)
            {
#if WINDOWS
                string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string errorFileName = System.IO.Path.Combine(desktopFolder, "EitrixError.txt");
                System.IO.File.WriteAllText(errorFileName, e.ToString());
#endif
            }
#endif
            finally
            {

            }
        }
    }
}


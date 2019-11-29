using System;

namespace Airman
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
#endif

    //description of levels...
    public enum Levels
    {
        MAIN_MENU,
        MAIN_SPLASH,
        LEVEL_ONE,
        DYING_SPLASH,
        TESTING,
    };
}

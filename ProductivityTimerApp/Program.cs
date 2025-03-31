namespace ProductivityTimerApp;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Length > 0 && args[0].Equals("--minimal", StringComparison.OrdinalIgnoreCase))
        {
            Application.Run(new MinimalForm());
        }
        else
        {
            Application.Run(new Form1());
        }
    }

}
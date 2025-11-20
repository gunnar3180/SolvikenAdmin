using StyreWebAutomation;

namespace SolvikenAdmin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // PlayWrightRunner.InstallPlaywright();
            PlayWrightRunner.Go(log: Console.Write).GetAwaiter().GetResult();
        }
    }
}
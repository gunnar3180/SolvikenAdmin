using StyreWebAutomation;

namespace SolvikenAdmin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // PlayWrightRunner.InstallPlaywright();

            PlayWrightRunner.Init(Console.Write).GetAwaiter().GetResult();
            PlayWrightRunner.LogOnStyreWeb().GetAwaiter().GetResult();
            PlayWrightRunner.DownLoadReports().GetAwaiter().GetResult();
            PlayWrightRunner.LogOffStyreWeb().GetAwaiter().GetResult();
        }
    }
}
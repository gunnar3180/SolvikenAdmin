using StyreWebAutomation;

namespace SolvikenAdmin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // PlayWrightRunner.InstallPlaywright();

            //DoTheJob().GetAwaiter().GetResult();

            var innskudd2024 = InnskuddOverforing.FinnHwInnskudd();
        }

        static async Task DoTheJob()
        {
            await PlayWrightRunner.Init(Console.Write);
            await PlayWrightRunner.LogOnStyreWeb();

            await PlayWrightRunner.DownLoadReports();

            //for (int plassNr = 1; plassNr <= 11; plassNr++)
            //{
            //    string plass = $"D{plassNr:00}";
            //    await PlayWrightRunner.SetVareVariant(plass, "Landopplag");
            //}

            await PlayWrightRunner.LogOffStyreWeb();
        }
    }
}
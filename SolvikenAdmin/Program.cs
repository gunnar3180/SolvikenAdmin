using PlayWrightRunner;

namespace SolvikenAdmin
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // PlayWrightRunner.InstallPlaywright();

            //DownloadSwReports().GetAwaiter().GetResult();

            //InnskuddOverforing.FinnSwAndelsplasserUtenInnskudd();

            //var innskudd2024 = InnskuddOverforing.FinnHwInnskudd();
            //Console.WriteLine();
            //if (innskudd2024.Count() == 0)
            //{
            //    Console.WriteLine("Alle innskudd fra 2024 er allerede overført");
            //    return;
            //}

            //KopierInnskuddTilSw(manuelleInnskudd).GetAwaiter().GetResult();
            //var manuelleInnskudd = GetManuelleInnskudd();

            TestStromSettings().GetAwaiter().GetResult();
        }

        private static async Task TestStromSettings()
        {
            await StyreWebAutomation.Init(Console.Write);
            await StyreWebAutomation.LogOnStyreWeb();

            await StyreWebAutomation.EndrePlassVerdier("5V16", [("Strøm", "true"), ("Strømboks", "boksen min")]);

            await StyreWebAutomation.LogOffStyreWeb();
        }

        private static IEnumerable<(string plassId, int innskudd)> GetManuelleInnskudd()
        {
            return new List<(string plassId, int innskudd)>
            {
                ("1H03",19750),
                ("1H06",14700),
                ("1H27",19750),
                ("1H32",14700),
                ("1H35",19750),
                ("1H42",14700),
                ("2H02-Longside",23250),
                ("2H41",11100),
                ("2H45",27900),
                ("2V03",7750),
                ("2V10",7750),
                ("2V12",7750),
                ("2V35",14700),
                ("2V49",11100),
                ("2V52",14700),
                ("3H07",11100),
                ("3H15",11100),
                ("3H23",8500),
                ("3H30",5000),
                ("3H44",14700),
                ("3V01",15700),
                ("3V05",14700),
                ("3V11",11100),
                ("3V12",7750),
                ("3V13",11100),
                ("3V21",7750),
                ("3V25",14700),
                ("3V26",7750),
                ("3V27",11100),
                ("3V44",11100),
                ("4V15",6500),
                ("4V24",6500),
                ("4V25",6500),
                ("4V26",6500),
                ("4V30",6500),
                ("4V33",6500),
                ("4V41",11100),
                ("4V44",11100),
                ("4V65",7750),
                ("4V68",7750),
                ("4V70",7750),
                ("5H03",11100),
                ("5H05",14700),
                ("5H16",34500),
                ("5H20",25150),
                ("5H21",33800),
                ("5V06",14700),
                ("5V10",12600),
                ("5V14",27900),
                ("5V17",15390),
                ("6H20",22500),
                ("6V14",14700),
                ("6V29",25150),
            };
        }

        private static async Task KopierInnskuddTilSw(IEnumerable<(string plassId, int innskudd)> gamleInnskudd)
        {
            await StyreWebAutomation.Init(Console.Write);
            await StyreWebAutomation.LogOnStyreWeb();

            foreach (var plassInnskudd in gamleInnskudd)
            {
                await StyreWebAutomation.EndrePlassVerdier(plassInnskudd.plassId, [ ("Innskudd", plassInnskudd.innskudd.ToString()) ]);
            }

            await StyreWebAutomation.LogOffStyreWeb();
        }

        static async Task DownloadSwReports()
        {
            await StyreWebAutomation.Init(Console.Write);
            await StyreWebAutomation.LogOnStyreWeb();

            await StyreWebAutomation.DownLoadReports();

            await StyreWebAutomation.LogOffStyreWeb();
        }
    }
}
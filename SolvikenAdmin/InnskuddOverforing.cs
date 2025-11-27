using HavneData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolvikenAdmin
{
    internal static class InnskuddOverforing
    {
        internal static IEnumerable<(string plassId, int innskudd)> FinnHwInnskudd()
        {
            var innskuddListe = new List<(string plassId, int innskudd)>();

            var hwExport = new HavneWebExport().LesData();
            var swExport = new StyreWebExport().LesData();
            var hwAndelsplasser = hwExport.GetAndelsPlasser().ToDictionary(k => k.PlassId, v => v);
            var swAndelsplasser = swExport.GetAndelsPlasser();
            foreach (var swAndelsplass in swAndelsplasser)
            {
                var hwPlassId = GetHwPlassId(swAndelsplass.PlassId);

                if (!hwAndelsplasser.TryGetValue(hwPlassId, out var hwAndelsplass))
                {
                    Console.WriteLine($"   *Plass {swAndelsplass.PlassId} var ikke andelsplass i HavneWeb");
                    continue;
                }

                if (hwAndelsplass.Eier != swAndelsplass.Eier)
                {
                    Console.WriteLine($"   **Plass {swAndelsplass.PlassId} har annen eier i HavneWeb");
                    continue;
                }

                if (hwAndelsplass.Innskudd == 0)
                {
                    Console.WriteLine($"   ****Plass {swAndelsplass.PlassId}: {swAndelsplass.Eier} har 0 i innskudd i HavneWeb");
                    continue;
                }

                if (swAndelsplass.Innskudd > 0)
                {           
                    Console.WriteLine($"   *****Plass {swAndelsplass.PlassId} har allerede innskudd i StyreWeb");
                    continue;
                }

                if (hwAndelsplass.Innskudd == swAndelsplass.Innskudd)
                {
                    Console.WriteLine($"   ******Plass {swAndelsplass.PlassId} innskudd er allerede oppdatert");
                    continue;
                }

                Console.WriteLine($"Oppdaterer plass {swAndelsplass.PlassId}: {swAndelsplass.Eier} innskudd til {hwAndelsplass.Innskudd}");
                innskuddListe.Add((swAndelsplass.PlassId, hwAndelsplass.Innskudd));
            }

            return innskuddListe;
        }

        public static IEnumerable<string> FinnSwAndelsplasserUtenInnskudd()
        {
            var swAndelsplasserUtenInnskudd = new StyreWebExport()
                .LesData()
                .GetAndelsPlasser()
                .Where(p => p.Innskudd <= 0);

            Console.WriteLine("Andelsplasser uten innskudd:");
            foreach (var plass in swAndelsplasserUtenInnskudd)
            {
                Console.WriteLine($"{plass.PlassId}: {plass.Eier}");
            }

            return swAndelsplasserUtenInnskudd.Select(p => p.PlassId);
        }

        private static string GetHwPlassId(string swPlassId)
        {
            switch (swPlassId)
            {
                case "1H34": return "1H35";
                case "1H35": return "1H36";
                case "1H36": return "1H37";
                case "1H37": return "1H38";
                case "1H38": return "1H39";
                case "1H39": return "1H40";
                case "1H40": return "1H41";
                case "1H41": return "1H42";
                case "1H42": return "1H43";

                case "2V14": return "2H14";
                case "2H14": return "2V14";

                default: return swPlassId;
            }
        }
    }
}

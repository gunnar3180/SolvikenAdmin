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
        internal static void Execute()
        {
            var hwExport = new HavneWebExport().LesData();
            var swExport = new StyreWebExport().LesData();
            var hwAndelsplasser = hwExport.GetAndelsPlasser().ToDictionary(k => k.PlassId, v => v);
            var swAndelsplasser = swExport.GetAndelsPlasser();
            foreach (var swAndelsplass in swAndelsplasser)
            {
                if (hwAndelsplasser.TryGetValue(swAndelsplass.PlassId, out var hwAndelsplass))
                {
                    if (hwAndelsplass.Eier == swAndelsplass.Eier && hwAndelsplass.Innskudd != 0)
                    {
                        if (hwAndelsplass.Innskudd != swAndelsplass.Innskudd)
                        {
                            Console.WriteLine($"   Oppdaterer plass {swAndelsplass.PlassId}: {swAndelsplass.Eier} innskudd fra {swAndelsplass.Innskudd} til {hwAndelsplass.Innskudd}");
                            //swAndelsplass.Innskudd = hwAndelsplass.Innskudd;
                            // Legg til i liste
                        }
                        else
                        {
                            Console.WriteLine($"   *Plass {swAndelsplass.PlassId} innskudd er allerede oppdatert"); 
                        }
                    }
                    else
                    {
                        Console.WriteLine($"   *Plass {swAndelsplass.PlassId} har annen eier eller innskudd er 0 i HavneWebExport");
                    }
                }
                else
                {
                    Console.WriteLine($"   *Plass {swAndelsplass.PlassId} var ikke andelsplass i HavneWebExport");
                }
            }
        }
    }
}

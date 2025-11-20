using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavneData
{
    public class BatPlass
    {
        public string PlassId { get; set; }
        public string Eier { get; set; }
        public DateTime UtlevertFra { get; set; }
        public DateTime UtLeidFra { get; set; }
        public string Leier { get; set; }
        public int BatBredde { get; set; }
        public int BatLengde { get; set; }
        public int LysApning { get; set; }
        public int Innskudd { get; set; }
        public bool SesongPlass { get; set; }
        public bool UngdomsPlass { get; set; }
        public bool JollePlass { get; set; }
        public bool LanePlass { get; set; }
        public bool TilLeie { get; set; }
        public bool Reservert { get; set; }
        public bool LandOpplag { get; set; }
        public string Vaktfritak { get; set; }
        public string BatType { get; set; }
        public VareVariant VareVariant { get; set; }

        public int BeregnBatplassAvgift()
        {
            if (UngdomsPlass)
            {
                return 1000;
            }

            double bredde = (double)BatBredde / 100;
            double lengde = (double)BatLengde / 100;
            int beregnetAvgift = (int)Math.Round(bredde * lengde * PrisFaktor(lengde));
            if (beregnetAvgift < 2500)
            {
                beregnetAvgift = 2500;
            }

            var leiePlass = Leier != null;
            return beregnetAvgift + (leiePlass ? LeieTillegg(lengde) : 0);
        }

        public static int PrisFaktor(double lengde)
        {
            return lengde <= 7.2 ? 160 : lengde >= 9.2 ? 200 : 180;
        }

        static int LeieTillegg(double lengde)
        {
            return lengde switch
            {
                double len when len <= 7.0 => 1900,
                double len when len > 7.0 && len <= 8.7 => 2000,
                double len when len > 8.7 && len <= 9.1 => 3000,
                _ => 4000,
            };
        }
    }
}

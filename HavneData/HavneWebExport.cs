using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavneData
{
    public class HavneWebExport : HavneData
    {
        private string hwExportFil;

        protected override SortedDictionary<string, BatPlass> BatPlasser { get; set; }
        public override string Navn { get => "HavneWeb"; }

        public HavneWebExport()
        {
            var workFolder = @"C:\Users\Solviken\OneDrive\Solviken\2025\Havnedatabasen";
            hwExportFil = Path.Combine(workFolder, "SolvikenBtforening_311224_124226.csv");
            BatPlasser = new SortedDictionary<string, BatPlass>();
        }

        protected override HavneData Read(string fromDate = null)
        {
            using (var stream = new FileStream(hwExportFil, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream, Encoding.GetEncoding("ISO-8859-1")))
                {
                    reader.ReadLine();      // Skip header
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var fields = line.Split('\t');
                        var plassId = fields[2].Split(' ')[0];
                        var eier = NavnExcel2StyreWeb(fields[14].Trim());
                        var leier = fields[21].Trim();
                        var innskudd = fields[9];
                        var breddeMeter = fields[4];
                        var lengdeMeter = fields[5];
                        var vaktFritak = fields[11] == "on" ? "Fritak" : null;
                        var batType = leier != string.Empty ? fields[24] : fields[17];
                        int breddeCm = 0;
                        int lengdeCm = 0;
                        int innskuddKr = 0;

                        if (eier == "Solviken Båtforening" || eier == "" || eier == "Ledig")
                        {
                            eier = null;
                        }

                        if (leier == "")
                        {
                            leier = null;
                        }

                        int.TryParse(breddeMeter, out breddeCm);
                        int.TryParse(lengdeMeter, out lengdeCm);
                        int.TryParse(innskudd, out innskuddKr);

                        BatPlasser[plassId] = new BatPlass
                        {
                            PlassId = plassId,
                            Eier = eier,
                            Innskudd = innskuddKr,
                            Leier = leier,
                            BatBredde = breddeCm,
                            BatLengde = lengdeCm,
                            Vaktfritak = vaktFritak,
                            BatType = batType
                        };
                    }
                }
            }

            return this;
        }
    }
}

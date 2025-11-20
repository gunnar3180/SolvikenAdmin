using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavneData
{
    public class LysApninger
    {
        private string lysApningFil;
        private Dictionary<string, int> BatPlasser { get; set; }

        public LysApninger()
        {
            var workFolder = @"C:\Users\Solviken\OneDrive\Solviken\2025\Havnedatabasen";
            lysApningFil = Path.Combine(workFolder, "LysApninger.csv");
            BatPlasser = new Dictionary<string, int>();
        }

        public LysApninger Read()
        {
            using (var stream = new FileStream(lysApningFil, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = new StreamReader(stream, Encoding.GetEncoding("ISO-8859-1")))
                {
                    reader.ReadLine();      // Skip header
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var fields = line.Split(';');
                        var plassId = fields[0];
                        var lysApningString = fields[1];
                        if (double.TryParse(lysApningString, out var lysApning))
                        {
                            BatPlasser[plassId] = (int)Math.Round(lysApning * 100); // Unit = cm
                        }
                    }
                }
            }

            return this;
        }

        public int GetLysApning(string plassId)
        {
            if (BatPlasser.TryGetValue(plassId, out var lysApning))
            {
                return lysApning;
            }

            return -1;
        }
    }
}

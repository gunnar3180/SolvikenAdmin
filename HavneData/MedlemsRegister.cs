using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavneData
{
    public class MedlemsRegister
    {
        private string swFolder;
        private string swEksportFil;
        private string downloadFolder;

        public Dictionary<string, Medlem> Medlemmer { get; private set; }

        public MedlemsRegister()
        {
            swFolder = @"C:\MyLocal\Solviken\FraStyreWeb";
            downloadFolder = @"C:\Users\gunnar\Downloads";
            swEksportFil = "Detaljert_Rapport.csv";
        }

        public MedlemsRegister LesData()
        {
            Medlemmer = new Dictionary<string, Medlem>();
            CopyNewerFile(Path.Combine(downloadFolder, swEksportFil), swFolder);
            using (var reader = new StreamReader(Path.Combine(swFolder, swEksportFil), Encoding.GetEncoding("UTF-8")))
            {
                reader.ReadLine();      // Skip header
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split('\t');
                    if (fields.Length >= 5)
                    {
                        var name = $"{fields[1]} {fields[0]}";
                        var avd = fields[4];
                        var tlf = fields[16];
                        var epost = fields[17];
                        Medlemmer[name] = new Medlem { Navn = name, Avdeling = avd, Tlf = tlf, Epost = epost };
                    }
                }
            }

            return this;
        }

        private void CopyNewerFile(string source, string destination)
        {
            var fileName = Path.GetFileName(source);
            var destinationFile = Path.Combine(destination, fileName);

            if (File.Exists(source))
            {
                File.Delete(destinationFile);
                File.Move(source, destinationFile);
                Console.WriteLine($"Oppdaterte StyreWeb export fil \"{fileName}\" fra Nedlastinger");
            }
        }
    }
}

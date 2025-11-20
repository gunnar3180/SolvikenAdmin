using System.Diagnostics;
using System.Text;

namespace HavneData
{
    public class StyreWebExport : HavneData
    {
        private readonly string downloadFolder;
        private string swMarinaDetaljertFil;
        private string swFramleieFil;
        private readonly string swGruppeVaktplikt;
        private string swExportFolder;
        private readonly List<string> swFritaksGrupper;

        protected override SortedDictionary<string, BatPlass> BatPlasser { get; set; }
        public override string Navn { get => "StyreWeb"; }

        public StyreWebExport()
        {
            downloadFolder = @"C:\Users\gunnar\Downloads";
            var workFolder = @"C:\MyLocal\Solviken";
            swExportFolder = Path.Combine(workFolder, "FraStyreweb");
            swGruppeVaktplikt = "Vaktplikt-2025";
            swFritaksGrupper =
            [
                "Styre",
                "Havneutvalget",
                "Revisorer",
                "Valgkomite",
                "Elektrikergruppa",
                "Diverse_verv",
                "Omsøkt_vaktfritak",
            ];

            BatPlasser = [];
        }

        protected override HavneData Read(string fromDate = null)
        {
            if (fromDate == null)
            {
                CopyNewerFile(Path.Combine(downloadFolder, "Marina_-_Detaljert.csv"), swExportFolder);
                CopyNewerFile(Path.Combine(downloadFolder, "Fremleie_historie.csv"), swExportFolder);

                foreach (var gruppe in swFritaksGrupper.Append(swGruppeVaktplikt))
                {
                    CopyNewerFile(Path.Combine(downloadFolder, $"Gruppe{gruppe}.xlsx"), swExportFolder);
                    ConvertFromXlsx2Csv(Path.Combine(swExportFolder, $"Gruppe{gruppe}.xlsx"));
                }
            }
            else
            {
                if (DateTime.TryParse(fromDate, out var from))
                {
                    var subFolders = Directory.GetDirectories(swExportFolder);
                    var backups = new List<DateTime>();
                    foreach (var folder in subFolders)
                    {
                        var levels = folder.Split('\\');
                        var dateString = levels[^1];
                        if (DateTime.TryParse(dateString, out var date))
                        {
                            backups.Add(date);
                        }
                    }

                    if (backups.Count > 0)
                    {
                        backups.Sort();
                        string date = null;
                        for (int i = backups.Count - 1; i >= 0; i--)
                        {
                            if (backups[i] <= from)
                            {
                                date = backups[i].ToString("d");
                                swExportFolder = Path.Combine(swExportFolder, date);
                                Console.WriteLine($"Leser Styreweb data fra {date}");
                                break;
                            }
                        }

                        if (date == null)
                        {
                            date = backups[0].ToString("d");
                            swExportFolder = Path.Combine(swExportFolder, date);
                            Console.WriteLine($"Fant ikke StyreWeb data for {fromDate}, henter fra eldste backup: {date}");
                        }
                    }
                }
            }

            swMarinaDetaljertFil = Path.Combine(swExportFolder, "Marina_-_Detaljert.csv");
            swFramleieFil = Path.Combine(swExportFolder, "Fremleie_historie.csv");

            var oppmaling = new LysApninger().Read();

            if (File.Exists(swMarinaDetaljertFil))
            {
                using var reader = new StreamReader(swMarinaDetaljertFil, Encoding.GetEncoding("UTF-8"));
                reader.ReadLine();      // Skip header
                string line;
                while ((line = reader.ReadLine()) != null && !line.StartsWith('#'))
                {
                    var fields = line.Split('\t');
                    var plassId = fields[1];
                    var plassType = fields[2];
                    var breddeMeter = fields[3];
                    var lengdeMeter = fields[4];
                    var innskudd = fields[8];
                    DateTime utlevertFra;
                    if (DateTime.TryParse(fields[13], out var time))
                    {
                        utlevertFra = time;
                    }
                    else
                    {
                        utlevertFra = DateTime.Now;
                    }
                    var eier = fields[14];
                    var vareVariant = VareVariant.Create(fields[19]);

                    int breddeCm = 0;
                    int lengdeCm = 0;

                    if (plassType == "Kan ikke brukes")
                    {
                        continue;
                    }

                    if (eier == "Solviken Båtforening" || eier == "" || eier == "Ledig")
                    {
                        eier = null;
                    }

                    if (double.TryParse(breddeMeter, out var bredde))
                    {
                        breddeCm = (int)Math.Round(bredde * 100);
                    }

                    if (double.TryParse(lengdeMeter, out var lengde))
                    {
                        lengdeCm = (int)Math.Round(lengde * 100);
                    }

                    var sesongPlass = plassType == "Sesongplass";
                    var ungdomsPlass = plassType == "Ungdomsplass";
                    var jollePlass = plassType == "Jolleplass";
                    var lanePlass = plassType == "Låneplass";
                    var reservert = plassType == "Reservert";
                    var tilLeie = plassType == "Til leie";
                    var landOpplag = plassType == "Landopplag";

                    int innskuddKr = -1;
                    if (innskudd.Length > 0)
                    {
                        innskuddKr = int.Parse(innskudd.Split(',')[0]);
                    }

                    BatPlasser[plassId] = new BatPlass
                    {
                        PlassId = plassId,
                        Eier = eier,
                        UtlevertFra = utlevertFra,
                        BatBredde = breddeCm,
                        BatLengde = lengdeCm,
                        SesongPlass = sesongPlass,
                        UngdomsPlass = ungdomsPlass,
                        JollePlass = jollePlass,
                        LanePlass = lanePlass,
                        TilLeie = tilLeie,
                        Reservert = reservert,
                        LandOpplag = landOpplag,
                        VareVariant = vareVariant,
                        Innskudd = innskuddKr,
                        LysApning = oppmaling.GetLysApning(plassId)
                    };
                }
            }

            using (var reader = new StreamReader(swFramleieFil, Encoding.GetEncoding("UTF-8")))
            {
                reader.ReadLine();      // Skip header
                string line;
                while ((line = reader.ReadLine()) != null && !line.StartsWith('#'))
                {
                    var fields = line.Split('\t');
                    var plassId = fields[0];
                    var utleidFra = fields[2];
                    var tilDato = fields[3];
                    var leier = fields[4];

                    if (leier != "")
                    {
                        if (BatPlasser.TryGetValue(plassId, out var batPlass))
                        {
                            if (DateTime.TryParse(tilDato, out var sluttDato))
                            {
                                if (sluttDato > DateTime.Now)
                                {
                                    batPlass.Leier = leier;

                                    if (DateTime.TryParse(utleidFra, out var startDato))
                                    {
                                        batPlass.UtLeidFra = startDato;
                                    }

                                    if (!(batPlass.SesongPlass
                                            || batPlass.UngdomsPlass
                                            || batPlass.JollePlass
                                            || batPlass.LanePlass))
                                    {
                                        Console.WriteLine($"Plass {plassId} framleid, feil plasstype");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Plass {plassId} framleie, mangler sluttdato");
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"Plass \"{plassId}\" framleid, finnes ikke i marina");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Plass {plassId} framleie uten leietager");
                    }
                }
            }

            var kjenteFritak = swFritaksGrupper
            .SelectMany(g => LesGruppe(g))
            .OrderBy(g => g.Item1)
            .Distinct(new VervNavnComparer())
            .ToDictionary(key => key.Item1, value => value.Item2);
            var pliktigePlasser = GetAndelsPlasser().Concat(GetSesongPlasser());
            var vaktpliktige = LesGruppe(swGruppeVaktplikt);
            var fritak = pliktigePlasser.Where(p => !vaktpliktige.Any(v => (p.Leier ?? p.Eier) == v.Item1));

            foreach (var plass in fritak)
            {
                var bruker = plass.Leier ?? plass.Eier;
                if (bruker == null)
                {

                }
                if (kjenteFritak.TryGetValue(plass.Leier ?? plass.Eier, out var reason))
                {
                    plass.Vaktfritak = reason;
                }
                else
                {
                    plass.Vaktfritak = "Fritak, ukjent årsak";
                }
            }

            return this;
        }

        private List<(string, string)> LesGruppe(string gruppe)
        {
            var gruppeFil = Path.Combine(swExportFolder, $"Gruppe{gruppe}.csv");
            var medlemmer = new List<(string, string)>();   // (Navn, gruppe)
            if (File.Exists(gruppeFil))
            {
                using var reader = new StreamReader(gruppeFil, Encoding.GetEncoding("UTF-8"));
                reader.ReadLine();      // Skip header
                reader.ReadLine();      // Skip header
                reader.ReadLine();      // Skip header
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split('\t');
                    if (fields[0] == string.Empty)
                    {
                        break;
                    }

                    var navn = $"{fields[1]} {fields[0]}";
                    medlemmer.Add((navn, gruppe));
                }
            }

            return medlemmer;
        }

        private static void ConvertFromXlsx2Csv(string excelFile)
        {
            var folder = Path.GetDirectoryName(excelFile);
            var csvFile = Path.Combine(folder, Path.GetFileNameWithoutExtension(excelFile)) + ".csv";

            if (File.GetLastWriteTime(excelFile) > File.GetLastWriteTime(csvFile))
            {
                string scriptName = @"C:\MyLocal\Solviken\xlsx2csv.vbs"; // full path to script
                ProcessStartInfo ps = new()
                {
                    FileName = "cscript.exe",
                    Arguments = $"{scriptName} {excelFile} {csvFile}",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                var process = Process.Start(ps);
                process.WaitForExit();
                process.Close();
            }
        }

        private static void CopyNewerFile(string source, string destination)
        {
            var fileName = Path.GetFileName(source);
            var destinationFile = Path.Combine(destination, fileName);

            if (File.Exists(source))
            {
                if (File.Exists(destinationFile))
                {
                    File.Delete(destinationFile);
                }

                File.Move(source, destinationFile);
                Console.WriteLine($"Oppdaterte StyreWeb export fil \"{fileName}\" fra Nedlastinger");

                var date = DateTime.Now.ToString("d");
                var backupPath = Path.Combine(destination, date);
                Directory.CreateDirectory(backupPath);
                File.Copy(destinationFile, Path.Combine(backupPath, fileName), true);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavneData
{
    public abstract class HavneData
    {
        protected abstract SortedDictionary<string, BatPlass> BatPlasser { get; set; }
        protected abstract HavneData Read(string fromDate = null);

        public abstract string Navn { get; }

        public string PlassPrefix { get; set; }

        public HavneData LesData(string prefix = null, string fromDate = null)
        {
            PlassPrefix = prefix;
            Read(fromDate);

            if (prefix != null)
            {
                var excluded = BatPlasser.Keys.Where(p => !p.StartsWith(prefix)).ToList();
                foreach (var plassId in excluded)
                {
                    BatPlasser.Remove(plassId);
                }
            }

            return this;
        }

        public BatPlass GetBatPlass(string plassId)
        {
            if (BatPlasser.TryGetValue(plassId, out var plass))
            {
                return plass;
            }

            return null;
        }

        public List<BatPlass> GetAllePlasser()
        {
            return BatPlasser.Values.ToList();
        }

        public List<BatPlass> GetAndelsPlasser()
        {
            return BatPlasser.Values.Where(v => v.Eier != null)
            .Except(GetLandOpplagsPlasser())
            .ToList();
        }

        public List<BatPlass> GetSesongPlasser()
        {
            return BatPlasser.Values.Where(v => v.SesongPlass).ToList();
        }

        public List<BatPlass> GetUngdomsPlasser()
        {
            return BatPlasser.Values.Where(v => v.UngdomsPlass).ToList();
        }

        public List<BatPlass> GetJollePlasser()
        {
            return BatPlasser.Values.Where(v => v.JollePlass).ToList();
        }

        public List<BatPlass> GetLanePlasser()
        {
            return BatPlasser.Values.Where(v => v.LanePlass).ToList();
        }

        public List<BatPlass> GetTilLeiePlasser()
        {
            return BatPlasser.Values.Where(v => v.TilLeie).ToList();
        }

        public List<BatPlass> GetReservertePlasser()
        {
            return BatPlasser.Values.Where(v => v.Reservert).ToList();
        }

        public List<BatPlass> GetLandOpplagsPlasser()
        {
            return BatPlasser.Values.Where(v => v.LandOpplag).ToList();
        }

        public List<BatPlass> GetLedigePlasser()
        {
            return BatPlasser.Values
            .Except(GetLandOpplagsPlasser())
            .Except(GetAndelsPlasser())
            .Except(GetSesongPlasser())
            .Except(GetUngdomsPlasser())
            .Except(GetJollePlasser())
            .Except(GetReservertePlasser())
            .ToList();
        }

        protected string NavnExcel2StyreWeb(string navn)
        {
            switch (navn)
            {
                case "Hilde Risan / Christian Schønfeldt":
                    return "Hilde Risan";
                case "Simen T. Aasheim":
                    return "Simen Aasheim";
                case "Jan Robert  Andersen":
                    return "Jan Robert Andersen";
                case "Øyvind Tellefsen (reservert)":
                    return "Øyvind Tellefsen";
                case "Joakim Haugen":
                    return "Joakim Mordt Haugen";
                case "Joackim H  Hansen":
                    return "Joackim H. Hansen";
                case "rune kr.  Stålstrøm":
                    return "Rune Stålstrøm";
                case "Harald Olsen (Æ)":
                    return "Harald Olsen";
                case "Jan di Leggerini":
                    return "Jan Di Leggerini";
                case "Roald Bartholdsen (Æ)":
                    return "Roald Bartholdsen";
                case "Bexrud Bil AS":
                    return "Bexrud Bil AS Bexrud Bil AS";
                case "Morten  Gregersen":
                    return "Morten Gregersen";
                case "Anders L. S. Herlofsen":
                    return "Anders Herlofsen";
                case "Gerhard  Bagge":
                    return "Gerhard Bagge";

                default:
                    return navn;
            }
        }

        public (int, string) BeregnInnskudd(string plassId)
        {
            var plass = GetBatPlass(plassId);
            if (plass != null)
            {
                double lengde = (double)plass.BatLengde / 100;
                switch (lengde)
                {
                    case double len when len <= 5.4:
                        return (7750, "A");
                    case double len when len <= 7.0:
                        return (11100, "B");
                    case double len when len <= 8.7:
                        return (14700, "C");
                    case double len when len <= 9.1:
                        return (19750, "D");
                    case double len when len <= 10.0:
                        return (22500, "E");
                    case double len when len <= 10.6:
                        return (25150, "F");
                    case double len when len <= 11.8:
                        return (27900, "G");
                    case double len when len <= 12.4:
                        return (34500, "H");
                    default:
                        return (45500, "L");
                }
            }

            return (-1, "X");
        }
    }
}

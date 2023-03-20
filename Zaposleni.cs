using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ZadatakRareCrew
{
    public class Zaposleni
    {

        public Zaposleni(string ime, double ukupniRadniSati)
        {
            Ime = ime;
            UkupniRadniSati = ukupniRadniSati;
        }

        public Zaposleni()
        {
        }

        [JsonProperty("EmployeeName")]
    public string Ime { get; set; }

    [JsonProperty("StarTimeUtc")]
    public DateTime DatumPocetka { get; set; }

    [JsonProperty("EndTimeUtc")]
    public DateTime DatumZavrsetka { get; set; }

    [JsonProperty("EntryNotes")]
    public string Beleske { get; set; }

    [JsonProperty("DeletedOn")]
    public DateTime? Izbrisano { get; set; }

    public TimeSpan IntervalRada { get; set; }

    public double BrRadnihSati { get; set; }

    public double UkupniRadniSati { get; set; }

    }
}

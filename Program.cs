using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Net.Http;
using static ZadatakRareCrew.Zaposleni;
using Aspose.Html;
using Aspose.Html.Dom;
using System.Text;
using System.Web.Helpers;
using FastReport;
using System.Windows.Forms;
using FastReport.DataVisualization.Charting;
using Chart = FastReport.DataVisualization.Charting.Chart;
using OxyPlot.Series;
using Series = FastReport.DataVisualization.Charting.Series;
using OxyPlot;
using DataPoint = FastReport.DataVisualization.Charting.DataPoint;

namespace ZadatakRareCrew
{
    internal class Program
    { 

        public static async Task<List<Zaposleni>> vratiSveZaposlene()
        {
            using (var httpKlijent = new HttpClient())
            {
                var zahtev = await httpKlijent.GetAsync("https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==");
                var sadrzajJSONfajla = await zahtev.Content.ReadAsStringAsync();
                var listaZaposlenih = JsonConvert.DeserializeObject<List<Zaposleni>>(sadrzajJSONfajla);
                return listaZaposlenih;
            }
        }

        public static async Task<List<Zaposleni>> napraviNovuListu()
        {
            var lista = await vratiSveZaposlene();
            var grupeZaposlenih = lista.GroupBy(z => z.Ime);

            var novaLista = new List<Zaposleni>();
            foreach (var grupa in grupeZaposlenih) {

                string Ime = grupa.Key;
                double ukupnoSati = 0;

                foreach (Zaposleni z in grupa)
                {
                    TimeSpan IntervalRada = z.DatumZavrsetka.Subtract(z.DatumPocetka);
                    z.BrRadnihSati = IntervalRada.TotalHours;
                    ukupnoSati += z.BrRadnihSati;
     
                }

                Zaposleni radnik = new Zaposleni();
                radnik.Ime = Ime;
                radnik.UkupniRadniSati = ukupnoSati;

                novaLista.Add(radnik);

            }
            return novaLista;

        }

        public static async Task ispisiListuZaposlenih()
        {
            //var lista = await napraviNovuListu();
            var lista1 = await vratiSveZaposlene();

            foreach (var radnik in lista1)
            {
                TimeSpan IntervalRada = radnik.DatumZavrsetka.Subtract(radnik.DatumPocetka);
                radnik.BrRadnihSati = IntervalRada.TotalHours;

                Console.WriteLine("Ime: " + radnik.Ime);
                Console.WriteLine("Broj radnih sati: " + radnik.BrRadnihSati);
                
            }

        }


        public static async Task napraviPieChart()
        {
            var listaZaposlenih = await napraviNovuListu();
            double totalRadniSati = 0;

            var dijagram = new Chart();
            dijagram.Size = new Size(600, 400);
            dijagram.Titles.Add("Ukupan broj radnih sati zaposlenih izrazen u procentima");

            var chartArea = new ChartArea();
            dijagram.ChartAreas.Add(chartArea);

            var serije = new Series();
            serije.ChartType = SeriesChartType.Pie;
            serije["PieLabelStyle"] = "Outside";

            foreach (Zaposleni z in listaZaposlenih)
            {
                totalRadniSati += z.UkupniRadniSati;
            }

            foreach (Zaposleni zap in listaZaposlenih)
            {

                if (zap.Ime == null)
                {
                    zap.Ime = "/";
                }

                double procenat = zap.UkupniRadniSati / totalRadniSati * 100;
                serije.Points.AddXY(zap.Ime, procenat);

                foreach (DataPoint point in serije.Points)
                {
                    serije.Label = "#VALX - #PERCENT{P0}";
                    point.LabelForeColor = Color.Black;
                    point.LabelBackColor = Color.White;
                    point.Label = string.Format("{0} - {1:0.00}%", point.AxisLabel, point.YValues[0]);

                }

            }

            dijagram.ApplyPaletteColors();
            dijagram.Series.Add(serije);
            dijagram.SaveImage("piechart.png", ChartImageFormat.Png);

        }


        static async Task Main(string[] args)
            
        {
            //Console.WriteLine("Hello, World!");
            //await ispisiListuZaposlenih();

            var lista = await napraviNovuListu();
            lista.Sort((a, b) => b.UkupniRadniSati.CompareTo(a.UkupniRadniSati));

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html>");
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("<title>Tabela zaposlenih</title>");
            sb.Append("<style>table, th, td { border: 1px solid black; border-collapse: collapse; text-align: center; }</style>");
            sb.Append("<style> th, td { padding: 4px; }</style>");
            sb.Append("<style> td { min-width: 25ch; } </style>");
            sb.Append("<style>html { margin-left: 25px; margin-right: 67%; }</style>");
            sb.Append("</head>");

            sb.Append("<body>");
            sb.Append("<h1>Svi zaposleni</h1>");
            sb.Append("<style>h1 { color: #292727; font-family: 'Lato', sans-serif; font-size: 20px; font-weight: 300;} </style>");
            sb.Append("<hr>");
            sb.Append("<table>");
            sb.Append("<thead><tr><th>IME</th><th>BROJ RADNIH SATI</th></tr></thead>");

            sb.Append("<tbody>");
            foreach (var radnik in lista)
            {
                sb.Append("<tr>");
     
                if (radnik.UkupniRadniSati < 100)
                {
                    sb.AppendFormat("<td style='background-color: #F8C471; '>{0}</td>", radnik.Ime);
                    sb.AppendFormat("<td style='background-color: #F8C471; '>{0}</td>", (int)radnik.UkupniRadniSati);

                }
                else
                {
                    sb.AppendFormat("<td>{0}</td>", radnik.Ime);
                    sb.AppendFormat("<td>{0}</td>", (int)radnik.UkupniRadniSati);
                }

                sb.Append("</tr>");
            }
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</body>");

            var html = sb.ToString();
            File.WriteAllText("zaposleni.html", html);
            Console.WriteLine("Kreiran je HTML fajl!");

            await napraviPieChart();
            Console.WriteLine("PNG je kreiran!");
            Console.ReadLine();

        }
    }
}
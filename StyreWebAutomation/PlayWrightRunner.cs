using Microsoft.Playwright;
using System.Reflection.Emit;

namespace StyreWebAutomation
{
    public static class PlayWrightRunner
    {
        private static IPlaywright _playwright;
        private static IBrowser _browser;
        private static IPage _page;

        private static readonly string _brukerNavn = "gb3180@online.no";
        private static readonly string _kodetPassord = "Cf77D57G1vfiv1S";
        private static readonly string _loginUrl = "https://portal.styreweb.com/account/login.aspx";
        private static readonly string _hjemUrl = "https://solvikenbatforening.portal.styreweb.com/secure/";
        private static readonly string _marinaUrl = _hjemUrl + "archive/marina.aspx";
        private static readonly string _framleieUrl = _hjemUrl + "archive/marinasublet.aspx";
        private static readonly string _medlemmerUrl = _hjemUrl + "Members.aspx";
        private static readonly string _downloadFolder = $@"C:\Users\{Environment.UserName}\Downloads";
        private static Action<string> _log;

        public static void InstallPlaywright()
        {
            Program.Main(new[] { "install" });
        }

        public static async Task Init(Action<string> log)
        {
            _log = log;

            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync();
            _page = await _browser.NewPageAsync();
        }
        public static async Task LogOnStyreWeb()
        {
            _log("Logger inn på StyreWeb...");
            await _page.GotoAsync(_loginUrl);
            await _page.GetByLabel("Brukernavn").FillAsync(_brukerNavn);
            await _page.GetByLabel("Passord").FillAsync(DecodeString(_kodetPassord));
            await _page.GetByRole(AriaRole.Button).ClickAsync();
            await _page.WaitForURLAsync(_hjemUrl);
            _log("OK\n");
        }

        public static async Task LogOffStyreWeb()
        {
            await _page.ScreenshotAsync(new PageScreenshotOptions { Path = @"C:\MyLocal\Solviken\screenshot.png" });
            await _browser.DisposeAsync();
        }

        public static async Task DownLoadReports()
        {
            await DownloadMarina();
            await DownloadFramleie();
            await DownloadMedlemmer();
        }

        static async Task DownloadMarina()
        {
            await _page.GotoAsync(_marinaUrl);
            await _page.Locator("#cboAction").SelectOptionAsync(new SelectOptionValue { Label = "Marina - Detaljert" });
            await VisRapportOgLastNed(Path.Combine(_downloadFolder, "Marina_-_Detaljert.csv"));
        }

        static async Task DownloadFramleie()
        {
            await _page.GotoAsync(_framleieUrl);
            await VisRapportOgLastNed(Path.Combine(_downloadFolder, "Fremleie_historie.csv"));
        }

        static async Task DownloadMedlemmer()
        {
            await _page.GotoAsync(_medlemmerUrl);
            await _page.Locator("#Main_cboAction").SelectOptionAsync(new SelectOptionValue { Label = "Detaljert Rapport" });
            await VisRapportOgLastNed(Path.Combine(_downloadFolder, "Detaljert_Rapport.csv"));
        }

        public static async Task EndrePlassVerdier(string plass, List<(string field, string value)> verdier)
        {
            if (!await FinnPlass(plass))
            {
                 return;
            }

            var endreButton = _page.Locator("input[type='button'][value='Endre']");
            await endreButton.ClickAsync();

            foreach (var verdi in verdier)
            {
                var verdiId = GetEditFieldId(verdi.field);
                if (verdiId == null)
                {
                    _log($"{plass}: Fant ikke felt {verdi.field}\n");
                    continue;
                }

                await _page.Locator($"#{verdiId}").FillAsync(verdi.value);
                _log($"{plass}: {verdi.field} = {verdi.value}\n");
            }

            var lagreButton = _page.Locator("input[type='submit'][value='Lagre']");
            await lagreButton.ClickAsync();
        }

        private static async Task<bool> FinnPlass(string plass)
        {
            await _page.GotoAsync(_marinaUrl);
            var inputField = _page.Locator("#LeftNavBar_txtSerieNr");
            await inputField.FillAsync(plass);
            await _page.ClickAsync("button:has-text(\"Søk\")");

            var tableLocator = _page.Locator("sw-panel#pnlMain table#Main_grdv");
            try
            {
                await tableLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
            }
            catch (Exception)
            {
                _log($"Fant ikke båtplass {plass}\n");
                return false;
            }

            //Console.WriteLine("Table with ID 'Main_grdv' exists inside <sw-panel>.");
            await _page.Locator("a", new PageLocatorOptions { HasTextString = plass }).First.ClickAsync();
            return true;
        }


        public static async Task SetVareVariant(string plass, string vareVariant)
        {
            if (!await FinnPlass(plass))
            {
                _log("Fant ikke plass " + plass + "\n");
                return;
            }

            var tildelt = _page.Locator("td.GridView-MainNavigationCell a");
            try
            {
                await tildelt.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
            }
            catch (Exception)
            {
                _log("Plass " + plass + " har ingen tildeling\n");
                return;
            }

            await tildelt.First.ClickAsync();
            var endreButton = _page.Locator("input[type='button'][value='Endre']");
            await endreButton.ClickAsync();

            await _page.Locator("#Main_detailGenericArchiveMember_cboProductVariant")
                .SelectOptionAsync(new SelectOptionValue { Label = "Landopplag" });

            var lagreButton = _page.Locator("input[type='submit'][value='Lagre']");
            await lagreButton.ClickAsync();
            _log($"Endret varevariant for plass {plass} til {vareVariant}\n");
        }

        static string GetEditFieldId(string fieldName)
        {
            switch (fieldName)
            {
                case "Bredde":
                    return "Main_details_txtUserDefFlt1";
                case "Lengde":
                    return "Main_details_txtUserDefFlt2";
                case "Dybde":
                    return "Main_details_txtUserDefFlt3";
                case "Høyde":
                    return "Main_details_txtUserDefFlt4";
                case "Innskudd":
                    return "Main_details_txtPrice";
            }

            return null;
        }

        private static async Task VisRapportOgLastNed(string savePath)
        {
            _log("Genererer rapport...");
            var newPageTask = _page.Context.WaitForPageAsync();
            await _page.ClickAsync("button:has-text(\"Vis\")");
            var newTab = await newPageTask;
            await newTab.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            _log("Ferdig...");

            _log("Laster ned...");
            var downloadTask = newTab.WaitForDownloadAsync();
            await newTab.ClickAsync("button:has-text(\"Eksporter som csv fil\")");
            var download = await downloadTask;
            await download.SaveAsAsync(savePath);
            _log(savePath + "\n");
            await newTab.CloseAsync();
        }

        // Brukt høst 25 for å opprette opplagsplasser på land. Kan brukes som mal for å opprette båtplasser også.
        public static async Task LagOpplagsplass(IPage page, string marinaUrl, string felt, int nummer, Action<string> log)
        {
            await page.GotoAsync(marinaUrl);
            await page.ClickAsync("button:has-text(\"Lag ny\")");

            // Set inn seksjon
            var seksjon = $"Land {felt}";
            var dropdown = page.Locator("select[title='Seksjon']");
            await dropdown.SelectOptionAsync(new SelectOptionValue { Label = seksjon });

            var inputField = page.Locator("#Main_details_txtSortOrder");
            await inputField.FillAsync(nummer.ToString());

            inputField = page.Locator("#Main_details_txtGenericArchiveShortName");
            await inputField.FillAsync($"{felt}{nummer.ToString("d2")}");

            dropdown = page.Locator("select[title='Type']");
            await dropdown.SelectOptionAsync(new SelectOptionValue { Label = "Landopplag" });

            inputField = page.Locator("#Main_details_txtUserDefFlt1");
            await inputField.FillAsync("5");

            inputField = page.Locator("#Main_details_txtUserDefFlt2");
            await inputField.FillAsync("10");

            dropdown = page.Locator("select[title='Vare']");
            await dropdown.SelectOptionAsync(new SelectOptionValue { Label = "Båtplass Avgift" });

            var checkbox = page.Locator("#Main_details_ctl23");
            await checkbox.CheckAsync();

            checkbox = page.Locator("#Main_details_ctl24");
            await checkbox.CheckAsync();

            await page.GetByText("Opprett").ClickAsync();

            await page.GetByText("<< Marina").ClickAsync();

            log($"Opprettet opplagsplass {felt}{nummer.ToString("d2")} på felt {seksjon}\n");
        }

        private static string DecodeString(string coded)
        {
            string alfaNum = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            int maxPos = alfaNum.Length - 1;
            var decoded = new List<char>();

            foreach (var c in coded)
            {
                int index = alfaNum.IndexOf(c);
                int newIndex = index - 52;
                if (newIndex < 0)
                {
                    newIndex += maxPos;
                }
                newIndex %= maxPos;
                decoded.Add(alfaNum[newIndex]);
            }

            return new string(decoded.ToArray());
        }
    }
}

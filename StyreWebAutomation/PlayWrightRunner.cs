using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Microsoft.Playwright;

namespace StyreWebAutomation
{
    internal static class PlayWrightRunner
    {
        static void InstallPlaywright()
        {
            Program.Main(new[] { "install" });
        }

        public static async Task Go(Action<string> log)
        {
            var brukerNavn = "gb3180@online.no";
            var kodetPassord = "Cf77D57G1vfiv1S";
            var loginUrl = "https://portal.styreweb.com/account/login.aspx";
            var hjemUrl = "https://solvikenbatforening.portal.styreweb.com/secure/";
            var marinaUrl = hjemUrl + "archive/marina.aspx";
            var framleieUrl = hjemUrl + "archive/marinasublet.aspx";
            var medlemmerUrl = hjemUrl + "Members.aspx";
            var downloadFolder = @"C:\Users\gunnar\Downloads";

            using var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync();
            var page = await browser.NewPageAsync();
            await page.GotoAsync(loginUrl);
            await page.GetByLabel("Brukernavn").FillAsync(brukerNavn);
            await page.GetByLabel("Passord").FillAsync(DecodeString(kodetPassord));
            await page.GetByRole(AriaRole.Button).ClickAsync();
            await page.WaitForURLAsync(hjemUrl);
            log("Logget inn på StyreWeb\n");

            await page.GotoAsync(marinaUrl);

            await page.Locator("#cboAction").SelectOptionAsync(new SelectOptionValue { Label = "Marina - Detaljert" });
            await VisRapportOgLastNed(page, Path.Combine(downloadFolder, "Marina_-_Detaljert.csv"), log);

            await page.GotoAsync(framleieUrl);
            await VisRapportOgLastNed(page, Path.Combine(downloadFolder, "Fremleie_historie.csv"), log);

            await page.GotoAsync(medlemmerUrl);
            await page.Locator("#Main_cboAction").SelectOptionAsync(new SelectOptionValue { Label = "Detaljert Rapport" });
            await VisRapportOgLastNed(page, Path.Combine(downloadFolder, "Detaljert_Rapport.csv"), log);

            //await page.ScreenshotAsync(new PageScreenshotOptions { Path = @"C:\MyLocal\Solviken\screenshot.png" });
            await browser.DisposeAsync();
        }

        static async Task VisRapportOgLastNed(IPage page, string savePath, Action<string> log)
        {
            log("Genererer rapport...");
            var newPageTask = page.Context.WaitForPageAsync();
            await page.ClickAsync("button:has-text(\"Vis\")");
            var newTab = await newPageTask;
            await newTab.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            log("Ferdig...");

            log("Laster ned...");
            var downloadTask = newTab.WaitForDownloadAsync();
            await newTab.ClickAsync("button:has-text(\"Eksporter som csv fil\")");
            var download = await downloadTask;
            await download.SaveAsAsync(savePath);
            log(savePath + "\n");
            await newTab.CloseAsync();
        }

        static string DecodeString(string coded)
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

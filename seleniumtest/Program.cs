using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;


namespace seleniumtest
{
    class Program
    {
        static void Main(string[] args)
        {
            var driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://caribetecno.campodata.com");
            var userEmail = driver.FindElement(By.Id("user_email"));
            userEmail.Clear();
            userEmail.SendKeys("casey@lionsgatehomesllc.com");
            var userPassword = driver.FindElementById("user_password");
            userPassword.Clear();
            userPassword.SendKeys("lionsgate123");
            driver.FindElementByClassName("btn").Click();
            driver.FindElement(By.CssSelector("a[href*='/tasks']")).Click();
            var pageLimit = driver.FindElementByName("tasks_length");
            var pageLimitSelect = new SelectElement(pageLimit);
            pageLimitSelect.SelectByValue("100");
            Thread.Sleep(1000);
            var paginateButtons = driver.FindElementsByClassName("paginate_button");
            var numberPages = paginateButtons.Select(x => x.Text).Where(x => x.ToLower() != "previous" && x.ToLower() != "next").Distinct().ToList();
            foreach (var page in numberPages) {
                if (page != "1") {
                    //click the next button
                }

                var detailButtons = driver.FindElementsByClassName("details-control");
                foreach (var button in detailButtons) {
                    try
                    {
                        button.Click();
                        //needs to wait for the previous click to load before a new one can kick off or it will skip over it.
                        Thread.Sleep(200);
                    }
                    catch (Exception) { /*This will be expected since they have two elements with that class that are not clickable*/ }
                }
                var headerRows = new List<CompoDataRow>();
                var headers = driver.FindElementsByXPath("//tr[@id]");
                foreach (var row in headers) {
                    var phonePattern = new Regex("[\n<p>â˜ ☏]");
                    var addressPattern = new Regex("[â˜ž☞]");
                    var headerRow = new CompoDataRow();
                    var id = row.GetAttribute("id");
                    headerRow.Id = Convert.ToInt32(id);

                    var rowCells = driver.FindElementsByXPath("//tr[@id=" + headerRow.Id.ToString() + "]/td");
                    var caseText = rowCells[1].Text.Replace("add comment", "").Trim().Split('-');
                    headerRow.PRGNumber = caseText[0].Trim();
                    headerRow.Case = rowCells[1].Text.Replace("add comment", "").Trim().Replace(headerRow.PRGNumber + " - ", "");
                    var descriptiontext = rowCells[2].Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    headerRow.Phone = phonePattern.Replace(descriptiontext[0], "");
                    headerRow.Address = addressPattern.Replace(descriptiontext[1], "").Trim();
                    headerRow.City = rowCells[3].Text;
                    headerRow.SubContractor = rowCells[4].Text;
                    headerRow.Team = rowCells[5].Text;
                    headerRow.AssignedAt = rowCells[6].Text;
                    headerRow.DaysSinceAssigned = rowCells[7].Text;
                    headerRow.CompletedAt = rowCells[8].Text;
                    headerRow.Status = rowCells[9].Text;
                    headerRows.Add(headerRow);
                }
            }
        }
    }
}

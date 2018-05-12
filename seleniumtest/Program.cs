using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
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
            var headerRows = new List<CompoDataRow>();
            foreach (var page in numberPages) {
                if (page != "1") {
                    paginateButtons = driver.FindElementsByClassName("paginate_button");
                    var pageButton = paginateButtons.Where(x => x.Text == page).LastOrDefault();
                    pageButton.Click();
                    Thread.Sleep(1000);
                }

                var detailButtons = driver.FindElementsByClassName("details-control");
                foreach (var button in detailButtons) {
                    try
                    {
                        button.Click();
                        Thread.Sleep(200);
                    }
                    catch (Exception) { }
                }

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
                    headerRow.Details = new List<DetailRow>();
                    var detailRows = driver.FindElementsByXPath("//table[contains(@id, 'subtasks-table-" + headerRow.Id + "')]//tr");
                    foreach (var detailRow in detailRows)
                    {
                        if (!detailRow.Equals(detailRows.LastOrDefault()))
                        {
                            var numberPattern = new Regex(@"#(\d+)");
                            var quantityPattern = new Regex(@"Qty: (\d+)");
                            var headerDetailRow = new DetailRow();
                            var children = detailRow.FindElements(By.CssSelector("td"));
                            var descriptionLine = children[1].Text;
                            headerDetailRow.Number = Convert.ToInt32(numberPattern.Match(descriptionLine).Groups[1].Value);
                            descriptionLine = descriptionLine.Replace("#" + headerDetailRow.Number, "").Trim();
                            headerDetailRow.Quantity = Convert.ToInt32(quantityPattern.Match(descriptionLine).Groups[1].Value);
                            descriptionLine = descriptionLine.Replace("Qty: " + headerDetailRow.Quantity, "");
                            headerDetailRow.Description = descriptionLine.Replace(".", "").Trim().Replace("&amp;", "&");
                            headerDetailRow.Team = children[2].Text.Replace("Team:", "").Trim();
                            headerDetailRow.Status = children[3].Text.Trim();
                            headerRow.Details.Add(headerDetailRow);
                        }
                    }
                    headerRows.Add(headerRow);
                }
            }
            driver.Quit();

            var csv = new StringBuilder();
            csv.AppendLine("\"Id\",\"Case\",\"Phone\",\"Address\",\"City\",\"Sub Contractor\",\"Team\",\"Assigned At\",\"Days Since Assigned\",\"Completed At\",\"Status\"");
            foreach (var project in headerRows)
            {
                csv.AppendLine("\"" + project.PRGNumber + "\",\"" + project.Case + "\",\"" + project.Phone + "\",\"" + project.Address + "\",\"" + project.City + "\",\"" + project.SubContractor + "\",\"" + project.Team + "\",\"" + project.AssignedAt + "\",\"" + project.DaysSinceAssigned + "\",\"" + project.CompletedAt + "\",\"" + project.Status + "\"");
                csv.AppendLine("\"Tasks\",\"Number\",\"Description\",\"Quantity\",\"Team\",\"Status\"");
                foreach (var detail in project.Details)
                {
                    csv.AppendLine("\"\",\"" + detail.Number.ToString() + "\",\"" + detail.Description + "\",\"" + detail.Quantity.ToString() + "\",\"" + detail.Team + "\",\"" + detail.Status + "\"");
                }
            }
            File.WriteAllText(@"c:\test\output.csv", csv.ToString());
        }
    }
}

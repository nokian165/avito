using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace topface.Models
{
    public class AvitoParser
    {
        private string AvitoUrl = "https://www.avito.ru";


        public Bitmap AvitoParserPhoneImage(string link)
        {
            IWebDriver driver = new ChromeDriver(HttpContext.Current.Server.MapPath("~/WebDriver/"));
            driver.Navigate().GoToUrl(AvitoUrl + link);

            driver.FindElement(By.ClassName("item-phone-button-sub-text")).Click();
            Thread.Sleep(1000);

            driver.FindElement(By.CssSelector("div.b-popup.item-popup > span.close")).Click();


            var scrElement = driver.FindElement(By.XPath("//div[contains(@class, 'item-phone-number') and contains(@class, 'js-item-phone-number')]/a/img")).GetAttribute("src");
            var base64Image = scrElement.Split(',')[1];


            var bytes = Convert.FromBase64String(base64Image);
            Bitmap bmp;
            using (var imageFile = new MemoryStream(bytes))
            {
                bmp = new Bitmap(imageFile);
            }

            return bmp;
        }

    }
}
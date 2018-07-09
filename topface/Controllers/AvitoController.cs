using System; 
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using topface.Models;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using HtmlDocument = System.Windows.Forms.HtmlDocument;

namespace topface.Controllers
{
    public class AvitoController : Controller
    {
        private ApplicationDbContext _context;
        public AvitoController()
        {
            _context = new ApplicationDbContext();
        }
        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        public ActionResult Index(string startId) //int startId, int finishId
        {


            HtmlAgilityPack.HtmlWeb web = new HtmlWeb();

            for (int i = 0; i < 11; i++)
            {
                HtmlAgilityPack.HtmlDocument doc = web.Load("https://www.avito.ru/moskva/kvartiry/prodam/1-komnatnye/vtorichka?p=" + i + "&user=1&view=list");


                //foreach (HtmlNode row in doc.DocumentNode.SelectNodes("//div[@class='item item_list js-catalog-item-enum clearfix item-highlight cat_24']"))
                //foreach (HtmlNode row in doc.DocumentNode.SelectNodes("//div[@class='item item_list js-catalog-item-enum clearfix c-b-0 cat_24']"))
                foreach (HtmlNode row in doc.DocumentNode.SelectNodes("//div[contains(@class, 'js-catalog-item-enum') and contains(@class, 'clearfix')]"))

                {

                    // ссылка на подробную объяву
                    var link = row.SelectSingleNode("div[@class='title description-title']/h3[@class='h3 fader description-title-h3']/a").GetAttributeValue("href", "");
                    HtmlAgilityPack.HtmlDocument childDoc = web.Load("https://www.avito.ru" + link);

                    var opisanie = childDoc.DocumentNode.SelectSingleNode("//div[@class = 'item-description-text']/p").InnerHtml;

                    // id
                    var adId = row.Id;
                    var sAdId = adId.Replace("i", "");

                    // цена
                    var price = row.SelectSingleNode("div[@class='price']/p").InnerText;

                    var bPrice = new StringBuilder();
                    foreach (var p in price)
                    {
                        if (p == '&')
                            break;

                        if (p == '0' | p == '1' | p == '2' | p == '3' | p == '4' | p == '5' | p == '6' | p == '7' | p == '8' | p == '9')
                            bPrice.Append(p);
                    }
                    var sPrice = int.Parse(bPrice.ToString());

                    // проверка существующей записи в базе
                    var infoAds = _context.InfoAds.FirstOrDefault(m => m.AdId == sAdId);

                    if (infoAds == null)
                    {

                        //получить номер телефона
                        var avitoParser = new AvitoParser();

                        var img = avitoParser.AvitoParserPhoneImage(link);

                        var recognizer = new PhoneRecognizer(System.Web.HttpContext.Current.Server.MapPath("~/Content/digits/"));

                        var sTel = recognizer.Recognize(img);

                        var tel = long.Parse(sTel.Replace("-", string.Empty));

                        // площадь
                        var area = row.SelectSingleNode("div[@class='params clearfix']/div[@class='param area']").InnerText;

                        var bArea = new StringBuilder();

                        foreach (var a in area)
                        {
                            if (a == ' ')
                                break;

                            if (a == '.')
                                bArea.Append(',');
                            else
                                bArea.Append(a);
                        }
                        var sArea = float.Parse(bArea.ToString());

                        // этаж и этажность
                        var floor = row.SelectSingleNode("div[@class='params clearfix']/div[@class='param floor']").InnerText;

                        // этажность
                        var bFloors = new StringBuilder();
                        var indexFloor = floor.IndexOf("/");
                        var floorToGetFloors = floor.Substring(indexFloor + 1);

                        foreach (var f in floorToGetFloors)
                        {
                            if (f == ' ')
                                break;

                            bFloors.Append(f);
                        }
                        var sFloors = int.Parse(bFloors.ToString());

                        // этаж
                        var bFloor = new StringBuilder();

                        foreach (var f in floor)
                        {
                            if (f == '/')
                                break;

                            if (f == '0' | f == '1' | f == '2' | f == '3' | f == '4' | f == '5' | f == '6' | f == '7' | f == '8' | f == '9')
                                bFloor.Append(f);
                        }
                        var sFloor = int.Parse(bFloor.ToString());


                        // адрес
                        var address = row.SelectSingleNode("div[@class='params clearfix']/div[@class='param address']/div[@class='fader']").InnerText;

                        var sAddress = address.Replace("\n ", "");

                        // ближайшее метро к адресу
                        var nearestMetro = row.SelectSingleNode("div[@class='params clearfix']" +
                                                                "/div[@class='param address']" +
                                                                "/div[@class='metro-info__wrap']" +
                                                                "/div[@class='metro-info__wrap-2']" +
                                                                "/div[@class='metro-name__wrap']" +
                                                                "/div[@class='metro-name__wrap-2']/span[@class='metro-name']")?.InnerText;

                        var sNearestMetro = "";
                        if (nearestMetro == null)
                            sNearestMetro = "Не указано";
                        else
                            sNearestMetro = nearestMetro.Replace("\n ", "").Trim();





                        // расстояние до ближайшего метро (в метрах)
                        var nearestMetroDistance = row.SelectSingleNode("div[@class='params clearfix']" +
                                                                "/div[@class='param address']" +
                                                                "/div[@class='metro-info__wrap']" +
                                                                "/div[@class='metro-info__wrap-2']" +
                                                                "/div[@class='metro-distance']")?.InnerText;

                        var sNearestMetroDistance = 0;

                        if (nearestMetroDistance == null)
                            sNearestMetroDistance = 0;
                        else
                        {
                            var bNearestMetroDistance = new StringBuilder();
                            foreach (var n in nearestMetroDistance)
                            {
                                if (n == 'к' || n == 'м' || n == ' ' || n == '+')
                                    continue;

                                bNearestMetroDistance.Append(n);
                            }

                            if (bNearestMetroDistance.Length == 1 | bNearestMetroDistance.Length == 2)
                            {
                                bNearestMetroDistance = bNearestMetroDistance.Append('0');
                                bNearestMetroDistance = bNearestMetroDistance.Append('0');
                                bNearestMetroDistance = bNearestMetroDistance.Append('0');
                            }




                            var distanceClean = bNearestMetroDistance.ToString();
                            var nearestMetroDistanceInMetres = "";

                            var indexNearestMetroDistance = distanceClean.IndexOf(".");


                            if (indexNearestMetroDistance == -1)
                                nearestMetroDistanceInMetres = distanceClean;
                            else
                                nearestMetroDistanceInMetres = distanceClean.Insert(indexNearestMetroDistance + 2, "00");

                            var finalResult = new StringBuilder();

                            foreach (var r in nearestMetroDistanceInMetres)
                            {
                                if (r == '.')
                                    continue;

                                finalResult.Append(r);
                            }



                            sNearestMetroDistance = int.Parse(finalResult.ToString());
                        }


                        var infoAd = new InfoAd()
                        {
                            AdId = sAdId,
                            Rooms = "1",
                            Price = sPrice,
                            Area = sArea,
                            Floor = sFloor,
                            Floors = sFloors,
                            Address = sAddress,
                            NearestMetro = sNearestMetro,
                            NearestMetroDistance = sNearestMetroDistance,
                            AdAdded = DateTime.Now,
                            Description = opisanie,
                            TelNumber = tel
                        };

                        _context.InfoAds.Add(infoAd);
                        _context.SaveChanges();
                    }

                    Thread.Sleep(8000);
                }


                Thread.Sleep(5000);

            }


            return View();
        }




    }


  
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace UniversalFeedReader.Models {
  public class DataManager {
    private DataManager() {}
    public static DataManager Instance = new DataManager();

    public async Task<IEnumerable<FeedItem>> UpdateFeed(string url) {

      string result = "";
      var request = WebRequest.CreateHttp(url);
      request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

      request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
      var response = await request.GetResponseAsync();

      using (var stream = response.GetResponseStream()) {
        using (var reader = new StreamReader(stream)) {
          result = await reader.ReadToEndAsync();
        }
      }

      var feed = XElement.Parse(result);

      var articles = from item in feed.Descendants("item")
        select new FeedItem {
          Title = item.Element("title").Value,
          DatePublished = item.Element("pubDate").Value,
          Description = item.Element("description").Value,
          ArticleURL = item.Element("guid").Value
        };

      return articles;
    }
  }
}



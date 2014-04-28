namespace UniversalFeedReader.Models {
  /// <summary>
  /// Repraesentiert einen einzelnen Artikel eines RSS-Feeds.
  /// </summary>
  public class FeedItem  {
    /// <summary>
    /// Titel des Artikels
    /// </summary>
    public string Title { get; set;  }

    /// <summary>
    /// Beschreibung des Artikels
    /// </summary>
    public string Description { get; set; }
  
    /// <summary>
    /// Veröffentlichung des Artikels
    /// </summary>
    public string DatePublished { get; set; }

    /// <summary>
    /// URL zum Artikel
    /// </summary>
    public string ArticleURL { get; set;  }

  }
}

using System;
using System.Collections.ObjectModel;
using UniversalFeedReader.Models;
using UniversalFeedReader.Utilities;

namespace UniversalFeedReader.ViewModels {
  public class FeedReaderViewModel : ViewModelBase {

    private ObservableCollection<FeedItem> _feedItems;

    public FeedReaderViewModel() {
      if (IsInDesignMode) {
        FeedItems = new ObservableCollection<FeedItem> {
          new FeedItem {
            ArticleURL = "http://demo.derver.de/some/path/someId.html",
            DatePublished = DateTime.Now.ToString("f"),
            Title = "Design Entry 01",
            Description = "Das ist ein DesignDateneintrag für die Visualisierung im UI Designer oder Blend."
          },
          new FeedItem {
            ArticleURL = "http://demo.derver.de/some/path/someId.html",
            DatePublished = DateTime.Now.ToString("f"),
            Title = "Design Entry 02",
            Description = "Das ist ein noch ein DesignDateneintrag, mit einer anderem Beschreibung."
          },
        };

      } else {

        RefreshCommand = new DelegateCommand(async arg => {
          var res = await DataManager.Instance.UpdateFeed("http://dotnetautor.de/GetRssFeed");
          FeedItems = new ObservableCollection<FeedItem>(res);
        });
      }
    }

    public ObservableCollection<FeedItem> FeedItems {
      get { return _feedItems; }
      set {
        if (_feedItems == value) return;
        _feedItems = value;
        OnPropertyChanged();
      }
    }

    public DelegateCommand RefreshCommand { get; protected set; }
  }
}

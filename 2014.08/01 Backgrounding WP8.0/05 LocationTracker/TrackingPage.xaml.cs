using System;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;

namespace LocationTracker {
  public partial class TrackingPage : PhoneApplicationPage {
    public TrackingPage() {
      InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      if (App.Geolocator != null) return;

      App.Geolocator = new Geolocator {
        DesiredAccuracy = PositionAccuracy.High, 
        MovementThreshold = 100
      };

      App.Geolocator.PositionChanged += geolocator_PositionChanged;
    }

    // Hinweis: Mit dem Entfernen der Aktuellen Seite aus dem "app's journal" sollte sicher gestellt werden, 
    // dass alle Referenzen auf den Geolocator zerstört werden. Wird diese Seite das nächste Mal angezeigt,
    // wird ein neuer Geolocator erzeugt unt initialisiert in dem Ereignis "OnNavigatedTo"  
    protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e) {
      App.Geolocator.PositionChanged -= geolocator_PositionChanged;
      App.Geolocator = null;
    }

    void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args) {
      if (!App.RunningInBackground) {
        Dispatcher.BeginInvoke(() => {
          LatitudeTextBlock.Text = args.Position.Coordinate.Latitude.ToString("0.00");
          LongitudeTextBlock.Text = args.Position.Coordinate.Longitude.ToString("0.00");
        });
      } else {
        // Show toast if running in background
        var toast = new ShellToast {
          Content = string.Format("[{0} | {1}]", 
            args.Position.Coordinate.Latitude.ToString("0.00"),
            args.Position.Coordinate.Longitude.ToString("0.00")),
          Title = "Location: ",
          NavigationUri = new Uri("/TrackingPage.xaml", UriKind.Relative)
        };
        toast.Show();
      }
    }
  }
}
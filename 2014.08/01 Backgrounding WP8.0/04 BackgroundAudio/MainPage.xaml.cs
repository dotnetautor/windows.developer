using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Resources;
using Microsoft.Phone.BackgroundAudio;
using Microsoft.Phone.Controls;

namespace BackgroundAudio {
  public partial class MainPage : PhoneApplicationPage {
    // Constructor
    public MainPage() {
      InitializeComponent();
      CopyToIsolatedStorage();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      BackgroundAudioPlayer.Instance.PlayStateChanged += new EventHandler(Instance_PlayStateChanged);

      if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState) {
        playButton.Content = "pause";
        txtCurrentTrack.Text = BackgroundAudioPlayer.Instance.Track.Title +
                         " by " +
                         BackgroundAudioPlayer.Instance.Track.Artist;

      } else {
        playButton.Content = "play";
        txtCurrentTrack.Text = "";
      }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e) {
      BackgroundAudioPlayer.Instance.PlayStateChanged -= new EventHandler(Instance_PlayStateChanged);
    }

    void Instance_PlayStateChanged(object sender, EventArgs e) {
      switch (BackgroundAudioPlayer.Instance.PlayerState) {
        case PlayState.Playing:
          playButton.Content = "pause";
          break;

        case PlayState.Paused:
        case PlayState.Stopped:
          playButton.Content = "play";
          break;
      }

      if (null != BackgroundAudioPlayer.Instance.Track) {
        txtCurrentTrack.Text = BackgroundAudioPlayer.Instance.Track.Title +
                               " by " +
                               BackgroundAudioPlayer.Instance.Track.Artist;
      }
    }

    private void prevButton_Click(object sender, RoutedEventArgs e) {
      BackgroundAudioPlayer.Instance.SkipPrevious();
    }

    private void playButton_Click(object sender, RoutedEventArgs e) {
      if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState) {
        BackgroundAudioPlayer.Instance.Pause();
      } else {
        BackgroundAudioPlayer.Instance.Play();
      }
    }

    private void nextButton_Click(object sender, RoutedEventArgs e) {
      BackgroundAudioPlayer.Instance.SkipNext();
    }

    private void CopyToIsolatedStorage() {
      using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication()) {
        var files = new string[] { "Demo01.mp3", "Demo02.mp3", "Demo03.mp3" };

        foreach (var fileName in files) {
          if (!storage.FileExists(fileName)) {
            string filePath = "Audio/" + fileName;
            StreamResourceInfo resource = Application.GetResourceStream(new Uri(filePath, UriKind.Relative));

            using (IsolatedStorageFileStream file = storage.CreateFile(fileName)) {
              const int chunkSize = 4096;
              var bytes = new byte[chunkSize];
              int byteCount;

              while ((byteCount = resource.Stream.Read(bytes, 0, chunkSize)) > 0) {
                file.Write(bytes, 0, byteCount);
              }
            }
          }
        }
      }
    }
  }
}
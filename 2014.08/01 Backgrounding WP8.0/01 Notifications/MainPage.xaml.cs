using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;

namespace Notifications {
  public partial class MainPage : PhoneApplicationPage {

    IEnumerable<ScheduledNotification> _notifications;

    // Constructor
    public MainPage() {
      InitializeComponent();
    }

    private void UpdateItemsList() {
      _notifications = ScheduledActionService.GetActions<ScheduledNotification>();
      var scheduledNotifications = _notifications as ScheduledNotification[] ?? _notifications.ToArray();
      EmptyTextBlock.Visibility = scheduledNotifications.Any() ? Visibility.Collapsed : Visibility.Visible;
      NotificationListBox.ItemsSource = scheduledNotifications; 
    }


    protected override void OnNavigatedTo(NavigationEventArgs e) {
      UpdateItemsList();
    }

    private void deleteButton_Click(object sender, RoutedEventArgs e) {
      var name = (string)((Button)sender).Tag;
      ScheduledActionService.Remove(name);
      UpdateItemsList();
    }

    private void ApplicationBarAddButton_Click(object sender, EventArgs e) {
      // auf die Seite AddNotification.xaml navigieren um Elemente hinzuzufügen.
      NavigationService.Navigate(new Uri("/AddNotification.xaml", UriKind.RelativeOrAbsolute));
    }
  }
}
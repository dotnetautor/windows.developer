using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;

namespace Notifications {
  public partial class AddNotification : PhoneApplicationPage {
    public AddNotification() {
      InitializeComponent();
    }

    private void ApplicationBarAddButton_Click(object sender, EventArgs e) {
      // ReSharper disable PossibleInvalidOperationException

      String name = Guid.NewGuid().ToString();


      // Auslesen der Startzeit aus der UI
      var date = (DateTime)beginDatePicker.Value;
      var time = (DateTime)beginTimePicker.Value;
      var beginTime = date + time.TimeOfDay;

      // Prüfen dass die Startzeit nicht in der Vergangeheit liegt.
      if (beginTime < DateTime.Now) {
        MessageBox.Show("Die Startzeit muss in der Zukunft liegen.");
        return;
      }

      // Auslesen der Ablaufzeit aus der UI
      date = (DateTime)expirationDatePicker.Value;
      time = (DateTime)expirationTimePicker.Value;
      DateTime expirationTime = date + time.TimeOfDay;

      // Prüfen dass die Ablaufzeit nicht in der Verganheit oder vor der Startzeit liegt.
      if (expirationTime < beginTime) {
        MessageBox.Show("Die Ablaufzeit muss nach der Startzeit liegen.");
        return;
      }

      // Ermitteln der Wiedervorlage.
      var recurrence = RecurrenceInterval.None;
      if (dailyRadioButton.IsChecked == true) {
        recurrence = RecurrenceInterval.Daily;
      } else if (weeklyRadioButton.IsChecked == true) {
        recurrence = RecurrenceInterval.Weekly;
      } else if (monthlyRadioButton.IsChecked == true) {
        recurrence = RecurrenceInterval.Monthly;
      } else if (endOfMonthRadioButton.IsChecked == true) {
        recurrence = RecurrenceInterval.EndOfMonth;
      } else if (yearlyRadioButton.IsChecked == true) {
        recurrence = RecurrenceInterval.Yearly;
      }

      // Anlegen eines DeepLinks mit entsprechen Parametern
      string param1Value = param1TextBox.Text;
      string param2Value = param2TextBox.Text;
      string queryString = "";
      if (param1Value != "" && param2Value != "") {
        queryString = "?param1=" + param1Value + "&param2=" + param2Value;
      } else if (param1Value != "" || param2Value != "") {
        queryString = (param1Value != null) ? "?param1=" + param1Value : "?param2=" + param2Value;
      }
      var navigationUri = new Uri("/ShowParams.xaml" + queryString, UriKind.Relative);

     
      if ((bool)reminderRadioButton.IsChecked) {
        var reminder = new Reminder(name) {
          Title = titleTextBox.Text,
          Content = contentTextBox.Text,
          BeginTime = beginTime,
          ExpirationTime = expirationTime,
          RecurrenceType = recurrence,
          NavigationUri = navigationUri
        };


        // Registrieren der Erinnerung.
        ScheduledActionService.Add(reminder);
      } else {
        var alarm = new Alarm(name) {
          Content = contentTextBox.Text,
          Sound = new Uri("/Assets/Sounds/Bell.wma", UriKind.Relative),
          BeginTime = beginTime,
          ExpirationTime = expirationTime,
          RecurrenceType = recurrence
        };

        // Registrieren der Erinnerung.
        ScheduledActionService.Add(alarm);
      }

      // zur Hauptseite zurückkehren
      NavigationService.GoBack();

      // ReSharper restore PossibleInvalidOperationException
    }


  }
}
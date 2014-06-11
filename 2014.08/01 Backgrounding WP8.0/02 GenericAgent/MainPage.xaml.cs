using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Scheduler;

namespace GenericAgentDemo {
  public partial class MainPage : PhoneApplicationPage {
 
    public MainPage() {
      InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      // Referenz auf die PeriodicTask ermitteln
      var periodicTask = ScheduledActionService.Find("PeriodicAgent") as PeriodicTask;

      if (periodicTask != null) {
        RemoveAgent("PeriodicAgent");
      }

      // Eine periodic Task muss eine Beschreibung enthalten, die auf der Settings Seite vom OS angzeigt werden kann
      periodicTask = new PeriodicTask("PeriodicAgent") {
        Description = "This demos a periodic task."
      };


      //// Eine ResourceIntensiveTask muss eine Beschreibung enthalten, die auf der Settings Seite vom OS angzeigt werden kann
      //var intensivTask = new ResourceIntensiveTask("PeriodicAgent") {
      //  Description = "This demos a periodic task."
      //};

      // Das Hinzufügen eines Agants sollte immer in einem Try-Block erfolgen, der Benutzer könnte Agants für diese
      // App oder alle Agants deaktiviert haben, bzw. es könnten keine Ressourcen mehr übrig sein.
      try {
        ScheduledActionService.Add(periodicTask);

        // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
#if(DEBUG)
        ScheduledActionService.LaunchForTest("PeriodicAgent", TimeSpan.FromSeconds(30));
#endif
      } catch (InvalidOperationException exception) {
        if (exception.Message.Contains("BNS Error: The action is disabled")) {
          MessageBox.Show("Hintergrundaufgaben sind für diese App deaktiviert.");
        }

        if (
          exception.Message.Contains(
            "BNS Error: The maximum number of ScheduledActions of this type have already been added.")) {
          // Keine Aktion erforderlihc, das System gibt selständig keine Meldung mit dem aktuellen Limit aus.

        }
      } catch (SchedulerServiceException) {
        // Keine weitere aktion erforderlich  
      }
    }

    private void RemoveAgent(string name) {
      try {
        ScheduledActionService.Remove(name);
      }
      catch (Exception ex) {
        Debug.WriteLine(ex.ToString());
      }
    }
  }
}
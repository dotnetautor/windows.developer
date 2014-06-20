using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using BackgroundTaskDemo.Code;
using TheBackgroundTask;

namespace BackgroundTaskDemo.ViewModel
{
    public class MainViewModel : ViewModelBase {
      private IBackgroundTaskRegistration _taskRegistration;
      private string _status;

      private IBackgroundTaskRegistration TaskRegistration {
        get { return _taskRegistration; }
        set {
          if (_taskRegistration == value) return;
          _taskRegistration = value;
          OnPropertyChanged();
          EnableBgTaskCommand.RaiseCanExecuteChanged();
          DisableBgTaskCommand.RaiseCanExecuteChanged();
        }
      }

    
      public MainViewModel() {
        EnableBgTaskCommand = new DelegateCommand(p => RegisterTask(), (o) => TaskRegistration == null);

        DisableBgTaskCommand = new DelegateCommand(p => UnregisterTask(), (o) => TaskRegistration != null);

        if (TaskIsRegistered) {
          GetTask();
        }

      }

      private bool TaskIsRegistered {
        get {
          IReadOnlyDictionary<Guid, IBackgroundTaskRegistration> allTasks = BackgroundTaskRegistration.AllTasks;
          return (allTasks.Count > 0);
        }
      }  

      void OnProgress(BackgroundTaskRegistration sender, BackgroundTaskProgressEventArgs args) {
        Status = string.Format("running : {0}", (args == null ) ? 0 : args.Progress);
      }

      void OnCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args) {
        Status = string.Format("done ! ");
      }  

      private async void RegisterTask() {
        var result = await BackgroundExecutionManager.RequestAccessAsync();
        if (result == BackgroundAccessStatus.Denied) {
          // Handle this if it is importet for your app.
          
        }

        var taskBuilder = new BackgroundTaskBuilder { Name = "MyBackgroundTask" };
        var trigger = new SystemTrigger(SystemTriggerType.TimeZoneChange, false);

        taskBuilder.SetTrigger(trigger);
        taskBuilder.TaskEntryPoint = typeof(MyBackgroundTask).FullName;
        //taskBuilder.Register();
        var registration = taskBuilder.Register();
        registration.Completed += (sender, args) => {
          // Handle this if it is importet for your app.
        };
        GetTask();
      }

      private void GetTask() {
        TaskRegistration = BackgroundTaskRegistration.AllTasks.Values.First();
        TaskRegistration.Completed += OnCompleted;
        TaskRegistration.Progress += OnProgress;
      }

      private  void UnregisterTask() {
        TaskRegistration.Completed -= OnCompleted;
        TaskRegistration.Progress -= OnProgress;
        TaskRegistration.Unregister(false);
        TaskRegistration = null;
      }

      public string Status {
        get { return _status; }
        set {
          if (_status == value) return;
          _status = value;
          CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
            CoreDispatcherPriority.Normal, () => OnPropertyChanged("Status"));
        }
      }

      public DelegateCommand EnableBgTaskCommand { get; private set; }
      public DelegateCommand DisableBgTaskCommand { get; private set; }
    }
}

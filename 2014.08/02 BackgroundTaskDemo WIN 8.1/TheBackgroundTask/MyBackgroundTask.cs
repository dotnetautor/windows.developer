using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace TheBackgroundTask {
  public sealed class MyBackgroundTask : IBackgroundTask {
    public async void Run(IBackgroundTaskInstance taskInstance) {
      var deferral = taskInstance.GetDeferral();
      bool cancelled = false;
      taskInstance.Progress = 0;

      BackgroundTaskCanceledEventHandler handler = (s, e) => {
        cancelled = true;
      };

      for (uint i = 0; ((i < 10) && !cancelled); i++) {
        await Task.Delay(5000);
        taskInstance.Progress = i + 1;
        ShowToast(i);
      }
      ApplicationData.Current.LocalSettings.Values["LAST_RUN_TIME"] = DateTimeOffset.Now;

      deferral.Complete();
    }

    private void ShowToast(uint cnt) {
      var toastXml  = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
      XmlNodeList toastTextAttributes = toastXml.GetElementsByTagName("text");
      toastTextAttributes[0].InnerText = String.Format("Hello from Background {0}", cnt);
      var toast = new ToastNotification(toastXml);
      ToastNotificationManager.CreateToastNotifier().Show(toast);
    }
  }
}

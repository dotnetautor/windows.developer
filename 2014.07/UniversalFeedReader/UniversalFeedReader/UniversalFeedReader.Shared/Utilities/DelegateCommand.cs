using System;
using System.Windows.Input;

namespace UniversalFeedReader.Utilities {
  public class DelegateCommand : ICommand {
    readonly Func<object, bool> _canExecute;
    readonly Action<object> _executeAction;

    public DelegateCommand(Action<object> executeAction)
      : this(executeAction, null) {
    }

    public DelegateCommand(Action<object> executeAction,
         Func<object, bool> canExecute) {
      if (executeAction == null) {
        throw new
          ArgumentNullException("executeAction");
      }
      this._executeAction = executeAction;
      this._canExecute = canExecute;
    }

    public bool CanExecute(object parameter) {
      bool result = true;
      if (_canExecute != null) {
        result = _canExecute(parameter);
      }
      return result;
    }

    public event EventHandler CanExecuteChanged;
    public void RaiseCanExecuteChanged() {
      if (CanExecuteChanged != null) {
        CanExecuteChanged(this,
                    new EventArgs());
      }
    }

    public void Execute(object parameter) {
      _executeAction(parameter);
    }
  }

}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Yort.Ntp.XFSample
{
	public class RelayCommand : ICommand
	{
		private Action _Execute;
		private Func<bool> _CanExecute;

		public RelayCommand(Action execute, Func<bool> canExecute)
		{
			_Execute = execute;
			_CanExecute = canExecute;
		}

		public event EventHandler CanExecuteChanged;

		public void RaiseCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		public bool CanExecute(object parameter)
		{
			return _CanExecute?.Invoke() ?? true;
		}

		public void Execute(object parameter)
		{
			_Execute?.Invoke();
		}
	}
}
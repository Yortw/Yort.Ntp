using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace Yort.Ntp.XFSample
{
	public class MainViewModel : INotifyPropertyChanged
	{
		private DateTime? _NtpDateTime;
		private string _LastError;
		private Yort.Ntp.NtpClient _Client;
		private RelayCommand _SyncTimeCommand;
		private bool _IsBusy;

		public event PropertyChangedEventHandler PropertyChanged;

		public DateTime? NtpDateTime
		{
			get { return _NtpDateTime; }
			set
			{
				_NtpDateTime = value;
				OnPropertyChanged(nameof(NtpDateTime));
				OnPropertyChanged(nameof(Offset));
			}
		}

		public string Offset
		{
			get { return (_NtpDateTime == null ? TimeSpan.Zero : DateTime.UtcNow.Subtract(_NtpDateTime.Value)).ToString(); }
		}

		public string LastError
		{
			get { return _LastError; }
			set
			{
				_LastError = value;
				OnPropertyChanged(nameof(LastError));
			}
		}

		public async void SyncTime()
		{
			_Client = _Client ?? new NtpClient();

			try
			{
				IsBusy = true;
				LastError = null;

				NtpDateTime = await _Client.RequestTimeAsync();
			}
			catch (Exception ex)
			{
				LastError = ex.ToString();
			}
			finally
			{
				IsBusy = false;
			}
		}

		public bool IsBusy
		{
			get { return _IsBusy; }
			set
			{
				_IsBusy = value;
				OnPropertyChanged(nameof(IsBusy));
				_SyncTimeCommand?.RaiseCanExecuteChanged();
			}
		}

		public ICommand SyncTimeCommand
		{
			get
			{
				return _SyncTimeCommand ?? (_SyncTimeCommand = new RelayCommand(SyncTime, () => !IsBusy));
			}
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
using Ethereal.FAF.API.Client.Models.Base;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Ethereal.FAF.UI.Client.ViewModels.Base
{
	internal class FafApiResultViewModel<T> : ApiUniversalTypeId, INotifyPropertyChanged
		where T : FafApiViewModel
	{
		[JsonPropertyName("attributes")]
		public T Attributes { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
		protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
		{
			if (Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(PropertyName);
			return true;
		}
	}
	internal abstract class FafApiViewModel : ApiUniversalTypeId, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string PropertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
		protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
		{
			if (Equals(field, value)) return false;
			field = value;
			OnPropertyChanged(PropertyName);
			return true;
		}
	}
}

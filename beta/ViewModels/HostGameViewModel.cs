using System.ComponentModel.DataAnnotations;

namespace beta.ViewModels
{
    internal class HostGameViewModel : Base.ViewModel
    {
        #region Title
        private string _Title;
        [Required(ErrorMessage = "Please enter title")]
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }
        #endregion


    }
}

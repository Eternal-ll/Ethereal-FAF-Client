using Ethereal.FAF.UI.Client.ViewModels.Base;
using System;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class ExceptionViewModel : ViewModel
    {
        public Exception Exception { get; set; }

        public string Message => Exception?.Message;

        public string ExceptionType => Exception?.GetType().Name ?? "";
    }
}

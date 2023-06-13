using Ethereal.FAF.API.Client.Models.Base;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Mediator;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class ChangeEmailValidator : AbstractValidator<ChangeEmail>
    {
        public ChangeEmailValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("non-valid email address");
            RuleFor(x => x.Password)
                .NotEmpty();
        }
    }
    public class ChangeEmail : Base.ViewModel, INotifyDataErrorInfo
    {
        private readonly AbstractValidator<ChangeEmail> _validator;
        private ValidationResult validationResult;

        public ChangeEmail()
        {
            _validator = new ChangeEmailValidator();
        }

        private string _Email;
        public string Email
        {
            get => _Email;
            set
            {
                if (Set(ref _Email, value))
                {
                    Validate();
                }
            }
        }
        private string _Password;
        public string Password
        {
            get => _Password;
            set
            {
                if (Set(ref _Password, value))
                {
                    Validate();
                }
            }
        }

        public bool HasErrors => validationResult?.IsValid != true;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        private void Validate([CallerMemberName]string propertyName = null)
        {
            var result = _validator.Validate(this);
            var changed = result.IsValid != validationResult?.IsValid ||
                result.Errors.Count != validationResult?.Errors.Count;
            if (changed)
            {
                validationResult = result;
                ErrorsChanged?.Invoke(this, new(propertyName));
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            return validationResult?.Errors.Where(e => e.PropertyName == propertyName).Select(x => x.ErrorMessage).ToList();
        }
    }
    public class ChangeEmailViewModel : Base.ViewModel
    {
        private readonly IMediator _mediator;
        public ChangeEmailViewModel(IMediator mediator)
        {
            Model = new();
            ChangeEmailCommand = new LambdaCommand(OnChangeEmailCommand, CanChangeEmailCommand);
            _mediator = mediator;
        }
        public string Email { get; set; }
        public string Password { get; set; }
        public ChangeEmail Model { get; set; }
        public ICommand ChangeEmailCommand { get; }
        public ICommand HideModalCommand { get; set; }
        public ICommand ShowUpdateButton { get; set; }
        public ICommand HideUpdateButton { get; set; }
        private bool CanChangeEmailCommand(object arg)
        {
            var valid = !Model.HasErrors;
            if (valid) ShowUpdateButton?.Execute(null);
            else HideUpdateButton?.Execute(null);
            return valid;
        }
        private async void OnChangeEmailCommand(object arg)
        {
            await ChangeEmail();
        }

        public async Task<bool> ChangeEmail()
        {
            var email = Model.Email;
            var password = Model.Password;
            //await _mediator.Send(new NotifyCommand("Error", "Bad password", Wpf.Ui.Common.SymbolRegular.Warning12, Wpf.Ui.Common.ControlAppearance.Danger));
            return await _mediator.Send(new ChangeEmailCommand(email, password));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}

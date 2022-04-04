using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    internal interface INotificationService
    {
        public Task ShowPopupAsync(string text);
        public Task ShowPopupAsync(object model);
    }
}

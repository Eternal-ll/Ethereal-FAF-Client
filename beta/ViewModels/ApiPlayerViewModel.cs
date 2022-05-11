namespace beta.ViewModels
{
    public abstract class ApiPlayerViewModel : ApiViewModel
    {
        public int PlayerId { get; private set; }

        public ApiPlayerViewModel(int playerId) => PlayerId = playerId;
    }
}

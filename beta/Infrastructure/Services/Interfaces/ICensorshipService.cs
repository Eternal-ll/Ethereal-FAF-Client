namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// Censor service
    /// </summary>
    public interface ICensorshipService
    {
        /// <summary>
        /// Is map banned
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsBannedMap(string name);
    }
}

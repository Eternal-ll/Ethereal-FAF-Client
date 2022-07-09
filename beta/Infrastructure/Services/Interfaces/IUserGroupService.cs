namespace beta.Infrastructure.Services.Interfaces
{
    /// <summary>
    /// https://api.faforever.com/data/userGroup
    /// </summary>
    public interface IUserGroupService
    {
        public bool IsInAnyGroup(int playerId);
        public bool TryGetGroupOfPlayer(int playerId, out string groupName);
        public int[] GetGroupMembers(string groupName);
    }
}

using System.Threading.Tasks;

namespace beta.Infrastructure.Services.Interfaces
{
    public interface INoteService
    {
        /// <summary>
        /// Returns note about player. False = no note
        /// </summary>
        /// <param name="player">Player</param>
        /// <param name="note">Text</param>
        /// <returns>Note about <paramref name="player"/></returns>
        public bool TryGet(string player, out string note);
        
        /// <summary>
        /// Set note to player
        /// </summary>
        /// <param name="player">Target player</param>
        /// <param name="note">Text of note</param>
        public void Set(string player, string note = null);

        // (NO) Saves all notes in JSON format in cache like { "player": "_", "note": "_" }
        /// <summary>
        /// Saves all notes in cache as notes.txt in format - player : note
        /// </summary>
        public void Save();

        public bool IsEmpty();
    }
}

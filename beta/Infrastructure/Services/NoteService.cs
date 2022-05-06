using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.Enums;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace beta.Infrastructure.Services
{
    public class NoteService : INoteService
    {
        /// <summary>
        /// Notes: player - note
        /// </summary>
        private readonly Dictionary<string, string> Notes = new();
        private static string Path => App.GetPathToFolder(Folder.Common) + "notes.txt";

        public NoteService()
        {
            Task.Run(() => Load());
        }

        private async Task Load()
        {
            if (!File.Exists(Path)) return;

            var notes = await File.ReadAllLinesAsync(Path);

            for (int i = 0; i < notes.Length; i++)
            {
                var data = notes[i].Split(':');
                if (data.Length < 2)
                {
                    continue;
                }
                var player = notes[0];
                var note = string.Join(':', notes[1..]);

                if (note.Length > PlayerNoteVM.MaxLengthOfNote) note = note[..(PlayerNoteVM.MaxLengthOfNote - 1)];

                Set(player, note);
            }
        }

        public void Save()
        {
            if (Notes.Count == 0) return;

            using var sw = File.CreateText(Path);

            foreach (var note in Notes)
            {
                if (note.Value.Trim().Length == 0) continue;

                sw.WriteLine($"{note.Key}:{note.Value}");
            }
        }

        public void Set(string player, string note = null)
        {
            if (Notes.ContainsKey(player))
            {
                Notes[player] = note;
            }
            else
            {
                Notes.Add(player, note);
            }
        }

        public bool TryGet(string player, out string note) => Notes.TryGetValue(player, out note);

        public bool IsEmpty() => Notes.Count == 0;
    }
}

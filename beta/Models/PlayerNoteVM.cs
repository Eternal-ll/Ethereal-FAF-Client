using System.Windows.Media;

namespace beta.Models
{
    /// <summary>
    /// Player note with INPC
    /// </summary>
    public class PlayerNoteVM : ViewModels.Base.ViewModel
    {
        public static int MaxLengthOfNote = 256;

        public PlayerNoteVM() { }

        /// <summary>
        /// Initialize new instance of player note with text and white color
        /// </summary>
        /// <param name="note">Note</param>
        public PlayerNoteVM(string note) => Text = note;

        /// <summary>
        /// Initialize new instance of player note with text and color
        /// </summary>
        /// <param name="note">Note</param>
        /// <param name="color">Text color?</param> TODO (Text color or what?)
        public PlayerNoteVM(string note, Color color) : this(note) => Color = color;

        #region Color
        private Color _Color = Colors.White;
        /// <summary>
        /// Note color, default - White
        /// </summary>
        public Color Color
        {
            get => _Color;
            set => Set(ref _Color, value);
        }
        #endregion

        #region Text
        private string _Text = string.Empty;
        /// <summary>
        /// Note text
        /// </summary>
        public string Text
        {
            get => _Text;
            set
            {
                if (Set(ref _Text, value))
                {
                    OnPropertyChanged(nameof(RemainsLength));
                }
            }
        } 
        #endregion

        /// <summary>
        /// Max lenght of note
        /// </summary>
        public int MaxLength => MaxLengthOfNote;

        /// <summary>
        /// Remains length of note
        /// </summary>
        public int RemainsLength => Text.Length - MaxLengthOfNote;
    }
}

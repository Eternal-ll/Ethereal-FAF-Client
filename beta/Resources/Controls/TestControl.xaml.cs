using beta.Infrastructure;
using beta.Infrastructure.Services.Interfaces;
using beta.Models;
using beta.Models.IRC;
using beta.Models.IRC.Base;
using beta.Models.IRC.Enums;
using beta.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace beta.Resources.Controls
{
    public enum InputMode
    {
        None,
        Command,
        Player,
        Emoji
    }
    public abstract class ChatCommand
    {
        public string Name { get; }
        public string Description { get; }
        public string Group { get; }
        public string[] Fields { get; }
        public ChatCommand(string name, string description, string group, string[] fields = null)
        {
            Name = "/" + name;
            Description = description;
            Group = group;
            Fields = fields;
        }
    }

    public class IrcCommand : ChatCommand
    {
        public IrcUserCommand Code { get; }
        public IrcCommand(IrcUserCommand code, string description, string group, string[] fields = null)
            : base(code.ToString().ToLower(), description, group, fields) => Code = code;
    }

    public class ClientCommand : ChatCommand
    {
        public ClientCommand(string name, string description, string group, string[] fields = null)
            : base(name, description, group, fields)
        { }
    }

    public class BotCommand : ChatCommand
    {
        public string Bot { get; }
        public BotCommand(string bot, string name, string description, string group, string[] fields = null) : base(name, description, group, fields)
        {
            Bot = bot;
        }
    }

    public class SuperInputMessage
    {
        public InputMode InputMode { get; }
        public string Text { get; }

        public SuperInputMessage(string text, InputMode inputMode)
        {
            Text = text;
            InputMode = inputMode;
        }
    }
    /// <summary>
    /// Interaction logic for TestControl.xaml
    /// </summary>
    public partial class TestControl : UserControl, INotifyPropertyChanged
    {
        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;
        private protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new(propertyName));
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(PropertyName);
            return true;
        }
        #endregion

        public event EventHandler LeaveRequired;
        public event EventHandler<IrcMessage> SentMessage;

        private void OnLeaveRequired() => LeaveRequired?.Invoke(this, null);

        private readonly IIrcService IrcService;

        #region Chat properties

        public string[] Users { get; set; }
        private IrcChannelVM _SelectedChannel;
        public IrcChannelVM SelectedChannel
        {
            get => _SelectedChannel;
            set
            {
                if (Set(ref _SelectedChannel, value))
                {
                    if (value is not null)
                    {
                        Users = value.Users.ToArray();
                    }
                    else
                    {
                        Users = Array.Empty<string>();
                    }
                }
            }
        }

        #endregion

        public void UpdateUsers(string[] users) => Users = users;

        public TestControl()
        {
            InitializeComponent();
            DataContext = this;

            //RichTextBox.PreviewTextInput += RichTextBox_PreviewTextInput;
            //RichTextBox.TextInput += RichTextBox_TextInput;
            RichTextBox.PreviewKeyDown += RichTextBox_PreviewKeyDown;
            IrcService = App.Services.GetService<IIrcService>();
        }
        public TextBox Input => RichTextBox;

        private void RichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                AutoCompletionEnabled = false;
            }

            if (e.Key == Key.Tab)
            {
                if (_CurrentText.Length != 0)
                {
                    if (RichTextBox.SelectionStart == _CurrentText.Length)
                        TryCompletePlayerNameAtEnd();
                    else TryCompletePlayerNameSomeWhere();
                }

                SelectedCommandIndex++;
                e.Handled = true;
            }

            if (e.Key == Key.Enter)
            {
                if (AutoCompletionEnabled)
                {
                    AutoCompletionEnabled = false;
                    CurrentText += " ";
                    RichTextBox.SelectionStart = CurrentText.Length;
                    return;
                }

                if (_InputMode == InputMode.Command)
                {
                    if (_SelectedCommand is not null)
                    {
                        if (!IsCommandSelected)
                        {
                            RichTextBox.Text = _SelectedCommand.Name + ' ';
                            RichTextBox.SelectionStart = RichTextBox.Text.Length;
                            return;
                        }

                        bool isAllFilled = true;

                        if (SelectedCommandFields.Count != 0)
                        {
                            foreach (var key in SelectedCommandFields.Keys)
                            {
                                if (SelectedCommandFields[key].Length == 0)
                                {
                                    isAllFilled = false;
                                    break;
                                }
                            }
                        }

                        IsRequiredFieldsFilled = isAllFilled;

                        if (isAllFilled)
                        {
                            var command = ((IrcCommand)SelectedCommand);

                            // Because we setting NULL to SelectedChannel on ChatView below
                            string channel = SelectedChannel?.Name;

                            if (command.Code == IrcUserCommand.PART)
                            {
                                // SelectedChannel is not null;
                                OnLeaveRequired();
                                // SelectedChannel = null;
                            }

                            IrcService.SendCommand(command.Code, CurrentText, channel);
                            CurrentText = string.Empty;
                        }
                        return;
                    }
                }
                if (CurrentText.Trim().Length > 0)
                {
                    if (SelectedChannel is null) return;
                    var nick = Properties.Settings.Default.PlayerNick;

                    IrcService.SendMessage(SelectedChannel.Name, CurrentText);
                }
                CurrentText = string.Empty;
                e.Handled = true;
            }

            if (e.Key == Key.Down)
            {
                if (AutoCompletionEnabled)
                {
                    SelectedSuggestedPlayerIndex++;
                    return;
                }
                //if (CommandSelected)
                //{
                //    if (AutoCompletionEnabled && PlayersSuggestionBoxVisiblity == Visibility.Visible)
                //    {

                //        return;
                //    }
                //}
                SelectedCommandIndex++;
            }
            if (e.Key == Key.Up)
            {
                if (AutoCompletionEnabled)
                {
                    SelectedSuggestedPlayerIndex--;
                    return;
                }
                SelectedCommandIndex--;
            }
        }

        #region InputMode
        private InputMode _InputMode;
        public InputMode InputMode
        {
            get => _InputMode;
            set
            {
                if (Set(ref _InputMode, value))
                {
                    OnPropertyChanged(nameof(CommandsHelperVisibility));
                    switch (value)
                    {
                        case InputMode.None:
                            if (AutoCompletionEnabled)
                            {
                                AutoCompletionEnabled = false;
                                SuggestedPlayers.Clear();
                            }
                            break;
                        case InputMode.Command:
                            break;
                        case InputMode.Player:
                            break;
                        case InputMode.Emoji:
                            break;
                    }
                }
            }
        }
        #endregion

        #region CurrentWord
        private string _CurrentWord;
        public string CurrentWord
        {
            get => _CurrentWord;
            set
            {
                if (Set(ref _CurrentWord, value))
                {

                }
            }
        }
        #endregion

        #region CurrentText
        private string _CurrentText = string.Empty;
        public string CurrentText
        {
            get => _CurrentText;
            set
            {
                if (Set(ref _CurrentText, value))
                {
                    if (value.Length == 0)
                    {
                        InputMode = InputMode.None;
                        if (AutoCompletionEnabled)
                        {
                            AutoCompletionEnabled = false;
                        }
                    }
                    if (value.StartsWith('/'))
                    {
                        InputMode = InputMode.Command;

                        var data = value.Split();

                        WrittenCommand = data[0];

                        SuggestedCommands.Clear();
                        IsCommandSelected = false;
                        for (int i = 0; i < AvailableCommands.Length; i++)
                        {
                            var command = AvailableCommands[i];
                            if (command.Name.StartsWith(WrittenCommand))
                                SuggestedCommands.Add(command);
                            if (command.Name.Equals(WrittenCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                SelectedCommand = command;
                                IsCommandSelected = true;
                            }
                        }
                        OnPropertyChanged(nameof(CommandsHelperVisibility));
                        if (SuggestedCommands.Count > 0 && SelectedCommand is null) SelectedCommandIndex = 0;

                        var writtenValues = value[WrittenCommand.Length..].Trim();

                        if (writtenValues.Length != 0)
                        {
                            var values = writtenValues.Split(' ');
                            int i = 0;

                            if (SelectedCommandFields is not null)
                            {
                                SelectedCommandFields.Clear();

                                for (int j = 0; j < SelectedCommand?.Fields.Length; j++)
                                {
                                    if (i < values.Length)
                                    {
                                        SelectedCommandFields.Add(SelectedCommand.Fields[j], values[i]);
                                        i++;
                                    }
                                    else SelectedCommandFields.Add(SelectedCommand.Fields[j], string.Empty);
                                }
                            }

                        }
                        else
                        {
                            if (SelectedCommandFields is not null)
                            {
                                SelectedCommandFields.Clear();
                                for (int j = 0; j < SelectedCommand?.Fields.Length; j++)
                                {
                                    SelectedCommandFields.Add(SelectedCommand.Fields[j], string.Empty);
                                }
                            }
                        }
                    }
                    else
                    {
                        SuggestedCommands.Clear();
                        OnPropertyChanged(nameof(CommandsHelperVisibility));
                    }
                }
            }
        }
        #endregion

        #region Players auto suggestion on TAB

        public ObservableCollection<string> SuggestedPlayers { get; set; } = new();
        public Visibility PlayersSuggestionBoxVisiblity => SuggestedPlayers.Count > 0 && AutoCompletionEnabled ? Visibility.Visible : Visibility.Collapsed;


        #region SelectedSuggestedPlayerIndex
        private int _SelectedSuggestedPlayerIndex = 0;
        public int SelectedSuggestedPlayerIndex
        {
            get => _SelectedSuggestedPlayerIndex;
            set
            {
                var difference = value - _SelectedSuggestedPlayerIndex;
                if (difference > 0)
                {
                    value = SuggestedPlayers.Count > value ? value : 0;
                }
                else if (difference < 0)
                {
                    value = 0 > value ? SuggestedPlayers.Count - 1 : value;
                }
                if (Set(ref _SelectedSuggestedPlayerIndex, value))
                {

                }
            }
        }
        #endregion

        #region AutoCompletionEnabled
        private bool _AutoCompletionEnabled = false;
        public bool AutoCompletionEnabled
        {
            get => _AutoCompletionEnabled;
            set
            {
                if (Set(ref _AutoCompletionEnabled, value))
                {
                    if (!value)
                    {
                        SuggestedPlayers.Clear();
                        OnPropertyChanged(nameof(PlayersSuggestionBoxVisiblity));
                    }
                }
            }
        }
        #endregion

        #region SelectedPlayer
        private string _SelectedPlayer;
        public string SelectedPlayer
        {
            get => _SelectedPlayer;
            set
            {
                if (Set(ref _SelectedPlayer, value))
                {
                    if (value is not null)
                    {
                        CurrentText = completionLine + value;
                        RichTextBox.SelectionStart = _CurrentText.Length;
                        if (!RichTextBox.IsFocused)
                        {
                            RichTextBox.Focus();
                            AutoCompletionEnabled = false;
                            CurrentText += " ";
                            RichTextBox.SelectionStart = CurrentText.Length;
                        }
                    }
                }
            }
        }
        #endregion

        #region Thickness
        // for suggestion box above the input box
        // offset on X from left
        private Thickness _Thickness;
        public Thickness Thickness
        {
            get => _Thickness;
            set => Set(ref _Thickness, value);
        }
        #endregion 

        private string completionText;
        private string completionLine;

        private void TryCompletePlayerNameAtEnd()
        {
            if (!AutoCompletionEnabled)
            {
                if (_CurrentText[^1] == ' ') return;

                AutoCompletionEnabled = true;

                completionText = _CurrentText.Split()[^1];
                completionLine = string.Join(' ', _CurrentText.Split()[..^1]);

                var width = Input.ActualWidth;
                var height = Input.ActualHeight;
                for (int j = 0; j < width; j++)
                {
                    var func = Input.GetCharacterIndexFromPoint(new(j, height / 2), false);
                    if (func == completionLine.Length)
                    {
                        Thickness = new Thickness(j, 0, 0, 4);
                        break;
                    }
                }

                SuggestedPlayers.Clear();

                if (Users is not null)
                    for (int i = 0; i < Users.Length; i++)
                    {
                        if (Users[i].StartsWith(completionText, StringComparison.OrdinalIgnoreCase))
                            SuggestedPlayers.Add(Users[i]);
                    }

                OnPropertyChanged(nameof(PlayersSuggestionBoxVisiblity));

                if (SuggestedPlayers.Count > 0)
                {
                    if (completionLine.Length > 0) completionLine += " ";
                    SelectedSuggestedPlayerIndex = 0;
                }
                else SelectedSuggestedPlayerIndex = -1;
            }
            else
            {
                if (SelectedSuggestedPlayerIndex != -1)
                {
                    if (SuggestedPlayers.Count == 0)
                    {
                        AutoCompletionEnabled = false;
                        return;
                    }

                    SelectedSuggestedPlayerIndex++;
                }

            }
        }
        private void TryCompletePlayerNameSomeWhere()
        {
            if (!AutoCompletionEnabled)
            {
                if (_CurrentText.Length > RichTextBox.SelectionStart &&
                    _CurrentText[RichTextBox.SelectionStart + 1] != ' ')
                    return;
            }
            else
            {

            }
        }

        #endregion

        #region Commands

        private string WrittenCommand = string.Empty;

        #region SelectedCommand
        private ChatCommand _SelectedCommand;
        public ChatCommand SelectedCommand
        {
            get => _SelectedCommand;
            set
            {
                if (Set(ref _SelectedCommand, value))
                {
                    if (value is not null)
                    {
                        ObservableDictionary<string, string> commandValues = new();
                        for (int i = 0; i < value.Fields.Length; i++)
                        {
                            commandValues.Add(value.Fields[i], string.Empty);
                        }
                        SelectedCommandFields = commandValues;

                        if (!RichTextBox.IsFocused)
                        {
                            RichTextBox.Focus();
                        }
                    }
                    else
                    {
                        SelectedCommandFields = null;
                    }
                }
            }
        }

        #endregion

        #region SelectedCommandIndex
        private int _SelectedCommandIndex = 0;
        public int SelectedCommandIndex
        {
            get => _SelectedCommandIndex;
            set
            {
                var difference = value - _SelectedCommandIndex;
                if (difference > 0)
                {
                    value = SuggestedCommands.Count > value ? value : 0;
                }
                else if (difference < 0)
                {
                    value = 0 > value ? SuggestedCommands.Count - 1 : value;
                }

                if (Set(ref _SelectedCommandIndex, value))
                {

                }
            }
        }
        #endregion

        #region SelectedCommandFields
        private ObservableDictionary<string, string> _SelectedCommandFields;
        public ObservableDictionary<string, string> SelectedCommandFields
        {
            get => _SelectedCommandFields;
            set => Set(ref _SelectedCommandFields, value);
        }
        #endregion

        #region IsCommandSelected
        private bool _IsCommandSelected;
        public bool IsCommandSelected
        {
            get => _IsCommandSelected;
            set
            {
                if (Set(ref _IsCommandSelected, value))
                {
                    OnPropertyChanged(nameof(SelectedCommandFieldsFillingVisibility));
                }
            }
        }
        #endregion

        #region IsRequiredFieldsFilled
        private bool _IsRequiredFieldsFilled;
        public bool IsRequiredFieldsFilled
        {
            get => _IsRequiredFieldsFilled;
            set
            {
                if (Set(ref _IsRequiredFieldsFilled, value))
                {

                }
            }
        }
        #endregion

        public Visibility CommandsHelperVisibility => SuggestedCommands.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SelectedCommandFieldsFillingVisibility => IsCommandSelected ? Visibility.Visible : Visibility.Collapsed;

        private static ChatCommand[] AvailableCommands = new ChatCommand[]
        {
            // join to <channel>
            new IrcCommand(IrcUserCommand.JOIN, "Join to specific channel", "Built-in", new[]{"channel"}),
            // invite <target> to <channel>
            // to channel
            // to TMM
            // to game
            new IrcCommand(IrcUserCommand.INVITE, "Invite specific player to current channel", "Built-in", new[]{"player"}),
            // change topic of <channel> to <text>
            new IrcCommand(IrcUserCommand.TOPIC, "Change topic for current channel", "Built-in", new[]{"text"}),
            new IrcCommand(IrcUserCommand.QUIT, "Disconnect from IRC server", "Built-in", Array.Empty<string>()),
            new IrcCommand(IrcUserCommand.PART, "Leave from current channel", "Built-in", Array.Empty<string>()),
            // invite to current game
            //new("game", ""),
            // Invite players to TMM. Should be implemented with party players so in the chat will appear card with party players,
            // selected queueries, total party rating and etc, maybe % win/ratio of players
            //new("tmm", ""), 
            // Map card
            //new("map", ""),

            // summon clan/player
            //new("summon", ""),

            // gif
            //new("giphy", ""),
            // gif
            //new("tenor", ""),
        };

        public ObservableCollection<ChatCommand> SuggestedCommands { get; } = new();

        private void CommandsViewFilter(object sender, FilterEventArgs e)
        {
            var filter = _CurrentText;
            var command = (ChatCommand)e.Item;

            if (!IsCommandSelected)
            {
                if (command.Name.Equals(WrittenCommand, StringComparison.OrdinalIgnoreCase))
                {
                    SelectedCommand = command;
                    IsCommandSelected = true;
                    return;
                }
            }

            if (command.Name.StartsWith(filter)) return;

            e.Accepted = false;
        }

        #endregion
    }
}

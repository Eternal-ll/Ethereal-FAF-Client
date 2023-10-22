using Ethereal.FAF.API.Client.Models.Clans;
using Ethereal.FAF.UI.Client.Infrastructure.Commands;
using Ethereal.FAF.UI.Client.Infrastructure.Extensions;
using Ethereal.FAF.UI.Client.Infrastructure.Mediator;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Ethereal.FAF.UI.Client.ViewModels
{
    public class PaginationDto<T>
    {
        public List<T> Items { get; set; }
        public Paging Paging { get; set; }
    }
    public record Paging(int Page, int PageSize, int TotalPages, int TotalCount);
    public class ClansViewModel : Base.ViewModel
    {
        private readonly IMediator _mediator;

        public ClansViewModel(IMediator mediator)
        {
            LoadPageCommand = new LambdaCommand(OnLoadPageCommand, CanLoadPageCommand);

            Clans = new();
            _mediator = mediator;
            BindingOperations.EnableCollectionSynchronization(Clans, new object());
        }

        public ObservableCollection<ClanDto> Clans { get; }

        private Paging _Paging;

        public ICommand LoadPageCommand { get; }
        private bool CanLoadPageCommand(object arg) => true;
        private void OnLoadPageCommand(object arg)
        {
            Task.Run(LoadPage);
        }
        private async Task LoadPage()
        {
            CanLoad = false;
            var include = new string[] { "founder", "leader" };
            var data = await _mediator.Send(new GetDataCommand<ClanDto>(1, 20, include));
            _Paging = data.Paging;
            var rndm = new Random();
            KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            Clans.Clear();
            foreach (var clanDto in data.Items)
            {
                var randomColorName = names[rndm.Next(names.Length)];
                var randomColor = Color.FromKnownColor(randomColorName);
                clanDto.TagColor = randomColor.ToHexString();
                Clans.Add(clanDto);
            }
            OnPropertyChanged(nameof(Clans));
            CanLoad = true;
        }
        private bool CanLoad = false;
        public bool CanLoadPage() => CanLoad;
        public async Task AddPage()
        {
            CanLoad = false;
            var include = new string[] { "founder", "leader" };
            var data = await _mediator.Send(new GetDataCommand<ClanDto>(_Paging.Page + 1, _Paging.PageSize, include));
            _Paging = data.Paging;
            var rndm = new Random();
            KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
            foreach (var clanDto in data.Items)
            {
                var randomColorName = names[rndm.Next(names.Length)];
                var randomColor = Color.FromKnownColor(randomColorName);
                clanDto.TagColor = randomColor.ToHexString();
                Clans.Add(clanDto);
            }
            CanLoad = true;
        }
    }
}

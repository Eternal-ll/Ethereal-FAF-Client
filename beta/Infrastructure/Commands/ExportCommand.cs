using beta.Infrastructure.Services.Interfaces;
using beta.Models.API;
using DevExpress.Data.Utils;

namespace beta.Infrastructure.Commands
{
    internal class ExportCommand : Base.Command
    {
        private readonly IExportService ExportService;

        public ExportCommand() => ExportService = App.Services.GetService<IExportService>();
        
        public override bool CanExecute(object parameter) => true;

        public override void Execute(object parameter)
        {
            if (parameter is null) return;

            if (parameter is ApiMapData[] maps) ExportService.ExportMaps(maps);
        }
    }
}

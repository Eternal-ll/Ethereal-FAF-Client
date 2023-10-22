using Ethereal.FAF.API.Client;
using Ethereal.FAF.API.Client.Models;
using Ethereal.FAF.API.Client.Models.Base;
using Ethereal.FAF.API.Client.Models.Clans;
using Ethereal.FAF.UI.Client.ViewModels;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ethereal.FAF.UI.Client.Infrastructure.Mediator
{
    public interface IRelationParser<T>
    {
        public void Parse(ApiListResult<T> data);
    }
    public class ClanDtoRelationParser : IRelationParser<ClanDto>
    {
        public void Parse(ApiListResult<ClanDto> result)
        {
            var included = result.Included;
            if (included is null || included.Length == 0) return;
            foreach (var mod in result.Data)
            {
                if (mod.Relations is null) continue;
                foreach (var relation in mod.Relations)
                {
                    if (relation.Value.Data is null) continue;
                    if (relation.Value.Data.Count == 0) continue;
                    var entity = ApiUniversalTools.GetDataFromIncluded(included, relation.Value.Data[0].Type, relation.Value.Data[0].Id);
                    if (entity is null) continue;

                    if (relation.Value.Data[0].Type == API.Client.Models.Enums.ApiDataType.player)
                    {
                        if (relation.Key == "founder")
                        {
                            mod.Attributes.Founder = entity.CastTo<ApiPlayerData>();
                        }
                        if (relation.Key == "leader")
                        {
                            mod.Attributes.Leader = entity.CastTo<ApiPlayerData>();
                        }
                    }
                }
            }
        }
    }
    internal record GetDataCommand<T>(int Page, int PageSize, string[] include = null) : IRequest<PaginationDto<T>>
        where T : class;
    internal class GetDataCommandHandler<T> : IRequestHandler<GetDataCommand<T>, PaginationDto<T>>
        where T : class
    {
        private readonly IFafApi<T> _fafApi;
        private readonly IRelationParser<T>? RelationParser;
        private readonly IMediator _mediator;

        public GetDataCommandHandler(IFafApi<T> fafApi, IMediator mediator, IRelationParser<T>? relationParser)
        {
            _fafApi = fafApi;
            _mediator = mediator;
            RelationParser = relationParser;
        }

        public async Task<PaginationDto<T>> Handle(GetDataCommand<T> request, CancellationToken cancellationToken)
        {
            var pagination = new Pagination()
            {
                PageNumber = request.Page,
                PageSize = request.PageSize,
            };
            var response = await _fafApi.Get(null, null, pagination, request.include, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                await _mediator.Publish(new ApiNotification("Api error", response.ReasonPhrase, false));
                return new PaginationDto<T>()
                {
                    Items = new System.Collections.Generic.List<T>(),
                    Paging = new(request.Page, request.PageSize, 0, 0)
                };
            }
            RelationParser?.Parse(response.Content);
            return new PaginationDto<T>()
            {
                Items = response.Content.Data.Select(x => x.Attributes).ToList(),
                Paging = new(request.Page, request.PageSize, response.Content.Meta.Page.AvaiablePagesCount, response.Content.Meta.Page.TotalRecords)
            };
        }
    }
}

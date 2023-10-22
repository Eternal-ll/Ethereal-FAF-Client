namespace Ethereal.FAF.API.Client.Models.Base
{
    public static class ApiUniversalDataExtensions
    {
        public static T CastTo<T>(this ApiUniversalData entity)
            where T : ApiUniversalData, new()
        {
            var model = Activator.CreateInstance<T>();
            model.Id = entity.Id;
            model.Type = entity.Type;
            model.Included = entity.Included;
            model.Attributes = entity.Attributes;
            model.Relations = entity.Relations;
            model.Meta = entity.Meta;
            return model;
        }
    }
}

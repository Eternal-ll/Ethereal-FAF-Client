using beta.Models.API.Base;
using System;

namespace beta.Models.API
{
    public class ApiPlayerNameRecord : ApiUniversalWithAttributes
    {
        public string Name => Attributes["name"];
        public DateTime ChangedAt => DateTime.Parse(Attributes["changeTime"]);
    }
}

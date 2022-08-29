using beta.Models.API.Base;
using System;

namespace beta.Models.API
{
    public class ApiPlayerNameRecord : ApiUniversalData
    {
        public string Name => Attributes["name"];
        public DateTime ChangedAt => DateTime.Parse(Attributes["changeTime"]);
    }
}

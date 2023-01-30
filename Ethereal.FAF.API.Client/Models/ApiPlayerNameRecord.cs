using Ethereal.FAF.API.Client.Models.Base;
using System;

namespace Ethereal.FAF.API.Client.Models
{
    public class ApiPlayerNameRecord : ApiUniversalData
    {
        public string Name => Attributes["name"];
        public DateTime ChangedAt => DateTime.Parse(Attributes["changeTime"]);
    }
}

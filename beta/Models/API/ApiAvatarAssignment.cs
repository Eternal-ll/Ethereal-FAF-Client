using beta.Models.API.Base;
using System;

namespace beta.Models.API
{
    public class ApiAvatarAssignment : ApiUniversalData
    {
        public DateTime CreateTime => DateTime.Parse(Attributes["createTime"]);
        public DateTime UpdateTime => DateTime.Parse(Attributes["updateTime"]);
        public DateTime? ExpiresAt => DateTime.TryParse(Attributes["expiresAt"], out var res) ? res : DateTime.MaxValue;
        public bool IsSelected => bool.Parse(Attributes["selected"]);
    }
}

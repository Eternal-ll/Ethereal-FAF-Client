using beta.Models.API.Base;
using System;

namespace beta.Models.API
{
    public class ApiPlayerData : ApiUniversalWithRelations
    {
        public string Login => Attributes["login"];
        public DateTime CreateTime => DateTime.Parse(Attributes["createTime"]);
        public DateTime UpdateTime => DateTime.Parse(Attributes["updateTime"]);
        public string UserAgent => Attributes["userAgent"];

        public ApiUniversalArrayRelationship ClanMemberShip => Relations["clanMembership"];
        public ApiUniversalArrayRelationship Avatars => Relations["avatarAssignments"];
        public ApiUniversalArrayRelationship Names => Relations["names"];
        public ApiUniversalArrayRelationship Bans => Relations["avatarAssignments"];
    }
}

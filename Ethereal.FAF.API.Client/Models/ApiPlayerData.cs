using Ethereal.FAF.API.Client.Models.Base;
using System;

namespace Ethereal.FAF.API.Client.Models
{
    public class ApiPlayerData : ApiUniversalData
    {
        /* Example json with power 
                "createTime": "2019-04-26T05:38:24Z",
                "email": "user302176@faforever-test.com",
                "lastLogin": "2022-10-30T11:48:20Z",
                "login": "Eternal-",
                "recentIpAddress": "77.3.21.236",
                "updateTime": "2022-10-30T11:48:20Z",
                "userAgent": "downlords-faf-client"
        */
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

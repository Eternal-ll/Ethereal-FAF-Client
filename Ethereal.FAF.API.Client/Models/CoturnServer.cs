namespace beta.Models.API
{
    public class CoturnServer : Base.ApiUniversalData
    {
        public bool Active => bool.Parse(Attributes["active"]);
        public string Host => Attributes["host"];
        public string Key => Attributes["key"];
        public int Port => int.Parse(Attributes["port"]);
        public string Region => Attributes["region"];
    }
}

namespace Ethereal.FAF.LobbyServer
{
    public static class StringExtensions
    {
        public static string GetRequiredJsonRowValue(this string json, int row = 0)
        {
            // {"command": "ask_session", "version": "0.20.1+12-g2d1fa7ef.git", "user_agent": "ethereal-faf-client"}

            var data = json.Split(',');
            var value = data[row].Split(':')[1];
            value = value.Replace("\"", string.Empty);
            value = value.Trim();
            return value;
            //StringBuilder bb = new();

            //for (int i = 2; i < json.Length; i++)
            //{
            //    var letter = json[i];
            //    if (bb.Length > 0)
            //        if (json[i + 1] == '\"' || json[i + 1] == ',' || json[i + 1] == '}')
            //            break;
            //        else bb.Append(json[i + 1]);

            //    if (json[i] == ':')
            //        if (row != 1)
            //        {
            //            row--;
            //            continue;
            //        }
            //        else { }
            //    else continue;
            //    if (json[i + 1] == '\"')
            //        i++;
            //    bb.Append(json[i + 1]);
            //}
            //return bb.ToString();
        }
    }
}

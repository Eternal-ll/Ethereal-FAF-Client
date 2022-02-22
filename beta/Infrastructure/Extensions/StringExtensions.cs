using System.Text;

namespace beta.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ToJsonFormat(this string json)
        {
            int depth = 0;
            StringBuilder depthB = new();
            StringBuilder sb = new();
            for (int i = 0; i < json.Length; i++)
            {
                var letter = json[i];

                if (letter == '}')
                {
                    depth--;

                    sb.Append('\n');
                    for (int j = 0; j < (depth * 4); j++)
                    {
                        sb.Append(' ');
                    }
                }

                sb.Append(letter);


                if (letter == '{')
                {
                    depth++;

                    sb.Append('\n');
                    for (int j = 0; j < (depth * 4); j++)
                    {
                        sb.Append(' ');
                    }
                }

                if (letter == ',')
                {
                    sb.Append('\n');

                    for (int j = 0; j < (depth * 4); j++)
                    {
                        sb.Append(' ');
                    }
                }
            }
            return sb.ToString();
        }

        public static string GetRequiredJsonRowValue(this string json, int row = 1)
        {
            StringBuilder bb = new();

            for (int i = 2; i < json.Length; i++)
            {
                var letter = json[i];
                if (bb.Length > 0)
                    if (json[i + 1] == '\"' || json[i + 1] == ',' || json[i + 1] == '}')
                        break;
                    else bb.Append(json[i + 1]);

                if (json[i] == ':')
                    if (row != 1)
                    {
                        row--;
                        continue;
                    }
                    else { }
                else continue;
                if (json[i + 1] == '\"')
                    i++;
                bb.Append(json[i + 1]);
            }
            return bb.ToString();
        }
    }
}

using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using NetTopologySuite.IO.Properties;

namespace NetTopologySuite.IO.Converters
{
    internal static class Utility
    {
        internal static void SkipComments(this ref Utf8JsonReader reader)
        {
            // Skip comments
            while (reader.TokenType == JsonTokenType.Comment)
            {
                if (!reader.Read())
                {
                    break;
                }
            }
        }

        internal static bool ReadToken(this ref Utf8JsonReader reader, JsonTokenType tokenType, bool throwException = true)
        {
            if (reader.TokenType != tokenType)
            {
                if (throwException)
                    throw new JsonException(string.Format(Resources.EX_UnexpectedToken, tokenType, reader.TokenType, reader.GetString()));
                return false;
            }
            return reader.Read();
        }
    }

}

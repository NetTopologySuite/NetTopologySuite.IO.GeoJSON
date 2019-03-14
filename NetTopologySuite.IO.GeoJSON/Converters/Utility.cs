using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    internal static class Utility
    {
        internal static void SkipComments(JsonReader reader)
        {
            // Skip comments
            while (reader.TokenType == JsonToken.Comment)
            {
                if (!reader.Read())
                    break;
            }
        }
    }
}

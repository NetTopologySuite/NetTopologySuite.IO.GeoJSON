using System.IO;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    internal static class Utility
    {
        internal static void SkipComments(this JsonReader reader)
        {
            // Skip comments
            while (reader.TokenType == JsonToken.Comment)
            {
                if (!reader.Read())
                {
                    break;
                }
            }
        }

        internal static void ReadOrThrow(this JsonReader reader)
        {
            do
            {
                if (!reader.Read())
                {
                    ThrowExceptionForEndOfStream();
                }
            }
            while (reader.TokenType == JsonToken.Comment);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowExceptionForEndOfStream() => throw new EndOfStreamException();
    }
}

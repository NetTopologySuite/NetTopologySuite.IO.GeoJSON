using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.IO.Properties;

namespace NetTopologySuite.IO.Converters
{
    internal static class Utility
    {
        internal static bool ShouldWriteNullValues(this JsonSerializerOptions options)
        {
#pragma warning disable SYSLIB0020
            return options.DefaultIgnoreCondition == JsonIgnoreCondition.Never && !options.IgnoreNullValues;
#pragma warning restore SYSLIB0020
        }

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
                    ThrowForUnexpectedToken(tokenType, ref reader);
                return false;
            }
            return reader.Read();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ReadOrThrow(this ref Utf8JsonReader reader)
        {
            if (!reader.Read())
            {
                ThrowForUnexpectedEndOfStream();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AssertToken(this ref Utf8JsonReader reader, JsonTokenType requiredCurrentTokenType)
        {
            if (reader.TokenType != requiredCurrentTokenType)
            {
                ThrowForUnexpectedToken(requiredCurrentTokenType, ref reader);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowForUnexpectedEndOfStream()
            => throw new JsonException(Resources.EX_UnexpectedEndOfStream);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowForUnexpectedToken(JsonTokenType requiredNextTokenType, ref Utf8JsonReader reader)
            => throw new JsonException(string.Format(Resources.EX_UnexpectedToken, requiredNextTokenType, reader.TokenType, reader.GetString()));
    }
}

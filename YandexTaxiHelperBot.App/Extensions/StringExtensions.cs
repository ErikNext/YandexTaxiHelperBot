using System.Collections.Generic;

namespace YandexTaxiHelperBot.App.Extensions;

public static class StringExtensions
{
    public static KeyValuePair<string, string> ToKeyValuePairs(this string text)
    {
        string[] keyValuePairs = text.Split(':');

        if(keyValuePairs.Length == 2) 
            return new KeyValuePair<string, string>(keyValuePairs[0], keyValuePairs[1]);
        else
            return new KeyValuePair<string, string>(keyValuePairs[0], string.Empty);
    }
}
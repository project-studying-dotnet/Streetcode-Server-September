using Microsoft.Extensions.Localization;

namespace Streetcode.BLL.Extensions;

public static class StringLocalizerExtensions
{
    public static string GetErrorMessage(this IStringLocalizer stringLocalizer, string key, params object[] args)
    {
        return string.Format(stringLocalizer[key], args);
    }
}
using System.Text.RegularExpressions;
public class StringUtil
{
    public static string RemoveTags(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        return Regex.Replace(input, "<.*?>", string.Empty);
    }
}
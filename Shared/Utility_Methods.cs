using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MiniTwit.Shared;
public class Utility_Methods
{
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Normalize the domain
            email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                  RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                string domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException e)
        {
            return false;
        }
        catch (ArgumentException e)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }
    public static StringContent stringify_object(Object ob)
    {
        return new StringContent(
                    JsonConvert.SerializeObject(ob).ToString(),
                    Encoding.UTF8,
                    "application/json"
        );
    }

    public static String GravatarUrlStringFromEmail(string email, int size)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] md5ed = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(email.Trim().ToLower()));
        return $"http://www.gravatar.com/avatar/{Convert.ToHexString(md5ed).ToLower()}?d=identicon&s={size}";
    }

    public static String GravatarUrlStringFromEmail(string email) => GravatarUrlStringFromEmail(email, 48);
}
using System;
using System.Text.RegularExpressions;

namespace OneDriveBot.Services
{
    public static class TextUtilities
    {
        /// <summary>
        /// Extract email from a text that maybe a hyperlink HTML element 
        /// Sample data: <a href="mailto:hytyke@outlook.com">hytyke@outlook.com</a>
        /// </summary>
        /// <param name="emailText"></param>
        /// <returns></returns>
        public static string ExtractEmailFromHyperlink(string emailText)
        {
            if (!string.IsNullOrEmpty(emailText))
            {
                if (emailText.StartsWith("<a"))
                {
                    var segments = emailText.Split(new char[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length == 3)
                    {
                        return segments[1];
                    }
                }
                else
                {
                    return emailText;
                }
            }

            return string.Empty;
        }

        public static string EmailRegexString
        {
            get
            {
                return @"(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))";
            }
        }
    }
}
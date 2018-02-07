using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

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
    }
}
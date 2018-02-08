namespace OneDriveBot.Services.Response
{
    /// <summary>
    /// Represents an option in a menu option dialog.
    /// </summary>
    public class MenuOption
    {
        /// <summary>
        /// Value to be displayed on the button and on the message when clicked.
        /// </summary>
        public string DisplayValue { get; set; }

        /// <summary>
        /// Value to be replied to the bot, but not seen on the chat screen.
        /// </summary>
        public string ReturnValue { get; set; }
    }
}
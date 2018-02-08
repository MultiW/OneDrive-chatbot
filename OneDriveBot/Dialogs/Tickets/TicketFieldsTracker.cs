namespace OneDriveBot.Dialogs.Tickets
{
    using OneDriveBot.Services;
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Ticket field type. Note the values represent the processing ordering.
    /// </summary>
    public enum TicketFieldType : int
    {
        None = 0,
        Subject,
        Description,
        Platform,
        Email
    }

    /// <summary>
    /// Represents one field to be filled in a ticket creating process.
    /// </summary>
    /// <typeparam name="T">Type of the content to be filled.</typeparam>
    [Serializable]
    public class FieldDetails<T>
    {
        public string Question { get; set; }

        public T Content { get; set; }

        public bool IsFilled { get; set; }

        public string FieldRegexString { get; set; }
    }

    /// <summary>
    /// Keeps track of the ticket creating process.
    /// </summary>
    [Serializable]
    public class TicketFieldsTracker
    {
        public FieldDetails<string> Subject { get; set; }

        public FieldDetails<string> Description { get; set; }

        public FieldDetails<string> Platform { get; set; }

        public FieldDetails<string> UserEmail { get; set; }

        private TicketFieldType currentFieldType;
        private string[] ticketSummary;

        public TicketFieldsTracker()
        {
            Subject = new FieldDetails<string>
            {
                Question = "Please give a brief title for the problem you encountered.",
                Content = "",
                IsFilled = false
            };
            Description = new FieldDetails<string>
            {
                Question = "Please describe the details of problem",
                Content = "",
                IsFilled = false
            };
            Platform = new FieldDetails<string>
            {
                Question = "What was the device you used when you ran into the issue? \r\n[M:]Windows\r\n[M:]Windows Phone\r\n[M:]Mac\r\n[M:]iOS\r\n[M:]Android",
                Content = "",
                IsFilled = false,
                FieldRegexString = "Windows|Windows Phone|Mac|iOS|Android"
            };
            UserEmail = new FieldDetails<string>
            {
                Question = "Please provide an email so that we can contact you.",
                Content = "",
                IsFilled = false,
                FieldRegexString = TextUtilities.EmailRegexString
            };
            this.currentFieldType = TicketFieldType.None;
            this.ticketSummary = new string[Enum.GetNames(typeof(TicketFieldType)).Length];
        }

        /// <summary>
        /// Gets the current field to be filled.
        /// Contains mapping of enum value to the FieldDetails value.
        /// </summary>
        public FieldDetails<string> CurrentField
        {
            get
            {
                switch (this.currentFieldType)
                {
                    case TicketFieldType.Subject:
                        return Subject;
                    case TicketFieldType.Description:
                        return Description;
                    case TicketFieldType.Platform:
                        return Platform;
                    case TicketFieldType.Email:
                        return UserEmail;
                    case TicketFieldType.None:
                        return null;
                    default:
                        throw new InvalidOperationException("Invalid TicketFieldType value");
                }
            }
        }

        /// <summary>
        /// Steps to next field in enum.
        /// Returns the next field to be filled in.
        /// </summary>
        /// <returns>Field to be filled in next.</returns>
        private FieldDetails<string> MoveNext()
        {
            while (this.currentFieldType < TicketFieldType.Email)
            {
                this.currentFieldType++;
                if (!CurrentField.IsFilled)
                {
                    break;
                }
            }

            return this.CurrentField;
        }

        /// <summary>
        /// Return whether ticket creation process has started.
        /// </summary>
        public bool IsStarted
        {
            get
            {
                return this.currentFieldType != TicketFieldType.None;
            }
        }

        /// <summary>
        /// Return whether ticket creation process is done.
        /// </summary>
        public bool IsDone
        {
            get
            {
                return this.currentFieldType == TicketFieldType.Email && this.CurrentField.IsFilled;
            }
        }

        /// <summary>
        /// Begins ticket creation process.
        /// </summary>
        public void Start()
        {
            if (!IsStarted)
            {
                this.MoveNext();
            }
        }

        /// <summary>
        /// Prepares this tracking object to be ready for editing.
        /// </summary>
        /// <param name="editField">The field to be edited</param>
        public void SetupEditAnswer(TicketFieldType editField)
        {
            this.currentFieldType = editField;
            this.CurrentField.IsFilled = false;
        }

        /// <summary>
        /// Cancels editing process.
        /// </summary>
        public void CancelEditAnswer()
        {
            Subject.IsFilled = true;
            Description.IsFilled = true;
            Platform.IsFilled = true;
            UserEmail.IsFilled = true;
            currentFieldType = TicketFieldType.Email;
        }

        /// <summary>
        /// Receives user's input to a field, validates its formatting correctness, and stores it.
        /// Return validation result.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Result of field input validation</returns>
        public bool ReceiveAnswer(string text)
        {
            var currentField = this.CurrentField;

            // Check if user's answer is valid. True if no validation pattern needed.
            bool isValid = (currentField != null) && 
                (currentField.FieldRegexString == null || Regex.IsMatch(text, currentField.FieldRegexString));

            // Receive user's answer if valid.
            if (isValid)
            {
                currentField.Content = text;
                currentField.IsFilled = true;
                ticketSummary[(int)currentFieldType] = "* " + this.currentFieldType.ToString() + ": " + text + "\n\n";
                this.MoveNext();
            }

            return isValid;
        }

        /// <summary>
        /// Return summary of ticket: all fields and their filled in values.
        /// </summary>
        public string Summary
        {
            get
            {
                string summary = "";
                foreach (var line in ticketSummary)
                {
                    summary += line;
                }
                return summary;
            }
        }
    }
}
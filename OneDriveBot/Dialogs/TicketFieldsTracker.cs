namespace OneDriveBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    /// <summary>
    /// Ticket field type. Note the values represent the processing ordering.
    /// </summary>
    public enum TicketFieldType : int
    {
        None = 0,
        Subject,
        Description,
        Platform,
        Email,
    }

    [Serializable]
    public class FieldDetails<T>
    {
        public string Question { get; set; }

        public T Content { get; set; }

        public bool IsFilled { get; set; }
    }

    [Serializable]
    public class TicketFieldsTracker
    {
        private TicketFieldType currentFieldType;

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
                IsFilled = false
            };
            UserEmail = new FieldDetails<string>
            {
                Question = "Please provide an email so that we can contact you.",
                Content = "",
                IsFilled = false
            };
            this.currentFieldType = TicketFieldType.None;
        }

        public FieldDetails<string> Subject { get; set; }

        public FieldDetails<string> Description { get; set; }

        public FieldDetails<string> Platform { get; set; }

        public FieldDetails<string> UserEmail { get; set; }

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

        private FieldDetails<string> MoveNext()
        {
            if (this.currentFieldType < TicketFieldType.Email)
            {
                this.currentFieldType++;
            }

            return this.CurrentField;
        }

        public bool IsStarted
        {
            get
            {
                return this.currentFieldType != TicketFieldType.None;
            }
        }

        public bool IsDone
        {
            get
            {
                return this.currentFieldType == TicketFieldType.Email && this.CurrentField.IsFilled;
            }
        }

        public void Start()
        {
            if (!IsStarted)
            {
                this.MoveNext();
            }
        }

        public void ReceiveAnswer(string text)
        {
            var currentField = this.CurrentField;
            if (currentField != null)
            {
                currentField.Content = text;
                currentField.IsFilled = true;
                this.MoveNext();
            }
        }
    }
}
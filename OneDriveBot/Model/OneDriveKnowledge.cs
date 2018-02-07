namespace OneDriveBot.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;

    public class OneDriveKnowledge
    {
        [Key]
        public string Intent { get; set; }
        public string ResponseType { get; set; }
        public string Response { get; set; }
    }
}
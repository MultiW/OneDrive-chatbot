using System;
using System.ComponentModel.DataAnnotations;

namespace OneDriveBot.Model
{
    [Serializable]
    public class OneDriveKnowledge
    {
        [Key]
        public string Intent { get; set; }
        public string ResponseType { get; set; }
        public string Response { get; set; }
    }
}
using System;

namespace ESG.Models
{
    public class ChangeLog
    {
        public int LogId { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string ActionType { get; set; }
        public string Details { get; set; }
        public int UserId { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string FormattedChangedAt => ChangedAt.ToString("dd.MM.yyyy HH:mm:ss");
    }
} 
namespace IKBR_Report_Puller.Domain
{
    public class Position 
    {        
        public int Id { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int InstrumentId { get; set; }
    }
}

namespace MessageApp.API.Models.Messages
{
    public class MessageResponseModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SequenceNumber { get; set; }
    }
}
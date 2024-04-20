namespace PostHubAPI.Models
{
    public class Picture
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string MimeType { get; set; } = null!;

        public virtual int CommentId { get; set; }
        public virtual Comment Comment { get; set; }
    }
}

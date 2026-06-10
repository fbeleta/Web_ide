namespace WebIde.Web.DTOs;

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProblemId { get; set; }
    public string DownloadUrl { get; set; } = "";
}

namespace Streetcode.BLL.Dto.Streetcode.TextContent.Comment;

public class CommentDto
{
    public int Id { get; set; }
    public string? CommentContent { get; set; }
    public int StreetcodeId { get; set; }
    public int UserId { get; set; }
    public DateTime DateCreated { get; set; }
    public int? ParentCommentId { get; set; }
}

using Streetcode.DAL.Enums;

namespace Streetcode.BLL.Dto.Streetcode.TextContent.Comment
{
    public class CommentWithRepliesDto
    {
        public int Id { get; set; }
        public string? CommentContent { get; set; }
        public int StreetcodeId { get; set; }
        public int UserId { get; set; }
        public DateTime DateCreated { get; set; }
        public int? ParentCommentId { get; set; }
        
        public CommentStatus Status { get; set; }
        
        public List<CommentWithRepliesDto> Replies { get; set; } = new List<CommentWithRepliesDto>();
    }
}

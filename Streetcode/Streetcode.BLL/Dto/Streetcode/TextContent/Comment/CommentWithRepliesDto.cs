using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.Dto.Streetcode.TextContent.Comment
{
    public class CommentWithRepliesDto : CommentDto
    {
        public int Id { get; set; }
        public string? CommentContent { get; set; }
        public int StreetcodeId { get; set; }
        public int UserId { get; set; }
        public DateTime DateCreated { get; set; }
        public int? ParentCommentId { get; set; }
        public List<CommentWithRepliesDto> Replies { get; set; } = new List<CommentWithRepliesDto>();
    }
}

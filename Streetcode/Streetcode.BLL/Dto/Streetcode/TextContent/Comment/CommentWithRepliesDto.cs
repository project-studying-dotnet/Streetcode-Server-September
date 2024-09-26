using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.Dto.Streetcode.TextContent.Comment
{
    public class CommentWithRepliesDto : CommentDto
    {
        public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
    }
}

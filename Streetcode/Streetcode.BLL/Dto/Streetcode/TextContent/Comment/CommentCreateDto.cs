using Streetcode.DAL.Entities.Streetcode;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Streetcode.BLL.Dto.Streetcode.TextContent.Comment;

public class CommentCreateDto
{
    public string CommentContent { get; set; } = null!;

    public int StreetcodeId { get; set; }

    public int? ParentCommentId { get; set; }

    public int UserId { get; set; }

    public DateTime DateCreated { get; set; }
}

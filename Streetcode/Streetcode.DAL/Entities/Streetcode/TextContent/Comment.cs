using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Streetcode.DAL.Enums;

namespace Streetcode.DAL.Entities.Streetcode.TextContent;

[Table("comments", Schema = "streetcode")]
public class Comment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(1000)]
    public string? CommentContent { get; set; }

    [Required]
    public int StreetcodeId { get; set; }

    public StreetcodeContent? Streetcode { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public DateTime DateCreated { get; set; }

    public int? ParentCommentId { get; set; }
    
    public CommentStatus Status { get; set; }

    [ForeignKey("ParentCommentId")]
    public Comment? ParentComment { get; set; }
    
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
}

using AutoMapper;
using Streetcode.BLL.Dto.Streetcode.TextContent.Comment;
using Streetcode.BLL.Mapping.Streetcode.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Streetcode.XUnitTest.MappingTests.Streetcode.Comment
{
    public class CommentProfileTests
    {
        private readonly IMapper _mapper;

        public CommentProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<CommentProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Should_Map_Comment_To_CommentDto()
        {
            // Arrange
            var comment = new DAL.Entities.Streetcode.TextContent.Comment
            {
                Id = 1,
                CommentContent = "Test Comment",
                StreetcodeId = 100,
                UserId = 200,
                DateCreated = DateTime.UtcNow,
                ParentCommentId = null
            };

            // Act
            var commentDto = _mapper.Map<CommentDto>(comment);

            // Assert
            Assert.Equal(comment.Id, commentDto.Id);
            Assert.Equal(comment.CommentContent, commentDto.CommentContent);
            Assert.Equal(comment.StreetcodeId, commentDto.StreetcodeId);
            Assert.Equal(comment.UserId, commentDto.UserId);
            Assert.Equal(comment.DateCreated, commentDto.DateCreated);
            Assert.Equal(comment.ParentCommentId, commentDto.ParentCommentId);
        }

        [Fact]
        public void Should_Map_CommentDto_To_Comment()
        {
            // Arrange
            var commentDto = new CommentDto
            {
                Id = 1,
                CommentContent = "Test Comment",
                StreetcodeId = 100,
                UserId = 200,
                DateCreated = DateTime.UtcNow,
                ParentCommentId = null
            };

            // Act
            var comment = _mapper.Map<DAL.Entities.Streetcode.TextContent.Comment>(commentDto);

            // Assert
            Assert.Equal(commentDto.Id, comment.Id);
            Assert.Equal(commentDto.CommentContent, comment.CommentContent);
            Assert.Equal(commentDto.StreetcodeId, comment.StreetcodeId);
            Assert.Equal(commentDto.UserId, comment.UserId);
            Assert.Equal(commentDto.DateCreated, comment.DateCreated);
            Assert.Equal(commentDto.ParentCommentId, comment.ParentCommentId);
        }

        [Fact]
        public void Should_Map_Comment_To_CommentWithRepliesDto()
        {
            // Arrange
            var comment = new DAL.Entities.Streetcode.TextContent.Comment
            {
                Id = 1,
                CommentContent = "Parent Comment",
                StreetcodeId = 100,
                UserId = 200,
                DateCreated = DateTime.UtcNow,
                ParentCommentId = null,
                Replies = new List<DAL.Entities.Streetcode.TextContent.Comment>
                {
                    new DAL.Entities.Streetcode.TextContent.Comment
                    {
                        Id = 2,
                        CommentContent = "Reply Comment",
                        StreetcodeId = 100,
                        UserId = 201,
                        DateCreated = DateTime.UtcNow,
                        ParentCommentId = 1
                    }
                }
            };

            // Act
            var commentWithRepliesDto = _mapper.Map<CommentWithRepliesDto>(comment);

            // Assert
            Assert.Equal(comment.Id, commentWithRepliesDto.Id);
            Assert.Equal(comment.CommentContent, commentWithRepliesDto.CommentContent);
            Assert.Equal(comment.StreetcodeId, commentWithRepliesDto.StreetcodeId);
            Assert.Equal(comment.UserId, commentWithRepliesDto.UserId);
            Assert.Equal(comment.DateCreated, commentWithRepliesDto.DateCreated);
            Assert.Equal(comment.ParentCommentId, commentWithRepliesDto.ParentCommentId);

            Assert.NotNull(commentWithRepliesDto.Replies);
            Assert.Empty(commentWithRepliesDto.Replies);
        }
    }
}

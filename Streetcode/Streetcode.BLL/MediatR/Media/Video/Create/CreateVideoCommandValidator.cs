using System.Text.RegularExpressions;
using FluentValidation;

namespace Streetcode.BLL.MediatR.Media.Video.Create;

public class CreateVideoCommandValidator: AbstractValidator<CreateVideoCommand>
{
    private readonly Regex _youtubeLinkRegex = new (@"^(https?:\/\/)?(www\.)?(youtube\.com|youtu\.?be)\/.+$");

    public CreateVideoCommandValidator()
    {
        RuleFor(x => x.VideoCreateDto.Title)
            .MaximumLength(100)
            .WithMessage("Video Title should nor exceed 100 chars");
        
        RuleFor(x => x.VideoCreateDto.Url)
            .NotEmpty()
            .WithMessage("Youtube url is required")
            .Must(IsYoutubeLink)
            .WithMessage("Video url must be in youtube.com domain");
    }

    private bool IsYoutubeLink(string videoUrl) => _youtubeLinkRegex.IsMatch(videoUrl);
}
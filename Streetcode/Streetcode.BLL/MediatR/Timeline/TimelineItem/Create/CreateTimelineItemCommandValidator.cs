using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Timeline.TimelineItem.Create
{
    public class CreateTimelineItemCommandValidator: AbstractValidator<CreateTimelineItemCommand>
    {
        public CreateTimelineItemCommandValidator()
        {
            RuleFor(x => x.timelineItemCreateDto.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(26).WithMessage("Title must be at most 26 characters long.");

            RuleFor(x => x.timelineItemCreateDto.Description)
                .MaximumLength(400).WithMessage("Description must be at most 400 characters long.");

            RuleFor(x => x.timelineItemCreateDto.Date)
                .NotEmpty().WithMessage("Date is required.");

            RuleFor(x => x.timelineItemCreateDto.DateViewPattern)
                .IsInEnum().WithMessage("Invalid DateViewPattern value.");

            RuleFor(x => x.timelineItemCreateDto.StreetcodeId)
                .GreaterThan(0).WithMessage("StreetcodeId must be greater than 0.");
        }
    }
}

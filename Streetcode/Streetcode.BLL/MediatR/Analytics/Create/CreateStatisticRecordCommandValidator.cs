using FluentValidation;

namespace Streetcode.BLL.MediatR.Analytics.Create;

public class CreateStatisticRecordCommandValidator: AbstractValidator<CreateStatisticRecordCommand>
{
    public CreateStatisticRecordCommandValidator() 
    {
        RuleFor(x => x.newStreetcodeRecord.QrId)
            .GreaterThan(0)
            .WithMessage("A QR table number must be a positive integer.");

        RuleFor(x => x.newStreetcodeRecord.Address)
             .NotEmpty().WithMessage("Address is required.")
             .MaximumLength(150).WithMessage("Address must be at most 150 characters long.");

        RuleFor(x => x.newStreetcodeRecord.StreetcodeCoordinate)
           .NotNull().WithMessage("StreetcodeCoordinate is required.");

        RuleFor(x => x.newStreetcodeRecord.StreetcodeCoordinate.Latitude)
            .NotNull().WithMessage("Latitude is required.");

        RuleFor(x => x.newStreetcodeRecord.StreetcodeCoordinate.Longtitude)
            .NotNull().WithMessage("Longtitude is required.");

        RuleFor(x => x.newStreetcodeRecord.StreetcodeCoordinate.StreetcodeId)
            .GreaterThan(0).WithMessage("StreetcodeId must be greater than 0.");
    }
}

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Streetcode.BLL.ValidationBehavior
{
    public class ValidationPipelineBehavior<TRequest, TResponse>: IPipelineBehavior<TRequest, TResponse> 
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);

            var failures = _validators
                .Select(x => x.Validate(context))
                .SelectMany(x => x.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                Console.WriteLine($"Validation failed with {failures.Count} errors"); //
                foreach (var failure in failures)
                {
                    Console.WriteLine($"Property: {failure.PropertyName}, Error: {failure.ErrorMessage}");
                }
                throw new ValidationException(failures);
            }

            return next();
        }

    }
}

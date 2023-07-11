using System.Text.RegularExpressions;
using FluentValidation;

namespace WebCrawler.Controllers;

public class CreateRecordRequestDto
{
    public string Label { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Regex { get; set; } = null!;
    public int Periodicity { get; set; }
    public List<string> Tags { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class CreateRecordRequestDtoValidator : AbstractValidator<CreateRecordRequestDto>
{
    public CreateRecordRequestDtoValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Url).NotEmpty()
            .Must(x => Uri.TryCreate(x, UriKind.Absolute, out var uriResult) && 
                       (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps) &&
                       !string.IsNullOrEmpty(uriResult.Host) &&
                       !string.IsNullOrEmpty(uriResult.PathAndQuery) &&
                       !string.IsNullOrEmpty(uriResult.GetLeftPart(UriPartial.Authority)));
        RuleFor(x => x.Regex).NotEmpty();
        RuleFor(x => x.Periodicity).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Tags).Must(x => x.Count < 50);
        // Not tag can be null or empty
        RuleForEach(x => x.Tags).NotEmpty().MaximumLength(30);
        // We need to check if the regex is valid, aka if it can be compiled
        RuleFor(x => x.Regex).Must(x =>
        {
            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new Regex(x);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        });
        
    }
}
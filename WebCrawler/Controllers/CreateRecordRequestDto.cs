using FluentValidation;

namespace WebCrawler.Controllers;

public class CreateRecordRequestDto
{
    public string Label { get; set; }
    public string Url { get; set; }
    public string Regex { get; set; }
    public int Periodicity { get; set; }
    public List<string> Tags { get; set; }
    public bool IsActive { get; set; }
}

public class CreateRecordRequestDtoValidator : AbstractValidator<CreateRecordRequestDto>
{
    public CreateRecordRequestDtoValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Url).NotEmpty()
            .Must(x => Uri.TryCreate(x, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps));
        RuleFor(x => x.Regex).NotEmpty();
        RuleFor(x => x.Periodicity).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Tags).Must(x => x.Count < 50);
        // Not tag can be null or empty
        RuleForEach(x => x.Tags).NotEmpty().MaximumLength(30);
    }
}
using Microsoft.AspNetCore.Mvc;

namespace WebCrawler.Controllers;

public abstract class OurController : Controller
{
    public static readonly int InternalErrorCode = 500;
    public static readonly int NotImplementedCode = 501;
}

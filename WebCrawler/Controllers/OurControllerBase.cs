using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebCrawler.Controllers;

[ApiController]
[Route("[controller]")]
public abstract class OurControllerBase : ControllerBase
{
    protected static int InternalErrorCode => 500;
    protected static int NotImplementedCode => 501;
    protected static int BadRequestCode => (int)HttpStatusCode.BadRequest;
}

using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebCrawler.Controllers;

public abstract class OurController : Controller
{
    protected static int InternalErrorCode => 500;
    protected static int NotImplementedCode => 501;
    protected static int BadRequestCode => (int)HttpStatusCode.BadRequest;
}

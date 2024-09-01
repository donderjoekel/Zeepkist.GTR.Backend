using Microsoft.AspNetCore.Mvc;

namespace TNRD.Zeepkist.GTR.Backend.Upvotes;

[ApiController]
[Route("[controller]")]
public class UpvotesController : ControllerBase
{
    private readonly IUpvotesService _service;

    public UpvotesController(IUpvotesService service)
    {
        _service = service;
    }
}

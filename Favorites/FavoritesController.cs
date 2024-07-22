using Microsoft.AspNetCore.Mvc;

namespace TNRD.Zeepkist.GTR.Backend.Favorites;

[ApiController]
[Route("[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoritesService _service;

    public FavoritesController(IFavoritesService service)
    {
        _service = service;
    }
}

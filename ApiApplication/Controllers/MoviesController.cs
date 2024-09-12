using ApiApplication.ApiClient;
using ApiApplication.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private IApiClientGrpc _grpcClient;

        public MoviesController(IApiClientGrpc apiClientGrpc)
        {
            _grpcClient = apiClientGrpc;
        }

        [HttpGet]
        [LogExecutionTime]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _grpcClient.GetAll());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            return Ok(await _grpcClient.GetById(id));
        }

        [HttpGet("search/{search}")]
        public async Task<IActionResult> Search(string search)
        {
            return Ok(await _grpcClient.Search(search));
        }
    }
}

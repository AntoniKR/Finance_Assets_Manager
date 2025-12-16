using Microsoft.AspNetCore.Mvc;
using FinancialAssetsApp.Models;
using FinancialAssetsApp.Data.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinancialAssetsApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetalsApiController : ControllerBase
    {
        private readonly IMetalsService _metalsService;

        public MetalsApiController(IMetalsService metalsService)
        {
            _metalsService = metalsService;
        }

        //  GET api/metals/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserMetals(int userId)
        {
            var metals = await _metalsService.GetAssetsByID(userId);
            return Ok(metals);
        }

        // GET api/metals/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var metal = await _metalsService.GetAssetById(id);
            return metal == null ? NotFound() : Ok(metal);
        }

        // POST api/metals
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Metal metal)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            await _metalsService.Add(metal);
            return CreatedAtAction(nameof(Get), new { id = metal.Id }, metal);
        }

        // DELETE api/metals/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var metal = await _metalsService.GetAssetById(id);
            if (metal == null)
                return NotFound();

            await _metalsService.Delete(id);
            return NoContent();
        }
    }
}

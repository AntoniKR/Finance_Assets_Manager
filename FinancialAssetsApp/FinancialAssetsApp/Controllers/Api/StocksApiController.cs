using Microsoft.AspNetCore.Mvc;
using FinancialAssetsApp.Models;

using FinancialAssetsApp.Data.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FinancialAssetsApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class StocksApiController : ControllerBase
    {
        private readonly IStocksService _stocksService;

        public StocksApiController(IStocksService stocksService)
        {
            _stocksService = stocksService;
        }

        //  GET api/stocks/user/5
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserStocks(int userId)
        {
            var stocks = await _stocksService.GetAssetsByID(userId);
            return Ok(stocks);
        }

        // GET api/stocks/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var stock = await _stocksService.GetAssetById(id);
            return stock == null ? NotFound() : Ok(stock);
        }

        // POST api/stocks
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Stock stock)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            await _stocksService.Add(stock);
            return CreatedAtAction(nameof(Get), new { id = stock.Id }, stock);
        }
        
        // DELETE api/stocks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var stock = await _stocksService.GetAssetById(id);
            if (stock == null)
                return NotFound();

            await _stocksService.Delete(id);
            return NoContent();
        }
    }
}

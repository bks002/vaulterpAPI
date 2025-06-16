using Microsoft.AspNetCore.Mvc;
using vaulterpAPI.Models;
namespace vaulterpAPI.Controllers.Inventory
{
    [ApiController]
    [Route("api/inventory/[controller]")]
    public class Item : ControllerBase
    {
        // Dummy data
        private static readonly List<ItemModel> items = new List<ItemModel>
        {
            new ItemModel { Id = 1, Name = "Hammer", Quantity = 50 },
            new ItemModel { Id = 2, Name = "Screwdriver", Quantity = 120 },
            new ItemModel { Id = 3, Name = "Wrench", Quantity = 80 }
        };

        [HttpGet]
        public ActionResult<IEnumerable<ItemModel>> Get()
        {
            return Ok(items);
        }
    }
}
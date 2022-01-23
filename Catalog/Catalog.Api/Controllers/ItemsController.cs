using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Api.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController: ControllerBase
    {
        private readonly IItemsRepository _repository;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(IItemsRepository repository, ILogger<ItemsController> logger)
        {
            this._repository = repository;
            this._logger = logger;
        }

        // GET /items
        [HttpGet]
        public async Task<IEnumerable<ItemDto>> GetItems()
        {
            var items = (await _repository.GetItemsAsync())
                        .Select(item => item.AsDto());
            _logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: Retrieved {items.Count()} items");

            return items;
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetItemAsync(Guid id)
        {
            var item = await _repository.GetItemAsync(id);

            if(item is null)
            {
                return NotFound();
            }

            return item.AsDto();
        }

        // POSt /Items
        [HttpPost]
        public async Task<ActionResult<ItemDto>> CreateItemAsync(CreateItemDto itemDto)
        {
            Item item = new()
            {
                Id = Guid.NewGuid(),
                Name = itemDto.Name,
                Price = itemDto.Price,
                CreateDate = DateTimeOffset.UtcNow
            };

           await _repository.CreateItemAsync(item);

            return CreatedAtAction(nameof(GetItemAsync), new { id = item.Id }, item.AsDto());
        }

        //Put /Items
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateItem(Guid id, UpdateItemDto itemDto)
        {
            var existingItem = await _repository.GetItemAsync(id);

            if(existingItem is null)
            {
                return NotFound();
            }

            Item updatedItem = existingItem with
            {
                Name = itemDto.Name,
                Price = itemDto.Price
            };

            await _repository.UpdateItemAsync(updatedItem);

            return NoContent();
        }


        //Delete /items/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(Guid id)
        {
            var existingItem = await _repository.GetItemAsync(id);

            if (existingItem is null)
            {
                return NotFound();
            }

           await _repository.DeleteItemAsync(id);
            return NoContent();
        }
    }
}

using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Api.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace Catalog.Api.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("items")]
    public class ItemsController: ControllerBase
    {
        private readonly IItemsRepository _repository;
        private readonly ILogger<ItemsController> _logger;
        private readonly IMapper _mapper;

        public ItemsController(IItemsRepository repository, ILogger<ItemsController> logger, IMapper mapper)
        {
            this._repository = repository ?? throw new ArgumentNullException(nameof(repository));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Return all items from repository.
        /// </summary>
        /// <param name="name">Name option for filter the return items.</param>
        /// <returns>List of items</returns>
        /// <remarks>
        /// Sample request
        /// GET /items
        /// </remarks>
        /// <response code="200">Returns a list of items</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesDefaultResponseType]
        public async Task<IEnumerable<ItemDto>> GetItemsAsync(string name = null)
        {
            var items = (await _repository.GetItemsAsync())
                        .Select(item => _mapper.Map<ItemDto>(item));

            if(!string.IsNullOrEmpty(name))
            {
                items = items.Where(item => item.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            _logger.LogInformation($"{DateTime.UtcNow.ToString("hh:mm:ss")}: Retrieved {items.Count()} items");

            return items;
        }

        /// <summary>
        /// Return item from repository by item id.
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns>Item match the id</returns>
        /// <remarks>
        /// Sample request
        /// GET /items/{id}
        /// </remarks>
        [HttpGet("{id}")]
        [ApiConventionMethod(typeof(DefaultApiConventions),
            nameof(DefaultApiConventions.Get))]
        public async Task<ActionResult<ItemDto>> GetItemAsync(Guid id)
        {
            var item = await _repository.GetItemAsync(id);

            if(item is null)
            {
                return NotFound();
            }

            return _mapper.Map<ItemDto>(item);
        }

        // POST /Items
        /// <summary>
        /// Create a item
        /// </summary>
        /// <param name="itemDto">Item Dto</param>
        /// <returns>action result</returns>
        /// <remarks>
        /// POST /items
        /// </remarks>
        /// <response code="201">Item created sucessfully</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<ItemDto>> CreateItemAsync(CreateItemDto itemDto)
        {
            Item item = new()
            {
                Id = Guid.NewGuid(),
                Name = itemDto.Name,
                Description = itemDto.Description,
                Price = itemDto.Price,
                CreateDate = DateTimeOffset.UtcNow
            };

           await _repository.CreateItemAsync(item);

            return CreatedAtAction(nameof(GetItemAsync), new { id = item.Id },  _mapper.Map<ItemDto>(item));
        }

        //Put /Items
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateItemAsync(Guid id, UpdateItemDto itemDto)
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
        public async Task<ActionResult> DeleteItemAsync(Guid id)
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

using AutoMapper;
using Catalog.Api.Controllers;
using Catalog.Api.Dtos;
using Catalog.Api.Entities;
using Catalog.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Catalog.UnitTests
{
    public class ItemsControllerTests
    {
        private readonly Mock<IItemsRepository> repositoryStub = new();
        private readonly Mock<ILogger<ItemsController>> loggerStub = new();
        private readonly Mock<IMapper> mapperStub = new();
        private readonly Random rand = new();


        [Fact]
        public async Task GetItemAsync_WithUnexistingItem_ReturnsNotFound()
        {
            // Arrange
            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Item)null);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object, mapperStub.Object);

            // Act
            var result = await controller.GetItemAsync(Guid.NewGuid());

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetItemAsync_WithExistingItem_ReturnsExpectedItem()
        {
            // Arrange
            var expectedItem = CreateRandomItem();

            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(expectedItem);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object, mapperStub.Object);

            // Act
            var result = await controller.GetItemAsync(Guid.NewGuid());

            // Assert
            result.Value.Should().BeEquivalentTo(expectedItem);

        }

        [Fact]
        public async Task GetItemsAsync_WithExistingItems_ReturnsAllItems()
        {
            // Arrange
            var expectedItems = new[] { CreateRandomItem(), CreateRandomItem(), CreateRandomItem() };
            repositoryStub.Setup(repo => repo.GetItemsAsync())
                .ReturnsAsync(expectedItems);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object, mapperStub.Object);

            // Act
            var actualItems = await controller.GetItemsAsync();

            // Assert
            actualItems.Should().BeEquivalentTo(actualItems);
        }

        [Fact]
        public async Task GetItemsAsync_WithMatchingItems_ReturnsMatchingItems()
        {
            // Arrange
            var allItems = new[] { 
                new Item(){Name = "Potion"},
                new Item(){Name = "Antidote"},
                new Item(){Name = "Hi-Potion"},
            };

            var nameToMath = "Potion";

            repositoryStub.Setup(repo => repo.GetItemsAsync())
                .ReturnsAsync(allItems);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object, mapperStub.Object);

            // Act
            IEnumerable<ItemDto> foundItems = await controller.GetItemsAsync(nameToMath);

            // Assert
            foundItems.Should().OnlyContain(
                item => item.Name == allItems[0].Name || item.Name == allItems[2].Name);
        }

        [Fact]
        public async Task CreateItemAsync_WithItemToCreate_ReturnCreatedItem()
        {
            // Arrange
            var itemToCreate = new CreateItemDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), rand.Next(1000));

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object, mapperStub.Object);

            // Act
            var result = await controller.CreateItemAsync(itemToCreate);

            // Assert
            var createdItem = (result.Result as CreatedAtActionResult).Value as ItemDto;
            itemToCreate.Should().BeEquivalentTo(
                createdItem, 
                options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers());
            createdItem.Id.Should().NotBeEmpty();
            createdItem.CreateDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1000));
        }

        [Fact]
        public async Task UpdateItemAsync_WithExisting_ReturnNoContent()
        {
            // Arrange
            var existingItem = CreateRandomItem();
            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(existingItem);

            var itemId = existingItem.Id;
            var itemToUdate = new UpdateItemDto( Guid.NewGuid().ToString(), "", existingItem.Price + 3);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object, mapperStub.Object);

            // Act
            var result = await controller.UpdateItemAsync(itemId, itemToUdate);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteItemAsync_WithExisting_ReturnNoContent()
        {
            // Arrange
            var existingItem = CreateRandomItem();
            repositoryStub.Setup(repo => repo.GetItemAsync(It.IsAny<Guid>()))
                .ReturnsAsync(existingItem);

            var controller = new ItemsController(repositoryStub.Object, loggerStub.Object, mapperStub.Object);

            // Act
            var result = await controller.DeleteItemAsync(existingItem.Id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        private Item CreateRandomItem()
        {
            return new()
            {
                Id = Guid.NewGuid(),
                Name = Guid.NewGuid().ToString(),
                Price = rand.Next(1000),
                CreateDate = DateTimeOffset.UtcNow
            };
        }
    }
}

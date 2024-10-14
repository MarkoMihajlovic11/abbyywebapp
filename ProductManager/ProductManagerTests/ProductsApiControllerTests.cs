using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using ProductManager.Controllers;
using ProductManager.Data;
using ProductManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagerTests
{
    public class ProductsApiControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductsApiController _controller;

        public ProductsApiControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ApiTestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new ProductsApiController(_context);
        }

        [Fact]
        public async Task GetProducts_ReturnsProductList()
        {
            // Arrange
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10, Description = "Description 1" },
                new Product { Id = 2, Name = "Product 2", Price = 20, Description = "Description 2" }
            };

            await _context.Products.AddRangeAsync(mockProducts);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetProducts();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<Product>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count());
        }

        [Fact]
        public async Task GetProduct_ReturnsProduct_WhenIdExists()
        {
            // Arrange
            var mockProduct = new Product { Id = 10, Name = "Product 10", Price = 10, Description = "Description 10" };
            await _context.Products.AddAsync(mockProduct);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetProduct(10);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var product = Assert.IsType<Product>(actionResult.Value);
            Assert.Equal(10, product.Id);
            Assert.Equal("Product 10", product.Name);
        }

        [Fact]
        public async Task GetProduct_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Act
            var result = await _controller.GetProduct(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateProduct_AddsProduct()
        {
            // Arrange
            var newProduct = new Product { Id = 3, Name = "Product 3", Price = 30, Description = "Description 3" };

            // Act
            var result = await _controller.CreateProduct(newProduct);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<Product>(createdAtActionResult.Value);
            Assert.Equal(3, returnValue.Id);
            Assert.Equal("Product 3", returnValue.Name);
        }

        [Fact]
        public async Task UpdateProduct_UpdatesProduct_WhenIdIsValid()
        {
            // Arrange
            var existingProduct = new Product { Id = 7, Name = "Product 7", Price = 10, Description = "Description 7" };
            await _context.Products.AddAsync(existingProduct);
            await _context.SaveChangesAsync();

            existingProduct.Name = "Updated Product 7";
            existingProduct.Price = 30;
            existingProduct.Description = "Updated Description 7";

            // Act
            var result = await _controller.UpdateProduct(7, existingProduct);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var productInDb = await _context.Products.FindAsync(7);
            Assert.Equal("Updated Product 7", productInDb.Name);
            Assert.Equal("Updated Description 7", productInDb.Description);
            Assert.Equal(30, productInDb.Price);
        }

        [Fact]
        public async Task UpdateProduct_ReturnsBadRequest_WhenIdIsInvalid()
        {
            // Arrange
            var updatedProduct = new Product { Id = 1, Name = "Updated Product", Price = 15, Description = "Updated Description" };

            // Act
            var result = await _controller.UpdateProduct(2, updatedProduct);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_RemovesProduct_WhenIdExists()
        {
            // Arrange
            var existingProduct = new Product { Id = 6, Name = "Product 6", Price = 10, Description = "Description 6" };
            await _context.Products.AddAsync(existingProduct);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteProduct(6);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var productInDb = await _context.Products.FindAsync(6);
            Assert.Null(productInDb);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteProduct(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // Cleanup: Dispose of the in-memory database after each test
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
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
    public class ProductsControllerTests
    {
        private readonly ProductsController _controller;
        private readonly ApplicationDbContext _context;

        public ProductsControllerTests()
        {
            // Set up the in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            // Ensure the database is cleared between tests
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed the in-memory database with some test data
            SeedTestData(_context);

            _controller = new ProductsController(_context);
        }

        private void SeedTestData(ApplicationDbContext context)
        {
            // Seed some initial test products into the in-memory database
            context.Products.AddRange(
                new Product { Name = "Product 1", Price = 100, Description = "Description 1" },
                new Product { Name = "Product 2", Price = 200, Description = "Description 2" }
            );
            context.SaveChanges();
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfProducts()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Product>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithProduct()
        {
            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Product>(viewResult.ViewData.Model);
            Assert.Equal("Product 1", model.Name);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenProductNotFound()
        {
            // Act
            var result = await _controller.Details(99); // Product with Id 99 doesn't exist

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_RedirectsToIndex_OnValidModel()
        {
            // Arrange
            var product = new Product { Name = "Product 3", Price = 300, Description = "Description 3" };

            // Act
            var result = await _controller.Create(product);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            // Ensure the product was added to the in-memory database
            Assert.Equal(3, _context.Products.Count());
        }

        [Fact]
        public async Task Create_ReturnsView_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = await _controller.Create(new Product());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.ViewData.Model);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenIdIsNull()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_ReturnsNotFound_WhenProductNotFound()
        {
            // Act
            var result = await _controller.Edit(99); // Product with Id 99 doesn't exist

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_RedirectsToIndex_OnValidEdit()
        {
            // Arrange
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == 1);
            existingProduct.Name = "Updated Product";
            existingProduct.Price = 150;
            existingProduct.Description = "Updated Description";

            // Act
            var result = await _controller.Edit(1, existingProduct);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            // Verify changes in the in-memory database
            var updatedProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == 1);
            Assert.Equal("Updated Product", updatedProduct.Name);
        }

        [Fact]
        public async Task DeleteConfirmed_RedirectsToIndex_AfterDeletingProduct()
        {
            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            // Ensure the product was removed from the in-memory database
            Assert.Equal(1, _context.Products.Count());
        }
    }
}
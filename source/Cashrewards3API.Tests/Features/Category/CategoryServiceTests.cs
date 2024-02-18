using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Cashrewards3API.Common;
using Cashrewards3API.Common.Dto;
using Cashrewards3API.Common.Services;
using Cashrewards3API.Common.Utils;
using Cashrewards3API.Features.Category;
using Cashrewards3API.Features.Category.Interface;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Tests.Helpers;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace Cashrewards3API.Tests.Features.Category
{
    public class CategoryServiceTests
    {
        private class TestState
        {
            public CategoryService CategoryService { get; set; }

            public List<CategoryModel> CategoryModels { get; } = new List<CategoryModel>()
            {
                new CategoryModel()
                {
                    CategoryId = 1,
                    Name = "Category-1",
                    Status = 2,
                    HyphenatedString = "Merchant-1",
                    ClientId = Constants.Clients.CashRewards,
                },
                new CategoryModel()
                {
                    CategoryId = 2,
                    Name = "Category-2",
                    Status = 1,
                    HyphenatedString = "Merchant-2",
                    ClientId = Constants.Clients.Blue,
                }
            };

            private object _clientIdParams = new
            {
                ClientId = new List<int>
                {
                    Constants.Clients.CashRewards, 
                    Constants.Clients.Blue,
                }
            };

            public TestState()
            {
                var configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection().Build();

                var cacheConfig = new CacheConfig()
                {
                    CategoryDataExpiry = 1
                };

                var categoryRepository = new Mock<ICategoryRepository>();

                categoryRepository
                    .Setup(s => s.GetCategoriesByClientIdAsync(
                                            It.IsAny<List<int>>()))
                    .ReturnsAsync((List<int> ids) => CategoryModels.Where(category => 
                                                category.ClientId != null 
                                                && ids.Contains(category.ClientId.Value)));

                categoryRepository
                    .Setup(s => s.GetCategoriesByClientIdAndStatusAsync(
                        It.IsAny<List<int>>(), It.IsAny<Status>()))
                    .ReturnsAsync((List<int> ids, Status status) => CategoryModels.Where(category =>
                        category.ClientId != null
                        && ids.Contains(category.ClientId.Value)
                        && (int)status == category.Status));

                
                CategoryService = new CategoryService(
                    configuration,
                    new RedisUtilMock().Setup<IEnumerable<CategoryDto>>().Object,
                    Mock.Of<ICacheKey>(),
                    cacheConfig,
                    categoryRepository.Object
                );
            }    
        }

        [Test]
        public async Task GetRootCategoriesAsync_ShouldReturnRootCategories_GivenStandardAndPremiumClientIds()
        {
            var state = new TestState();
            var result = await state.CategoryService.GetRootCategoriesAsync(Constants.Clients.CashRewards, Constants.Clients.Blue,
                Status.All);

            var expectedCategoryDtos = new List<CategoryDto>()
            {
                new CategoryDto()
                {
                    Id = 1,
                    Status = 2,
                    HyphenatedString = "Merchant-1",
                    Name = "Category-1"
                },
                new CategoryDto()
                {
                    Id = 2,
                    Status = 1,
                    HyphenatedString = "Merchant-2",
                    Name = "Category-2"
                }
            };

            result.Should().BeEquivalentTo(expectedCategoryDtos);
        }

        [Test]
        public async Task GetRootCategoriesAsync_ShouldNotReturnDuplicateRootCategories_GivenCategoryExistsForBothClientIds()
        {
            var state = new TestState();
            state.CategoryModels.Add(new CategoryModel()
            {
                CategoryId = 1,
                Name = "Category-1",
                Status = 2,
                HyphenatedString = "Merchant-1",
                ClientId = Constants.Clients.Blue,
            });

            var result = await state.CategoryService.GetRootCategoriesAsync(Constants.Clients.CashRewards, Constants.Clients.Blue,
                Status.All);

            var expectedCategoryDtos = new List<CategoryDto>()
            {
                new CategoryDto()
                {
                    Id = 1,
                    Status = 2,
                    HyphenatedString = "Merchant-1",
                    Name = "Category-1"
                },
                new CategoryDto()
                {
                    Id = 2,
                    Status = 1,
                    HyphenatedString = "Merchant-2",
                    Name = "Category-2"
                }
            };

            result.Should().BeEquivalentTo(expectedCategoryDtos);
        }

        [Test]
        public async Task GetRootCategoriesAsync_ShouldReturnRootCategories_GivenStandardClientOnly()
        {
            var state = new TestState();


            var result = await state.CategoryService.GetRootCategoriesAsync(Constants.Clients.CashRewards, null,
                Status.InActive);

            var expectedCategoryDtos = new List<CategoryDto>()
            {
                new CategoryDto()
                {
                    Id = 1,
                    Status = 2,
                    HyphenatedString = "Merchant-1",
                    Name = "Category-1"
                }
            };
            result.Should().BeEquivalentTo(expectedCategoryDtos);
        }
    }
}

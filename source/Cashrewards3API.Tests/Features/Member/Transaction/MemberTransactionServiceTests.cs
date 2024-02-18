using Cashrewards3API.Common.Dto;
using Cashrewards3API.Features.Transaction;
using Cashrewards3API.Features.Transaction.Model;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cashrewards3APITests.Features.Transaction
{
    [TestFixture()]
    public class MemberTransactionServiceTests
    {
        [Test()]
        public async Task GetTransactionsForMemberTest()
        {
            // Arrange
            int expectedTotalCount = 2;
            int expectedCount = 1;
            PagedList<MemberTransactionResultModel> expected = new PagedList<MemberTransactionResultModel>(expectedTotalCount, expectedCount, new List<MemberTransactionResultModel>() { 
                new MemberTransactionResultModel { MerchantName = "test", TransactionId=1 },
                new MemberTransactionResultModel { MerchantName = "test", TransactionId=2 }
            });
            Mock<IMemberTransactionService> memTransactionSvc = new Mock<IMemberTransactionService>();
            memTransactionSvc.Setup(x => x.GetTransactionsForMemberAsync(It.IsAny<MemberTransactionRequestInfoModel>())).ReturnsAsync(expected);

            // Act
            var result = await memTransactionSvc.Object.GetTransactionsForMemberAsync(It.IsAny<MemberTransactionRequestInfoModel>());

            // Assert
            Assert.AreEqual(expectedTotalCount, result.TotalCount);
            Assert.AreEqual(expectedCount, result.Count);
            Assert.AreEqual(expected, result);
        }

    }
}

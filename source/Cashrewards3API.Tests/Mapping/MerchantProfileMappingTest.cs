using AutoMapper;
using Cashrewards3API.Common;
using Cashrewards3API.Enum;
using Cashrewards3API.Features.Merchant.Models;
using Cashrewards3API.Mapper;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Cashrewards3API.Tests.Mapping
{
    [TestFixture]
    public class MerchantProfileMappingTest
    {
        private readonly MerchantTierClient _merchantTierClientMapped = new MerchantTierClient();

        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>();

        private readonly CreateInternalMerchantTierRequestModel internalMerchantTierRequest = new CreateInternalMerchantTierRequestModel
        {
            ClientIds = new List<int> { 10000, 100001, 20000 },
            MerchantId = 100000,
            PromotionBonusType = 101,
            PromotionBonusValue = 200,
            PromotionDateMax = DateTime.UtcNow.AddDays(10),
            PromotionDateMin = DateTime.UtcNow.AddDays(-1),
            TierName = "Tier Name"
        };

        [Test]
        public void MerchantTierMappedFromRequest_IsMappedFromRequest_ToMerchantTier()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MerchantTierProfile>());
            var mapper = config.CreateMapper();

            var merchantTier = mapper.Map<MerchantTier>(internalMerchantTierRequest, opts =>
            {
                opts.Items[Constants.Mapper.CurrencyId] = "10";
                opts.Items[Constants.Mapper.TierTypePromotionId] = "100";
            });

            Assert.AreEqual(merchantTier.CurrencyId, 10);
            Assert.AreEqual(merchantTier.TierTypeId, 100);
            Assert.AreEqual(merchantTier.MerchantId, 100000);
            Assert.AreEqual(merchantTier.Commission, 200);
            Assert.AreEqual(merchantTier.TierCommTypeId, 101);
            Assert.AreEqual(merchantTier.Status, (int)MerchantTierStatusTypeEnum.Active);
            Assert.IsFalse(merchantTier.IsAdvancedTier);
            Assert.IsTrue(merchantTier.ClientIds.Count > 1);
        }
    }
}
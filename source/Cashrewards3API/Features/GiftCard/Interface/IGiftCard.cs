using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashrewards3API.Features.GiftCard.Interface
{
    public interface IGiftCard
    {
        Task<Model.GiftCardDto> GetGiftCard(int clientId, int? premiumClientId, string buketKey);
    }
}

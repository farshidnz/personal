using System;

namespace Cashrewards3API.Features.Member.Model
{
    public class Member
    {
        public string AccessCode { get; set; }

        public int ClientId { get; set; }
        public string CookieIpAddress { get; set; }
        public DateTime? DateofBirth { get; set; }

        public DateTime? DateJoined { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }
        public string HashedEmail { get; set; }
        public string HashedMobile { get; set; }

        public string PostCode { get; set; }

        public string Mobile { get; set; }

        public DateTime? ActivateBy { get; set; }

        public int Status { get; set; }

        public bool ReceiveNewsLetter { get; set; }

        public string SaltKey { get; set; }

        public string UserPassword { get; set; }

        public string TwoFactorAuthyId { get; set; }

        public bool IsTwoFactorAuthEnabled { get; set; }

        public string TwoFactorAuthActivationToken { get; set; }

        public DateTime? TwoFactorAuthActivateBy { get; set; }

        public DateTime? TwoFactorAuthActivateByUtc { get; set; }

        public string TwoFactorAuthActivationMobile { get; set; }

        public string TwoFactorAuthActivationCountryCode { get; set; }

        public int MemberId { get; set; }

        public bool IsRisky { get; set; }

        public bool HasVisaCardLinked { get; set; }

        public Guid MemberNewId { get; set; }

        public string MobileSHA256 { get; set; }

        public int? CampaignId { get; set; }

        public int? Source { get; set; }

        public int KycStatusId { get; set; } = 1;
        public bool ClickWindowActive { get; set; } = false;

        public bool PopUpActive { get; set; } = false;

        public bool IsValidated { get; set; } = false;
        public bool IsResetPassword { get; set; } = false;

        public bool RequiredLogin { get; set; } = false;
        public bool IsAvailable { get; set; } = false;
        public bool InstallNotifier { get; set; } = false;

        public bool SmsConsent { get; set; } = false;

        public bool CommsPromptShownCount { get; set; } = false;

        public string FacebookUsername { get; set; }

    }
}
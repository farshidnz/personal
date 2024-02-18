namespace Cashrewards3API.Features.Member.Request.SignInMember
{
    public class SignInMemberInternalResponse
    {
        public SignInMemberInternalResponse(string errorMessage)
        {
            Msg = errorMessage;
        }

        public SignInMemberInternalResponse(SignInMemberInternalResponseMember member)
        {
            Status = true;
            Member = member;
        }

        public bool Status { get; set; }

        public string Msg { get; set; }

        public SignInMemberInternalResponseMember Member { get; set; }
    }

    public class SignInMemberInternalResponseMember
    {
        public string MemberNewId { get; set; }

        public int MemberId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PostCode { get; set; }

        public string AccessCode { get; set; }

        public string PhoneNumber { get; set; }

        public int Status { get; set; }
    }
}

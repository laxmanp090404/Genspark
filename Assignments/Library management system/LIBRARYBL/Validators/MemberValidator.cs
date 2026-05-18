using LIBRARYMODELS.Models;

namespace LIBRARYBL.Validators;

public class MemberValidator
{
    public bool ValidateMember(Member member)
    {
        if (string.IsNullOrWhiteSpace(member.FullName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(member.Email))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(member.Phone))
        {
            return false;
        }
        if (member.Phone.StartsWith('+'))
        {
            return false;
        }
        if (member.Phone.Length != 10)
        {
            return false;
        }

        return true;
    }
}
using LIBRARYMODELS.Models;

namespace LIBRARYBL.Interfaces;

public interface IMemberService
{
    // add a member
    Member AddMember(Member member);
    // get all members
    ICollection<Member> GetAllMembers();
    // get member by his id
    Member? GetMemberById(int memberId);
    //search for a member
    Member? SearchMember(string searchText);
    // update a member
    Member UpdateMember(Member member);
    //deactivate or soft delete a member 
    // for this i thought we could use the mobile number as part of authentication instead of setting up password and stuff
    Member? DeactivateMember(int memberId,string phonenumber);
   // method to get membership types bcos if membership types are added newly
   // then we can fetch dynamically
    ICollection<MembershipType> GetMembershipTypes();
}
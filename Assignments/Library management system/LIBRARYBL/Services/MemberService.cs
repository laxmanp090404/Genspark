using LIBRARYBL.Interfaces;
using LIBRARYBL.Validators;
using LIBRARYDALL.Repositories;
using LIBRARYMODELS.Models;
using Npgsql.Internal;

namespace LIBRARYBL.Services;

public class MemberService : IMemberService
{
    // private readonly faster access
    private readonly MemberRepository _memberRepository;
    private readonly MemberValidator _validator;

    // dependency injection
    public MemberService(MemberRepository memberRepository)
    {
        _memberRepository = memberRepository;
        _validator = new MemberValidator();
    }

    public Member AddMember(Member member)
    {
        bool isValid = _validator.ValidateMember(member);

        if (!isValid)
        {
            throw new Exception("Invalid member details");
        }

        return _memberRepository.Add(member);
    }

    // OVERLOADED METHOD I WANT ADMIN TO DO THIS 
    //NOT IMPLEMENTED YET as admin functionality
    public Member DeactivateMember(int memberId)
    {
        Member? member = _memberRepository.GetById(memberId);

        if (member == null)
        {
            throw new Exception("Member not found");
        }

        member.IsActive = false;
        member.Deletedat = DateTime.Now;

        return _memberRepository.Update(member);
    }
    // AUTHENTICATED DEACTIVATE VOLUNTATILY BY USER
    public Member? DeactivateMember(int memberId, string phonenumber)
    {
        Member? member = _memberRepository.GetById(memberId);
        try
        {
            
            if (member == null) {
                
                Console.WriteLine("Member not found with the given Id " + memberId + " to deactivate");
                return member;
            }
            //verifcation
            if (member.Phone != phonenumber)
            {
                Console.WriteLine("Phone number verification failed. Unauthorised access");
                return null;
            }
            if(!member.IsActive)
            {
            Console.WriteLine("Member is already inactive");
            return null;
            }
            member.IsActive = false;
            member.Deletedat = DateTime.Now;
            return _memberRepository.Update(member);
        }
        catch (System.Exception e)
        {

            System.Console.WriteLine("Error occured at deactivating member "+e.Message);
            throw;
        }
       
    }

    public ICollection<Member> GetAllMembers()
    {
        return _memberRepository.GetAll();
    }

    public Member? GetMemberById(int memberId)
    {
        return _memberRepository.GetById(memberId);
    }

    // Dynamic fetching of Membershiptypes
    public ICollection<MembershipType> GetMembershipTypes()
    {
        return _memberRepository.GetMembershipTypes();
    }

    public Member? SearchMember(string searchtext)
    {
        // ensure only the local part is searched instead of domain part of mail
        return _memberRepository.GetAll().FirstOrDefault(m => m.FullName.ToLower().Contains(searchtext.ToLower()) || m.Email.Split('@')[0].ToLower().Contains(searchtext.ToLower()));
    }

    public Member UpdateMember(Member member)
    {
        return _memberRepository.Update(member);
    }
}
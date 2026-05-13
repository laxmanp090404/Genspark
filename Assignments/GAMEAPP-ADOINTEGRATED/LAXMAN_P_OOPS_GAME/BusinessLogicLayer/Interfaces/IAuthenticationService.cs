using GameApp.ModelLayer.Models;

namespace GameApp.BusinessLogicLayer.Interfaces
{
    internal interface IAuthenticationService
    {
        User? LoginUser();
        void RegisterUser();
    }
}
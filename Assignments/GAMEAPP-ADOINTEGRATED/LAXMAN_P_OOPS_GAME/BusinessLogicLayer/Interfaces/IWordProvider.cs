using GameApp.ModelLayer.Models;

namespace GameApp.BusinessLogicLayer.Interfaces
{
    internal interface IWordProvider
    {
        // provides random hiddenWord
        HiddenWord GetRandomHidden();
    }
}
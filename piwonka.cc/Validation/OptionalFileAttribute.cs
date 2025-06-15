using System.ComponentModel.DataAnnotations;

namespace Piwonka.CC.Validation
{
    public class OptionalFileAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            // Immer gültig - auch wenn null oder leer
            return true;
        }
    }
}
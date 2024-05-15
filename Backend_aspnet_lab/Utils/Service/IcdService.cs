using Backend_aspnet_lab.Models;
using System.Text.RegularExpressions;

namespace Backend_aspnet_lab.Utils.Service
{
    public static class IcdService
    {
        public static bool IsICD10Code(string code)
        {
            string icd10Pattern = @"[A-TV-Z][0-9][0-9AB]\.?[0-9A-TV-Z]{0,4}$";

            return Regex.IsMatch(code, icd10Pattern);
        }

        public static Guid FindIcdRoot(Icd10Record icd)
        {
            return (Guid) icd.RootId;
        }
    }
}

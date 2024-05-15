using System.ComponentModel.DataAnnotations;

namespace Backend_aspnet_lab.Utils.ValidationAttributes
{
    public class DatePriorValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {

            if (value == null) { return true; }

            if (value is DateTimeOffset date)
            {
                var today = DateTimeOffset.UtcNow;
                var minDate = today.AddYears(-100);

                return date <= today && date >= minDate;
            }

            return false;
        }
    }
}

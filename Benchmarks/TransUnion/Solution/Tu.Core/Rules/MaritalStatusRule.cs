using Tu.Extensions;

namespace Tu.Rules
{
    public class MaritalStatusRule : Rule
    {
        public MaritalStatusRule(IValidator validator, string fieldName)
            : base(validator, fieldName)
        {
        }

        public override bool IsValid(object obj)
        {
            var value = obj.ToStringSafe();

            return IsValid(() => Validator.IsNullOrEmpty(value), "")
                   || IsValid(() => Validator.IsLengthLessThanOrEqualTo(value, 50), "Length Out Of Range");
        }
    }
}
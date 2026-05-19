using System.ComponentModel.DataAnnotations;
using Homecare.Application.Constants.CouponCondition;
using Homecare.Application.Constants.Offers;

namespace Homecare.Application.DTOs.CouponCondition;

public class CreateConditionTypeDto : IValidatableObject
{
    [Required(ErrorMessage = ConditionTypeMessages.LabelRequired)]
    [MaxLength(100, ErrorMessage = ConditionTypeMessages.LabelMaxLength)]
    public string Label { get; set; } = null!;

    [Required(ErrorMessage = ConditionTypeMessages.ContextKeyRequired)]
    public string ContextKey { get; set; } = null!;

    [Required(ErrorMessage = ConditionTypeMessages.InputTypeRequired)]
    public string InputType { get; set; } = null!;

    [Required(ErrorMessage = ConditionTypeMessages.DefaulOperatorRequired)]
    public string DefaultOperator { get; set; } = null!;

    [Required(ErrorMessage = ConditionTypeMessages.DefaultFailBehaviourRequired)]
    public string DefaultFailBehaviour { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var inputType = InputType?.Trim().ToLower();
        var operator_ = DefaultOperator?.Trim().ToLower();
        var contextKey = ContextKey?.Trim().ToLower();

      
        if (ContextKeys.AllowedInputTypes.TryGetValue(contextKey ?? "", out var allowedTypes))
        {
            if (!allowedTypes.Contains(inputType ?? ""))
            {
                yield return new ValidationResult(
                    $"Input type '{InputType}' is not valid for context key '{ContextKey}'. Allowed: {string.Join(", ", allowedTypes)}",
                    new[] { nameof(InputType) });
                yield break;
            }
        }


        if (!InputTypeOperators.Allowed.TryGetValue(inputType ?? "", out var allowedOps))
        {
            yield return new ValidationResult(
                $"Invalid input type '{InputType}'.",
                new[] { nameof(InputType) });
            yield break;
        }

        if (!allowedOps.Contains(operator_ ?? ""))
        {
            yield return new ValidationResult(
                $"Operator '{DefaultOperator}' is not valid for input type '{InputType}'. Allowed: {string.Join(", ", allowedOps)}",
                new[] { nameof(DefaultOperator) });
        }
    }
}

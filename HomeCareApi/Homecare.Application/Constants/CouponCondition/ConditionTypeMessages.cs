using System;

namespace Homecare.Application.Constants.CouponCondition;

public class ConditionTypeMessages
{
    public const string FetchSuccess = "Condition types fetched.";
    public const string CreateSuccess = "Condition type created successfully.";
    public const string LabelAlreadyExists = "A condition type with this label already exists.";
    public const string LabelRequired = "Label is required.";
    public const string LabelMaxLength = "Label cannot exceed 100 characters.";
    public const string ContextKeyRequired = "Context key is required.";
    public const string InputTypeRequired = "Input type is required.";
    public const string DefaulOperatorRequired = "Default operator is required.";
    public const string DefaultFailBehaviourRequired = "Default fail behaviour is required.";



}

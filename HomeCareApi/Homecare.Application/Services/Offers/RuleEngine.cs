
using Homecare.Application.Common.Models;
using Homecare.Application.Constants.Offers;
using Homecare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Homecare.Application.Services.Offers;

public class RuleEngine
{
    private readonly ILogger<RuleEngine> _logger;

    public RuleEngine(ILogger<RuleEngine> logger)
    {
        _logger = logger;
    }
    public bool Evaluate(CouponCondition condition, CouponCheckContext ctx)
    {
        if (condition.ConditionType == null)
            return true;
        var contextValue = ResolveContextValue(
            condition.ConditionType.ContextKey, ctx
        );

        if (contextValue == null)
            return false;

        return condition.Operator switch
        {
            "gte" => GteCheck(contextValue, condition.Value),
            "lte" => LteCheck(contextValue, condition.Value),
            "eq" => EqCheck(contextValue, condition.Value),
            "in" => InCheck(contextValue, condition.Value),
            "between" => BetweenCheck(contextValue, condition.Value),
            _ => false
        };
    }

    private object? ResolveContextValue(string contextKey, CouponCheckContext ctx)
    {
        _logger.LogInformation("Evaluating contextKey: {ContextKey}", contextKey);

        return contextKey switch
        {
            ContextKeys.CartTotal => ctx.CartTotal,
            ContextKeys.UserBookingCount => ctx.UserBookingCount,
            ContextKeys.UserCouponUses => ctx.UserCouponUses,
            ContextKeys.SlotDayOfWeek => ctx.SlotDayOfWeek,
            ContextKeys.SlotTime => ctx.SlotTime,
            ContextKeys.SlotDate => ctx.SlotDate,
            ContextKeys.ServiceSubCategory => ctx.ServiceSubCategoryId?.ToString(),
            _ => null
        };
    }

    private bool GteCheck(object contextValue, string storedValue)
    {
        return contextValue switch
        {
            decimal d => d >= decimal.Parse(storedValue),
            int i => i >= int.Parse(storedValue),
            _ => false
        };
    }

    private bool LteCheck(object contextValue, string storedValue)
    {
        return contextValue switch
        {
            decimal d => d <= decimal.Parse(storedValue),
            int i => i <= int.Parse(storedValue),
            _ => false
        };
    }

    private bool EqCheck(object contextValue, string storedValue)
    {
        return contextValue switch
        {
            decimal d => d == decimal.Parse(storedValue),
            int i => i == int.Parse(storedValue),
            string s => string.Equals(s, storedValue,
                              StringComparison.OrdinalIgnoreCase),
            DateTime dt => dt.Date == DateTime.Parse(storedValue).Date,
            bool b => b == bool.Parse(storedValue),
            _ => false
        };
    }

    private bool InCheck(object contextValue, string storedValue)
    {
        var allowed = storedValue
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLower())
            .ToHashSet();


        _logger.LogInformation("InCheck | ContextValue: {ContextValue} | Allowed: {AllowedValues}",contextValue,string.Join(", ", allowed));

        return contextValue switch
        {
            string s => allowed.Contains(s.ToLower()),
            int i => allowed.Contains(i.ToString()),
            _ => false
        };
    }

    private bool BetweenCheck(object contextValue, string storedValue)
    {
        var parts = storedValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2) return false;

        return contextValue switch
        {
            TimeSpan ts =>
             LogAndReturn(
                 $"[BetweenCheck] SlotTime={ts} | From={parts[0].Trim()} | To={parts[1].Trim()} | Result={ts >= TimeSpan.Parse(parts[0].Trim()) && ts <= TimeSpan.Parse(parts[1].Trim())}",
                 ts >= TimeSpan.Parse(parts[0].Trim()) && ts <= TimeSpan.Parse(parts[1].Trim())
             ),
            DateTime dt => dt.Date >= DateTime.Parse(parts[0].Trim()).Date
                        && dt.Date <= DateTime.Parse(parts[1].Trim()).Date,

            decimal d => d >= decimal.Parse(parts[0].Trim())
                        && d <= decimal.Parse(parts[1].Trim()),

            int i => i >= int.Parse(parts[0].Trim())
                        && i <= int.Parse(parts[1].Trim()),

            _ => false
        };
    }
    private bool LogAndReturn(string message, bool result)
    {
        _logger.LogInformation(message);
        return result;
    }
}

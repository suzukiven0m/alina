using System.Text.RegularExpressions;

namespace CargoShipMonitoring.Shared.Models;

public readonly record struct IMONumber
{
    private readonly string _value;

    public IMONumber(string value)
    {
        if (!IsValid(value))
            throw new ArgumentException($"Invalid IMO number: {value}", nameof(value));
        _value = value;
    }

    public override string ToString() => _value;

    public static bool IsValid(string? imo)
    {
        if (string.IsNullOrWhiteSpace(imo)) return false;
        if (!Regex.IsMatch(imo, @"^\d{7}$")) return false;

        int sum = 0;
        for (int i = 0; i < 6; i++)
        {
            sum += (imo[i] - '0') * (7 - i);
        }
        int checkDigit = sum % 10;
        return checkDigit == (imo[6] - '0');
    }
}

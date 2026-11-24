namespace JobApplicationTracker;

/// <summary>
/// Safe console input helpers (requirement R1 + general validation).
/// Every helper loops until a valid value is entered; none of them throw on
/// bad input, so the menu can recover from "abc" without crashing.
/// </summary>
internal static class ConsoleHelper
{
    public static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("  A value is required. Please try again.");
                continue;
            }
            if (int.TryParse(input.Trim(), out int result))
                return result;
            Console.WriteLine($"  '{input}' is not a valid integer. Please try again.");
        }
    }

    public static int ReadIntOrDefault(string prompt, int defaultValue)
    {
        Console.Write(prompt);
        Console.Write($" (default {defaultValue}): ");
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
            return defaultValue;
        if (int.TryParse(input.Trim(), out int result))
            return result;
        Console.WriteLine($"  '{input}' is not a valid integer. Keeping current value.");
        return defaultValue;
    }

    public static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("  A value is required. Please try again.");
                continue;
            }
            if (decimal.TryParse(input.Trim(), out decimal result))
                return result;
            Console.WriteLine($"  '{input}' is not a valid decimal number. Please try again.");
        }
    }

    /// <summary>
    /// Reads a decimal that must be strictly greater than zero. Used for
    /// Job salary (validation requirement: invalid salary).
    /// </summary>
    public static decimal ReadPositiveDecimal(string prompt)
    {
        while (true)
        {
            decimal value = ReadDecimal(prompt);
            if (value > 0)
                return value;
            Console.WriteLine("  Value must be greater than zero. Please try again.");
        }
    }

    /// <summary>
    /// Reads an int that must be greater than or equal to zero. Used for
    /// Years of experience (validation requirement: negative years of
    /// experience).
    /// </summary>
    public static int ReadNonNegativeInt(string prompt)
    {
        while (true)
        {
            int value = ReadInt(prompt);
            if (value >= 0)
                return value;
            Console.WriteLine("  Value cannot be negative. Please try again.");
        }
    }

    /// <summary>
    /// Reads an int that must be strictly greater than zero. Used for Id
    /// prompts in update/remove flows (validation requirement: negative Ids).
    /// </summary>
    public static int ReadPositiveInt(string prompt)
    {
        while (true)
        {
            int value = ReadInt(prompt);
            if (value > 0)
                return value;
            Console.WriteLine("  Id must be a positive integer. Please try again.");
        }
    }

    public static decimal ReadDecimalOrDefault(string prompt, decimal defaultValue)
    {
        Console.Write(prompt);
        Console.Write($" (default {defaultValue}): ");
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
            return defaultValue;
        if (decimal.TryParse(input.Trim(), out decimal result))
            return result;
        Console.WriteLine($"  '{input}' is not a valid decimal. Keeping current value.");
        return defaultValue;
    }

    public static string ReadString(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? string.Empty;
    }

    public static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
                return input.Trim();
            Console.WriteLine("  This field is required. Please try again.");
        }
    }

    public static string ReadStringOrDefault(string prompt, string defaultValue)
    {
        Console.Write(prompt);
        Console.Write($" (default {defaultValue}): ");
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
            return defaultValue;
        return input.Trim();
    }

    public static string ReadRequiredEmail(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("  Email is required. Please try again.");
                continue;
            }
            if (Models.Recruiter.IsValidEmail(input.Trim()))
                return input.Trim();
            Console.WriteLine($"  '{input}' is not a valid email (must contain '@' and a '.'). Please try again.");
        }
    }

    public static string ReadEmailOrDefault(string prompt, string defaultValue)
    {
        Console.Write(prompt);
        Console.Write($" (default {defaultValue}): ");
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
            return defaultValue;
        string trimmed = input.Trim();
        if (Models.Recruiter.IsValidEmail(trimmed))
            return trimmed;
        Console.WriteLine($"  '{input}' is not a valid email. Keeping current value.");
        return defaultValue;
    }
}

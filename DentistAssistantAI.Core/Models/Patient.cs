namespace DentistAssistantAI.Core.Models
{
    public sealed class Patient
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string Name { get; init; }
        public DateTime LastVisit { get; init; } = DateTime.Now;
        public string? Phone { get; init; }
        public DateTime? DateOfBirth { get; init; }

        /// <summary>Up to two uppercase initials, e.g. "Ivanov Ivan" → "II".</summary>
        public string Initials => string.Concat(
            Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(w => char.ToUpper(w[0])));

        /// <summary>Formatted date of birth, e.g. "March 15, 1990", or empty string when not set.</summary>
        public string DateOfBirthText => DateOfBirth?.ToString("MMMM d, yyyy") ?? string.Empty;
    }
}

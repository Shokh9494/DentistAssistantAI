namespace DentistAssistantAI.Core.Models
{
    public sealed class Patient
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string Name { get; init; }
        public DateTime LastVisit { get; init; } = DateTime.Now;

        /// <summary>Up to two uppercase initials, e.g. "Ivanov Ivan" → "II".</summary>
        public string Initials => string.Concat(
            Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Take(2)
                .Select(w => char.ToUpper(w[0])));
    }
}

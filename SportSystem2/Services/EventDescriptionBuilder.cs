using SportSystem2.Models;

namespace SportSystem2.Services
{
    public static class EventDescriptionBuilder
    {
        public static string GetPlayerEventDescription(PlayerEvent ev)
        {
            if (ev == null)
                return string.Empty;

            var playerNumber = ev.Player?.Number ?? 0;
            var playerName = ev.Player?.FullName ?? "Unknown";

            string baseDescription = ev.EventType switch
            {
                EventType.Touchdown => $"Touchdown by player #{playerNumber} {playerName}",
                EventType.Interception => $"Interception by player #{playerNumber} {playerName}",
                EventType.FlagsPulled => $"Flags pulled by player #{playerNumber} {playerName}",
                EventType.Sack => $"Sack by player #{playerNumber} {playerName}",
                EventType.Fumble => $"Fumble by player #{playerNumber} {playerName}",
                EventType.Safety => $"Safety by player #{playerNumber} {playerName}",
                EventType.FieldGoal => $"Field Goal by player #{playerNumber} {playerName}",
                EventType.ExtraPoint => $"Extra Point by player #{playerNumber} {playerName}",
                EventType.Kickoff => $"Kickoff by player #{playerNumber} {playerName}",
                _ => $"Event: {ev.EventType} by player #{playerNumber} {playerName}"
            };

            if (ev.Yards != null)
                baseDescription += $" for {ev.Yards} yards";

            return baseDescription;
        }
    }
}

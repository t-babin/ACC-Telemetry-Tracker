namespace AccTelemetryTracker.Logic
{
    public interface IDiscordNotifier
    {
        /// <summary>
        /// Sends a notification via a Discord webhook whenever a motec file is uploaded to the application
        /// </summary>
        /// <param name="motecFile">The motec file object that was just upload</param>
        /// <param name="avatarUrl">The avatar of the discord user</param>
        /// <returns></returns>
        Task Notify(Datastore.Models.MotecFile motecFile, string? avatarUrl);
    }
}
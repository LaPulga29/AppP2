namespace AppP2.Services
{
    public static class MessagingService
    {
        public static event Action? ProfessorsUpdated;

        public static void NotifyProfessorsUpdated()
        {
            ProfessorsUpdated?.Invoke();
        }
    }
}
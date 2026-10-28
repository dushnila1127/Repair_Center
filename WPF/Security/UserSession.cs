namespace RepairCenter.Security
{
    public sealed class UserSessionInfo
    {
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public bool IsAdministrator { get { return RoleName == "Administrator"; } }
        public string RoleCaption { get { return IsAdministrator ? "Администратор" : "Сотрудник"; } }
    }

    public static class UserSession
    {
        public static UserSessionInfo Current { get; private set; }
        public static void Start(UserSessionInfo user) { Current = user; }
        public static void End() { Current = null; }
    }
}

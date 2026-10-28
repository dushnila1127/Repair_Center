using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using RepairCenter.Entities;

namespace RepairCenter.Security
{
    public static class AuthService
    {
        private const int Iterations = 600000;
        private const int SaltLength = 16;
        private const int HashLength = 32;
        private const string Prefix = "PBKDF2-SHA256";

        public static bool TryLogin(string login, string password, out UserSessionInfo session, out string message)
        {
            session = null;
            message = "Неверный логин или пароль.";
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrEmpty(password)) return false;
            try
            {
                using (var context = App.Context)
                {
                    var administratorCount = context.Course_project_0502_Users
                        .Include("Course_project_0502_Roles")
                        .Where(x => x.IsActive && x.AccountType == "Employee")
                        .ToList()
                        .Count(x => x.Course_project_0502_Roles.Any(r => r.RoleName == "Administrator") && IsSupportedHash(x.PasswordHash));
                    if (administratorCount != 1)
                    {
                        message = "В базе должен быть ровно один активный администратор с корректным хешем пароля. Проверь начальные учётные записи и скрипт RepairCenter_Auth_Accounts_Upgrade.sql.";
                        return false;
                    }

                    var user = context.Course_project_0502_Users
                        .Include("Course_project_0502_Roles")
                        .Include("Course_project_0502_Employees")
                        .FirstOrDefault(x => x.LoginName == login.Trim() && x.IsActive);
                    if (user == null || !VerifyPassword(password, user.PasswordHash)) return false;
                    if (user.AccountType != "Employee" || !user.EmployeeID.HasValue || user.Course_project_0502_Employees == null)
                    {
                        message = "Эта учётная запись не предназначена для входа в настольное приложение.";
                        return false;
                    }
                    if (!user.Course_project_0502_Employees.IsActive)
                    {
                        message = "Учётная запись привязана к неактивному сотруднику. Обратись к администратору.";
                        return false;
                    }
                    var roleNames = user.Course_project_0502_Roles.Select(x => x.RoleName).Distinct().ToList();
                    var isAdmin = roleNames.Contains("Administrator");
                    if (!isAdmin && !roleNames.Contains("Technician") && !roleNames.Contains("Receptionist"))
                    {
                        message = "Для учётной записи не назначена роль сотрудника или администратора.";
                        return false;
                    }
                    session = new UserSessionInfo
                    {
                        UserID = user.UserID,
                        LoginName = user.LoginName,
                        FullName = user.Course_project_0502_Employees.FullName,
                        RoleName = isAdmin ? "Administrator" : "Employee"
                    };
                    message = string.Empty;
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Не удалось проверить учётную запись. Проверь подключение к БД и наличие таблиц Users, Roles и UserRoles. " + ex.Message;
                return false;
            }
        }

        public static bool RegisterEmployee(UserSessionInfo actor, string login, string fullName, string phone, string password, out string message)
        {
            message = string.Empty;
            if (actor == null || !actor.IsAdministrator)
            {
                message = "Создавать учётные записи сотрудников может только администратор.";
                return false;
            }

            login = (login ?? string.Empty).Trim();
            fullName = (fullName ?? string.Empty).Trim();
            phone = (phone ?? string.Empty).Trim();
            if (!Regex.IsMatch(login, @"^[A-Za-z0-9._-]{3,60}$"))
            {
                message = "Логин должен содержать от 3 до 60 латинских букв, цифр или символов . _ - .";
                return false;
            }
            if (fullName.Length < 5 || fullName.Length > 150)
            {
                message = "Укажи ФИО длиной от 5 до 150 символов.";
                return false;
            }
            if (phone.Length < 7 || phone.Length > 20)
            {
                message = "Телефон должен содержать от 7 до 20 символов.";
                return false;
            }
            if (!IsStrongPassword(password))
            {
                message = "Пароль должен содержать от 10 до 128 символов, включая заглавную и строчную латинские буквы и цифру.";
                return false;
            }

            var passwordHash = HashPassword(password);
            using (var context = App.Context)
            using (var transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    var users = context.Course_project_0502_Users
                        .Include("Course_project_0502_Roles")
                        .Include("Course_project_0502_Employees")
                        .Where(x => x.IsActive && x.AccountType == "Employee")
                        .ToList();
                    var administrators = users.Where(x => x.Course_project_0502_Roles.Any(r => r.RoleName == "Administrator") && IsSupportedHash(x.PasswordHash)).ToList();
                    var currentAdministrator = administrators.FirstOrDefault(x => x.UserID == actor.UserID);
                    if (administrators.Count != 1 || currentAdministrator == null)
                    {
                        message = "Операцию нельзя выполнить: в базе должен быть ровно один действующий администратор, выполнивший вход.";
                        transaction.Rollback();
                        return false;
                    }

                    var technicianRole = context.Course_project_0502_Roles.FirstOrDefault(x => x.RoleName == "Technician");
                    if (technicianRole == null)
                    {
                        message = "В базе отсутствует роль Technician. Проверь установочный SQL-скрипт.";
                        transaction.Rollback();
                        return false;
                    }
                    if (context.Course_project_0502_Users.Any(x => x.LoginName == login))
                    {
                        message = "Этот логин уже занят. Выбери другой.";
                        transaction.Rollback();
                        return false;
                    }

                    var employee = context.Course_project_0502_Employees
                        .Include("Course_project_0502_Users")
                        .FirstOrDefault(x => x.Phone == phone);
                    if (employee != null && !string.Equals(employee.FullName.Trim(), fullName, StringComparison.OrdinalIgnoreCase))
                    {
                        message = "Этот телефон уже указан у другого сотрудника. Проверь ФИО или исправь карточку сотрудника.";
                        transaction.Rollback();
                        return false;
                    }
                    if (employee != null && !employee.IsActive)
                    {
                        message = "Нельзя создать учётную запись для неактивного сотрудника.";
                        transaction.Rollback();
                        return false;
                    }

                    Course_project_0502_Users account = null;
                    if (employee != null)
                    {
                        account = employee.Course_project_0502_Users.FirstOrDefault(x => x.AccountType == "Employee");
                        if (account != null && account.Course_project_0502_Roles.Any(r => r.RoleName == "Administrator"))
                        {
                            message = "Нельзя переназначать учётную запись администратора.";
                            transaction.Rollback();
                            return false;
                        }
                        if (account != null && account.IsActive && IsSupportedHash(account.PasswordHash))
                        {
                            message = "Для этого сотрудника уже существует активная учётная запись.";
                            transaction.Rollback();
                            return false;
                        }
                    }
                    else
                    {
                        employee = new Course_project_0502_Employees { FullName = fullName, Phone = phone, IsActive = true };
                        context.Course_project_0502_Employees.Add(employee);
                        context.SaveChanges();
                    }

                    if (account == null)
                    {
                        account = new Course_project_0502_Users();
                        context.Course_project_0502_Users.Add(account);
                    }
                    account.LoginName = login;
                    account.PasswordHash = passwordHash;
                    account.AccountType = "Employee";
                    account.EmployeeID = employee.EmployeeID;
                    account.ClientID = null;
                    account.IsActive = true;
                    account.Course_project_0502_Roles.Clear();
                    account.Course_project_0502_Roles.Add(technicianRole);
                    context.SaveChanges();
                    transaction.Commit();
                    message = "Учётная запись создана. Роль сотрудника назначена автоматически.";
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    message = "Не удалось создать учётную запись сотрудника. Проверь уникальность логина и телефона, внешние ключи и права записи в SQL Server. " + ex.Message;
                    return false;
                }
            }
        }

        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 10 || password.Length > 128) return false;
            return password.Any(c => c >= 'A' && c <= 'Z')
                && password.Any(c => c >= 'a' && c <= 'z')
                && password.Any(char.IsDigit);
        }

        private static bool IsSupportedHash(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            var parts = value.Split('$');
            if (parts.Length != 4 || parts[0] != Prefix) return false;
            int iterations;
            if (!int.TryParse(parts[1], out iterations) || iterations < 100000 || iterations > 2000000) return false;
            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var hash = Convert.FromBase64String(parts[3]);
                return salt.Length == SaltLength && hash.Length == HashLength;
            }
            catch (FormatException) { return false; }
        }

        private static string HashPassword(string password)
        {
            var salt = new byte[SaltLength];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);
            byte[] hash;
            using (var derive = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256)) hash = derive.GetBytes(HashLength);
            return Prefix + "$" + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        private static bool VerifyPassword(string password, string stored)
        {
            if (!IsSupportedHash(stored)) return false;
            var parts = stored.Split('$');
            byte[] salt;
            byte[] expected;
            try { salt = Convert.FromBase64String(parts[2]); expected = Convert.FromBase64String(parts[3]); }
            catch (FormatException) { return false; }
            byte[] actual;
            using (var derive = new Rfc2898DeriveBytes(password, salt, int.Parse(parts[1]), HashAlgorithmName.SHA256)) actual = derive.GetBytes(expected.Length);
            if (actual.Length != expected.Length) return false;
            var difference = 0;
            for (var i = 0; i < actual.Length; i++) difference |= actual[i] ^ expected[i];
            return difference == 0;
        }
    }
}

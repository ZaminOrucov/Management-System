using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace InMemoryUserManagement
{
    
    public class Role
    {
        public string Name { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();

        public Role(string name)
        {
            Name = name;
        }
    }

    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public Role UserRole { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"{Id} | {Name,-20} | {Email,-25} | {UserRole.Name,-10} | {CreatedAt}";
        }
    }

  
    public static class SecurityHelper
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }

    public class UserManager
    {
        public List<User> Users { get; private set; } = new List<User>();

        public void CreateUser(string name, string email, string password, Role role)
        {
            if (Users.Any(u => u.Email == email))
            {
                Console.WriteLine("Bu E-mail artıq mövcuddur!");
                return;
            }

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = SecurityHelper.HashPassword(password),
                UserRole = role
            };
            Users.Add(user);
            Console.WriteLine("İstifadəçi yaradıldı!");
        }

        public void ListUsers(int page = 1, int pageSize = 20)
        {
            if (Users.Count == 0)
            {
                Console.WriteLine("İstifadəçi tapılmadı.");
                return;
            }

            var paged = Users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            Console.WriteLine($"--- Page {page} ---");
            foreach (var u in paged)
                Console.WriteLine(u);
        }

        public void UpdateUser(string email)
        {
            var user = Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                Console.WriteLine("İstifadəçi tapılmadı!");
                return;
            }

            Console.Write("Yeni ad (boş buraxsa dəyişmir): ");
            string name = Console.ReadLine();
            Console.Write("Yeni email (boş buraxsa dəyişmir): ");
            string newEmail = Console.ReadLine();

            user.Name = string.IsNullOrEmpty(name) ? user.Name : name;
            user.Email = string.IsNullOrEmpty(newEmail) ? user.Email : newEmail;
            user.UpdatedAt = DateTime.Now;

            Console.WriteLine("İstifadəçi yeniləndi!");
        }

        public void DeleteUser(string email)
        {
            var user = Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                Users.Remove(user);
                Console.WriteLine("İstifadəçi silindi!");
            }
            else
            {
                Console.WriteLine("İstifadəçi tapılmadı!");
            }
        }

   
        public void GenerateDummyUsers(int count, Role defaultRole)
        {
            for (int i = 1; i <= count; i++)
            {
                string name = $"User{i}";
                string email = $"user{i}@example.com";
                string password = "Password123";
                CreateUser(name, email, password, defaultRole);
            }
        }
    }

 
    class Program
    {
        static void Main(string[] args)
        {
            var adminRole = new Role("Admin");
            adminRole.Permissions.AddRange(new string[] { "create", "read", "update", "delete" });

            var userRole = new Role("User");
            userRole.Permissions.Add("read");

            var userManager = new UserManager();
            userManager.GenerateDummyUsers(100, userRole);

            while (true)
            {
                Console.WriteLine("\n=== In-Memory User Management System ===");
                Console.WriteLine("1. İstifadəçi yarat");
                Console.WriteLine("2. İstifadəçiləri siyahıla");
                Console.WriteLine("3. İstifadəçi yenilə");
                Console.WriteLine("4. İstifadəçi sil");
                Console.WriteLine("5. Çıxış");
                Console.Write("Seçiminiz: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.Write("Ad: ");
                        string name = Console.ReadLine();
                        Console.Write("E-mail: ");
                        string email = Console.ReadLine();
                        Console.Write("Parol: ");
                        string password = Console.ReadLine();
                        Console.Write("Rol (Admin/User): ");
                        string roleInput = Console.ReadLine();
                        Role role = roleInput.ToLower() == "admin" ? adminRole : userRole;
                        userManager.CreateUser(name, email, password, role);
                        break;

                    case "2":
                        Console.Write("Səhifə nömrəsi: ");
                        int page = int.TryParse(Console.ReadLine(), out int p) ? p : 1;
                        userManager.ListUsers(page);
                        break;

                    case "3":
                        Console.Write("Yenilənəcək istifadəçinin E-maili: ");
                        string emailToUpdate = Console.ReadLine();
                        userManager.UpdateUser(emailToUpdate);
                        break;

                    case "4":
                        Console.Write("Silinəcək istifadəçinin E-maili: ");
                        string emailToDelete = Console.ReadLine();
                        userManager.DeleteUser(emailToDelete);
                        break;

                    case "5":
                        return;

                    default:
                        Console.WriteLine("Yanlış seçim!");
                        break;
                }
            }
        }
    }
}

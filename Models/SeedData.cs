using chuyendoiso.Data;
using Microsoft.EntityFrameworkCore;

namespace chuyendoiso.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new chuyendoisoContext(
                serviceProvider.GetRequiredService<DbContextOptions<chuyendoisoContext>>()))
            {
                Console.WriteLine("SeedData is running");

                // Look for any users
                if (context.Auth.Any())
                {
                    Console.WriteLine("DB already has data, skip seed.");
                    return;   // DB has been seeded
                }

                context.Auth.AddRange(
                    new Auth
                    {
                        Username = "sogtvt_khanhhoa",
                        Password = BCrypt.Net.BCrypt.HashPassword("sogtvt123"),
                        FullName = "Sở Giao thông Vận tải Khánh Hòa",
                        Email = "sogtvt@khanhhoa.gov.vn",
                        Phone = "02583889999"
                    },
                    new Auth
                    {
                        Username = "huyen_dienkhanh",
                        Password = BCrypt.Net.BCrypt.HashPassword("dienkhanh123"),
                        FullName = "Phòng CĐS Huyện Diên Khánh",
                        Email = "cds@dienkhanh.gov.vn",
                        Phone = "02583778899"
                    },
                    new Auth
                    {
                        Username = "tp_nhatrang",
                        Password = BCrypt.Net.BCrypt.HashPassword("nhatrang123"),
                        FullName = "Phòng Chuyển đổi số - TP Nha Trang",
                        Email = "cds@nhatrang.gov.vn",
                        Phone = "02583556677"
                    }
                );
                context.SaveChanges();
                Console.WriteLine("Sample data seeded.");
            }
        }
    }
}

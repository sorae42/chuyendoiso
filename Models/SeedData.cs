using chuyendoiso.Data;
using Microsoft.EntityFrameworkCore;

namespace chuyendoiso.Models
{
    public class SeedData
    {
        private static DateTime UtcDate(int year, int month, int day)
        {
            return DateTime.SpecifyKind(new DateTime(year, month, day), DateTimeKind.Utc);
        }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new chuyendoisoContext(
                serviceProvider.GetRequiredService<DbContextOptions<chuyendoisoContext>>()))
            {
                Console.WriteLine("SeedData is running");

                if (!context.Auth.Any())
                {
                    context.Auth.AddRange(
                        new Auth
                        {
                            Username = "sogtvt_khanhhoa",
                            Password = BCrypt.Net.BCrypt.HashPassword("sogtvt123"),
                            FullName = "Sở Giao thông Vận tải Khánh Hòa",
                            Email = "sogtvt@khanhhoa.gov.vn",
                            Phone = "02583889999",
                            Role = "user",
                            Unit = "Tỉnh Khánh Hòa"
                        },
                        new Auth
                        {
                            Username = "huyen_dienkhanh",
                            Password = BCrypt.Net.BCrypt.HashPassword("dienkhanh123"),
                            FullName = "Phòng CĐS Huyện Diên Khánh",
                            Email = "cds@dienkhanh.gov.vn",
                            Phone = "02583778899",
                            Role = "user",
                            Unit = "Huyện Diên Khánh"
                        },
                        new Auth
                        {
                            Username = "tp_nhatrang",
                            Password = BCrypt.Net.BCrypt.HashPassword("nhatrang123"),
                            FullName = "Phòng Chuyển đổi số - TP Nha Trang",
                            Email = "cds@nhatrang.gov.vn",
                            Phone = "02583556677",
                            Role = "user",
                            Unit = "TP Nha Trang"
                        }
                    );
                    context.SaveChanges();
                    Console.WriteLine("Seeded Auth users.");
                }
                else
                {
                    Console.WriteLine("Auth data already exists. Skipping...");
                }

                if (!context.TargetGroup.Any())
                {
                    context.TargetGroup.AddRange(
                        new TargetGroup { Name = "TIÊU CHÍ ĐÁNH GIÁ MỨC ĐỘ CHUYỂN ĐỔI SỐ CẤP XÃ" },
                        new TargetGroup { Name = "TIÊU CHÍ ĐÁNH GIÁ MỨC ĐỘ CHUYỂN ĐỔI SỐ CẤP HUYỆN" }
                    );
                    context.SaveChanges();
                    Console.WriteLine("Seeded TargetGroup.");
                }
                else
                {
                    Console.WriteLine("TargetGroup data already exists. Skipping...");
                }

                if (!context.ParentCriteria.Any())
                {
                    context.ParentCriteria.AddRange(
                        new ParentCriteria { Name = "CHÍNH QUYỀN SỐ", TargetGroupId = 1 },
                        new ParentCriteria { Name = "KINH TẾ SỐ", TargetGroupId = 1 },
                        new ParentCriteria { Name = "XÃ HỘI SỐ", TargetGroupId = 1 },
                        new ParentCriteria { Name = "CHỈ TIÊU CHUNG", TargetGroupId = 2 },
                        new ParentCriteria { Name = "CHÍNH QUYỀN SỐ", TargetGroupId = 2 },
                        new ParentCriteria { Name = "KINH TẾ SỐ", TargetGroupId = 2 },
                        new ParentCriteria { Name = "XÃ HỘI SỐ", TargetGroupId = 2 }
                    );
                    context.SaveChanges();
                    Console.WriteLine("Seeded ParentCriteria.");
                }
                else
                {
                    Console.WriteLine("ParentCriteria data already exists. Skipping...");
                }

                if (!context.SubCriteria.Any())
                {
                    context.SubCriteria.AddRange(
                        new SubCriteria 
                        {   
                            Name = "Tỷ lệ thủ tục hành chính đủ điều kiện theo quy định của pháp luật được cung cấp dưới hình thức dịch vụ công trực tuyến toàn trình.",
                            MaxScore = 30,
                            Description = "Cổng dịch vụ công cấp tỉnh.",
                            ParentCriteriaId = 1,
                            EvidenceInfo = "Quyết định số 942/QĐ- TTg ngày 15/6/2021 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "Tỉnh Khánh Hòa",
                            EvaluatedAt = UtcDate(2022, 3, 10)
                        },
                        new SubCriteria 
                        {   
                            Name = "Tỷ lệ hồ sơ thủ tục hành chính được thực hiện trực tuyến toàn trình.",
                            MaxScore = 20,
                            Description = "CHệ thống giám sát, đo lường mức độ cung cấp và sử dụng dịch vụ Chính phủ số.",
                            ParentCriteriaId = 1,
                            EvidenceInfo = "Quyết định số 942/QĐ- TTg ngày 15/6/2021 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "Tỉnh Khánh Hòa",
                            EvaluatedAt = UtcDate(2022, 7, 5)
                        },
                        new SubCriteria 
                        { 
                            Name = "Tỷ lệ xử lý văn bản, hồ sơ công việc (trừ hồ sơ mật) trên môi trường mạng.",
                            MaxScore = 70,
                            Description = "Tỷ lệ phần trăm của số văn bản được xử lý trên phần mềm quản lý văn bản và điều hành trên tổng số văn bản đến và đi của cơ quan cấp xã.",
                            ParentCriteriaId = 1,
                            EvidenceInfo = "- Quyết định số 942/QĐ- TTg ngày 15/6/2021 của Thủ tướng Chính phủ.\n- Nghị quyết số 01/NQ- CP ngày 05/01/2024 của Chính phủ.",
                            UnitEvaluate = "Huyện Diên Khánh",
                            EvaluatedAt = UtcDate(2022, 12, 28)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ doanh nghiệp nhỏ và vừa sử dụng nền tảng số phục vụ sản xuất, kinh doanh.",
                            MaxScore = 30,
                            Description = "Tỷ lệ phần trăm số doanh nghiệp nhỏ và vừa có sử dụng nền tảng số phục vụ sản xuất, kinh doanh trên tổng số doanh nghiệp nhỏ và vừa trên địa bàn xã.",
                            ParentCriteriaId = 2,
                            EvidenceInfo = "Quyết định số 411/QĐ- TTg ngày 31/3/2022 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "TP Nha Trang",
                            EvaluatedAt = UtcDate(2023, 1, 15)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ thành viên của hợp tác xã, doanh nghiệp được định hướng, tập huấn ứng dụng công nghệ số phục vụ sản xuất, kinh doanh",
                            MaxScore = 40,
                            Description = "Tỷ lệ phần trăm của số thành viên của hợp tác xã, doanh nghiệp được định hướng, tập huấn ứng dụng công nghệ số phục vụ sản xuất, kinh doanh trên tổng số thành viên của hợp tác xã, doanh nghiệp trên địa bàn xã.",
                            ParentCriteriaId = 2,
                            EvidenceInfo = "Công văn số 3445/BNN- VPĐP ngày 29/5/2023 của Bộ Nông nghiệp và Phát triển nông thôn.",
                            UnitEvaluate = "Huyện Diên Khánh",
                            EvaluatedAt = UtcDate(2023, 5, 20)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ dân số trưởng thành từ 15 tuổi trở lên có điện thoại thông minh.",
                            MaxScore = 50,
                            Description = "Tỷ lệ phần trăm dân số trưởng thành từ 15 tuổi trở lên có điện thoại thông minh trên tổng dân số từ 15 tuổi trở lên tại địa phương cấp xã.",
                            ParentCriteriaId = 3,
                            EvidenceInfo = "Quyết định số 411/QĐ- TTg ngày 31/3/2022 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "TP Nha Trang",
                            EvaluatedAt = UtcDate(2023, 8, 12)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ hộ gia đình được phủ mạng Internet băng rộng cáp quang.",
                            MaxScore = 10,
                            Description = "Tỷ lệ phần trăm của số hộ gia đình được phủ mạng Internet băng rộng cáp quang trên tổng số hộ gia đình tại địa phương cấp xã.",
                            ParentCriteriaId = 3,
                            EvidenceInfo = "Quyết định số 411/QĐ- TTg ngày 31/3/2022 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "TP Nha Trang",
                            EvaluatedAt = UtcDate(2023, 11, 30)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ xã, phường, thị trấn thuộc quận, huyện, thị xã, thành phố đạt chuẩn chuyển đổi số.",
                            ParentCriteriaId = 4
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ thủ tục hành chính đủ điều kiện theo quy định của pháp luật được cung cấp dưới hình thức dịch vụ công trực tuyến toàn trình.",
                            Description = "Cổng dịch vụ công cấp tỉnh.",
                            ParentCriteriaId = 5,
                            EvidenceInfo = "Quyết định số 942/QĐ- TTg ngày 15/6/2021 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "Tỉnh Khánh Hòa"
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ hồ sơ thủ tục hành chính được thực hiện trực tuyến toàn trình.",
                            MaxScore = 30,
                            Description = "Hệ thống giám sát, đo lường mức độ cung cấp và sử dụng dịch vụ Chính phủ số.",
                            ParentCriteriaId = 5,
                            EvidenceInfo = "Quyết định số 942/QĐ- TTg ngày 15/6/2021 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "Huyện Diên Khánh",
                            EvaluatedAt = UtcDate(2024, 4, 25)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ doanh nghiệp nhỏ và vừa sử dụng nền tảng số phục vụ sản xuất, kinh doanh.",
                            MaxScore = 20,
                            Description = "Tỷ lệ phần trăm của số doanh nghiệp nhỏ và vừa có sử dụng nền tảng số phục vụ sản xuất, kinh doanh trên tổng số doanh nghiệp nhỏ và vừa trên địa bàn huyện.",
                            ParentCriteriaId = 6,
                            EvidenceInfo = "Quyết định số 411/QĐ-TTg ngày 31/3/2022 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "TP Nha Trang",
                            EvaluatedAt = UtcDate(2024, 12, 5)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ doanh nghiệp, hợp tác xã sử dụng hợp đồng điện tử.",
                            MaxScore = 20,
                            Description = "Tỷ lệ phần trăm của số doanh nghiệp, hợp tác xã có sử dụng hợp đồng điện tử trên tổng số doanh nghiệp, hợp tác xã trên địa bàn huyện.",
                            ParentCriteriaId = 6,
                            EvidenceInfo = "Quyết định số 411/QĐ-TTg ngày 31/3/2022 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "TP Nha Trang",
                            EvaluatedAt = UtcDate(2025, 4, 1)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ trạm y tế triển khai hệ thống thông tin quản lý trạm y tế xã, phường, thị trấn theo Quyết định số 3532/QĐ-BYT ngày 12 tháng 8 năm 2020 của Bộ trưởng Bộ Y tế.",
                            Description = "Tỷ lệ phần trăm của số trạm y tế có triển khai theo Quyết định số 3532/QĐ-BYT trên tổng số trạm y tế của địa phương cấp huyện.",
                            ParentCriteriaId = 7,
                            EvidenceInfo = "Quyết định số 942/QĐ-TTg ngày 15/6/2021 của Thủ tướng Chính phủ."
                        }
                        
                    );
                    context.SaveChanges();
                    Console.WriteLine("Seeded SubCriteria.");
                }
                else
                {
                    Console.WriteLine("SubCriteria data already exists. Skipping...");
                }

                Console.WriteLine("SeedData completed.");
            }
        }
    }
}

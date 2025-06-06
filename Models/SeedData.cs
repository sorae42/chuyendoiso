﻿using chuyendoiso.Data;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
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

                // Seed Units
                if (!context.Unit.Any())
                {
                    context.Unit.AddRange(
                        new Unit { Name = "Tỉnh Khánh Hòa", Type = "Tỉnh", Code = "TKH" },
                        new Unit { Name = "TP Nha Trang", Type = "Thành phố", Code = "NT" },
                        new Unit { Name = "Huyện Diên Khánh", Type = "Huyện", Code = "DK" }
                    );
                    context.SaveChanges();
                    Console.WriteLine("Seeded Unit.");
                }

                var authUsernamesToSeed = new List<(string Username, Auth Auth)>
                {
                    ("sogtvt_khanhhoa", new Auth
                    {
                        Username = "sogtvt_khanhhoa",
                        Password = BCrypt.Net.BCrypt.HashPassword("sogtvt123"),
                        FullName = "Sở Giao thông Vận tải Khánh Hòa",
                        Email = "sogtvt@khanhhoa.gov.vn",
                        Phone = "02583889999",
                        Role = "user",
                        UnitId = 1
                    }),
                    ("huyen_dienkhanh", new Auth
                    {
                        Username = "huyen_dienkhanh",
                        Password = BCrypt.Net.BCrypt.HashPassword("dienkhanh123"),
                        FullName = "Phòng CĐS Huyện Diên Khánh",
                        Email = "cds@dienkhanh.gov.vn",
                        Phone = "02583778899",
                        Role = "user",
                        UnitId = 3
                    }),
                    ("tp_nhatrang", new Auth
                    {
                        Username = "tp_nhatrang",
                        Password = BCrypt.Net.BCrypt.HashPassword("nhatrang123"),
                        FullName = "Phòng Chuyển đổi số - TP Nha Trang",
                        Email = "cds@nhatrang.gov.vn",
                        Phone = "02583556677",
                        Role = "user",
                        UnitId = 2
                    })
                };

                foreach (var (username, auth) in authUsernamesToSeed)
                {
                    if (!context.Auth.Any(u => u.Username == username))
                    {
                        context.Auth.Add(auth);
                        Console.WriteLine($"Seeded Auth user: {username}");
                    }
                    else
                    {
                        Console.WriteLine($"Auth user {username} already exists. Skip.");
                    }
                }
                context.SaveChanges();

                // Seed TargetGroup
                var targetGroup1 = new TargetGroup { Name = "TIÊU CHÍ ĐÁNH GIÁ MỨC ĐỘ CHUYỂN ĐỔI SỐ CẤP XÃ" };
                var targetGroup2 = new TargetGroup { Name = "TIÊU CHÍ ĐÁNH GIÁ MỨC ĐỘ CHUYỂN ĐỔI SỐ CẤP HUYỆN" };

                if (!context.TargetGroup.Any())
                {
                    context.TargetGroup.AddRange(targetGroup1, targetGroup2);
                    context.SaveChanges();
                    Console.WriteLine("Seeded TargetGroup.");
                }
                else
                {
                    targetGroup1 = context.TargetGroup.FirstOrDefault(x => x.Name.Contains("CẤP XÃ"));
                    targetGroup2 = context.TargetGroup.FirstOrDefault(x => x.Name.Contains("CẤP HUYỆN"));
                    Console.WriteLine("TargetGroup already exists. Reused.");
                }

                // Seed EvaluationPeriod
                if (!context.EvaluationPeriod.Any())
                {
                    context.EvaluationPeriod.Add(new EvaluationPeriod
                    {
                        Name = "Kỳ đánh giá 2024",
                        StartDate = UtcDate(2024, 1, 1),
                        EndDate = UtcDate(2024, 12, 31),
                        EvaluationUnits = new List<EvaluationUnit>
                        {
                            new EvaluationUnit { UnitId = 1 },
                            new EvaluationUnit { UnitId = 2 },
                            new EvaluationUnit { UnitId = 3 }
                        },
                        
                    });
                    context.SaveChanges();
                    Console.WriteLine("Seeded EvaluationPeriod.");
                }

                // Seed EvaluationUnit
                if (!context.EvaluationUnit.Any())
                {
                    context.EvaluationUnit.AddRange(
                        new EvaluationUnit { EvaluationPeriodId = 1, UnitId = 1 },
                        new EvaluationUnit { EvaluationPeriodId = 1, UnitId = 2 },
                        new EvaluationUnit { EvaluationPeriodId = 1, UnitId = 3 }
                    );
                    context.SaveChanges();
                    Console.WriteLine("Seeded EvaluationUnit.");
                }

                var pc1 = new ParentCriteria { Name = "CHÍNH QUYỀN SỐ", TargetGroup = targetGroup1 };
                var pc2 = new ParentCriteria { Name = "KINH TẾ SỐ", TargetGroup = targetGroup1 };
                var pc3 = new ParentCriteria { Name = "XÃ HỘI SỐ", TargetGroup = targetGroup1 };
                var pc4 = new ParentCriteria { Name = "CHỈ TIÊU CHUNG", TargetGroup = targetGroup2 };
                var pc5 = new ParentCriteria { Name = "CHÍNH QUYỀN SỐ", TargetGroup = targetGroup2 };
                var pc6 = new ParentCriteria { Name = "KINH TẾ SỐ", TargetGroup = targetGroup2 };
                var pc7 = new ParentCriteria { Name = "XÃ HỘI SỐ", TargetGroup = targetGroup2 };

                if (!context.ParentCriteria.Any())
                {
                    context.ParentCriteria.AddRange(pc1, pc2, pc3, pc4, pc5, pc6, pc7);
                    context.SaveChanges();
                    Console.WriteLine("Seeded ParentCriteria.");
                }
                else
                {
                    var all = context.ParentCriteria.Include(p => p.TargetGroup).ToList();
                    pc1 = all.FirstOrDefault(x => x.Name == "CHÍNH QUYỀN SỐ" && x.TargetGroupId == targetGroup1.Id)!;
                    pc2 = all.FirstOrDefault(x => x.Name == "KINH TẾ SỐ" && x.TargetGroupId == targetGroup1.Id)!;
                    pc3 = all.FirstOrDefault(x => x.Name == "XÃ HỘI SỐ" && x.TargetGroupId == targetGroup1.Id)!;
                    pc4 = all.FirstOrDefault(x => x.Name == "CHỈ TIÊU CHUNG" && x.TargetGroupId == targetGroup2.Id)!;
                    pc5 = all.FirstOrDefault(x => x.Name == "CHÍNH QUYỀN SỐ" && x.TargetGroupId == targetGroup2.Id)!;
                    pc6 = all.FirstOrDefault(x => x.Name == "KINH TẾ SỐ" && x.TargetGroupId == targetGroup2.Id)!;
                    pc7 = all.FirstOrDefault(x => x.Name == "XÃ HỘI SỐ" && x.TargetGroupId == targetGroup2.Id)!;

                    Console.WriteLine("ParentCriteria already exists. Reused.");
                }

                // Seed SubCriteria
                if (!context.SubCriteria.Any())
                {
                    context.SubCriteria.AddRange(
                        new SubCriteria
                        {
                            Name = "Tỷ lệ thủ tục hành chính đủ điều kiện theo quy định của pháp luật được cung cấp dưới hình thức dịch vụ công trực tuyến toàn trình.",
                            MaxScore = 30,
                            Description = "Cổng dịch vụ công cấp tỉnh.",
                            ParentCriteria = pc1,
                            EvidenceInfo = "Quyết định số 942/QĐ- TTg ngày 15/6/2021 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "Tỉnh Khánh Hòa",
                            EvaluatedAt = UtcDate(2022, 3, 10)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ hồ sơ thủ tục hành chính được thực hiện trực tuyến toàn trình.",
                            MaxScore = 20,
                            Description = "CHệ thống giám sát, đo lường mức độ cung cấp và sử dụng dịch vụ Chính phủ số.",
                            ParentCriteria = pc1,
                            EvidenceInfo = "Quyết định số 942/QĐ- TTg ngày 15/6/2021 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "Tỉnh Khánh Hòa",
                            EvaluatedAt = UtcDate(2022, 7, 5)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ xử lý văn bản, hồ sơ công việc (trừ hồ sơ mật) trên môi trường mạng.",
                            MaxScore = 70,
                            Description = "Tỷ lệ phần trăm của số văn bản được xử lý trên phần mềm quản lý văn bản và điều hành trên tổng số văn bản đến và đi của cơ quan cấp xã.",
                            ParentCriteria = pc1,
                            EvidenceInfo = "- Quyết định số 942/QĐ- TTg ngày 15/6/2021 của Thủ tướng Chính phủ.\n- Nghị quyết số 01/NQ- CP ngày 05/01/2024 của Chính phủ.",
                            UnitEvaluate = "Huyện Diên Khánh",
                            EvaluatedAt = UtcDate(2022, 12, 28)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ doanh nghiệp nhỏ và vừa sử dụng nền tảng số phục vụ sản xuất, kinh doanh.",
                            MaxScore = 30,
                            Description = "Tỷ lệ phần trăm số doanh nghiệp nhỏ và vừa có sử dụng nền tảng số phục vụ sản xuất, kinh doanh trên tổng số doanh nghiệp nhỏ và vừa trên địa bàn xã.",
                            ParentCriteria = pc2,
                            EvidenceInfo = "Quyết định số 411/QĐ- TTg ngày 31/3/2022 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "TP Nha Trang",
                            EvaluatedAt = UtcDate(2023, 1, 15)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ thành viên của hợp tác xã, doanh nghiệp được định hướng, tập huấn ứng dụng công nghệ số phục vụ sản xuất, kinh doanh",
                            MaxScore = 40,
                            Description = "Tỷ lệ phần trăm của số thành viên của hợp tác xã, doanh nghiệp được định hướng, tập huấn ứng dụng công nghệ số phục vụ sản xuất, kinh doanh trên tổng số thành viên của hợp tác xã, doanh nghiệp trên địa bàn xã.",
                            ParentCriteria = pc2,
                            EvidenceInfo = "Công văn số 3445/BNN- VPĐP ngày 29/5/2023 của Bộ Nông nghiệp và Phát triển nông thôn.",
                            UnitEvaluate = "Huyện Diên Khánh",
                            EvaluatedAt = UtcDate(2023, 5, 20)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ dân số trưởng thành từ 15 tuổi trở lên có điện thoại thông minh.",
                            MaxScore = 50,
                            Description = "Tỷ lệ phần trăm dân số trưởng thành từ 15 tuổi trở lên có điện thoại thông minh trên tổng dân số từ 15 tuổi trở lên tại địa phương cấp xã.",
                            ParentCriteria = pc3,
                            EvidenceInfo = "Quyết định số 411/QĐ- TTg ngày 31/3/2022 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "TP Nha Trang",
                            EvaluatedAt = UtcDate(2023, 8, 12)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ hộ gia đình được phủ mạng Internet băng rộng cáp quang.",
                            MaxScore = 10,
                            Description = "Tỷ lệ phần trăm của số hộ gia đình được phủ mạng Internet băng rộng cáp quang trên tổng số hộ gia đình tại địa phương cấp xã.",
                            ParentCriteria = pc3,
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
                            ParentCriteria = pc5,
                            EvidenceInfo = "Quyết định số 942/QĐ- TTg ngày 15/6/2021 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "Tỉnh Khánh Hòa"
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ hồ sơ thủ tục hành chính được thực hiện trực tuyến toàn trình.",
                            MaxScore = 30,
                            Description = "Hệ thống giám sát, đo lường mức độ cung cấp và sử dụng dịch vụ Chính phủ số.",
                            ParentCriteria = pc5,
                            EvidenceInfo = "Quyết định số 942/QĐ- TTg ngày 15/6/2021 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "Huyện Diên Khánh",
                            EvaluatedAt = UtcDate(2024, 4, 25)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ doanh nghiệp nhỏ và vừa sử dụng nền tảng số phục vụ sản xuất, kinh doanh.",
                            MaxScore = 20,
                            Description = "Tỷ lệ phần trăm của số doanh nghiệp nhỏ và vừa có sử dụng nền tảng số phục vụ sản xuất, kinh doanh trên tổng số doanh nghiệp nhỏ và vừa trên địa bàn huyện.",
                            ParentCriteria = pc6,
                            EvidenceInfo = "Quyết định số 411/QĐ-TTg ngày 31/3/2022 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "TP Nha Trang",
                            EvaluatedAt = UtcDate(2024, 12, 5)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ doanh nghiệp, hợp tác xã sử dụng hợp đồng điện tử.",
                            MaxScore = 20,
                            Description = "Tỷ lệ phần trăm của số doanh nghiệp, hợp tác xã có sử dụng hợp đồng điện tử trên tổng số doanh nghiệp, hợp tác xã trên địa bàn huyện.",
                            ParentCriteria = pc6,
                            EvidenceInfo = "Quyết định số 411/QĐ-TTg ngày 31/3/2022 của Thủ tướng Chính phủ.",
                            UnitEvaluate = "TP Nha Trang",
                            EvaluatedAt = UtcDate(2025, 4, 1)
                        },
                        new SubCriteria
                        {
                            Name = "Tỷ lệ trạm y tế triển khai hệ thống thông tin quản lý trạm y tế xã, phường, thị trấn theo Quyết định số 3532/QĐ-BYT ngày 12 tháng 8 năm 2020 của Bộ trưởng Bộ Y tế.",
                            Description = "Tỷ lệ phần trăm của số trạm y tế có triển khai theo Quyết định số 3532/QĐ-BYT trên tổng số trạm y tế của địa phương cấp huyện.",
                            ParentCriteria = pc7,
                            EvidenceInfo = "Quyết định số 942/QĐ-TTg ngày 15/6/2021 của Thủ tướng Chính phủ."
                        }

                    );
                    context.SaveChanges();
                    Console.WriteLine("Seeded SubCriteria.");
                }
                else
                {
                    Console.WriteLine("SubCriteria data already exists. Skip");
                }

                // Seed ReviewCouncil
                if (!context.ReviewCouncil.Any())
                {
                    var admin = context.Auth.FirstOrDefault(u => u.Username == "admin");
                    if (admin != null)
                    {
                        var council = new ReviewCouncil
                        {
                            Name = "Hội đồng đánh giá chuyển đổi số tỉnh Khánh Hòa",
                            CreatedAt = UtcDate(2024, 1, 1),
                            CreatedById = admin.Id
                        };

                        context.ReviewCouncil.Add(council);
                        context.SaveChanges();

                        // Seed 1 Chủ tịch: admin
                        context.Reviewer.Add(new Reviewer
                        {
                            ReviewCouncilId = council.Id,
                            AuthId = admin.Id,
                            IsChair = true
                        });
                        context.SaveChanges();
                        Console.WriteLine("Seeded ReviewCouncil and admin as Chair.");
                    }
                    else
                    {
                        Console.WriteLine("Admin not found. Cannot seed ReviewCouncil.");
                    }
                }

                // Seed 1 thành viên reviewer (tp_nhatrang)
                var nhatrangUser = context.Auth.FirstOrDefault(u => u.Username == "tp_nhatrang");
                var existingCouncil = context.ReviewCouncil.FirstOrDefault();

                if (nhatrangUser != null && existingCouncil != null)
                {
                    bool alreadyExists = context.Reviewer.Any(r =>
                        r.ReviewCouncilId == existingCouncil.Id && r.AuthId == nhatrangUser.Id);

                    if (!alreadyExists)
                    {
                        context.Reviewer.Add(new Reviewer
                        {
                            ReviewCouncilId = existingCouncil.Id,
                            AuthId = nhatrangUser.Id,
                            IsChair = false
                        });
                        context.SaveChanges();
                        Console.WriteLine("Seeded tp_nhatrang as reviewer member.");
                    }
                    else
                    {
                        Console.WriteLine("tp_nhatrang already seeded as reviewer.");
                    }
                }
                else
                {
                    Console.WriteLine("Reviewer member not found or no council.");
                }

                // Seed ReviewAssignment
                if (!context.ReviewAssignment.Any())
                {
                    var reviewer = context.Reviewer.FirstOrDefault(r => !r.IsChair); // lấy thành viên
                    var unit = context.Unit.FirstOrDefault(u => u.Id == 2);
                    var sub = context.SubCriteria.FirstOrDefault(s => s.Id == 5);

                    if (reviewer != null && unit != null && sub != null)
                    {
                        context.ReviewAssignment.Add(new ReviewAssignment
                        {
                            ReviewerId = reviewer.Id,
                            UnitId = unit.Id,
                            SubCriteriaId = sub.Id
                        });
                        context.SaveChanges();
                        Console.WriteLine("Seeded ReviewAssignment.");
                    }
                    else
                    {
                        Console.WriteLine("Cannot seed ReviewAssignment: reviewer/unit/subcriteria missing.");
                    }
                }
                else
                {
                    Console.WriteLine("ReviewAssignment already exists. Skip");
                }

                Console.WriteLine("SeedData completed.");
            }
        }
    }
}

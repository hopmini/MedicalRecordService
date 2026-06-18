using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MedicalRecordService.Data;
using MedicalRecordService.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Nối cáp Database Postgres 
var connectionString = builder.Configuration["CONNECTION_STRING"] 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// ĐỔI TÊN THÀNH MedicalDbContext
builder.Services.AddDbContext<MedicalDbContext>(options =>
    options.UseNpgsql(connectionString));

// Đăng ký HttpClient
builder.Services.AddHttpClient();

// BỎ HẾT MẤY CÁI SERVICE CỦA BỌN NHÓM 1 ĐI
// Chỗ này sau m có service riêng (ví dụ IRecordService) thì add vào sau

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --- CẤU HÌNH JWT GIỮ NGUYÊN ---
var jwtSecret = builder.Configuration["JWT_SECRET"] ?? builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? builder.Configuration["Jwt:Audience"];
var key = Encoding.ASCII.GetBytes(jwtSecret!);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- TỰ ĐỘNG MIGRATE DB CƠ BẢN (VỚI VÒNG LẶP RETRY CHO DOCKER) ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MedicalDbContext>();
    int maxRetries = 15;
    int delaySeconds = 3;
    bool success = false;

    for (int i = 1; i <= maxRetries; i++)
    {
        try
        {
            // Tự động check và tạo bảng nếu DB chưa có gì
            if (db.Database.GetPendingMigrations().Any())
            {
                db.Database.Migrate();
                Console.WriteLine("✅ Đã chạy Migration tự động cho MedicalDB.");
            }
            else
            {
                // Ensure db is created if there are no pending migrations
                db.Database.EnsureCreated();
                Console.WriteLine("✅ MedicalDB Database ensured & created successfully.");
            }
            success = true;

            // Self-healing: Ensure new columns exist
            try
            {
                // Patient columns
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"GatewayPatientId\" integer NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"BloodGroup\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"Phone\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"Email\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"Address\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"IdentityCard\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"InsuranceNumber\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"Occupation\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"Ethnicity\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"Nationality\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"EmergencyContact\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"EmergencyPhone\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"CreatedAt\" timestamp NOT NULL DEFAULT NOW();");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"UpdatedAt\" timestamp NULL;");
                Console.WriteLine("✅ Self-healing: Ensured new Patient columns exist.");

                // MedicalRecord columns
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"Title\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"Weight\" double precision NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"Height\" double precision NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"BloodPressure\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"HeartRate\" integer NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"Temperature\" double precision NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"CustomMetricsJson\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"AttachmentsJson\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"DiagnosisCode\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"DiagnosisCodeName\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"AdmissionDate\" timestamp NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"DischargeDate\" timestamp NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"DischargeDiagnosis\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"DischargeInstructions\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"FollowUpInstructions\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"FollowUpClinic\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"FollowUpDate\" timestamp NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"UpdatedAt\" timestamp NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"UpdatedBy\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"AllergiesSnapshot\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"PreliminaryDiagnosis\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"FinalDiagnosis\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"MedicalHistorySnapshot\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"IsDeleted\" boolean NOT NULL DEFAULT false;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"IsLocked\" boolean NOT NULL DEFAULT false;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"DeletedAt\" timestamp NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"DeletedBy\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"LockedAt\" timestamp NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"LockedBy\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"Status\" integer NOT NULL DEFAULT 0;");
                Console.WriteLine("✅ Self-healing: Ensured new MedicalRecords columns exist.");

                // Prescription columns
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Prescriptions\" ADD COLUMN IF NOT EXISTS \"Status\" text NOT NULL DEFAULT 'active';");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Prescriptions\" ADD COLUMN IF NOT EXISTS \"ExpiryDate\" timestamp NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Prescriptions\" ADD COLUMN IF NOT EXISTS \"RefillCount\" integer NOT NULL DEFAULT 0;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Prescriptions\" ADD COLUMN IF NOT EXISTS \"UpdatedAt\" timestamp NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Prescriptions\" ADD COLUMN IF NOT EXISTS \"UpdatedBy\" text NULL;");
                Console.WriteLine("✅ Self-healing: Ensured new Prescription columns exist.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Self-healing warning: Could not verify/add columns: {ex.Message}");
            }

            // Ensure Icd10Codes table exists (for databases created before model update)
            try
            {
                db.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS ""Icd10Codes"" (
                        ""Id"" serial PRIMARY KEY,
                        ""Code"" text NOT NULL,
                        ""Name"" text NOT NULL,
                        ""Category"" text NOT NULL
                    );");
                Console.WriteLine("✅ Ensured Icd10Codes table exists.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Icd10Code table creation warning: {ex.Message}");
            }

            // Seed ICD-10 codes
            try
            {
                if (!db.Icd10Codes.Any())
                {
                    db.Icd10Codes.AddRange(new List<Icd10Code>
                    {
                        new() { Code = "A00.0", Name = "Bệnh tả do Vibrio cholerae", Category = "Bệnh truyền nhiễm" },
                        new() { Code = "A09", Name = "Viêm dạ dày-ruột và viêm đại tràng nhiễm khuẩn", Category = "Bệnh truyền nhiễm" },
                        new() { Code = "B00", Name = "Nhiễm Herpes virus", Category = "Bệnh truyền nhiễm" },
                        new() { Code = "C16", Name = "U ác tính ở dạ dày", Category = "Ung thư" },
                        new() { Code = "C18", Name = "U ác tính ở đại tràng", Category = "Ung thư" },
                        new() { Code = "C34", Name = "U ác tính ở phế quản và phổi", Category = "Ung thư" },
                        new() { Code = "D50", Name = "Thiếu máu do thiếu sắt", Category = "Bệnh máu" },
                        new() { Code = "E10", Name = "Đái tháo đường type 1", Category = "Nội tiết" },
                        new() { Code = "E11", Name = "Đái tháo đường type 2", Category = "Nội tiết" },
                        new() { Code = "E14", Name = "Đái tháo đường không xác định type", Category = "Nội tiết" },
                        new() { Code = "E78.0", Name = "Tăng cholesterol máu thuần túy", Category = "Nội tiết" },
                        new() { Code = "F32", Name = "Giai đoạn trầm cảm", Category = "Tâm thần" },
                        new() { Code = "F41", Name = "Rối loạn lo âu khác", Category = "Tâm thần" },
                        new() { Code = "G40", Name = "Động kinh", Category = "Thần kinh" },
                        new() { Code = "G47.0", Name = "Rối loạn giấc ngủ (mất ngủ)", Category = "Thần kinh" },
                        new() { Code = "H25", Name = "Đục thủy tinh thể do tuổi già", Category = "Mắt" },
                        new() { Code = "H66", Name = "Viêm tai giữa có mủ", Category = "Tai Mũi Họng" },
                        new() { Code = "H81", Name = "Rối loạn chức năng tiền đình", Category = "Tai Mũi Họng" },
                        new() { Code = "I10", Name = "Tăng huyết áp nguyên phát", Category = "Tim mạch" },
                        new() { Code = "I11", Name = "Bệnh tim do tăng huyết áp", Category = "Tim mạch" },
                        new() { Code = "I20", Name = "Đau thắt ngực", Category = "Tim mạch" },
                        new() { Code = "I21", Name = "Nhồi máu cơ tim cấp", Category = "Tim mạch" },
                        new() { Code = "I25", Name = "Bệnh tim thiếu máu cục bộ mãn tính", Category = "Tim mạch" },
                        new() { Code = "I48", Name = "Rung nhĩ", Category = "Tim mạch" },
                        new() { Code = "I50", Name = "Suy tim", Category = "Tim mạch" },
                        new() { Code = "I64", Name = "Đột quỵ", Category = "Tim mạch" },
                        new() { Code = "J00", Name = "Viêm mũi họng cấp (cảm lạnh)", Category = "Hô hấp" },
                        new() { Code = "J01", Name = "Viêm xoang cấp", Category = "Hô hấp" },
                        new() { Code = "J02", Name = "Viêm họng cấp", Category = "Hô hấp" },
                        new() { Code = "J03", Name = "Viêm amidan cấp", Category = "Hô hấp" },
                        new() { Code = "J04", Name = "Viêm thanh quản cấp", Category = "Hô hấp" },
                        new() { Code = "J06", Name = "Nhiễm trùng đường hô hấp trên cấp", Category = "Hô hấp" },
                        new() { Code = "J10", Name = "Cúm do virus cúm đã xác định", Category = "Hô hấp" },
                        new() { Code = "J15", Name = "Viêm phổi do vi khuẩn", Category = "Hô hấp" },
                        new() { Code = "J20", Name = "Viêm phế quản cấp", Category = "Hô hấp" },
                        new() { Code = "J30", Name = "Viêm mũi dị ứng", Category = "Hô hấp" },
                        new() { Code = "J42", Name = "Viêm phế quản mãn tính", Category = "Hô hấp" },
                        new() { Code = "J45", Name = "Hen phế quản", Category = "Hô hấp" },
                        new() { Code = "K21", Name = "Trào ngược dạ dày-thực quản", Category = "Tiêu hóa" },
                        new() { Code = "K25", Name = "Loét dạ dày", Category = "Tiêu hóa" },
                        new() { Code = "K26", Name = "Loét tá tràng", Category = "Tiêu hóa" },
                        new() { Code = "K27", Name = "Loét dạ dày-tá tràng", Category = "Tiêu hóa" },
                        new() { Code = "K29", Name = "Viêm dạ dày và tá tràng", Category = "Tiêu hóa" },
                        new() { Code = "K30", Name = "Chứng khó tiêu", Category = "Tiêu hóa" },
                        new() { Code = "K52", Name = "Viêm dạ dày-ruột không nhiễm khuẩn", Category = "Tiêu hóa" },
                        new() { Code = "K59", Name = "Rối loạn chức năng ruột khác", Category = "Tiêu hóa" },
                        new() { Code = "K70", Name = "Bệnh gan do rượu", Category = "Tiêu hóa" },
                        new() { Code = "K80", Name = "Sỏi mật", Category = "Tiêu hóa" },
                        new() { Code = "L20", Name = "Viêm da cơ địa (chàm)", Category = "Da liễu" },
                        new() { Code = "L40", Name = "Vảy nến", Category = "Da liễu" },
                        new() { Code = "L50", Name = "Mề đay", Category = "Da liễu" },
                        new() { Code = "M05", Name = "Viêm đa khớp dạng thấp huyết thanh dương tính", Category = "Cơ xương khớp" },
                        new() { Code = "M10", Name = "Gout (thống phong)", Category = "Cơ xương khớp" },
                        new() { Code = "M17", Name = "Thoái hóa khớp gối", Category = "Cơ xương khớp" },
                        new() { Code = "M47", Name = "Thoái hóa cột sống", Category = "Cơ xương khớp" },
                        new() { Code = "M51", Name = "Thoát vị đĩa đệm thắt lưng", Category = "Cơ xương khớp" },
                        new() { Code = "M54", Name = "Đau lưng", Category = "Cơ xương khớp" },
                        new() { Code = "N10", Name = "Viêm thận kẽ cấp", Category = "Thận - Tiết niệu" },
                        new() { Code = "N18", Name = "Bệnh thận mãn tính", Category = "Thận - Tiết niệu" },
                        new() { Code = "N20", Name = "Sỏi thận và niệu quản", Category = "Thận - Tiết niệu" },
                        new() { Code = "N40", Name = "Tăng sản lành tính tuyến tiền liệt", Category = "Thận - Tiết niệu" },
                        new() { Code = "N80", Name = "Lạc nội mạc tử cung", Category = "Sản phụ khoa" },
                        new() { Code = "R05", Name = "Ho", Category = "Triệu chứng" },
                        new() { Code = "R06.4", Name = "Tăng thông khí", Category = "Triệu chứng" },
                        new() { Code = "R10", Name = "Đau bụng", Category = "Triệu chứng" },
                        new() { Code = "R11", Name = "Buồn nôn và nôn", Category = "Triệu chứng" },
                        new() { Code = "R42", Name = "Chóng mặt", Category = "Triệu chứng" },
                        new() { Code = "R51", Name = "Đau đầu", Category = "Triệu chứng" },
                        new() { Code = "S06", Name = "Chấn thương nội sọ", Category = "Chấn thương" },
                        new() { Code = "S22", Name = "Gãy xương sườn", Category = "Chấn thương" },
                        new() { Code = "S52", Name = "Gãy xương cẳng tay", Category = "Chấn thương" },
                        new() { Code = "S72", Name = "Gãy xương đùi", Category = "Chấn thương" },
                        new() { Code = "T14", Name = "Chấn thương bề mặt không xác định", Category = "Chấn thương" },
                        new() { Code = "Z00", Name = "Khám sức khỏe tổng quát", Category = "Khám sức khỏe" },
                        new() { Code = "Z01", Name = "Khám sức khỏe định kỳ", Category = "Khám sức khỏe" },
                        new() { Code = "Z23", Name = "Tiêm chủng", Category = "Dự phòng" }
                    });
                    db.SaveChanges();
                    Console.WriteLine("✅ Seeded ICD-10 codes successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ ICD-10 seed warning: {ex.Message}");
            }

            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MedicalRecordService Retry {i}/{maxRetries}] Failed to connect to DB: {ex.Message}. Retrying in {delaySeconds}s...");
            System.Threading.Thread.Sleep(delaySeconds * 1000);
        }
    }

    if (!success)
    {
        Console.WriteLine("❌ LỖI KẾT NỐI DATABASE: Không thể kết nối tới Postgres database sau 15 lần thử.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll"); 
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
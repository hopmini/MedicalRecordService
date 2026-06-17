using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MedicalRecordService.Data; // Đổi namespace
// using MedicalRecordService.Models; // Comment tạm chờ m tạo Models

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

            // Self-healing: Ensure "GatewayPatientId" column exists in "Patients" table
            try
            {
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"GatewayPatientId\" integer NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"Patients\" ADD COLUMN IF NOT EXISTS \"BloodGroup\" text NULL;");
                Console.WriteLine("✅ Self-healing: Ensured \"GatewayPatientId\" and \"BloodGroup\" columns exist in \"Patients\" table.");
                
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"Title\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"Weight\" double precision NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"Height\" double precision NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"BloodPressure\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"HeartRate\" integer NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"Temperature\" double precision NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"CustomMetricsJson\" text NULL;");
                db.Database.ExecuteSqlRaw("ALTER TABLE \"MedicalRecords\" ADD COLUMN IF NOT EXISTS \"AttachmentsJson\" text NULL;");
                Console.WriteLine("✅ Self-healing: Ensured new MedicalRecords columns exist.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Self-healing warning: Could not verify/add columns: {ex.Message}");
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
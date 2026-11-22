using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ReviewIntelligence.Api.Data;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Middleware;
using ReviewIntelligence.Api.Models.Entities;
using ReviewIntelligence.Api.Options;
using ReviewIntelligence.Api.Services;
using ReviewIntelligence.Api.Services.Ai;

var builder = WebApplication.CreateBuilder(args);

// ---------- Database (SQL Server – ReviewIntelligenceDB) ----------
var connectionString = builder.Configuration.GetConnectionString("ReviewIntelligenceDB")
    ?? throw new InvalidOperationException("Thiếu ConnectionStrings:ReviewIntelligenceDB trong cấu hình.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

// ---------- JWT ----------
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Thiếu cấu hình Jwt.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Giá trị khớp CHECK constraint CK_NguoiDung_VaiTro: 'KhachHang' | 'QuanTri'
    options.AddPolicy(ChinhSach.QuanTri, p => p.RequireRole(VaiTroNguoiDung.QuanTri));
    options.AddPolicy(ChinhSach.KhachHang, p => p.RequireRole(VaiTroNguoiDung.KhachHang));
});

// ---------- CORS ----------
const string FrontendCors = "FrontendCors";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy(FrontendCors, policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

// ---------- Services ----------
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITraCuuService, TraCuuService>();
builder.Services.AddScoped<ISanPhamService, SanPhamService>();
builder.Services.AddScoped<IDanhGiaService, DanhGiaService>();

// ---------- ai-service (gửi đánh giá vừa duyệt đi phân tích ABSA + vector Qdrant) ----------
builder.Services.AddOptions<AiServiceOptions>()
    .Bind(builder.Configuration.GetSection(AiServiceOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddHttpClient<IAiServiceClient, AiServiceClient>((sp, http) =>
{
    var ai = sp.GetRequiredService<IOptions<AiServiceOptions>>().Value;
    http.BaseAddress = new Uri(ai.BaseUrl.EndsWith('/') ? ai.BaseUrl : ai.BaseUrl + "/");
    http.Timeout = TimeSpan.FromSeconds(ai.ThoiGianChoGiay);
});
builder.Services.AddSingleton<HangDoiPhanTichDanhGia>();
builder.Services.AddSingleton<IHangDoiPhanTichDanhGia>(sp => sp.GetRequiredService<HangDoiPhanTichDanhGia>());
builder.Services.AddHostedService<PhanTichDanhGiaWorker>();

// ---------- Controllers + lỗi chuẩn ProblemDetails ----------
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter()))
    .ConfigureApiBehaviorOptions(o =>
    {
        // 400 validation: tiêu đề tiếng Việt, giữ nguyên danh sách lỗi theo trường.
        o.InvalidModelStateResponseFactory = ctx =>
        {
            var problem = new ValidationProblemDetails(ctx.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dữ liệu đầu vào không hợp lệ.",
                Instance = ctx.HttpContext.Request.Path
            };
            return new BadRequestObjectResult(problem);
        };
    });
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Review Intelligence API", Version = "v1" });

    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT access token (không cần tiền tố 'Bearer ').",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

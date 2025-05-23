
using IdentityAPI.IServices;
using IdentityAPI.JWT;
using IdentityAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace IdentityAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			// Add services to the container.(26-Apr-25)
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

			// Add ASP.NET Core Identity START(26-Apr-25)
			builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
																			options => { options.Password.RequiredLength = 10;          //cahnging the default/configuring the password
																				         options.Password.RequiredUniqueChars = 3;
																				options.SignIn.RequireConfirmedEmail = true;		    //For Email Conformation
																			})
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();
			//IDENTITY END


			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			//SWAGGER AUTHORIZATION START
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Identity API", Version = "v1" });

				c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = Microsoft.OpenApi.Models.ParameterLocation.Header,
					Description = "Enter 'Bearer' [space] and then your valid JWT token.\r\n\r\nExample: \"Bearer abcdef12345\"",
				});

				c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
				{
					{
						new Microsoft.OpenApi.Models.OpenApiSecurityScheme
						{
							Reference = new Microsoft.OpenApi.Models.OpenApiReference
							{
								Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						Array.Empty<string>()
					}
				});
			});
			//SWAGGER AUTHORIZATION END

			builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));//for binding apsettings.json with smtpSettings class
			builder.Services.AddScoped<IEmailSender, EmailSender>();

			builder.Services.AddScoped<ISendGridEmailService,SendGridEmailService>();//SENDGRID


			//JWT START(27-Apr-25)
			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(options =>
			{
				options.SaveToken = true;
				options.RequireHttpsMetadata = false; // Only set to false during development!
				options.TokenValidationParameters = new TokenValidationParameters()
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidAudience = builder.Configuration["JWT:ValidAudience"],
					ValidateLifetime = true, //  CRITICAL middleware for checking token expiry! This enforces expiration
					ClockSkew = TimeSpan.Zero, //  CRITICAL: no grace period after expiration(JWT tokens are valid for up to 5 minutes after their expiry time)
					ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
				};
			});
			//JWT END

			builder.Services.AddScoped<JwtTokenGenerator>();//Seprate class is created so

			// Add Authorization - GLOBAL AUTHORIZATION POLICY START
			builder.Services.AddAuthorization(options =>
			{
				options.DefaultPolicy = new AuthorizationPolicyBuilder()
					.RequireAuthenticatedUser() //  Require ALL users to be authenticated.
					.Build();

				// Add your CLAIM-based policies(30-Apr-25)
				options.AddPolicy("DeleteRolePolicy", policy =>
					policy.RequireClaim("DeleteRole"));
			});

			builder.Services.AddControllers(options =>
			{
				options.Filters.Add(new AuthorizeFilter());
			});
			//GLOBAL AUTHORIZATION POLICY END


			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthentication();//For (Identity Setup) and Validates Token -> Creates User.ClaimsPrincipal

			app.UseAuthorization();//Checks [Authorize(Roles = "Admin")]


			app.MapControllers();

			app.Run();
		}
	}
}

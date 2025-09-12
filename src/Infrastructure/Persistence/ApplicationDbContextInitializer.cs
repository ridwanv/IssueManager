using System;
using System.Reflection;
using CleanArchitecture.Blazor.Application.Common.Constants.ClaimTypes;
using CleanArchitecture.Blazor.Application.Common.Constants.Roles;
using CleanArchitecture.Blazor.Application.Common.Constants.User;
using CleanArchitecture.Blazor.Application.Common.Security;
using CleanArchitecture.Blazor.Domain.Identity;
using CleanArchitecture.Blazor.Domain.Entities;
using CleanArchitecture.Blazor.Domain.Enums;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence;

public class ApplicationDbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger,
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _context = dbContextFactory.CreateDbContext();
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsRelational())
                await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedTenantsAsync();
            await SeedRolesAsync();
            await MigrateUserTypeToRolesAsync(); // Convert existing UserType values to roles
            await SeedUsersAsync();
            await SeedDataAsync();
            _context.ChangeTracker.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static IEnumerable<string> GetAllPermissions()
    {
        var allPermissions = new List<string>();
        var modules = typeof(Permissions).GetNestedTypes();

        foreach (var module in modules)
        {
            var moduleName = string.Empty;
            var moduleDescription = string.Empty;

            var fields = module.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (var fi in fields)
            {
                var propertyValue = fi.GetValue(null);

                if (propertyValue is not null)
                    allPermissions.Add((string)propertyValue);
            }
        }

        return allPermissions;
    }




    private async Task SeedTenantsAsync()
    {
        if (await _context.Tenants.AnyAsync()) return;

        _logger.LogInformation("Seeding organizations...");
        var tenants = new[]
        {
                new Tenant { Name = "Main", Description = "Main Site" },
                new Tenant { Name = "Europe", Description = "Europe Site" }
            };

        await _context.Tenants.AddRangeAsync(tenants);
        await _context.SaveChangesAsync();
    }

    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Seeding roles...");
        
        // Create all persona-based roles as global (TenantId = null)
        await CreateGlobalRoleIfNotExistsAsync(RoleName.PlatformOwner, "Platform super admin with cross-tenant access");
        await CreateGlobalRoleIfNotExistsAsync(RoleName.TenantOwner, "Tenant administrator with full tenant management");
        await CreateGlobalRoleIfNotExistsAsync(RoleName.IssueManager, "Manages issues and assigns work within tenant");
        await CreateGlobalRoleIfNotExistsAsync(RoleName.IssueAssignee, "Works on assigned issues and updates status");
        await CreateGlobalRoleIfNotExistsAsync(RoleName.ChatAgent, "Handles escalated customer conversations");
        await CreateGlobalRoleIfNotExistsAsync(RoleName.ChatSupervisor, "Supervises chat agents and handles escalations");
        await CreateGlobalRoleIfNotExistsAsync(RoleName.EndUser, "Basic user with limited system access");
        await CreateGlobalRoleIfNotExistsAsync(RoleName.ApiConsumer, "External system integration access");

        // Migrate existing tenant-scoped roles to global if needed
        await MigrateLegacyRolesToGlobalAsync();

        // Assign permissions to roles
        await AssignPermissionsToRolesAsync();
    }

    private async Task CreateGlobalRoleIfNotExistsAsync(string roleName, string description)
    {
        var existingRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == null);
        if (existingRole == null)
        {
            var role = new ApplicationRole(roleName)
            {
                Description = description,
                TenantId = null // Global role
            };
            await _roleManager.CreateAsync(role);
            _logger.LogInformation("Created global role: {RoleName}", roleName);
        }
    }

    private async Task MigrateLegacyRolesToGlobalAsync()
    {
        // Migrate existing tenant-scoped legacy roles to global
        var legacyRoles = new[] { RoleName.Admin, RoleName.Basic, RoleName.Users };
        
        foreach (var roleName in legacyRoles)
        {
            var globalRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == null);
            if (globalRole == null)
            {
                var description = roleName switch
                {
                    RoleName.Admin => "Legacy admin role - use TenantOwner instead",
                    RoleName.Basic => "Legacy basic role - use EndUser instead", 
                    RoleName.Users => "Legacy users role - use persona-specific roles instead",
                    _ => "Legacy role"
                };
                
                await CreateGlobalRoleIfNotExistsAsync(roleName, description);
            }
        }
    }

    private async Task AssignPermissionsToRolesAsync()
    {
        var permissions = GetAllPermissions();
        
        // Assign full permissions to PlatformOwner (super admin across all tenants)
        await AssignAllPermissionsToRoleAsync(RoleName.PlatformOwner, permissions);
        
        // Assign tenant admin permissions to TenantOwner
        await AssignPermissionsToRoleAsync(RoleName.TenantOwner, permissions.Where(p => 
            p.StartsWith("Permissions.Tenant.") ||
            p.StartsWith("Permissions.Users.") ||
            p.StartsWith("Permissions.Roles.") ||
            p.StartsWith("Permissions.Issues.") ||
            p.StartsWith("Permissions.Conversations.") ||
            p.StartsWith("Permissions.Contacts.") ||
            p.StartsWith("Permissions.Products.") ||
            p.StartsWith("Permissions.Documents.") ||
            p.StartsWith("Permissions.AuditTrails.") ||
            p.StartsWith("Permissions.Logs.") ||
            p.StartsWith("Permissions.Dashboards.")));
        
        // Assign issue management permissions
        await AssignPermissionsToRoleAsync(RoleName.IssueManager, permissions.Where(p => 
            p.StartsWith("Permissions.IssueManager.") ||
            p.StartsWith("Permissions.Issues.") ||
            p.StartsWith("Permissions.Contacts.") ||
            p.StartsWith("Permissions.Products.View") ||
            p.StartsWith("Permissions.Dashboards.")));
            
        // Assign issue assignee permissions
        await AssignPermissionsToRoleAsync(RoleName.IssueAssignee, permissions.Where(p => 
            p.StartsWith("Permissions.IssueAssignee.") ||
            p.StartsWith("Permissions.Issues.View") ||
            p.StartsWith("Permissions.Issues.Edit") ||
            p.StartsWith("Permissions.Contacts.View") ||
            p.StartsWith("Permissions.Products.View")));
            
        // Assign chat agent permissions
        await AssignPermissionsToRoleAsync(RoleName.ChatAgent, permissions.Where(p => 
            p.StartsWith("Permissions.ChatAgent.") ||
            p.StartsWith("Permissions.Conversations.View") ||
            p.StartsWith("Permissions.Conversations.JoinAsAgent") ||
            p.StartsWith("Permissions.Conversations.Transfer") ||
            p.StartsWith("Permissions.Issues.View") ||
            p.StartsWith("Permissions.Issues.Create") ||
            p.StartsWith("Permissions.Contacts.")));
            
        // Assign chat supervisor permissions
        await AssignPermissionsToRoleAsync(RoleName.ChatSupervisor, permissions.Where(p => 
            p.StartsWith("Permissions.ChatSupervisor.") ||
            p.StartsWith("Permissions.ChatAgent.") ||
            p.StartsWith("Permissions.Conversations.") ||
            p.StartsWith("Permissions.Issues.") ||
            p.StartsWith("Permissions.Contacts.") ||
            p.StartsWith("Permissions.Agents.")));
            
        // Assign end user permissions
        await AssignPermissionsToRoleAsync(RoleName.EndUser, permissions.Where(p => 
            p.StartsWith("Permissions.EndUser.") ||
            p.StartsWith("Permissions.Issues.View") ||
            p.StartsWith("Permissions.Products.View") ||
            p.StartsWith("Permissions.Dashboards.View")));
            
        // Assign API consumer permissions
        await AssignPermissionsToRoleAsync(RoleName.ApiConsumer, permissions.Where(p => 
            p.StartsWith("Permissions.ApiConsumer.") ||
            p.StartsWith("Permissions.Issues.") ||
            p.StartsWith("Permissions.Conversations.") ||
            p.StartsWith("Permissions.Products.View") ||
            p.StartsWith("Permissions.Contacts.")));
            
        // Legacy role compatibility - maintain existing behavior
        await AssignAllPermissionsToRoleAsync(RoleName.Admin, permissions);
        await AssignPermissionsToRoleAsync(RoleName.Basic, permissions.Where(p => 
            p.StartsWith("Permissions.Products") || 
            p.StartsWith("Permissions.Conversations") ||
            p.StartsWith("Permissions.EndUser.")));
        await AssignPermissionsToRoleAsync(RoleName.Users, permissions.Where(p => 
            p.StartsWith("Permissions.Issues.View") ||
            p.StartsWith("Permissions.Products.View") ||
            p.StartsWith("Permissions.Conversations.View")));
    }

    private async Task AssignAllPermissionsToRoleAsync(string roleName, IEnumerable<string> permissions)
    {
        var role = await _roleManager.Roles.FirstAsync(r => r.Name == roleName && r.TenantId == null);
        
        foreach (var permission in permissions)
        {
            var existingClaim = await _roleManager.GetClaimsAsync(role);
            if (!existingClaim.Any(c => c.Type == ApplicationClaimTypes.Permission && c.Value == permission))
            {
                var claim = new Claim(ApplicationClaimTypes.Permission, permission);
                await _roleManager.AddClaimAsync(role, claim);
            }
        }
    }

    private async Task AssignPermissionsToRoleAsync(string roleName, IEnumerable<string> permissions)
    {
        var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName && r.TenantId == null);
        if (role == null) return;
        
        foreach (var permission in permissions)
        {
            var existingClaim = await _roleManager.GetClaimsAsync(role);
            if (!existingClaim.Any(c => c.Type == ApplicationClaimTypes.Permission && c.Value == permission))
            {
                var claim = new Claim(ApplicationClaimTypes.Permission, permission);
                await _roleManager.AddClaimAsync(role, claim);
            }
        }
    }

    private async Task MigrateUserTypeToRolesAsync()
    {
        _logger.LogInformation("Migrating UserType values to role assignments...");
        
        var usersWithoutRoles = await _userManager.Users
            .Where(u => !u.UserRoles.Any())
            .ToListAsync();
            
        foreach (var user in usersWithoutRoles)
        {
            var targetRoleName = user.UserType switch
            {
                UserType.PlatformOwner => RoleName.PlatformOwner,
                UserType.TenantOwner => RoleName.TenantOwner,
                UserType.IssueManager => RoleName.IssueManager,
                UserType.IssueAssignee => RoleName.IssueAssignee,
                UserType.ChatAgent => RoleName.ChatAgent,
                UserType.ChatSupervisor => RoleName.ChatSupervisor,
                UserType.EndUser => RoleName.EndUser,
                UserType.ApiConsumer => RoleName.ApiConsumer,
                _ => RoleName.EndUser // Default fallback
            };
            
            try
            {
                var result = await _userManager.AddToRoleAsync(user, targetRoleName);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Migrated user {UserId} from UserType.{UserType} to role {RoleName}",
                        user.Id, user.UserType, targetRoleName);
                }
                else
                {
                    _logger.LogWarning("Failed to migrate user {UserId} to role {RoleName}: {Errors}",
                        user.Id, targetRoleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating user {UserId} from UserType.{UserType} to role {RoleName}",
                    user.Id, user.UserType, targetRoleName);
            }
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _userManager.Users.AnyAsync()) return;

        _logger.LogInformation("Seeding users...");
        var adminUser = new ApplicationUser
        {
            UserName = UserName.Administrator,
            Provider = "Local",
            IsActive = true,
            TenantId = (await _context.Tenants.FirstAsync()).Id,
            DisplayName = UserName.Administrator,
            Email = "admin@example.com",
            EmailConfirmed = true,
            ProfilePictureDataUrl = "https://s.gravatar.com/avatar/78be68221020124c23c665ac54e07074?s=80",
            LanguageCode="en-US",
            TimeZoneId= "Asia/Shanghai",
            TwoFactorEnabled = false
        };

        var demoUser = new ApplicationUser
        {
            UserName = UserName.Demo,
            IsActive = true,
            Provider = "Local",
            TenantId = (await _context.Tenants.FirstAsync()).Id,
            DisplayName = UserName.Demo,
            Email = "demo@example.com",
            EmailConfirmed = true,
            LanguageCode = "de-DE",
            TimeZoneId = "Europe/Berlin",
            ProfilePictureDataUrl = "https://s.gravatar.com/avatar/ea753b0b0f357a41491408307ade445e?s=80"
        };

        var adminResult = await _userManager.CreateAsync(adminUser, UserName.DefaultPassword);
        if (adminResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(adminUser, RoleName.TenantOwner);
            _logger.LogInformation("Admin user created successfully");
        }
        else
        {
            _logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", adminResult.Errors.Select(e => e.Description)));
            throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", adminResult.Errors.Select(e => e.Description))}");
        }

        var demoResult = await _userManager.CreateAsync(demoUser, UserName.DefaultPassword);
        if (demoResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(demoUser, RoleName.EndUser);
            _logger.LogInformation("Demo user created successfully");
        }
        else
        {
            _logger.LogError("Failed to create demo user: {Errors}", string.Join(", ", demoResult.Errors.Select(e => e.Description)));
            throw new InvalidOperationException($"Failed to create demo user: {string.Join(", ", demoResult.Errors.Select(e => e.Description))}");
        }
        
        // Save changes to ensure users are persisted before SeedDataAsync
        await _context.SaveChangesAsync();
        _logger.LogInformation("Users saved to database successfully");
    }

    private async Task SeedDataAsync()
    {
        if (!await _context.PicklistSets.AnyAsync())
        {

            _logger.LogInformation("Seeding key values...");
            var keyValues = new[]
            {
                new PicklistSet
                {
                    Name = Picklist.Status,
                    Value = "initialization",
                    Text = "Initialization",
                    Description = "Status of workflow"
                },
                new PicklistSet
                {
                    Name = Picklist.Status,
                    Value = "processing",
                    Text = "Processing",
                    Description = "Status of workflow"
                },
                new PicklistSet
                {
                    Name = Picklist.Status,
                    Value = "pending",
                    Text = "Pending",
                    Description = "Status of workflow"
                },
                new PicklistSet
                {
                    Name = Picklist.Status,
                    Value = "done",
                    Text = "Done",
                    Description = "Status of workflow"
                },
                new PicklistSet
                {
                    Name = Picklist.Brand,
                    Value = "Apple",
                    Text = "Apple",
                    Description = "Brand of production"
                },
                new PicklistSet
                {
                    Name = Picklist.Brand,
                    Value = "Google",
                    Text = "Google",
                    Description = "Brand of production"
                },
                new PicklistSet
                {
                    Name = Picklist.Brand,
                    Value = "Microsoft",
                    Text = "Microsoft",
                    Description = "Brand of production"
                },
                new PicklistSet
                {
                    Name = Picklist.Unit,
                    Value = "EA",
                    Text = "EA",
                    Description = "Unit of product"
                },
                new PicklistSet
                {
                    Name = Picklist.Unit,
                    Value = "KM",
                    Text = "KM",
                    Description = "Unit of product"
                },
                new PicklistSet
                {
                    Name = Picklist.Unit,
                    Value = "PC",
                    Text = "PC",
                    Description = "Unit of product"
                },
                new PicklistSet
                {
                    Name = Picklist.Unit,
                    Value = "L",
                    Text = "L",
                    Description = "Unit of product"
                }
            };

            await _context.PicklistSets.AddRangeAsync(keyValues);
            await _context.SaveChangesAsync();
        }

        if (!await _context.Products.AnyAsync())
        {

            _logger.LogInformation("Seeding products...");
            var products = new[]
            {
                new Product
                {
                    Brand = "Apple",
                    Name = "IPhone 13 Pro",
                    Description =
                    "Apple iPhone 13 Pro smartphone. Announced Sep 2021. Features 6.1″ display, Apple A15 Bionic chipset, 3095 mAh battery, 1024 GB storage.",
                    Unit = "EA",
                    Price = 999.98m
                },
                new Product
                {
                    Brand = "Sony",
                    Name = "WH-1000XM4",
                    Description = "Sony WH-1000XM4 Wireless Noise-Canceling Over-Ear Headphones. Features industry-leading noise cancellation, up to 30 hours of battery life, touch sensor controls.",
                    Unit = "EA",
                    Price = 349.99m
                },
                new Product
                {
                    Brand = "Nintendo",
                    Name = "Switch OLED Model",
                    Description = "Nintendo Switch OLED Model console. Released October 2021. Features 7″ OLED screen, 64GB internal storage, enhanced audio, dock with wired LAN port.",
                    Unit = "EA",
                    Price = 349.99m
                },
                new Product
                {
                    Brand = "Apple",
                    Name = "MacBook Air M1",
                    Description = "Apple MacBook Air with M1 chip. Features 13.3″ Retina display, Apple M1 chip with 8‑core CPU, 8GB RAM, 256GB SSD storage, up to 18 hours of battery life.",
                    Unit = "EA",
                    Price = 999.99m
                }

            };

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();
        }

        if (!await _context.Agents.AnyAsync())
        {
            _logger.LogInformation("Seeding agents...");
            
            // Get existing users to create agents for
            var adminUser = await _userManager.FindByNameAsync(UserName.Administrator);
            var demoUser = await _userManager.FindByNameAsync(UserName.Demo);
            var tenant = await _context.Tenants.FirstAsync();
            
            var agents = new[]
            {
                new Agent
                {
                    ApplicationUserId = adminUser!.Id,
                    Status = AgentStatus.Available,
                    MaxConcurrentConversations = 10,
                    ActiveConversationCount = 0,
                    Skills = "General Support, Technical Issues, Escalations",
                    Priority = 10,
                    Notes = "Senior agent with full access",
                    TenantId = tenant.Id,
                    LastActiveAt = DateTime.UtcNow
                },
                new Agent
                {
                    ApplicationUserId = demoUser!.Id,
                    Status = AgentStatus.Available,
                    MaxConcurrentConversations = 5,
                    ActiveConversationCount = 0,
                    Skills = "Customer Support, Basic Inquiries",
                    Priority = 5,
                    Notes = "Demo agent for testing",
                    TenantId = tenant.Id,
                    LastActiveAt = DateTime.UtcNow
                }
            };

            await _context.Agents.AddRangeAsync(agents);
            await _context.SaveChangesAsync();
        }
    }
}
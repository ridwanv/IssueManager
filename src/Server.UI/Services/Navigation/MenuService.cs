using CleanArchitecture.Blazor.Application.Common.Constants.Roles;
using CleanArchitecture.Blazor.Server.UI.Models.NavigationMenu;

namespace CleanArchitecture.Blazor.Server.UI.Services.Navigation;

public class MenuService : IMenuService
{
    private readonly List<MenuSectionModel> _features = new()
    {
        new MenuSectionModel
        {
            Title = "Application",
            SectionItems = new List<MenuSectionItemModel>
            {
                new() { Title = "Home", Icon = Icons.Material.Filled.Home, Href = "/" },
                //new()
                //{
                //    Title = "E-Commerce",
                //    Icon = Icons.Material.Filled.ShoppingCart,
                //    PageStatus = PageStatus.Completed,
                //    IsParent = true,
                //    MenuItems = new List<MenuSectionSubItemModel>
                //    {
                //        new()
                //        {
                //            Title = "Products",
                //            Href = "/pages/products",
                //            PageStatus = PageStatus.Completed
                //        },
                //        new()
                //        {
                //            Title = "Documents",
                //            Href = "/pages/documents",
                //            PageStatus = PageStatus.Completed
                //        }
                //    }
                //},
                //new()
                //{
                //    Title = "Analytics",
                //    Roles = new[] { RoleName.Admin, RoleName.Users },
                //    Icon = Icons.Material.Filled.Analytics,
                //    Href = "/analytics",
                //    PageStatus = PageStatus.ComingSoon
                //},
                //new()
                //{
                //    Title = "Banking",
                //    Roles = new[] { RoleName.Admin, RoleName.Users },
                //    Icon = Icons.Material.Filled.Money,
                //    Href = "/banking",
                //    PageStatus = PageStatus.ComingSoon
                //},
                //new()
                //{
                //    Title = "Booking",
                //    Roles = new[] { RoleName.Admin, RoleName.Users },
                //    Icon = Icons.Material.Filled.CalendarToday,
                //    Href = "/booking",
                //    PageStatus = PageStatus.ComingSoon
                //}
            }
        },
        new MenuSectionModel
        {
            Title = "ISSUES",
            SectionItems = new List<MenuSectionItemModel>
            {
                new()
                {
                    Title = "My Issues",
                    Icon = Icons.Material.Filled.AssignmentInd,
                    Href = "/my-issues",
                    PageStatus = PageStatus.Completed
                },
                new()
                {
                    Title = "Issue Management",
                    Icon = Icons.Material.Filled.List,
                    Href = "/issues",
                    PageStatus = PageStatus.Completed
                },
                new()
                {
                    Title = "Issue Analytics",
                    Roles = new[] { RoleName.Admin, RoleName.Users },
                    Icon = Icons.Material.Filled.Analytics,
                    Href = "/issues/analytics",
                    PageStatus = PageStatus.Completed
                }
            }
        },
        new MenuSectionModel
        {
            Title = "CONVERSATIONS",
            SectionItems = new List<MenuSectionItemModel>
            {
                new()
                {
                    Title = "My Conversations",
                    Icon = Icons.Material.Filled.PersonalVideo,
                    Href = "/agent/my-conversations",
                    PageStatus = PageStatus.Completed
                },
                new()
                {
                    Title = "Past Conversations",
                    Icon = Icons.Material.Filled.History,
                    Href = "/agent/past-conversations",
                    PageStatus = PageStatus.Completed
                },
                new()
                {
                    Title = "Pending Conversations",
                    Icon = Icons.Material.Filled.PendingActions,
                    Href = "/agent/pending-conversations",
                    PageStatus = PageStatus.Completed
                },
                new()
                {
                    Title = "Conversation Manager",
                    Icon = Icons.Material.Filled.Dashboard,
                    Href = "/agent-dashboard",
                    PageStatus = PageStatus.Completed
                },
                new()
                {
                    Title = "Conversation Analytics",
                    Roles = new[] { RoleName.Admin, RoleName.Users },
                    Icon = Icons.Material.Filled.Analytics,
                    Href = "/conversations/analytics",
                    PageStatus = PageStatus.Completed
                },
                new()
                {
                    Title = "Contacts",
                    Icon = Icons.Material.Filled.Contacts,
                    Href = "/pages/contacts",
                    PageStatus = PageStatus.Completed
                }
            }
        },
        new MenuSectionModel
        {
            Title = "AGENT",
            SectionItems = new List<MenuSectionItemModel>
            {
                new()
                {
                    Title = "Agent Tools",
                    Icon = Icons.Material.Filled.SmartToy,
                    Href = "/agent/tools",
                    PageStatus = PageStatus.ComingSoon
                }
            }
        },
        new MenuSectionModel
        {
            Title = "MANAGEMENT",
            Roles = new[] { RoleName.Admin },
            SectionItems = new List<MenuSectionItemModel>
            {
                new()
                {
                    IsParent = true,
                    Title = "Authorization",
                    Icon = Icons.Material.Filled.ManageAccounts,
                    MenuItems = new List<MenuSectionSubItemModel>
                    {
                        new()
                        {
                            Title = "Multi-Tenant",
                            Href = "/system/tenants",
                            PageStatus = PageStatus.Completed
                        },
                        new()
                        {
                            Title = "Users",
                            Href = "/identity/users",
                            PageStatus = PageStatus.Completed
                        },
                        new()
                        {
                            Title = "Roles",
                            Href = "/identity/roles",
                            PageStatus = PageStatus.Completed
                        },
                        new()
                        {
                            Title = "Agents",
                            Href = "/pages/agents",
                            PageStatus = PageStatus.Completed
                        },
                        new()
                        {
                            Title = "Profile",
                            Href = "/user/profile",
                            PageStatus = PageStatus.Completed
                        },
                        new()
                        {
                            Title = "Login History",
                            Href = "/pages/identity/loginaudits",
                            PageStatus = PageStatus.Completed
                        },
                    }
                },
                new()
                {
                    IsParent = true,
                    Title = "System",
                    Icon = Icons.Material.Filled.Devices,
                    MenuItems = new List<MenuSectionSubItemModel>
                    {
                        new()
                        {
                            Title = "Picklist",
                            Href = "/system/picklistset",
                            PageStatus = PageStatus.Completed
                        },
                        new()
                        {
                            Title = "Audit Trails",
                            Href = "/system/audittrails",
                            PageStatus = PageStatus.Completed
                        },
                        new()
                        {
                            Title = "Logs",
                            Href = "/system/logs",
                            PageStatus = PageStatus.Completed
                        },
                        new()
                        {
                            Title = "Jobs",
                            Href = "/jobs",
                            PageStatus = PageStatus.Completed,
                            Target = "_blank"
                        }
                    }
                }
            }
        }
    };

    public IEnumerable<MenuSectionModel> Features => _features;
}
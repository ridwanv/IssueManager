# GitHub Copilot Instructions

> **Purpose:** Comprehensive development guidelines ensuring high-quality, secure, and maintainable code following Clean Architecture principles.

---

## 1  Project Overview

| Setting        | Value                                                                                |
| -------------- | ------------------------------------------------------------------------------------ |
| **Project**    | WhatsApp Issue Management System                                                      |
| **Solution**   | `IssueManager.slnx`                                                                  |
| **Runtime**    | .NET 9 · Blazor Server                                                               |
| **UI Library** | MudBlazor (latest stable)                                                            |
| **Patterns**   | Clean Architecture · CQRS · MediatR                                                  |
| **Integration**| WhatsApp Business API · SignalR · AI Solution Matching                              |
| **Tooling**    | AutoMapper · FluentValidation · EF Core · Identity.EntityFrameworkCore · FusionCache |
| **Testing**    | NUnit · bUnit · FluentAssertions · Moq · Testcontainers                             |
| **Quality**    | SonarQube · CodeQL · Dependabot                                                      |

---

## 2  Architecture Layers & Responsibilities

| Layer / Purpose                                 | Physical Path               | Responsibilities                                                                  | Root Namespace                                           |
| ----------------------------------------------- | --------------------------- | --------------------------------------------------------------------------------- | -------------------------------------------------------- |
| **Domain** (Enterprise core)                    | `src/Domain/`               | Entities (Issue, Contact, KnowledgeBase), Enums, Domain Events, Business Rules    | `CleanArchitectureWithBlazorServer.Domain`               |
| **Application** (Use‑cases)                     | `src/Application/`          | CQRS Commands & Queries, Handlers, DTOs, Validators, Mapping Profiles, Interfaces | `CleanArchitectureWithBlazorServer.Application`          |
| **Infrastructure** (Framework & external calls) | `src/Infrastructure/`       | EF Core persistence, Identity, WhatsApp API, Background Jobs, AI Matching, Caching | `CleanArchitectureWithBlazorServer.Infrastructure`       |
| **Bot** (WhatsApp Integration)                  | `src/Bot/`                  | WhatsApp webhook handling, conversation flows, message processing                 | `CleanArchitectureWithBlazorServer.Bot`                  |
| **Migrators** (DB migrations)                   | `src/Migrators/<Provider>/` | FluentMigrator projects for MSSQL, PostgreSQL, SQLite                             | `CleanArchitectureWithBlazorServer.Migrators.<Provider>` |
| **UI** (Blazor Server)                          | `src/Server.UI/`            | `.razor` pages, components, `Program.cs`, DI wiring, Issue Management Dashboard   | `CleanArchitectureWithBlazorServer.Server.UI`            |
| **Tests**                                       | `tests/`                    | Unit / Integration / E2E tests                                                   | Same as tested assembly                                  |

### Layer Dependencies (MUST follow)UI → Application → Domain
Bot → Api → Application → Domain  
Infrastructure → Application → Domain
Tests → Any layer (for testing purposes only)
---

## 3  Naming Conventions & Patterns

| Artifact            | Pattern                                    | Example                                    |
| ------------------- | ------------------------------------------ | ------------------------------------------ |
| **Command**         | `{Verb}{Entity}Command`                   | `CreateIssueCommand`, `UpdateIssueStatusCommand` |
| **Query**           | `Get{Entity}[By{Criteria}]Query`          | `GetIssueByIdQuery`, `GetIssuesWithPaginationQuery` |
| **Handler**         | `{Command/Query}Handler`                  | `CreateIssueCommandHandler`                |
| **DTO**             | `{Entity}Dto`                             | `IssueDto`, `ContactDto`                   |
| **Mapping Profile** | `{Entity}Profile`                         | `IssueProfile`, `KnowledgeBaseProfile`     |
| **Validator**       | `{Command/Query}Validator`                | `CreateIssueCommandValidator`              |
| **Entity**          | `PascalCase` (singular)                   | `Issue`, `Contact`, `KnowledgeBaseArticle` |
| **Service**         | `I{Service}Service` / `{Service}Service`  | `IWhatsAppService` / `WhatsAppService`     |
| **Component**       | `PascalCase.razor`                        | `IssueList.razor`, `IssueDashboard.razor` |
| **Test Class**      | `{ClassUnderTest}Tests`                   | `CreateIssueCommandHandlerTests`          |
| **Bot Handler**     | `{Action}MessageHandler`                  | `ProcessIssueMessageHandler`               |

---

## 4  Code Generation Standards

### 4.1 Mandatory Requirements

1. **Dependency Injection**: Always use constructor injection with `readonly` fields
2. **Async Programming**: Use `async/await` consistently, never block with `.Result` or `.Wait()`
3. **Null Safety**: Use `ArgumentNullException.ThrowIfNull()` and nullable reference types
4. **Validation**: Provide FluentValidation validators for all Commands/Queries
5. **Error Handling**: Use Result pattern or custom exceptions with proper error codes
6. **Logging**: Add structured logging with appropriate log levels
7. **Caching**: Use FusionCache for read-heavy operations
8. **Security**: Implement proper authorization and input validation
9. **Testing**: Write comprehensive unit and integration tests
10. **Documentation**: Add XML documentation for public APIs

### 4.2 Performance Guidelines
// ✅ Good: Async with ConfigureAwait(false) in libraries
public async Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken ct)
{
    var product = await _context.Products
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
        .ConfigureAwait(false);
    return await Result<ProductDto>.SuccessAsync(_mapper.Map<ProductDto>(product));
}

// ✅ Good: Use AsNoTracking for read-only queries
public async Task<List<Product>> GetProductsAsync(CancellationToken ct)
{
    return await _context.Products
        .AsNoTracking()
        .ToListAsync(ct);
}

// ✅ Good: Batch operations instead of loops
public async Task UpdateProductsAsync(List<Product> products, CancellationToken ct)
{
    _context.Products.UpdateRange(products);
    await _context.SaveChangesAsync(ct);
}
### 4.3 Security Best Practices
// ✅ Input Validation
public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[a-zA-Z0-9\s\-_]+$"); // Prevent injection
            
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .LessThan(1000000);
    }
}

// ✅ Authorization at Component level
@attribute [Authorize(Policy = Permissions.Products.Create)]

@code {
    private async Task OnCreateProduct()
    {
        var command = new CreateProductCommand(ProductName, Price, Description);
        var result = await Mediator.Send(command);
        if (result.Succeeded)
        {
            Snackbar.Add("Product created successfully", Severity.Success);
            await LoadProducts();
        }
        else
        {
            Snackbar.Add(result.ErrorMessage, Severity.Error);
        }
    }
}
---

## 5  CQRS Implementation Templates

### 5.1 Command Pattern// Command
public sealed record CreateIssueCommand(
    string ContactId,
    string ApplicationName, 
    string Category,
    string Description,
    string Urgency) : ICacheInvalidatorRequest<Result<int>>;

// Handler
internal sealed class CreateIssueCommandHandler : IRequestHandler<CreateIssueCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateIssueCommandHandler> _logger;
    private readonly IWhatsAppService _whatsAppService;

    public CreateIssueCommandHandler(
        IApplicationDbContext context, 
        IMapper mapper,
        ILogger<CreateIssueCommandHandler> logger,
        IWhatsAppService whatsAppService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _whatsAppService = whatsAppService;
    }

    public async Task<Result<int>> Handle(CreateIssueCommand request, CancellationToken ct)
    {
        try
        {
            var entity = new Issue(
                request.ContactId,
                request.ApplicationName, 
                request.Category,
                request.Description,
                Enum.Parse<IssueUrgency>(request.Urgency));
            
            _context.Issues.Add(entity);
            await _context.SaveChangesAsync(ct);
            
            // Send WhatsApp acknowledgment
            await _whatsAppService.SendMessageAsync(
                request.ContactId,
                $"Issue #{entity.Id} created successfully. We'll keep you updated.",
                ct);
            
            _logger.LogInformation("Issue created successfully with ID: {IssueId}", entity.Id);
            
            return await Result<int>.SuccessAsync(entity.Id, "Issue created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating issue for contact: {ContactId}", request.ContactId);
            return await Result<int>.FailureAsync("Failed to create issue");
        }
    }
}

// Validator
public sealed class CreateIssueCommandValidator : AbstractValidator<CreateIssueCommand>
{
    public CreateIssueCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithMessage("Contact ID is required")
            .Matches(@"^\d{10,15}$").WithMessage("Invalid WhatsApp number format");
            
        RuleFor(x => x.ApplicationName)
            .NotEmpty().WithMessage("Application name is required")
            .MaximumLength(100).WithMessage("Application name must not exceed 100 characters");
            
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Issue category is required")
            .Must(BeValidCategory).WithMessage("Invalid issue category");
            
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Issue description is required")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");
            
        RuleFor(x => x.Urgency)
            .NotEmpty().WithMessage("Urgency level is required")
            .Must(BeValidUrgency).WithMessage("Invalid urgency level");
    }
    
    private bool BeValidCategory(string category)
    {
        var validCategories = new[] { "Login Issue", "Data Error", "Performance", "Feature Request", "Bug Report" };
        return validCategories.Contains(category);
    }
    
    private bool BeValidUrgency(string urgency)
    {
        return Enum.TryParse<IssueUrgency>(urgency, out _);
    }
}
### 5.2 Query Pattern with Caching// Query
public sealed record GetIssueByIdQuery(int Id) : ICacheableRequest<Result<IssueDto>>
{
    public string CacheKey => $"Issue:{Id}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(30);
}

// Handler
internal sealed class GetIssueByIdQueryHandler : IRequestHandler<GetIssueByIdQuery, Result<IssueDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IFusionCache _cache;

    public GetIssueByIdQueryHandler(
        IApplicationDbContext context, 
        IMapper mapper,
        IFusionCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<Result<IssueDto>> Handle(GetIssueByIdQuery request, CancellationToken ct)
    {
        var issue = await _cache.GetOrSetAsync(
            request.CacheKey,
            async _ => await _context.Issues
                .Include(i => i.Contact)
                .Include(i => i.AssignedTo)
                .Include(i => i.Attachments)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == request.Id, ct),
            request.Expiry,
            token: ct);

        if (issue == null)
            return await Result<IssueDto>.FailureAsync("Issue not found");

        var dto = _mapper.Map<IssueDto>(issue);
        return await Result<IssueDto>.SuccessAsync(dto);
    }
}

// WhatsApp-specific Query Example
public sealed record GetIssuesForWhatsAppQuery(string ContactId, int PageSize = 5) 
    : ICacheableRequest<Result<List<IssueDto>>>
{
    public string CacheKey => $"WhatsAppIssues:{ContactId}:{PageSize}";
    public TimeSpan? Expiry => TimeSpan.FromMinutes(5);
}
### 5.3 Update/Command Handler Pattern (DbContextFactory + AutoMapper)// Handler for update commands using IApplicationDbContextFactory and IMapper
internal sealed class UpdateEntityCommandHandler : IRequestHandler<UpdateEntityCommand, Result<int>>
{
    private readonly IApplicationDbContextFactory _dbContextFactory;
    private readonly IMapper _mapper;
    public UpdateEntityCommandHandler(
        IApplicationDbContextFactory dbContextFactory,
        IMapper mapper
    )
    {
        _dbContextFactory = dbContextFactory;
        _mapper = mapper;
    }
    public async Task<Result<int>> Handle(UpdateEntityCommand request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateAsync(cancellationToken);
        var item = await db.Entities.FindAsync(new object[] { request.Id }, cancellationToken);
        if (item == null) return await Result<int>.FailureAsync($"Entity with id: [{request.Id}] not found.");
        _mapper.Map(request, item);
        await db.SaveChangesAsync(cancellationToken);
        return await Result<int>.SuccessAsync(item.Id);
    }
}

// Use this pattern for update commands (e.g., UpdateIssueCommandHandler, UpdateContactCommandHandler)
// - Use IApplicationDbContextFactory for context lifetime management
// - Use IMapper for mapping command to entity
// - Return Result<T> using the async static helpers
// - No direct logger or exception handling unless required by business rules


## 6  WhatsApp Bot Implementation Patterns

### 6.1 Bot Message Handler Template// Bot Controller
[ApiController]
[Route("api/[controller]")]
public class WhatsAppWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WhatsAppWebhookController> _logger;
    private readonly IWhatsAppService _whatsAppService;

    public WhatsAppWebhookController(
        IMediator mediator,
        ILogger<WhatsAppWebhookController> logger,
        IWhatsAppService whatsAppService)
    {
        _mediator = mediator;
        _logger = logger;
        _whatsAppService = whatsAppService;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveMessage([FromBody] WhatsAppWebhookRequest request)
    {
        try
        {
            if (!_whatsAppService.VerifyWebhookSignature(request))
                return Unauthorized();

            var command = new ProcessWhatsAppMessageCommand(
                request.From,
                request.Message.Text,
                request.MessageId,
                request.Timestamp);

            var result = await _mediator.Send(command);
            
            return result.Succeeded ? Ok() : BadRequest(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WhatsApp webhook");
            return StatusCode(500);
        }
    }
}

// Conversation State Management
public class ConversationStateService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _stateExpiry = TimeSpan.FromMinutes(30);

    public async Task<ConversationState> GetStateAsync(string contactId)
    {
        return _cache.Get<ConversationState>($"conv:{contactId}") 
               ?? new ConversationState { ContactId = contactId };
    }

    public async Task SaveStateAsync(ConversationState state)
    {
        _cache.Set($"conv:{state.ContactId}", state, _stateExpiry);
    }
}

// Message Processing Handler
internal sealed class ProcessWhatsAppMessageHandler 
    : IRequestHandler<ProcessWhatsAppMessageCommand, Result<string>>
{
    private readonly IConversationStateService _stateService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IMediator _mediator;

    public async Task<Result<string>> Handle(ProcessWhatsAppMessageCommand request, CancellationToken ct)
    {
        var state = await _stateService.GetStateAsync(request.ContactId);
        
        return state.CurrentStep switch
        {
            ConversationStep.AwaitingIssueDescription => await HandleIssueDescription(request, state, ct),
            ConversationStep.AwaitingCategory => await HandleCategorySelection(request, state, ct),
            ConversationStep.AwaitingUrgency => await HandleUrgencySelection(request, state, ct),
            _ => await HandleInitialMessage(request, state, ct)
        };
    }
}
### 6.2 WhatsApp Service Interfacepublic interface IWhatsAppService
{
    Task<Result<string>> SendMessageAsync(string to, string message, CancellationToken ct = default);
    Task<Result<string>> SendTemplateMessageAsync(string to, string templateName, 
        Dictionary<string, string> parameters, CancellationToken ct = default);
    Task<Result<string>> SendInteractiveMessageAsync(string to, WhatsAppInteractiveMessage message, 
        CancellationToken ct = default);
    bool VerifyWebhookSignature(WhatsAppWebhookRequest request);
    Task<Result<byte[]>> DownloadMediaAsync(string mediaId, CancellationToken ct = default);
}

// Implementation follows Infrastructure layer patterns
public class WhatsAppService : IWhatsAppService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppService> _logger;

    public async Task<Result<string>> SendMessageAsync(string to, string message, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                messaging_product = "whatsapp",
                to = to,
                type = "text",
                text = new { body = message }
            };

            var response = await _httpClient.PostAsJsonAsync("messages", payload, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                _logger.LogInformation("Message sent successfully to {To}", to);
                return await Result<string>.SuccessAsync(content);
            }
            
            return await Result<string>.FailureAsync("Failed to send WhatsApp message");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WhatsApp message to {To}", to);
            return await Result<string>.FailureAsync("WhatsApp service error");
        }
    }
}
---

## 7  Blazor Component Best Practices

### 7.1 Issue Management Component Structure@page "/issues"
@using CleanArchitectureWithBlazorServer.Application.Features.Issues.DTOs
@using CleanArchitectureWithBlazorServer.Application.Features.Issues.Queries.Pagination
@using CleanArchitectureWithBlazorServer.Application.Features.Issues.Commands.UpdateStatus
@using CleanArchitectureWithBlazorServer.Application.Features.Issues.Commands.AssignStaff
@using CleanArchitectureWithBlazorServer.Domain.Enums

@attribute [Authorize(Policy = Permissions.Issues.View)]
@inject IStringLocalizer<Issues> L
@inject IHubContext<NotificationHub> HubContext

<PageTitle>Issue Management Dashboard</PageTitle>

<MudDataGrid ServerData="@(ServerReload)"
             FixedHeader="true"
             FixedFooter="false"
             @bind-RowsPerPage="_defaultPageSize"
             Loading="@_loading"
             MultiSelection="true"
             @bind-SelectedItems="_selectedItems"
             ColumnResizeMode="ResizeMode.Column"
             Hover="true" @ref="_issuesGrid">
    <ToolBarContent>
        <MudStack Row Spacing="0" Class="flex-grow-1" Justify="Justify.SpaceBetween">
            <MudStack Row AlignItems="AlignItems.Start">
                <MudIcon Icon="@Icons.Material.Filled.Support" Size="Size.Large" />
                <MudStack Spacing="0">
                    <MudText Typo="Typo.subtitle2" Class="mb-2">@L["Issue Management"]</MudText>
                    <MudEnumSelect Style="min-width:120px"
                                   TEnum="IssueStatus"
                                   ValueChanged="OnStatusFilterChanged"
                                   Value="_issuesQuery.StatusFilter"
                                   Dense="true"
                                   Label="@L["Status Filter"]">
                    </MudEnumSelect>
                </MudStack>
            </MudStack>
            <MudStack Spacing="0" AlignItems="AlignItems.End">
                <MudToolBar Dense WrapContent="true" Class="py-1 px-0">
                    <MudButton Disabled="@_loading"
                               OnClick="@(() => OnRefresh())"
                               StartIcon="@Icons.Material.Outlined.Refresh">
                        @ConstantString.Refresh
                    </MudButton>
                    @if (_accessRights.Assign)
                    {
                        <MudButton Disabled="@(!(_selectedItems.Count > 0))"
                                   StartIcon="@Icons.Material.Outlined.AssignmentInd"
                                   OnClick="OnBulkAssign">
                            Bulk Assign
                        </MudButton>
                    }
                    <MudMenu TransformOrigin="Origin.BottomRight"
                             AnchorOrigin="Origin.BottomRight"
                             EndIcon="@Icons.Material.Filled.MoreVert"
                             Label="@ConstantString.More">
                        @if (_accessRights.Export)
                        {
                            <MudMenuItem Disabled="@_exporting" OnClick="OnExport">
                                @ConstantString.Export
                            </MudMenuItem>
                        }
                        @if (_accessRights.Analytics)
                        {
                            <MudMenuItem OnClick="OnShowAnalytics">
                                View Analytics
                            </MudMenuItem>
                        }
                    </MudMenu>
                </MudToolBar>
                <MudStack Row Spacing="1">
                    <MudTextField T="string"
                                  ValueChanged="@(s => OnSearch(s))"
                                  Value="@_issuesQuery.Keyword"
                                  Placeholder="Search issues..."
                                  Adornment="Adornment.End"
                                  AdornmentIcon="@Icons.Material.Filled.Search"
                                  IconSize="Size.Small">
                    </MudTextField>
                </MudStack>
            </MudStack>
        </MudStack>
    </ToolBarContent>
    <Columns>
        <SelectColumn ShowInFooter="false"></SelectColumn>
        <TemplateColumn HeaderStyle="width:60px" Title="@ConstantString.Actions" Sortable="false">
            <CellTemplate>
                @if (_accessRights.Edit)
                {
                    <MudMenu Icon="@Icons.Material.Filled.Edit"
                             Variant="Variant.Outlined"
                             Size="Size.Small"
                             Dense="true"
                             IconColor="Color.Info"
                             AnchorOrigin="Origin.CenterLeft">
                        <MudMenuItem OnClick="@(() => OnEditIssue(context.Item))">
                            View Details
                        </MudMenuItem>
                        <MudMenuItem OnClick="@(() => OnUpdateStatus(context.Item))">
                            Update Status
                        </MudMenuItem>
                        <MudMenuItem OnClick="@(() => OnSendWhatsAppMessage(context.Item))">
                            Send Message
                        </MudMenuItem>
                    </MudMenu>
                }
            </CellTemplate>
        </TemplateColumn>
        <PropertyColumn Property="x => x.Id" Title="ID" />
        <TemplateColumn Title="Status">
            <CellTemplate>
                <MudChip Color="@GetStatusColor(context.Item.Status)" 
                         Size="Size.Small" 
                         Variant="Variant.Filled">
                    @context.Item.Status
                </MudChip>
            </CellTemplate>
        </TemplateColumn>
        <PropertyColumn Property="x => x.ApplicationName" Title="Application" />
        <PropertyColumn Property="x => x.Category" Title="Category" />
        <TemplateColumn Title="Description">
            <CellTemplate>
                <MudText Style="max-width: 300px; overflow: hidden; text-overflow: ellipsis;">
                    @context.Item.Description
                </MudText>
            </CellTemplate>
        </TemplateColumn>
        <TemplateColumn Title="Contact">
            <CellTemplate>
                <div class="d-flex flex-column">
                    <MudText Typo="Typo.body2">@context.Item.ContactName</MudText>
                    <MudText Typo="Typo.caption" Class="mud-text-secondary">
                        @context.Item.ContactPhone
                    </MudText>
                </div>
            </CellTemplate>
        </TemplateColumn>
        <PropertyColumn Property="x => x.AssignedToName" Title="Assigned To" />
        <PropertyColumn Property="x => x.Urgency" Title="Urgency">
            <CellTemplate>
                <MudChip Color="@GetUrgencyColor(context.Item.Urgency)" 
                         Size="Size.Small">
                    @context.Item.Urgency
                </MudChip>
            </CellTemplate>
        </PropertyColumn>
        <PropertyColumn Property="x => x.CreatedOn" Title="Created" Format="MM/dd/yyyy HH:mm" />
    </Columns>
    <NoRecordsContent>
        <MudText>No issues found</MudText>
    </NoRecordsContent>
    <LoadingContent>
        <MudText>Loading issues...</MudText>
    </LoadingContent>
    <PagerContent>
        <MudDataGridPager PageSizeOptions="@(new[] { 10, 25, 50, 100 })" />
    </PagerContent>
</MudDataGrid>

@code {
    [CascadingParameter] private Task<AuthenticationState> AuthState { get; set; } = default!;
    [CascadingParameter] private UserProfile? UserProfile { get; set; }
    
    private HashSet<IssueDto> _selectedItems = new();
    private MudDataGrid<IssueDto> _issuesGrid = default!;
    private IssuesAccessRights _accessRights = new();
    private bool _loading;
    private bool _exporting;
    private int _defaultPageSize = 25;

    private GetIssuesWithPaginationQuery _issuesQuery { get; } = new();

    protected override async Task OnInitializedAsync()
    {
        _accessRights = await PermissionService.GetAccessRightsAsync<IssuesAccessRights>();
        
        // Set up SignalR for real-time updates
        await HubConnection.StartAsync();
        HubConnection.On<IssueDto>("IssueUpdated", async (issue) =>
        {
            await InvokeAsync(async () =>
            {
                await _issuesGrid.ReloadServerData();
                StateHasChanged();
            });
        });
    }

    private Color GetStatusColor(IssueStatus status) => status switch
    {
        IssueStatus.New => Color.Info,
        IssueStatus.InProgress => Color.Warning,
        IssueStatus.Resolved => Color.Success,
        IssueStatus.Closed => Color.Default,
        IssueStatus.Cancelled => Color.Error,
        _ => Color.Default
    };

    private Color GetUrgencyColor(IssueUrgency urgency) => urgency switch
    {
        IssueUrgency.Low => Color.Success,
        IssueUrgency.Medium => Color.Warning,
        IssueUrgency.High => Color.Error,
        IssueUrgency.Critical => Color.Error,
        _ => Color.Default
    };

    private async Task OnSendWhatsAppMessage(IssueDto issue)
    {
        var parameters = new DialogParameters<WhatsAppMessageDialog>
        {
            { x => x.Issue, issue }
        };
        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium };
        await DialogService.ShowAsync<WhatsAppMessageDialog>("Send WhatsApp Message", parameters, options);
    }

    private async Task OnUpdateStatus(IssueDto issue)
    {
        var command = new UpdateIssueStatusCommand(issue.Id, IssueStatus.InProgress);
        var result = await Mediator.Send(command);
        
        if (result.Succeeded)
        {
            await _issuesGrid.ReloadServerData();
            Snackbar.Add("Status updated successfully", Severity.Success);
        }
        else
        {
            Snackbar.Add(result.ErrorMessage, Severity.Error);
        }
    }
}
### 7.2 Component Guidelines
- **Size Limit**: Keep components under 300 lines. Split larger components into smaller ones
- **State Management**: Use `@bind` for two-way data binding
- **Error Handling**: Always wrap async operations in try-catch blocks
- **Loading States**: Show loading indicators for async operations
- **Accessibility**: Include proper ARIA labels and keyboard navigation
- **Responsive Design**: Use MudBlazor's grid system for responsive layouts
- **MudBlazor Standards**: Always use standard MudBlazor components without custom CSS styling. Maintain consistency and simplicity by leveraging built-in component properties, themes, and variants instead of adding custom styles

### 7.3 MudBlazor Usage Examples
<!-- ✅ Good: Keep components simple, use MudGlobal for defaults -->
<MudButton StartIcon="Icons.Material.Filled.Add">
    Add Product
</MudButton>

<MudTextField T="string" 
              Label="Product Name" 
              @bind-Value="ProductName"
              For="@(() => ProductName)" />

<MudDataGrid Items="@products" 
             Filterable="true" 
             SortMode="SortMode.Multiple">
    <Columns>
        <PropertyColumn Property="x => x.Name" Title="Name" />
        <PropertyColumn Property="x => x.Price" Title="Price" Format="C" />
    </Columns>
</MudDataGrid>

<!-- ❌ Bad: Don't add custom styles -->
<MudButton style="background-color: red; padding: 20px;">
    Custom Styled Button
</MudButton>

<div class="my-custom-grid-wrapper">
    <MudDataGrid class="custom-grid-style" Items="@products">
        <!-- Custom CSS defeats MudBlazor's theming -->
    </MudDataGrid>
</div>

<!-- ✅ Good: Use MudBlazor spacing classes when needed -->
<MudButton Color="Color.Error" Class="ma-2">
    Delete
</MudButton>

<MudPaper Class="pa-4 ma-2">
    <MudText Typo="Typo.h6">Product Details</MudText>
    <MudDivider Class="my-2" />
    <MudText>Content goes here</MudText>
</MudPaper>
---

## 8  AI Solution Matching & Knowledge Base

### 8.1 Solution Matching Servicepublic interface ISolutionMatchingService
{
    Task<Result<List<KnowledgeBaseArticleDto>>> FindMatchingSolutionsAsync(
        string description, 
        string category, 
        int maxResults = 3,
        CancellationToken ct = default);
    Task<Result<bool>> ValidateSolutionEffectivenessAsync(
        int articleId, 
        bool wasHelpful,
        CancellationToken ct = default);
}

// Implementation with AI pattern matching
public class SolutionMatchingService : ISolutionMatchingService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SolutionMatchingService> _logger;
    private readonly IConfiguration _configuration;

    public async Task<Result<List<KnowledgeBaseArticleDto>>> FindMatchingSolutionsAsync(
        string description, 
        string category, 
        int maxResults = 3,
        CancellationToken ct = default)
    {
        try
        {
            // Extract keywords and phrases
            var keywords = ExtractKeywords(description);
            
            // Query knowledge base with similarity scoring
            var articles = await _context.KnowledgeBaseArticles
                .Where(a => a.Category == category && a.IsActive)
                .Where(a => keywords.Any(k => 
                    a.Title.Contains(k) || 
                    a.Content.Contains(k) || 
                    a.Keywords.Contains(k)))
                .Select(a => new
                {
                    Article = a,
                    Score = CalculateSimilarityScore(a, keywords, description)
                })
                .OrderByDescending(x => x.Score)
                .Take(maxResults)
                .Select(x => x.Article)
                .ToListAsync(ct);

            var dtos = _mapper.Map<List<KnowledgeBaseArticleDto>>(articles);
            return await Result<List<KnowledgeBaseArticleDto>>.SuccessAsync(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding matching solutions for category: {Category}", category);
            return await Result<List<KnowledgeBaseArticleDto>>.FailureAsync("Solution matching failed");
        }
    }

    private List<string> ExtractKeywords(string text)
    {
        // Simple keyword extraction - can be enhanced with NLP libraries
        return text.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 3)
            .Distinct()
            .ToList();
    }

    private double CalculateSimilarityScore(KnowledgeBaseArticle article, List<string> keywords, string description)
    {
        var articleText = $"{article.Title} {article.Content} {article.Keywords}".ToLowerInvariant();
        var matchingKeywords = keywords.Count(k => articleText.Contains(k));
        
        // Simple scoring algorithm - can be enhanced with more sophisticated NLP
        return (double)matchingKeywords / keywords.Count * 100;
    }
}
### 8.2 Automated Solution Commandpublic sealed record ProcessAutomatedSolutionCommand(
    int IssueId,
    string ContactId,
    string IssueDescription,
    string Category) : IRequest<Result<bool>>;

internal sealed class ProcessAutomatedSolutionHandler 
    : IRequestHandler<ProcessAutomatedSolutionCommand, Result<bool>>
{
    private readonly ISolutionMatchingService _solutionService;
    private readonly IWhatsAppService _whatsAppService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<ProcessAutomatedSolutionHandler> _logger;

    public async Task<Result<bool>> Handle(ProcessAutomatedSolutionCommand request, CancellationToken ct)
    {
        try
        {
            // Find matching solutions
            var solutionsResult = await _solutionService.FindMatchingSolutionsAsync(
                request.IssueDescription, 
                request.Category, 
                maxResults: 3, 
                ct);

            if (!solutionsResult.Succeeded || !solutionsResult.Data.Any())
            {
                _logger.LogInformation("No automated solutions found for issue {IssueId}", request.IssueId);
                return await Result<bool>.SuccessAsync(false);
            }

            // Send solutions via WhatsApp
            var message = FormatSolutionsMessage(solutionsResult.Data);
            await _whatsAppService.SendMessageAsync(request.ContactId, message, ct);

            // Update issue with automated solution attempt
            var issue = await _context.Issues.FindAsync(request.IssueId);
            if (issue != null)
            {
                issue.AddAutomatedSolutionAttempt(solutionsResult.Data.Count);
                await _context.SaveChangesAsync(ct);
            }

            return await Result<bool>.SuccessAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing automated solution for issue {IssueId}", request.IssueId);
            return await Result<bool>.FailureAsync("Automated solution processing failed");
        }
    }

    private string FormatSolutionsMessage(List<KnowledgeBaseArticleDto> solutions)
    {
        var message = "💡 I found some solutions that might help:\n\n";
        
        for (int i = 0; i < solutions.Count; i++)
        {
            var solution = solutions[i];
            message += $"*{i + 1}. {solution.Title}*\n";
            message += $"{solution.Summary}\n\n";
            
            if (!string.IsNullOrEmpty(solution.StepByStepInstructions))
            {
                message += "📋 *Steps:*\n";
                message += $"{solution.StepByStepInstructions}\n\n";
            }
        }

        message += "Did any of these solutions help? Reply:\n";
        message += "✅ *YES* - if your issue is resolved\n";
        message += "❌ *NO* - if you need more help\n";
        message += "📞 *SUPPORT* - to speak with a support agent";

        return message;
    }
}
---

## 9  Advanced Caching Strategy

### 9.1 Cache Key Patterns// Entity-based caching for Issue Management
public static class CacheKeys
{
    public static string Issue(int id) => $"issue:{id}";
    public static string IssueList(string filter = "") => $"issues:list:{filter.GetHashCode()}";
    public static string ContactIssues(string contactId) => $"contact:issues:{contactId}";
    public static string KnowledgeBaseArticle(int id) => $"kb:article:{id}";
    public static string SolutionMatches(string category, string description) => 
        $"solutions:{category}:{description.GetHashCode()}";
    public static string UserProfile(Guid userId) => $"user:profile:{userId}";
    public static string UserPermissions(Guid userId) => $"user:permissions:{userId}";
    public static string ConversationState(string contactId) => $"conversation:{contactId}";
}

// Cache invalidation for Issue Management
public class IssueCreatedEventHandler : INotificationHandler<IssueCreatedEvent>
{
    private readonly IFusionCache _cache;

    public IssueCreatedEventHandler(IFusionCache cache)
    {
        _cache = cache;
    }

    public async Task Handle(IssueCreatedEvent notification, CancellationToken ct)
    {
        // Invalidate list caches
        await _cache.RemoveByPrefixAsync("issues:list:", token: ct);
        await _cache.RemoveByPrefixAsync("contact:issues:", token: ct);
        
        // Clear dashboard analytics cache
        await _cache.RemoveByPrefixAsync("dashboard:metrics:", token: ct);
    }
}

public class IssueStatusUpdatedEventHandler : INotificationHandler<IssueStatusUpdatedEvent>
{
    private readonly IFusionCache _cache;
    private readonly IHubContext<NotificationHub> _hubContext;

    public async Task Handle(IssueStatusUpdatedEvent notification, CancellationToken ct)
    {
        // Invalidate specific issue cache
        await _cache.RemoveAsync(CacheKeys.Issue(notification.IssueId), token: ct);
        
        // Send real-time update to connected clients
        await _hubContext.Clients.All.SendAsync("IssueStatusUpdated", notification, ct);
    }
}
### 7.2 Caching Configuration// In Infrastructure/DependencyInjection.cs
services.AddFusionCache()
    .WithDefaultEntryOptions(options =>
    {
        options.Duration = TimeSpan.FromMinutes(5);
        options.JitterMaxDuration = TimeSpan.FromSeconds(30);
        options.FailSafeMaxDuration = TimeSpan.FromHours(1);
        options.FailSafeThrottleDuration = TimeSpan.FromSeconds(10);
    })
    .WithSerializer(new FusionCacheSystemTextJsonSerializer())
    .WithDistributedCache(serviceProvider => 
        serviceProvider.GetRequiredService<IDistributedCache>());
---

## 10  Error Handling & Logging

### 10.1 Result Pattern Implementationpublic class Result<T>
{
    public bool Succeeded { get; init; }
    public T Data { get; init; } = default!;
    public string ErrorMessage { get; init; } = string.Empty;
    public List<string> ErrorMessages { get; init; } = new();
    public int ErrorCode { get; init; }

    public static async Task<Result<T>> SuccessAsync(T data, string message = "")
    {
        return new Result<T>
        {
            Succeeded = true,
            Data = data,
            ErrorMessage = message
        };
    }

    public static async Task<Result<T>> FailureAsync(string errorMessage, int errorCode = 400)
    {
        return new Result<T>
        {
            Succeeded = false,
            ErrorMessage = errorMessage,
            ErrorCode = errorCode
        };
    }
}
### 10.2 Structured Logginginternal sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateProductCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CreateProductCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CreateProductCommandHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<int>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["ProductName"] = request.Name,
            ["UserId"] = _currentUserService.UserId,
            ["OperationId"] = Guid.NewGuid()
        });

        _logger.LogInformation("Creating product {ProductName} for user {UserId}", 
            request.Name, _currentUserService.UserId);

        try
        {
            var entity = new Product(request.Name, request.Price, request.Description);
            
            _context.Products.Add(entity);
            await _context.SaveChangesAsync(ct);
            
            _logger.LogInformation("Product created successfully with ID {ProductId}", entity.Id);
            return await Result<int>.SuccessAsync(entity.Id, "Product created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product {ProductName}", request.Name);
            return await Result<int>.FailureAsync("Failed to create product");
        }
    }
}
---

## 11  Security Implementation

### 11.1 Permission Set Definition// src/Infrastructure/PermissionSet/Issues.cs
public static partial class Permissions
{
    [DisplayName("Issue Management Permissions")]
    [Description("Set permissions for issue management operations")]
    public static class Issues
    {
        [Description("Allows viewing issue details and dashboard")]
        public const string View = "Permissions.Issues.View";

        [Description("Allows creating new issue records (typically for admin/import)")]
        public const string Create = "Permissions.Issues.Create";

        [Description("Allows modifying issue details and status")]
        public const string Edit = "Permissions.Issues.Edit";

        [Description("Allows deleting issue records")]
        public const string Delete = "Permissions.Issues.Delete";

        [Description("Allows searching and filtering issues")]
        public const string Search = "Permissions.Issues.Search";

        [Description("Allows assigning issues to support staff")]
        public const string Assign = "Permissions.Issues.Assign";

        [Description("Allows sending WhatsApp messages to users")]
        public const string SendMessages = "Permissions.Issues.SendMessages";

        [Description("Allows exporting issue data and reports")]
        public const string Export = "Permissions.Issues.Export";

        [Description("Allows viewing analytics and metrics")]
        public const string Analytics = "Permissions.Issues.Analytics";

        [Description("Allows managing knowledge base articles")]
        public const string ManageKnowledgeBase = "Permissions.Issues.ManageKnowledgeBase";
    }

    [DisplayName("WhatsApp Bot Permissions")]
    [Description("Set permissions for WhatsApp bot operations")]
    public static class WhatsAppBot
    {
        [Description("Allows accessing WhatsApp webhook endpoints")]
        public const string ReceiveMessages = "Permissions.WhatsAppBot.ReceiveMessages";

        [Description("Allows sending automated responses")]
        public const string SendAutomatedResponses = "Permissions.WhatsAppBot.SendAutomatedResponses";

        [Description("Allows processing conversation flows")]
        public const string ProcessConversations = "Permissions.WhatsAppBot.ProcessConversations";

        [Description("Allows managing bot configuration")]
        public const string ManageConfiguration = "Permissions.WhatsAppBot.ManageConfiguration";
    }
}

// Access Rights Models
public class IssuesAccessRights
{
    public bool View { get; set; }
    public bool Create { get; set; }
    public bool Edit { get; set; }
    public bool Delete { get; set; }
    public bool Search { get; set; }
    public bool Assign { get; set; }
    public bool SendMessages { get; set; }
    public bool Export { get; set; }
    public bool Analytics { get; set; }
    public bool ManageKnowledgeBase { get; set; }
}

public class WhatsAppBotAccessRights
{
    public bool ReceiveMessages { get; set; }
    public bool SendAutomatedResponses { get; set; }
    public bool ProcessConversations { get; set; }
    public bool ManageConfiguration { get; set; }
}
### 11.2 WhatsApp Security Implementation// WhatsApp Webhook Security
public class WhatsAppWebhookAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppWebhookAuthenticationMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/whatsapp/webhook"))
        {
            if (!await ValidateWhatsAppSignature(context.Request))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }
        }

        await _next(context);
    }

    private async Task<bool> ValidateWhatsAppSignature(HttpRequest request)
    {
        try
        {
            var signature = request.Headers["X-Hub-Signature-256"].FirstOrDefault();
            if (string.IsNullOrEmpty(signature))
                return false;

            var secret = _configuration["WhatsApp:WebhookSecret"];
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            
            // Reset stream position for subsequent middleware
            request.Body.Position = 0;

            var computedSignature = ComputeHmacSha256(body, secret);
            return $"sha256={computedSignature}" == signature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating WhatsApp webhook signature");
            return false;
        }
    }

    private string ComputeHmacSha256(string data, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

// Rate Limiting for WhatsApp
public class WhatsAppRateLimitingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<WhatsAppRateLimitingService> _logger;

    public async Task<bool> IsRateLimitExceededAsync(string contactId, int maxMessages = 10, TimeSpan? window = null)
    {
        window ??= TimeSpan.FromMinutes(1);
        var key = $"ratelimit:whatsapp:{contactId}";
        
        var currentCount = _cache.Get<int>(key);
        if (currentCount >= maxMessages)
        {
            _logger.LogWarning("Rate limit exceeded for contact {ContactId}", contactId);
            return true;
        }

        _cache.Set(key, currentCount + 1, window.Value);
        return false;
    }
}
### 11.3 Input Sanitization for WhatsApp Messages// WhatsApp Message Validation
public sealed class ProcessWhatsAppMessageCommandValidator : AbstractValidator<ProcessWhatsAppMessageCommand>
{
    public ProcessWhatsAppMessageCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty()
            .Matches(@"^\d{10,15}$")
            .WithMessage("Invalid WhatsApp contact ID format");
            
        RuleFor(x => x.MessageText)
            .NotEmpty()
            .MaximumLength(4096) // WhatsApp message limit
            .Must(BeValidMessageContent)
            .WithMessage("Message contains potentially harmful content");
            
        RuleFor(x => x.MessageId)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9_-]+$")
            .WithMessage("Invalid message ID format");
    }

    private bool BeValidMessageContent(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        // Check for suspicious patterns
        var suspiciousPatterns = new[]
        {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", // Script tags
            @"javascript:", // JavaScript protocol
            @"data:text\/html", // HTML data URLs
            @"on\w+\s*=", // Event handlers
        };

        return !suspiciousPatterns.Any(pattern => 
            Regex.IsMatch(message, pattern, RegexOptions.IgnoreCase));
    }
}

// WhatsApp Message Sanitization
public static class WhatsAppSecurityHelper
{
    public static string SanitizeMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return string.Empty;

        // Remove HTML tags and decode entities
        message = Regex.Replace(message, @"<[^>]*>", string.Empty);
        message = System.Net.WebUtility.HtmlDecode(message);
        
        // Remove control characters except common whitespace
        message = Regex.Replace(message, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", string.Empty);
        
        // Limit length and normalize
        return message.Trim().Substring(0, Math.Min(message.Length, 4096));
    }

    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        return Regex.IsMatch(phoneNumber, @"^\d{10,15}$");
    }

    public static string FormatWhatsAppNumber(string phoneNumber)
    {
        // Remove any non-digit characters
        var digits = Regex.Replace(phoneNumber, @"\D", string.Empty);
        
        // Ensure it starts with country code (assume +1 for US if not provided)
        if (digits.Length == 10)
            digits = "1" + digits;
            
        return digits;
    }
}
---

## 10  Testing Standards

### 10.1 Unit Test Template[TestFixture]
public class CreateProductCommandHandlerTests
{
    private Mock<IApplicationDbContext> _mockContext;
    private Mock<IMapper> _mockMapper;
    private Mock<ILogger<CreateProductCommandHandler>> _mockLogger;
    private CreateProductCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CreateProductCommandHandler>>();
        _handler = new CreateProductCommandHandler(_mockContext.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", 10.99m, "Description");
        var cancellationToken = CancellationToken.None;

        _mockContext.Setup(x => x.SaveChangesAsync(cancellationToken))
                   .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Handle_DatabaseException_ReturnsFailureResult()
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", 10.99m, "Description");
        var cancellationToken = CancellationToken.None;

        _mockContext.Setup(x => x.SaveChangesAsync(cancellationToken))
                   .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to create product");
    }
}
### 10.2 Integration Test Basepublic abstract class IntegrationTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly ApplicationDbContext Context;

    protected IntegrationTestBase()
    {
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace with test database
                    services.RemoveDbContext<ApplicationDbContext>();
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));
                });
            });

        Client = Factory.CreateClient();
        Context = Factory.Services.GetRequiredService<ApplicationDbContext>();
    }

    public void Dispose()
    {
        Context?.Dispose();
        Client?.Dispose();
        Factory?.Dispose();
    }
}
---

## 11  Performance Optimization

### 11.1 Database Optimization// ✅ Use projection for read-only scenarios
public async Task<List<ProductSummaryDto>> GetProductSummariesAsync(CancellationToken ct)
{
    return await _context.Products
        .AsNoTracking()
        .Select(p => new ProductSummaryDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price
        })
        .ToListAsync(ct);
}

// ✅ Use Include for related data
public async Task<Product?> GetProductWithCategoryAsync(int id, CancellationToken ct)
{
    return await _context.Products
        .Include(p => p.Category)
        .FirstOrDefaultAsync(p => p.Id == id, ct);
}

// ✅ Use Split queries for multiple collections
public async Task<Product?> GetProductWithDetailsAsync(int id, CancellationToken ct)
{
    return await _context.Products
        .AsSplitQuery()
        .Include(p => p.Category)
        .Include(p => p.Reviews)
        .Include(p => p.Images)
        .FirstOrDefaultAsync(p => p.Id == id, ct);
}
### 11.2 Memory Management// ✅ Use IAsyncEnumerable for large datasets
public async IAsyncEnumerable<ProductDto> GetProductsStreamAsync([EnumeratorCancellation] CancellationToken ct = default)
{
    await foreach (var product in _context.Products.AsAsyncEnumerable().WithCancellation(ct))
    {
        yield return _mapper.Map<ProductDto>(product);
    }
}

// ✅ Dispose resources properly
public async Task ProcessLargeFileAsync(Stream fileStream, CancellationToken ct)
{
    using var reader = new StreamReader(fileStream);
    await using var writer = new StreamWriter("output.txt");
    
    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
        ct.ThrowIfCancellationRequested();
        await writer.WriteLineAsync(line);
    }
}
---

## 12  Deployment & Configuration

### 12.1 Environment Configuration// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "#{ConnectionString}#"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "FusionCache": {
    "DefaultEntryOptions": {
      "Duration": "00:05:00",
      "JitterMaxDuration": "00:00:30"
    }
  }
}
---

## 13  Code Quality Rules

### 13.1 SonarQube Rules
- **Cognitive Complexity**: Max 15 per method
- **Cyclomatic Complexity**: Max 10 per method
- **Method Length**: Max 50 lines
- **Class Length**: Max 500 lines
- **Parameter Count**: Max 7 parameters

### 13.2 Code Analysis<!-- In Directory.Build.props -->
<PropertyGroup>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <WarningsAsErrors />
  <WarningsNotAsErrors>CS1591</WarningsNotAsErrors>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisMode>All</AnalysisMode>
</PropertyGroup>
---

## 16  Forbidden Practices

### ❌ Never Do This:// ❌ Don't use .Result or .Wait()
var result = SomeAsyncMethod().Result;

// ❌ Don't mix layers
@code {
    public async Task LoadIssue(int id)
    {
        // ❌ Direct DbContext access in UI layer
        var issue = await _context.Issues.FindAsync(id);
        // UI logic mixed with data access
    }
}

// ❌ Don't use magic strings
await _cache.GetAsync("issue-list-all");

// ❌ Don't ignore exceptions
try 
{
    await SomeOperation();
} 
catch { } // ❌ Empty catch

// ❌ Don't use blocking operations in async methods
public async Task<string> ReadFileAsync()
{
    return File.ReadAllText("file.txt"); // ❌ Blocking I/O
}

// ❌ WhatsApp-specific: Don't store sensitive data in conversation state
public class ConversationState
{
    public string ContactId { get; set; }
    public string Password { get; set; } // ❌ Never store passwords in memory cache
    public string CreditCardNumber { get; set; } // ❌ Never store PII in conversation state
}

// ❌ WhatsApp-specific: Don't send unvalidated user input
public async Task SendWhatsAppMessage(string phoneNumber, string userInput)
{
    // ❌ Direct user input without validation/sanitization
    await _whatsAppService.SendMessageAsync(phoneNumber, userInput);
}

// ❌ WhatsApp-specific: Don't process webhooks without signature verification
[HttpPost("webhook")]
public async Task<IActionResult> ReceiveMessage([FromBody] WhatsAppWebhookRequest request)
{
    // ❌ Processing webhook without signature verification
    await ProcessMessage(request);
    return Ok();
}
### ✅ Do This Instead:// ✅ Use async/await properly
var result = await SomeAsyncMethod();

// ✅ Follow layer separation
@code {
    public async Task LoadIssue(int id)
    {
        var result = await Mediator.Send(new GetIssueByIdQuery(id));
        if (result.Succeeded)
        {
            Issue = result.Data;
        }
        else
        {
            Snackbar.Add(result.ErrorMessage, Severity.Error);
        }
    }
}

// ✅ Use constants for cache keys
await _cache.GetAsync(CacheKeys.IssueList());

// ✅ Handle exceptions properly
try 
{
    await SomeOperation();
} 
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
    throw;
}

// ✅ Use async I/O operations
public async Task<string> ReadFileAsync()
{
    return await File.ReadAllTextAsync("file.txt");
}

// ✅ WhatsApp-specific: Store only non-sensitive conversation data
public class ConversationState
{
    public string ContactId { get; set; }
    public ConversationStep CurrentStep { get; set; }
    public Dictionary<string, string> CollectedData { get; set; } = new();
    public DateTime LastActivity { get; set; }
    
    // Use secure, temporary storage for sensitive data collection if needed
}

// ✅ WhatsApp-specific: Always validate and sanitize input
public async Task SendWhatsAppMessage(string phoneNumber, string userInput)
{
    if (!WhatsAppSecurityHelper.IsValidPhoneNumber(phoneNumber))
        throw new ArgumentException("Invalid phone number");
        
    var sanitizedMessage = WhatsAppSecurityHelper.SanitizeMessage(userInput);
    await _whatsAppService.SendMessageAsync(phoneNumber, sanitizedMessage);
}

// ✅ WhatsApp-specific: Always verify webhook signatures
[HttpPost("webhook")]
public async Task<IActionResult> ReceiveMessage([FromBody] WhatsAppWebhookRequest request)
{
    if (!_whatsAppService.VerifyWebhookSignature(request))
        return Unauthorized();
        
    await ProcessMessage(request);
    return Ok();
}
---

## 17  WhatsApp Issue Management Specific Guidelines

### 17.1 Issue Lifecycle Management
- **Status Transitions**: Follow defined workflow (New → Assigned → In Progress → Resolved → Closed)
- **Automated Responses**: Use template messages for consistent communication
- **Escalation Rules**: Implement time-based escalation for critical issues
- **Solution Matching**: Always attempt automated solution matching before human assignment

### 17.2 WhatsApp Integration Best Practices
- **Rate Limiting**: Implement per-contact rate limiting to prevent spam
- **Conversation State**: Use memory cache with appropriate timeouts (30 minutes max)
- **Message Templates**: Use WhatsApp approved message templates for notifications
- **Media Handling**: Validate file types and sizes for attachments
- **Error Recovery**: Graceful handling of WhatsApp API failures with retry logic

### 17.3 Performance Requirements
- **Response Time**: WhatsApp bot responses under 5 seconds
- **Dashboard Load**: Issue list loads under 2 seconds
- **Concurrent Users**: Support 50+ staff members simultaneously
- **Message Throughput**: Handle 1000+ messages per hour during peak times

### 17.4 Data Management
- **Issue Retention**: Configurable retention policies for resolved issues
- **Contact Privacy**: Hash or encrypt phone numbers for storage
- **Audit Trail**: Complete conversation and action history
- **Analytics**: Track resolution times, solution effectiveness, and user satisfaction

---

## 18  Development Workflow

### 18.1 Before Committing
1. **Run Tests**: `dotnet test`
2. **Check Coverage**: Ensure >80% code coverage
3. **Run Analysis**: `dotnet build --verbosity normal`
4. **Format Code**: `dotnet format`
5. **Update Documentation**: XML docs for public APIs
6. **Test WhatsApp Integration**: Verify webhook endpoints and message flows

### 18.2 Code Review Checklist
- [ ] Follows Clean Architecture principles
- [ ] Proper error handling and logging
- [ ] Input validation and security checks
- [ ] WhatsApp security best practices implemented
- [ ] Performance considerations addressed
- [ ] Tests cover new functionality
- [ ] Documentation updated
- [ ] No breaking changes without versioning
- [ ] Issue management workflow compliance

---

When uncertain about implementation details, Copilot should:
// TODO: Clarify business rules with domain experts
// TODO: Verify WhatsApp API rate limits and compliance requirements
// TODO: Confirm issue escalation workflow with support team
// TODO: Validate solution matching accuracy requirements
This ensures human review for critical decisions in the Issue Management System.

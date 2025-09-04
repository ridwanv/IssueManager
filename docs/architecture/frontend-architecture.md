# Frontend Architecture

## Component Organization

```
Server.UI/
├── Pages/                          # Razor pages with routing
│   ├── Issues/
│   │   ├── Index.razor            # Issue list with real-time updates
│   │   ├── Details.razor          # Issue details with attachments
│   │   ├── Create.razor           # Issue creation form
│   │   └── Edit.razor             # Issue editing interface
│   ├── Contacts/
│   ├── Dashboard/
│   └── Admin/
├── Components/                     # Reusable UI components
│   ├── Common/
│   │   ├── LoadingSpinner.razor
│   │   ├── ConfirmationDialog.razor
│   │   └── ErrorBoundary.razor
│   ├── Issues/
│   │   ├── IssueCard.razor
│   │   ├── IssueStatusBadge.razor
│   │   ├── PrioritySelector.razor
│   │   └── AttachmentUpload.razor
│   └── Layout/
├── Services/                       # Frontend service layer
│   ├── INotificationService.cs
│   ├── IFileService.cs
│   └── IStateService.cs
├── Hubs/                          # SignalR hubs
│   └── NotificationHub.cs
└── wwwroot/                       # Static assets
    ├── css/
    ├── js/
    └── lib/
```

## Component Template

```csharp
@page "/issues"
@attribute [Authorize(Policy = Permissions.Issues.View)]
@inject IMediator Mediator
@inject ISnackbar Snackbar
@inject IDialogService DialogService
@inject IPermissionService PermissionService
@inject IStringLocalizer<Issues> L
@inject NavigationManager Navigation
@implements IAsyncDisposable

<PageTitle>@L["Issues"]</PageTitle>

<MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="mt-4">
    <MudText Typo="Typo.h4" Class="mb-4">@L["Issue Management"]</MudText>
    
    @if (PermissionService.HasPermission(Permissions.Issues.Create))
    {
        <MudButton Variant="Variant.Filled" 
                   Color="Color.Primary" 
                   StartIcon="Icons.Material.Filled.Add"
                   OnClick="CreateIssue">
            @L["Create Issue"]
        </MudButton>
    }
    
    <IssueDataTable @ref="dataTable" 
                    Issues="issues" 
                    Loading="loading"
                    OnRowClick="ViewIssueDetails"
                    OnStatusChange="UpdateIssueStatus" />
</MudContainer>

@code {
    private IssueDataTable dataTable = default!;
    private List<IssueDto> issues = new();
    private bool loading = true;
    private HubConnection? hubConnection;

    protected override async Task OnInitializedAsync()
    {
        await LoadIssues();
        await InitializeSignalR();
    }

    private async Task LoadIssues()
    {
        loading = true;
        var result = await Mediator.Send(new GetIssuesQuery());
        
        if (result.Succeeded)
        {
            issues = result.Data;
        }
        else
        {
            Snackbar.Add(result.Messages.FirstOrDefault(), Severity.Error);
        }
        
        loading = false;
    }

    private async Task InitializeSignalR()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/notificationHub"))
            .Build();

        hubConnection.On<IssueDto>("IssueCreated", OnIssueCreated);
        hubConnection.On<IssueDto>("IssueUpdated", OnIssueUpdated);

        await hubConnection.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }
}
```

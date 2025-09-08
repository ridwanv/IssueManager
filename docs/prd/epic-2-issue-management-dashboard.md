# Epic 2: Issue Management Dashboard

**Epic Goal:** Provide support staff with a comprehensive web-based dashboard that replaces Excel-based issue tracking with professional tools for viewing, managing, prioritizing, and resolving issues. This epic transforms how support teams work by centralizing all issue information and enabling efficient workflow management.

## Story 2.1: Issue List Dashboard

As a **support staff member**,
I want **to view all issues in a centralized dashboard with filtering and sorting capabilities**,
so that **I can efficiently manage my workload and prioritize issues**.

### Acceptance Criteria
1. Main dashboard displays all issues in paginated table with key information (ID, title, status, urgency, submitter, date)
2. Filtering options by status, urgency, application, assigned staff, and date ranges
3. Sorting functionality on all major columns with persistent user preferences
4. Search capability across issue descriptions, IDs, and user information
5. Real-time updates using SignalR when new issues arrive or status changes
6. Responsive design that works effectively on desktop and tablet devices

## Story 2.2: Issue Detail View & Management

As a **support staff member**,
I want **to view complete issue details and update status, priority, and assignments**,
so that **I can manage issues effectively and track resolution progress**.

### Acceptance Criteria
1. Detailed issue view showing all captured information, conversation history, and timeline
2. Status update dropdown with predefined workflow states (New, In Progress, Waiting, Resolved, Closed)
3. Priority assignment with visual indicators and automatic sorting by priority level
4. Staff assignment functionality with notification to assigned team members
5. Internal notes section for support team communication and resolution tracking
6. Audit trail showing all changes with timestamps and responsible staff members

## Story 2.3: Issue Contact Management & Linking

As a **support staff member**,
I want **to create new contacts or link existing contacts to issues during issue creation and management**,
so that **I can maintain complete contact information and establish proper relationships between contacts and their reported issues**.

### Acceptance Criteria
1. Contact selection/creation during issue creation process with dropdown showing existing contacts and "Create New Contact" option
2. Search existing contacts by name, email, or phone number with real-time filtering in contact selection interface  
3. Create new contact inline during issue creation without leaving the issue creation workflow
4. Edit contact information directly from issue detail view with navigation to contact management pages
5. Display complete contact information (name, email, phone, preferred language) in issue detail view
6. Link multiple issues to the same contact with proper relationship management and contact history
7. Contact validation ensuring required fields and proper data format (email validation, phone number formatting)
8. Update issue's contact assignment with audit trail logging for contact changes

## Story 2.4: Basic Analytics & Reporting

As a **support manager**,
I want **to view basic metrics about issue volume, resolution times, and team performance**,
so that **I can monitor service quality and identify improvement opportunities**.

### Acceptance Criteria
1. Dashboard widget showing daily/weekly issue volume trends with visual graphs
2. Average resolution time metrics by category, urgency, and assigned staff
3. Issue status distribution (open vs. resolved) with percentage breakdowns
4. Staff performance metrics showing issue handling capacity and resolution rates
5. Export functionality for basic reports in CSV format for external analysis
6. Date range filtering for all metrics with common presets (last 7 days, last month, etc.)

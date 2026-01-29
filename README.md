# LuminaryLife Org Chart

A ChartHop-style interactive org chart for LuminaryLife, replacing the current Google Sheets workflow. Built with C#/.NET 8 backend and Vue.js 3 frontend.

## Features

- **Hierarchical Tree View**: Sites > Teams > Managers > Agents
- **Interactive Filtering**: Filter by site, team, manager, tags, and search
- **Automated Tagging**: Tags auto-computed based on agent properties (Training/Performance, Commission Tier, Tenure, etc.)
- **Detail Panel**: Click any node to see full details
- **Responsive Design**: Works on desktop and mobile

## Project Structure

```
lisaorgchart/
├── LuminaryLife.Api/           # ASP.NET Core Web API
│   ├── HTTP/Controllers/       # API controllers
│   └── Program.cs              # Application entry point
├── LuminaryLife.Common/        # Shared library
│   ├── Data/                   # EF Core DbContext
│   ├── Entities/               # Entity models
│   ├── Enums/                  # Enum definitions
│   ├── Services/               # Cache service
│   └── Systems/                # Feature modules
│       ├── OrgChart/           # Org chart service & models
│       └── Tags/               # Tag service & repository
├── Seeds/                      # SQL seed scripts
├── frontend/                   # Vue.js frontend
│   └── src/
│       ├── api/                # API client
│       ├── components/         # Vue components
│       ├── composables/        # Vue composables
│       ├── router/             # Vue Router config
│       ├── types/              # TypeScript types
│       └── views/              # Page views
└── LuminaryLife.sln            # Visual Studio solution
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- npm or yarn

### Backend Setup

```bash
# Navigate to API project
cd LuminaryLife.Api

# Restore packages
dotnet restore

# Run the API (creates SQLite DB and seeds data automatically)
dotnet run
```

The API will start at `http://localhost:5000` with Swagger UI at `http://localhost:5000/swagger`.

### Frontend Setup

```bash
# Navigate to frontend
cd frontend

# Install dependencies
npm install

# Start development server
npm run dev
```

The frontend will start at `http://localhost:5173`.

## API Endpoints

| Method | Endpoint                     | Description                       |
| ------ | ---------------------------- | --------------------------------- |
| GET    | `/org-chart`                 | Get full org chart tree           |
| GET    | `/org-chart/flat`            | Get flat list of nodes            |
| GET    | `/org-chart/filter-options`  | Get available filter options      |
| GET    | `/org-chart/subtree`         | Get subtree for site/team/manager |
| GET    | `/org-chart/person/{userId}` | Get single person details         |
| POST   | `/org-chart/sync-tags`       | Trigger full tag sync             |

### Query Parameters

- `siteId` (int): Filter by site
- `teamId` (int): Filter by team
- `managerId` (string): Filter by manager
- `tagIds` (string): Comma-separated tag IDs
- `includeInactive` (bool): Include inactive agents
- `search` (string): Search by name

## Seeded Data

### Austin Site (45 Agents)

Site Manager: **Steve Kelley**

| Team Manager      | Direct Reports |
| ----------------- | -------------- |
| David Druxman     | 6 agents       |
| Frederick Holguin | 6 agents       |
| Jay Anderson      | 9 agents       |
| Jonathan Mejia    | 9 agents       |
| Mario Herrera     | 6 agents       |
| Roza Veravillalba | 6 agents       |
| Jovon Holts       | 3 agents       |

### Charlotte Site (59 Agents)

Site Manager: **Trent Terrell**

| Team Manager      | Direct Reports |
| ----------------- | -------------- |
| Vincent Blanchett | 7 agents       |
| Nisrin Hajmahmoud | 9 agents       |
| Jovan Espinoza    | 8 agents       |
| Katelyn Helms     | 7 agents       |
| Jacob Fuller      | 8 agents       |
| Miguel Roman      | 7 agents       |
| Montrell Morgan   | 7 agents       |
| Brent Lahti       | 6 agents       |

## Tag Definitions

| Tag         | Color      | Auto-Compute Rule        |
| ----------- | ---------- | ------------------------ |
| Training    | Orange     | AgentType == Training    |
| Performance | Green      | AgentType == Performance |
| Tier 1      | Blue       | CommissionTier == Tier1  |
| Tier 2      | Purple     | CommissionTier == Tier2  |
| Tier 3      | Brown      | CommissionTier == Tier3  |
| Manager     | Dark Gray  | Has team assigned        |
| New Hire    | Gold       | Start date <= 90 days    |
| Tenured     | Dark Green | Start date > 90 days     |
| Active      | Lime       | AgentStatus == Active    |
| Inactive    | Gray       | AgentStatus == Inactive  |

## Architecture Notes

### Backend

- **Entity Framework Core** with SQLite for demo (easily swappable to PostgreSQL/SQL Server)
- **Caching**: 5-minute cache on tree responses
- **Tag Engine**: Automated tag computation on entity changes
- **RESTful API** with proper filtering and pagination

### Frontend

- **Vue 3** with Composition API
- **Vue Router** for navigation
- **TypeScript** for type safety
- **Recursive components** for tree rendering
- **CSS-based** styling (no external UI framework dependency)

## For Production

1. Change SQLite to PostgreSQL/SQL Server
2. Enable authentication (`[Authorize]` instead of `[AllowAnonymous]`)
3. Configure proper CORS origins
4. Add role-based filtering (SiteManager can only see their site)
5. Set up Hangfire for daily tag sync job
6. Add event handlers for real-time tag updates

## Reference Documents

- [orgchartconcept.md](orgchartconcept.md) - Full concept documentation with C# and Vue.js code samples

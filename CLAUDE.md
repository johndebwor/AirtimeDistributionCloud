# CLAUDE.md

## Project Overview
RasidBase - Smart Airtime Distribution. ASP.NET Core 10 Blazor Server application with MudBlazor UI framework.

## Tech Stack
- **Framework**: ASP.NET Core 10, Blazor Server (`@rendermode InteractiveServer`)
- **UI**: MudBlazor component library
- **Charts**: Chart.js (via JS interop) — do NOT use MudBlazor's built-in `MudChart`
- **Reports**: QuestPDF (PDF), ClosedXML (Excel)
- **Database**: SQL Server (EF Core), `IUnitOfWork` pattern
- **Architecture**: Clean Architecture (Core / Application / Infrastructure / Web)

## Conventions
- Use Chart.js for all charts and data visualizations in the application
- Chart.js is loaded via CDN in `App.razor` and interop is in `wwwroot/js/charts.js`
- Create/update charts using `IJSRuntime` to call the interop functions

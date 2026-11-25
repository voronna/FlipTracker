# FlipTracker

A web application that helps hobbyists decide whether a secondhand Warhammer listing is worth buying for resale, and tracks each deal from "found" to "sold".

Coursework for module **UFCE4W-30-2 Web Development and User Experience** (TSI).

## Features

- **User accounts and sessions** (ASP.NET Core Identity, cookie authentication, account confirmation).
- **Listings with a Deal Score.** Each listing gets a Buy / Think / Skip verdict and a plain-language reason, based on price, shipping, material, completeness, paint state and age.
- **Personal rules.** Each user sets thresholds (for example minimum discount, whether metal models are allowed); listings are re-scored when rules change.
- **Deals funnel.** Found, Bought, In progress, Sold, with profit and ROI.
- **Public model catalog** with clean URLs (`/models/{slug}`), sitemap, robots.txt, canonical and Open Graph tags, JSON-LD breadcrumbs.
- **eBay market prices.** The backend queries the eBay Browse API for used listings, stores the median price (cached for 24 hours) and the ten cheapest offers; an offer can be turned into a pre-filled listing with one click.
- **Security measures:** `[Authorize]`, per-user data filtering, `[Bind]` whitelists, anti-forgery tokens, server and client validation, security headers, rate limiting on login pages, secrets kept out of source control.

## Tech stack

- ASP.NET Core MVC on .NET 8 (Razor views, server-side rendering)
- Entity Framework Core with SQL Server LocalDB
- ASP.NET Core Identity
- Bootstrap 5, jQuery validation
- xUnit (unit tests)

## Requirements

- Visual Studio 2022 with the ASP.NET workload, or the .NET 8 SDK
- SQL Server LocalDB (installed with Visual Studio)
- Optional: eBay developer keys (without them the app uses the recommended retail price instead of the market median)

## Getting started

1. Clone the repository and open `Web Development Coursework solution.sln`.
2. Create the database. The default connection string in `appsettings.json` uses LocalDB. Apply the migrations in one of two ways:
   - Visual Studio: Tools, NuGet Package Manager, Package Manager Console, then run `Update-Database`.
   - Command line (needs `dotnet tool install --global dotnet-ef` once):
     ```
     dotnet ef database update --project "Web Development Coursework"
     ```
3. (Optional) Add your eBay keys with user-secrets. They are stored outside the project folder and never committed:
   ```
   cd "Web Development Coursework"
   dotnet user-secrets set "Ebay:ClientId" "<your client id>"
   dotnet user-secrets set "Ebay:ClientSecret" "<your client secret>"
   ```
4. Run the project (F5 in Visual Studio, or `dotnet run --project "Web Development Coursework"`) and open the HTTPS address shown in the console.
5. Register an account. No e-mail provider is configured, so in development the registration page shows a link to confirm the account.

## Running the tests

```
dotnet test
```

The test project covers the Deal Score calculator (filters, penalties and thresholds).

## Project structure

| Folder | Content |
| --- | --- |
| `Web Development Coursework/Controllers` | MVC controllers (listings, rules, deals, catalog, market, SEO) |
| `Web Development Coursework/Models` | Entities and enums |
| `Web Development Coursework/Services` | Deal Score calculator, scoring service, eBay client, market price service |
| `Web Development Coursework/Data` | DbContext and EF Core migrations |
| `Web Development Coursework/Views` | Razor views |
| `Web_Development_Coursework.Tests` | xUnit tests |

## Known limitations

- Vinted is not integrated because it has no public API; listings from it are entered manually.
- eBay data are asking prices of active listings, not sold prices.
- The shared model catalog can be edited by any signed-in user; a production version would need an Admin role.
- Account lockout is configured but inactive on the default login page; no Content-Security-Policy is set.
- The application is intended to run locally and has not been deployed.

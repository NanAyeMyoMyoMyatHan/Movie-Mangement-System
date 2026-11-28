# RULES.md

Read and follow this file before creating, scaffolding, modifying, or reviewing code in this repository.

# Repository Development Rules

## 1. Project Architecture

Use a simple and clean separation of responsibilities.

The preferred solution structure is:

```text
Solution/
├── Domain/
│   ├── Models/
│   └── Interfaces/
│
├── Api/
│   ├── Controllers/
│   ├── Services/
│   ├── Data/
│   └── Program.cs
│
└── Blazor/
    ├── Components/
    ├── Pages/
    ├── Layout/
    ├── Models/
    ├── wwwroot/
    └── Program.cs
```

Adapt folder names to the existing solution when necessary. Do not restructure a working project without a clear reason.

### Domain project

The Domain project contains business concepts and contracts.

- Put domain entities and business models in `Domain/Entities` or `Domain/Models`.
- Put service and repository interfaces in `Domain/Interfaces`.
- Keep business rules and domain validation in the Domain layer.
- Do not put controllers, UI components, database contexts, or Blazor code in Domain.
- Do not add unnecessary abstractions or interfaces for classes that do not need them.

### API project

The API project handles database access, application services, and HTTP endpoints.

- Put Entity Framework Core `DbContext` and database configuration in the API project.
- Put service implementations in `Api/Services`.
- Put API controllers in `Api/Controllers`.
- Implement interfaces defined in Domain in the API project.
- Keep controllers thin: receive requests, call a service, and return an appropriate response.
- Keep business and database logic out of controllers.
- Register services, interfaces, and DbContext in dependency injection.
- Keep API request and response DTOs separate from database entities when that makes the API contract clearer.

### Blazor project

Blazor is responsible for the user interface only.

- Use Blazor for pages, layouts, components, forms, tables, dialogs, and UI interactions.
- Keep UI-specific state and presentation models in the Blazor project.
- Call the API through HTTP clients or a small API client/service layer.
- Do not connect Blazor directly to the database.
- Do not put Entity Framework Core queries or database contexts in Blazor.
- Do not put business rules or database operations in Razor components.
- Keep Razor components simple and readable.
- Reuse components for repeated UI elements when it makes the code easier to maintain.

## 2. Database Scaffolding and Entity Framework Core

Use EF Core Database-First scaffolding when the task requires generating models from an existing database.

- Scaffold database entities and the DbContext into the API project, not the Blazor project.
- Keep generated database models separate from Domain models when the project architecture requires it.
- Do not manually edit generated files unless there is a clear reason. Prefer partial classes or separate configuration files for custom behavior.
- Review generated files after scaffolding. Do not blindly overwrite existing custom code.
- Preserve existing tables, relationships, keys, constraints, and database naming.
- Never run destructive database commands without explaining the impact and obtaining approval.
- Do not create a new database if the user has already provided an existing database.
- Keep connection strings in configuration or user secrets. Never hardcode credentials in source code.

Every `dotnet ef dbcontext scaffold` command must include `--no-onconfiguring`.

Example:

```bash
dotnet ef dbcontext scaffold "Name=ConnectionStrings:DefaultConnection" Microsoft.EntityFrameworkCore.SqlServer --output-dir Data/Entities --context-dir Data --context AppDbContext --no-onconfiguring
```

Adjust the provider, connection name, and output folders to match the actual project.

## 3. Database and EF Core Query Rules

- Use EF Core for straightforward CRUD operations.
- Use asynchronous database methods where appropriate, such as `ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `AddAsync`, and `SaveChangesAsync`.
- Use `FirstOrDefaultAsync` when a query should return the first matching record or no record. Do not use `SingleOrDefaultAsync` for this purpose.
- Use `AsNoTracking()` for read-only queries when appropriate.
- Use transactions when multiple database changes must succeed or fail together.
- Avoid unnecessary database calls and repeated queries.
- Do not load all records when pagination or filtering is needed.
- Use parameterized queries. Never concatenate user input into SQL.
- For complex reports or queries, use a clear and maintainable approach. Use stored procedures when they are already part of the project or are genuinely more suitable.
- Do not introduce complicated LINQ joins or raw SQL when a simple query can solve the task.
- Check whether existing tables, columns, and relationships already exist before changing the database.

## 4. Service and Interface Rules

- Define service interfaces in `Domain/Interfaces`.
- Implement those interfaces in `Api/Services`.
- Keep each service focused on one feature or responsibility.
- Put database operations in API services or dedicated repositories when the existing architecture uses repositories.
- Do not place database access in controllers or Blazor components.
- Use dependency injection instead of manually creating service instances.
- Prefer simple methods with clear names and clear return types.
- Do not create unnecessary layers, factories, managers, or generic abstractions.

Example:

```csharp
// Domain/Interfaces/IProductService.cs
public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
}
```

```csharp
// Api/Services/ProductService.cs
public class ProductService : IProductService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        AppDbContext context,
        ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Implement the service methods here.
}
```

These examples are illustrative. Follow the actual namespaces and existing project conventions.

## 5. API Controller Rules

- Use controllers for HTTP request handling only.
- Keep controllers small and easy to understand.
- Call the appropriate service for application operations.
- Use appropriate HTTP methods and status codes.
- Use named request and response models for API endpoints.
- Do not return database entities directly when doing so would expose internal database structure or sensitive fields.
- Validate incoming requests before processing them, with authoritative business validation in the Domain layer.
- Return useful error responses without exposing internal exceptions or sensitive details.
- Use cancellation tokens for longer-running asynchronous operations when appropriate.
- Do not duplicate business logic between controllers and services.

## 6. Blazor UI Rules

Blazor must remain a presentation layer.

- Build pages and reusable components with Razor and C#.
- Keep UI code separate from API and database logic.
- Use `EditForm` and appropriate validation components for forms.
- Provide clear loading, empty, success, and error states.
- Disable submit buttons when a request is being processed to prevent accidental duplicate submissions.
- Show user-friendly validation and API error messages.
- Keep forms accessible with labels, clear focus states, and keyboard-friendly controls.
- Reuse existing CSS, components, and design patterns.
- Keep styling organized. Avoid adding unnecessary CSS frameworks or JavaScript libraries.
- Use JavaScript interop only when Blazor cannot reasonably handle the required browser behavior.
- Do not add JavaScript for a feature that can be implemented simply with Blazor.
- Do not put API secrets or database connection strings in Blazor client code.

## 7. Logging Convention

Log important application activities, especially in API services.

Important activities include:

- Login attempts and authentication results.
- Create, update, and delete operations.
- Important business-process steps.
- External API requests and responses, without logging sensitive payloads.
- Database operation failures.
- Unexpected exceptions.
- Important status changes.

Do not add excessive logs to every line or ordinary UI interaction.

### Required log format

Every application log message must include:

1. Project name.
2. Method name.
3. A clear message.

Use this exact prefix:

```text
[ProjectName] MethodName => Log message
```

Example:

```csharp
_logger.LogInformation(
    "[MyApi] CreateProductAsync => Product creation started. ProductName={ProductName}",
    request.Name);
```

```csharp
_logger.LogInformation(
    "[MyApi] CreateProductAsync => Product created successfully. ProductId={ProductId}",
    product.Id);
```

```csharp
_logger.LogWarning(
    "[MyApi] LoginAsync => Login failed because the credentials were invalid. Username={Username}",
    request.Username);
```

```csharp
_logger.LogError(
    exception,
    "[MyApi] CreateProductAsync => Product creation failed.");
```

### Logging rules

- Use `ILogger<T>` through dependency injection.
- Use structured logging placeholders such as `{UserId}` and `{ProductId}`.
- Use the exact method name that emits the log.
- Keep log messages concise and meaningful.
- Log important operations at the beginning and after successful completion when useful.
- Log exceptions with the exception object.
- Do not log passwords, PINs, OTPs, authentication tokens, connection strings, or other secrets.
- Do not log full sensitive personal or payment information.
- Do not catch an exception only to log it and silently continue.
- Avoid duplicate logging of the same exception at every layer.

## 8. Authentication and Security

- Use established ASP.NET Core authentication and authorization mechanisms.
- Store passwords using a secure password-hashing mechanism. Never store plain-text passwords.
- Protect private API endpoints with authorization.
- Check authorization on the server, not only by hiding UI elements in Blazor.
- Validate and sanitize user input appropriately.
- Keep secrets in user secrets or secure deployment configuration.
- Do not commit production credentials, API keys, tokens, or private certificates.
- Use HTTPS in deployed environments.
- Do not expose stack traces, internal database details, or sensitive information to users.
- Follow the principle of least privilege.
- Do not implement custom cryptography or authentication mechanisms when established framework features are suitable.

## 9. Error Handling

- Handle expected errors with clear and appropriate responses.
- Use `try-catch` only when an error can be handled meaningfully at that level.
- Do not wrap every method in unnecessary `try-catch` blocks.
- Do not swallow exceptions.
- Log unexpected errors with useful context.
- Show friendly error messages in the UI.
- Keep technical exception details in server-side logs, not in API responses shown to users.

## 10. Code Simplicity and Readability

This project prioritizes simple, readable, maintainable code.

### Do

- Use descriptive names.
- Keep methods short and focused.
- Give each class one clear responsibility.
- Prefer early returns and guard clauses.
- Reuse existing code when appropriate.
- Use `async` and `await` correctly.
- Use clear conditional statements.
- Keep code formatting consistent.
- Choose the simplest solution that meets the requirement.

### Don't

- Do not over-engineer simple features.
- Do not create unnecessary design patterns or abstractions.
- Do not use complicated LINQ when a simple loop or query is clearer.
- Do not create very large methods or classes.
- Do not duplicate existing functionality.
- Do not use unexplained magic numbers or strings.
- Do not add packages without a clear need.
- Do not change unrelated code during a focused task.
- Do not use single-letter variable names except for simple loop variables.

## 11. Comments and Important Changes

Comments should explain important decisions, business rules, or non-obvious behavior.

- Add comments when implementing or changing important business logic.
- Explain why a special condition or workaround is necessary.
- Do not comment on obvious code.
- Do not leave large blocks of commented-out code.
- Keep comments accurate when code changes.
- Do not add comments just to make the code look documented.

Example:

```csharp
// Keep the current profile image when the user does not upload a replacement.
if (request.ProfileImage == null)
{
    // Preserve the existing image.
}
```

### Change visibility

For every significant change:

1. Explain what will change and why before making the change.
2. Identify the files that will be added, modified, or removed.
3. Make changes in small, understandable steps.
4. Show a clear summary of each completed step and the important code changes.
5. Explain any database, API contract, or architecture impact.
6. Run the relevant build or tests.
7. Review the Git diff and check for unrelated changes.
8. Do not hide large changes inside one unexplained operation.

For small, straightforward changes, keep the explanation concise.

## 12. Git Workflow

Use Git to track meaningful changes.

- Check `git status` before starting work.
- Review existing uncommitted changes. Never overwrite or discard the user's work.
- Do not reset, clean, or force-push without explicit approval.
- Make small, focused commits after the related changes have been reviewed and validated.
- Do not mix unrelated features or fixes in one commit.
- Do not commit secrets, temporary files, build output, or debugging code.
- Show the user the files changed and a short summary before committing.
- After each logical task is completed and reviewed, create an appropriate commit when repository access and user authorization allow it.
- Do not claim a commit was created unless Git confirms it.

Use Conventional Commit messages:

```text
feat(product): add product management
fix(login): handle invalid credentials
refactor(memory): simplify memory service
docs(readme): update setup instructions
chore(database): add initial migration
```

## 13. Testing and Build Verification

After making changes:

- Build the affected projects.
- Run relevant tests when available.
- Test important API endpoints.
- Check both successful and unsuccessful cases.
- Verify database changes against the intended schema.
- Check that Blazor pages still load and that forms and API calls work.
- Fix errors introduced by the changes before considering the task complete.
- Do not claim that a feature works if it has not been verified.
- If a test cannot be run, explain why.

## 14. Configuration and Dependencies

- Preserve existing working configuration unless the requested task requires a change.
- Do not overwrite existing connection strings or environment settings without checking their use.
- Keep local development settings separate from production settings.
- Do not commit new secrets.
- Reuse installed packages and existing project utilities where possible.
- Explain why a new NuGet package or external dependency is necessary before adding it.
- Keep target frameworks and package versions compatible across the solution.

## 15. Project Structure and Naming

Keep feature-related files together where practical.

Example:

```text
Api/
├── Controllers/
│   └── ProductController.cs
├── Services/
│   └── ProductService.cs
└── Data/
    └── AppDbContext.cs

Domain/
├── Entities/
│   └── Product.cs
├── Models/
│   └── ProductRequestModel.cs
└── Interfaces/
    └── IProductService.cs

Blazor/
├── Pages/
│   └── Products.razor
├── Components/
│   └── ProductForm.razor
└── Services/
    └── ProductApiService.cs
```

- Use PascalCase for C# classes, methods, and public properties.
- Use clear, consistent names for database tables and columns.
- Follow the existing naming convention if the database already has one.
- Avoid unnecessary abbreviations.
- Keep namespaces aligned with the project structure.

## 16. Agent Workflow

When asked to implement a feature or modify the project:

1. Inspect the solution structure and relevant existing files first.
2. Read this `RULES.md` and follow the existing project conventions.
3. Understand the current implementation before changing it.
4. Explain the implementation plan briefly.
5. Make the smallest complete change.
6. Keep Domain, API, and Blazor responsibilities separated.
7. Add useful logs for important API service operations.
8. Add comments for important or non-obvious logic.
9. Show the changes clearly, step by step.
10. Build and test the affected projects.
11. Review the final diff and Git status.
12. Summarize what changed, what was tested, and any remaining issues.
13. Create a focused Git commit after review when authorized.

If requirements are unclear or a change could cause data loss, security issues, or a breaking API change, ask before proceeding.

## 17. Final Quality Checklist

Before completing a task, check:

- [ ] Database and EF Core code are in the API project.
- [ ] Domain entities, business models, and interfaces are in Domain.
- [ ] Blazor contains UI and presentation logic only.
- [ ] API services implement the Domain interfaces.
- [ ] Controllers are thin and call services.
- [ ] Important API service operations have useful logs.
- [ ] Sensitive data is not logged or exposed.
- [ ] Important business logic has meaningful comments where needed.
- [ ] Code is simple and readable.
- [ ] Existing features and user changes are preserved.
- [ ] Relevant builds and tests have been run.
- [ ] Git diff has been reviewed.
- [ ] Changes have been explained and committed when authorized.

## Development Philosophy

> Keep the architecture clean, the code simple, and every important change visible. Build software that another developer can understand, maintain, and improve without unnecessary complexity.

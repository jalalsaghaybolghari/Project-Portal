# SecureCadPortal — QA Test Plan

## Objective
Verify the functionality, security, reliability, and stability of the SecureCadPortal API.

## Scope
- Project CRUD operations
- Input validation and error handling
- Authentication and authorization
- Security headers and rate limiting
- Database integration
- Regression and smoke testing

## Test Levels
| Level | Purpose | Tool |
|---|---|---|
| Unit | Test isolated business logic | xUnit |
| Integration | Test API and component integration | xUnit |
| API automation | Test HTTP endpoints and workflows | Playwright API |
| Regression | Detect previously fixed defects | xUnit and Playwright |
| Smoke | Verify critical functionality | HTTP checks |

## Environment
- .NET 10 / ASP.NET Core
- SQL Server / Azure SQL
- Microsoft Entra ID
- xUnit
- Playwright with TypeScript
- Azure DevOps Pipelines

## Entry Criteria
- The application builds successfully.
- Test configuration is available.
- Test data can be created safely.

## Exit Criteria
- All mandatory tests pass.
- Critical and high-severity defects are resolved or formally accepted.
- Test results and known limitations are documented.

## Defect Management
Record the defect ID, environment, reproduction steps, expected and actual results, severity, priority, evidence, and verification status.

## Risks
- Identity-provider availability
- Database connectivity and permissions
- Environment-specific configuration
- Unstable automated tests
- Conflicting test data

## Maintenance
Review this plan whenever API contracts, security requirements, or release procedures change.

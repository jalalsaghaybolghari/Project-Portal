# SecureCadPortal — Test Cases

Status values: Not Run, Passed, Failed, Blocked.

| ID | Scenario | Expected Result | Priority | Status |
|---|---|---|---|---|
| API-001 | Request projects with a valid access token | HTTP 200 and valid response | High | Not Run |
| API-002 | Request projects without a token | HTTP 401 | High | Not Run |
| API-003 | Request projects with an invalid token | HTTP 401 | High | Not Run |
| API-004 | Use a valid token without the required permission | Access denied according to the configured policy | High | Not Run |
| API-005 | Submit a project with a missing required name | Validation error; no invalid record created | High | Not Run |
| API-006 | Request a nonexistent project | HTTP 404, if supported by the endpoint | Medium | Not Run |
| API-007 | Inspect response security headers | Configured security headers are present | High | Not Run |
| API-008 | Exceed the configured request limit | HTTP 429 when the limit is exceeded | Medium | Not Run |
| DB-001 | Create and retrieve a project | Persisted values match submitted values | High | Not Run |
| DB-002 | Update a project | Updated values are persisted | High | Not Run |
| DB-003 | Delete a project | Deleted project is no longer returned | High | Not Run |
| REG-001 | Run the existing automated test suite | All expected tests pass | High | Not Run |

## Execution Record

For each test run, record:
- Date and time
- Tester
- Commit or build identifier
- Environment
- Actual result
- Pass/fail status
- Related defect ID

Do not mark a test as Passed until it has actually been executed.

Confirm the API's actual endpoint contracts and validation rules before execution.

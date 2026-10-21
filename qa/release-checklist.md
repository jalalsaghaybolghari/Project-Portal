# Release QA Checklist

## Build and Tests
- [ ] Application builds successfully.
- [ ] Unit tests pass.
- [ ] Integration tests pass.
- [ ] API regression tests pass.
- [ ] Test reports are reviewed.

## Security
- [ ] Authentication and authorization are verified.
- [ ] Input validation is tested.
- [ ] Security headers are checked.
- [ ] Rate limiting is verified.
- [ ] No secrets are included in source control or test reports.

## Release Readiness
- [ ] Critical and high-severity defects are resolved or formally accepted.
- [ ] Configuration and database connectivity are verified.
- [ ] Smoke tests pass in the target environment.
- [ ] Known limitations are documented.
- [ ] Release decision is recorded.

Do not check an item until its verification is complete.

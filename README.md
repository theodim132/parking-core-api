# parking-core-api

ASP.NET Core API for parking permit applications.

## Repository Scope

This repo owns API business logic, persistence, and HTTP endpoints.

It does not own infrastructure orchestration (see `parking-infra`) or Keycloak realm assets (see `parking-auth`).

## Stack

- .NET 9
- PostgreSQL + EF Core
- Keycloak (OIDC)
- MinIO-compatible object storage

## Run Locally

```bash
dotnet run --project Parking.CoreApi
```

## Configuration

Use environment variables or `appsettings.json`:
- `ConnectionStrings__Default`
- `Auth__Authority`
- `Auth__Audience`
- `Storage__Provider`
- `Storage__Endpoint`
- `Storage__AccessKey`
- `Storage__SecretKey`
- `Storage__Bucket`

## Docker

Dockerfile path:

```bash
Parking.CoreApi/Dockerfile
```

Build example:

```bash
docker build -t parking-core-api:dev Parking.CoreApi
```

## CI/CD

Jenkins pipeline file:

```bash
Parking.CoreApi/Jenkinsfile
```

Pipeline stages include restore, build, test, image build/push, and deployment trigger.

## Main Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/applications | List my applications |
| POST | /api/applications | Create new application |
| PUT | /api/applications/{id} | Update draft |
| POST | /api/applications/{id}/submit | Submit for review |
| POST | /api/admin/applications/{id}/decision | Approve/reject |
| POST | /api/applications/{id}/documents | Upload document |

For local testing, `X-Citizen-Id` can be used instead of JWT.

## Related Repositories

- `parking-email-worker`
- `parking-auth`
- `parking-infra`

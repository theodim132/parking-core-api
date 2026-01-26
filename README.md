# parking-core-api

ASP.NET Core Web API for managing parking permit applications.

## Stack
- .NET 9
- PostgreSQL + EF Core
- Keycloak (OIDC)
- MinIO for document storage

## Run locally
```bash
dotnet run --project Parking.CoreApi
```

## Configuration
Environment variables or `appsettings.json`:
- `ConnectionStrings__Default` - PostgreSQL connection
- `Auth__Authority` - Keycloak realm URL
- `Storage__Provider` - `Local` or `S3`

## API
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/applications | List my applications |
| POST | /api/applications | Create new application |
| PUT | /api/applications/{id} | Update draft |
| POST | /api/applications/{id}/submit | Submit for review |
| POST | /api/admin/applications/{id}/decision | Approve/reject |
| POST | /api/applications/{id}/documents | Upload document |

For testing, use `X-Citizen-Id` header instead of JWT.

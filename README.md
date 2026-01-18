# parking-core-api

ASP.NET Core Web API for Parking Permit applications.

## Tech
- .NET 9
- PostgreSQL (EF Core)
- Keycloak (OIDC)
- Storage: Local or MinIO (S3-compatible)

## Configuration
Set via `appsettings.json` or environment variables:
- `ConnectionStrings__Default`
- `Auth__Authority` (Keycloak realm URL)
- `Auth__Audience` (client id, default: `parking-api`)
- `Storage__Provider` (`Local` or `S3`)
- `Storage__Endpoint`, `Storage__AccessKey`, `Storage__SecretKey`, `Storage__Bucket`

## Local run
```
dotnet run
```

## Migrations
```
dotnet ef database update
```

## API (basic)
- `GET /api/applications`
- `POST /api/applications`
- `PUT /api/applications/{id}`
- `POST /api/applications/{id}/submit`
- `GET /api/applications/{id}`
- `POST /api/admin/applications/{id}/decision`
- `POST /api/applications/{id}/documents`
- `GET /api/documents/{id}`

Note: For demo, endpoints accept `X-Citizen-Id` header when no JWT is present.

Webhook test.

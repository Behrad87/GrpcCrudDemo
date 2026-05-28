# GrpcCrudDemo

## Overview

A simple CRUD gRPC service for a `Person` model with the following fields:

- `Id` (GUID)
- `FirstName`
- `LastName`
- `NationalCode`
- `BirthDate`

The service uses Protocol Buffers (gRPC) instead of REST.

---

## What is Implemented

- **Proto file:** `Protos/person.proto` with RPCs: `CreatePerson`, `GetPerson`, `UpdatePerson`, `DeletePerson`, `GetAllPersons`
- **Server:** `Services/PersonService` implements the RPCs using `AppDbContext` (EF Core)
- **Storage:** EF Core in-memory database (`UseInMemoryDatabase`) for easy local execution
- **Input validation:** Checks for `FirstName`/`LastName` presence and `NationalCode` (must be exactly 10 digits)
- **Error handling:** Uses `RpcException` with gRPC status codes (`InvalidArgument`, `NotFound`)

---

## Run Locally

```bash
dotnet run
```

The server will start and print the URL (e.g. `localhost:5174`). Use that port in the commands below.

---

## CRUD Commands via grpcurl

> **Important:** Use **CMD** (not PowerShell) for all grpcurl commands. PowerShell mangles JSON quoting and will cause errors.

### Create a Person

```cmd
grpcurl.exe -plaintext -d "{\"firstName\":\"Ali\",\"lastName\":\"Rezaei\",\"nationalCode\":\"0123456789\",\"birthDate\":\"1990-01-01\"}" localhost:5174 person.PersonService/CreatePerson
```

The response will include an `id` (GUID). Copy it — you'll need it for the commands below.

---

### Get a Person by ID

```cmd
grpcurl.exe -plaintext -d "{\"id\":\"PASTE-GUID-HERE\"}" localhost:5174 person.PersonService/GetPerson
```

---

### Get All Persons

```cmd
grpcurl.exe -plaintext localhost:5174 person.PersonService/GetAllPersons
```

---

### Update a Person

```cmd
grpcurl.exe -plaintext -d "{\"id\":\"PASTE-GUID-HERE\",\"firstName\":\"Ali\",\"lastName\":\"Updated\",\"nationalCode\":\"0123456789\",\"birthDate\":\"1995-05-15\"}" localhost:5174 person.PersonService/UpdatePerson
```

---

### Delete a Person

```cmd
grpcurl.exe -plaintext -d "{\"id\":\"PASTE-GUID-HERE\"}" localhost:5174 person.PersonService/DeletePerson
```

Expected response:

```json
{
  "success": true
}
```

---

## PowerShell Workaround

If you must use PowerShell, write the JSON to a file without BOM and pass it with `@`:

```powershell
[System.IO.File]::WriteAllText("C:\grpcurl\body.json", '{"firstName":"Ali","lastName":"Rezaei","nationalCode":"0123456789","birthDate":"1990-01-01"}')
.\grpcurl.exe -plaintext -d '@body.json' localhost:5174 person.PersonService/CreatePerson
```

> **Why not `Out-File`?** PowerShell's `Out-File` adds a UTF-8 BOM by default, which grpcurl cannot parse. `[System.IO.File]::WriteAllText` writes clean UTF-8 without BOM.

---

## Suggested Improvements

### 1. Use a Real Persistent Database
Replace `UseInMemoryDatabase` with SQLite or SQL Server and add EF Core migrations for persistence across restarts.

### 2. Use `google.protobuf.Timestamp` for `birthDate`
Passing dates as strings is error-prone and loses timezone semantics. Example proto change:

```proto
import "google/protobuf/timestamp.proto";

message PersonModel {
  string id = 1;
  string firstName = 2;
  string lastName = 3;
  string nationalCode = 4;
  google.protobuf.Timestamp birthDate = 5;
}
```

C# mapping:

```csharp
// Timestamp → DateTime
var dateTime = personModel.BirthDate.ToDateTime();

// DateTime → Timestamp
var timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(dateTime.ToUniversalTime());
```

### 3. Structured Error Details
Use `google.rpc.Status` and error detail types for machine-readable error payloads. Log server-side errors with correlation IDs.

### 4. Authentication, Authorization & TLS
Protect endpoints with mTLS or JWT. Always enable TLS in production.

### 5. Stronger Validation
Add algorithm-based `NationalCode` validation. Consider using `FluentValidation`.

### 6. Unit & Integration Tests
Add unit tests for service logic and integration tests using an in-memory or TestContainers DB.

### 7. Pagination & Filtering for `GetAllPersons`
Avoid returning entire tables. Implement page size/token pagination and server-side filtering.

### 8. Optimistic Concurrency
Add rowversion/ETag tokens if multiple writers are expected.

### 9. Observability
Add structured logging, metrics, and distributed tracing via OpenTelemetry. Add health check endpoints.

### 10. CI/CD & Docker
Add a `Dockerfile`, GitHub Actions workflow with build/test steps, and deployment manifests.

### 11. Proto Versioning
Use semantic versioning or package name versioning for the proto service. Add deprecation strategies for breaking changes.

### 12. Streaming for Bulk Operations
For large datasets, consider server-side streaming for `GetAllPersons` or client-side streaming for bulk inserts.

### 13. Nullable Fields
Use `google.protobuf.StringValue` or proto3 `optional` to distinguish between empty and unset fields.

---

## Suggested Next Steps

1. Migrate to SQLite and add EF Core migrations
2. Change proto `birthDate` to `google.protobuf.Timestamp` and update C# mappings
3. Add unit and integration tests
4. Add a `Dockerfile` and GitHub Actions workflow
5. Strengthen validation with `FluentValidation` and add structured error details
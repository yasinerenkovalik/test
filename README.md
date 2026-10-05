# Test API (.NET 10, Onion Architecture, CQRS)

User, Authentication, Category ve Product yönetimi içeren RESTful Web API.

## Teknolojiler
.NET 10 · EF Core 10 + PostgreSQL · MediatR (CQRS) · FluentValidation · AutoMapper · JWT Bearer + Refresh Token ·
Serilog · Swagger · Docker / Compose · xUnit + FluentAssertions

> MediatR 12.5.0 ve AutoMapper 13.0.1 bilinçli olarak sabitlendi: sonraki sürümler ticari lisans/lisans anahtarı gerektiriyor.

## Mimari (Onion)

```
        API  ──────►  Infrastructure
         │                 │
         ▼                 ▼
      Application  ◄───────┘   (Infrastructure, Application'daki arayüzleri uygular)
         │
         ▼
       Domain   (hiçbir şeye bağımlı değil)
```

| Katman | Sorumluluk |
|---|---|
| `Test.Domain` | Entity'ler, enum, iş kuralları (`Product.Create` fiyat/stok kontrolü), `BusinessException` |
| `Test.Application` | Use case'ler (Command/Query + Handler + Validator), pipeline behavior'lar, DTO, AutoMapper profilleri, arayüzler |
| `Test.Infrastructure` | EF Core `ApplicationDbContext`, configuration'lar, audit/soft-delete interceptor, JWT, şifre hash, seed |
| `Test.API` | Controller'lar (ince), global exception handler, Swagger, Serilog, DI birleştirme |

Önemli kararlar:
- **Generic repository yok.** Application, `IApplicationDbContext` arayüzünü kullanır; böylece `IQueryable` (filtre, sıralama, sayfalama, `ProjectTo`) DB tarafında çalışır.
- **Audit + soft delete** `AuditableEntityInterceptor` içinde: handler'lar `Remove()` çağırır, interceptor bunu `IsDeleted=true` + `DeletedDate` yapar. Global Query Filter silinenleri gizler.
- **Refresh token**: ham token saklanmaz (SHA-256 hash), rotation uygulanır, kullanılmış token tekrar gelirse kullanıcının tüm oturumları kapatılır. Şifre değişince de tüm refresh token'lar iptal edilir.
- **Şifre**: PBKDF2-HMAC-SHA256 (600k iterasyon, kullanıcıya özel salt).

## CQRS ve pipeline
Her istek bir `IRequest` (Command = durum değiştirir, Query = okur) ve kendi Handler'ına sahip.
Controller sadece `ISender.Send(...)` çağırır. Pipeline sırası: **Logging → Performance (>500ms uyarı) → Validation → Handler**.
Loglanan request'lerde `password/token/secret/hash` içeren alanlar `***` olarak maskelenir.

## Authentication / Authorization
- `POST /api/auth/register` → normal `User` rolüyle kayıt
- `POST /api/auth/login`, `POST /api/auth/refresh-token`
- JWT claim'leri: `sub` (UserId), `email`, `role`
- Okuma endpoint'leri (ürün/kategori listeleme-detay) **herkese açık** (katalog); Admin olmayanlar yalnızca aktif kayıtları görür.
- Ürün/kategori oluşturma-güncelleme-silme ve stok güncelleme: **Admin**.
- `GET /api/users/{id}`: kullanıcı kendini, Admin herkesi görebilir.

## Çalıştırma

### Docker ile: veritabanı + backend + frontend tek komutla
```bash
docker compose up -d --build
```
- Arayüz: http://localhost:3000
- Swagger: http://localhost:8080/swagger
- Health: http://localhost:8080/health

Varsayılan şifre/secret değerleri yalnızca lokal geliştirme içindir; gerçek ortamda `.env` ile (bkz. `.env.example`) değiştirin.
Sıfırdan başlamak için: `docker compose down -v`

### Lokal (Rider / CLI)
```bash
docker compose up -d postgres
dotnet user-secrets set "Jwt:Secret" "<en az 32 karakterlik rastgele değer>" --project src/Test.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=test_db;Username=postgres;Password=<.env'deki şifre>" --project src/Test.API
dotnet run --project src/Test.API
```
Development ortamında açılışta migration/oluşturma + seed otomatik çalışır.
Seed kullanıcı bilgileri `src/Test.API/appsettings.Development.json` içindeki `Seed` bölümündedir (yalnızca development).

## Migration
```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate -p src/Test.Infrastructure -s src/Test.API -o Persistence/Migrations
# DB'ye elle uygulamak istenirse:
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=test_db;Username=postgres;Password=..."
dotnet ef database update -p src/Test.Infrastructure -s src/Test.API
```
Uygulama açılışta migration varsa `Migrate()`, yoksa `EnsureCreated()` kullanır.
Migration ürettikten sonra eski (EnsureCreated ile oluşmuş) DB'yi silin: `docker compose down -v`.

## Environment variable'lar
| Değişken | Açıklama |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL bağlantısı |
| `JWT__SECRET` | İmza anahtarı (>= 32 karakter, zorunlu) |
| `JWT__ISSUER`, `JWT__AUDIENCE` | Token issuer / audience |
| `Database__MigrateOnStartup` | `true` ise açılışta migration uygular |
| `ASPNETCORE_ENVIRONMENT` | `Development` iken Swagger + seed açık |

## Swagger'da kullanım
1. `POST /api/auth/login` ile giriş yapın, `accessToken` değerini kopyalayın.
2. **Authorize** butonuna sadece token'ı yapıştırın ("Bearer " eklemeyin).

## Testler
```bash
dotnet test
```
- `Test.UnitTests`: validator'lar, handler'lar (InMemory DB), domain kuralları
- `Test.IntegrationTests`: `WebApplicationFactory` ile gerçek pipeline (register, login, refresh, products, yetkilendirme). Docker gerektirmez.

## Endpoint'ler
| Metot | Yol | Yetki |
|---|---|---|
| POST | /api/auth/register, /login, /refresh-token | Herkes |
| GET | /api/users/{id} | Kendisi / Admin |
| GET, PUT | /api/users/me | Giriş yapmış |
| PUT | /api/users/me/change-password | Giriş yapmış |
| GET | /api/categories, /api/categories/{id} | Herkes |
| POST, PUT, DELETE | /api/categories[/{id}] | Admin |
| GET | /api/products?search=&categoryId=&minPrice=&maxPrice=&isActive=&sortBy=&sortDirection=&page=&pageSize= | Herkes |
| GET | /api/products/{id} | Herkes |
| POST, PUT, DELETE | /api/products[/{id}] | Admin |
| PATCH | /api/products/{id}/stock | Admin |

`sortBy`: `Name | Price | CreatedDate | Stock`, `sortDirection`: `Asc | Desc`.

## Örnekler
Register:
```json
POST /api/auth/register
{ "firstName": "Ada", "lastName": "Lovelace", "email": "ada@example.com", "password": "Passw0rdX" }
```
Başarılı cevap:
```json
{ "success": true, "data": { "accessToken": "...", "refreshToken": "...", "expiration": "...", "refreshTokenExpiration": "...",
  "user": { "id": "...", "email": "ada@example.com", "firstName": "Ada", "lastName": "Lovelace", "role": "User" } },
  "message": "User registered successfully.", "errors": [], "statusCode": 201, "traceId": "..." }
```
Hata:
```json
{ "success": false, "data": null, "message": "Validation failed.",
  "errors": [ { "field": "Email", "message": "'Email' is not a valid email address." } ],
  "statusCode": 400, "traceId": "..." }
```
Ürün listesi:
```json
{ "success": true, "data": { "items": [], "page": 1, "pageSize": 20, "totalCount": 100, "totalPages": 5 }, ... }
```

## Frontend (`web/`)
React + Vite test arayüzü. Docker ile `http://localhost:3000` adresinde açılır; nginx `/api` isteklerini backend'e iletir.
Docker'sız geliştirme: `cd web && npm install && npm run dev` (Vite proxy hedefi: `VITE_API_TARGET`, varsayılan `http://localhost:8080`).

## Geliştirme modu (canlı güncelleme)
```bash
docker compose -f docker-compose.dev.yml watch
```
- Backend (`src/**/*.cs`): kaydedince `dotnet watch` hot reload yapar, gerekirse API'yi kendisi yeniden başlatır.
- Frontend (`web/src/**`): Vite HMR ile tarayıcı anında güncellenir.
- Yeni NuGet/npm paketi eklersen (`.csproj` / `package.json`) imaj otomatik yeniden derlenir.
- Entity değişince şema: `EnsureCreated` mevcut tabloyu güncellemez. Migration üretin veya geliştirmede `docker compose -f docker-compose.dev.yml down -v` ile DB'yi sıfırlayın.
- Üretim benzeri çalıştırma için `docker compose up -d --build` kullanın; ikisini aynı anda çalıştırmayın (portlar çakışır).
# test

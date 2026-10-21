# Kaffeine — Contributor Instructions

## Architecture at a glance

Kaffeine is a browser-based graphical toolkit for managing, debugging, and operating Kafka across environments, clusters, and topics.

| Concern | Decision |
| --- | --- |
| Architecture | Vertical slices on backend and frontend |
| Backend | ASP.NET Core / C# |
| Frontend | React / TypeScript, client-rendered |
| Kafka client | `Confluent.Kafka`, backend only; companion schema packages where needed |
| Deployment | One application container; local or Kubernetes |

The backend owns Kafka connections, credentials, consumer lifetimes, and administrative operations. The browser communicates with the backend, never directly with brokers.

## Deployment

- Deliver one Docker image for local developer use and shared team deployments in Kubernetes.
- Use a multi-stage build: Node.js builds the frontend, the .NET SDK publishes the backend, and the final ASP.NET Core runtime image contains the published application and frontend assets.
- ASP.NET Core serves the frontend assets and API from one application process and HTTP port. Do not require a separate Node.js server or frontend container.
- Include the Kafka client's native dependencies and verify compatibility for each supported container architecture.
- Kafka brokers, an optional Schema Registry, and any team identity provider remain external services. One application container does not mean these services are bundled.
- Supply configuration and secrets externally. Persist saved state outside the container's writable layer.
- Keep the same application image usable locally and in Kubernetes. Do not assume multiple replicas are safe without coordinating automation ownership, including during rolling updates.

## Vertical slice architecture

This is a **vertical slice architecture application**, on both the backend and frontend. Organize around user capabilities, not horizontal technical layers.

- A slice implements one meaningful capability, such as reading topic messages, searching messages, validating a payload, or configuring a topic.
- Group related slices under a feature that owns their domain state. Keep a capability's logic, contracts, validation, and behavioral tests together within its feature rather than scattering them across global technical folders.
- Keep corresponding frontend and backend capabilities recognizable by consistent domain names. The HTTP contract is their boundary; they do not share runtime implementations.
- Slices must not directly depend on other slices. Do not reach into another feature's internals.
- Composition roots wire features, routes, layouts, dependencies, and application startup; they do not implement business rules.
- Shared code is limited to stateless infrastructure, pure utilities, and domain-independent presentation. Feature-specific state and business rules belong to their owning feature.
- Avoid speculative abstractions. Do not introduce horizontal `Domain`, `Application`, and `Infrastructure` layer projects, generic repositories, or mediator pipelines.

### Backend

- Group endpoints, handlers, request/response contracts, validation, and operation-specific Kafka access under `Features/<Feature>/`.
- Keep handlers focused on orchestration; extract substantial decision logic into testable functions within the owning feature.
- Keep host startup and dependency wiring in the composition root. Cross-cutting middleware and connection plumbing must not become a shared business-service layer.
- Keep automations within the single application deployment unless a separate deployment boundary is explicitly agreed.

### Frontend

- Group capabilities under `features/<feature>/<capability>/`, with their components, hooks, local types, and behavioral tests.
- Each feature exposes a narrow `index.ts` public surface. Cross-feature imports use that surface, not deep imports into implementation files.
- Features own their state; mount providers at the lowest page or layout that covers their consumers.
- Pages, layouts, and routing compose capabilities. Shared UI components remain domain-independent.

## Operational safety

- Keep operations explicit about environment, cluster, topic, and partition scope.
- Browsing must not commit production consumer offsets or interfere with application consumer groups.
- Bound reads and searches, support cancellation, and avoid unbounded buffering in the backend or browser.
- Destructive tools remain disabled by default. Enforce authorization on the backend; UI confirmation is not authorization.
- Shared deployments require authentication and user-level authorization, even when Kafka access uses a shared service account.
- Do not promise reversibility for record deletion or partition increases.

## Documentation

### `README.md`

Keep `README.md` up to date with the product: its purpose, the features and tools that exist, and current status. It is documentation for someone deciding what this application is and what they can do with it.

- Update `README.md` in the same change that adds, renames, removes, or changes the scope of a user-visible capability. A feature that ships without a README update is incomplete.
- Include: the purpose and the problems it addresses, who it is for, each available tool or capability with a plain-language description of what it does and its scope, and an accurate status.
- Describe only what is available to a user today. Planned, internal, or aspirational capabilities stay out, or are stated explicitly as not yet available in Status.
- State limitations that change how a tool is used — a tool disabled by default, an operation that is not reversible, a capability that needs a configured cluster.
- Exclude technical implementation and detail. Keep out: language, framework, and library choices; architecture and folder layout; HTTP routes, contracts, and payloads; configuration, environment variables, and secrets; build, run, test, and deployment commands; ports, versions, and toolchain prerequisites; internals of validation, persistence, and authorization; and anything that goes stale faster than the feature itself.
- Write in the product's own terms — what a user sees and does — not in terms of how it is built. `read` reads messages from a topic; it is not "a paged `GET /api/topics/{name}/messages` served by a pooled consumer".
- Keep it short and scannable. Prefer one table row or one sentence per capability over prose that restates the interface.

### `docs/roadmap.md`

- Planned capabilities belong here, in version order — never in `README.md`.
- When a capability ships, describe it in `README.md` and mark it shipped in the roadmap in the same change. Keep shipped entries so the sequence reads as history.
- Add, drop, reorder, or rescope a version entry when the plan changes, and say why in the change. Keep it product-facing: no implementation detail, no dates unless committed.

### `AGENTS.md`

- Keep stack, architecture, implementation, and development-environment instructions here, never in `README.md`. When a change moves a responsibility between layers or adds a stack constraint, document it here.
- The project is in early development. Do not describe planned functionality as released, or deployment packaging as verified, until it is exercised.

### Verification

- Verify changes through the affected user-visible behavior. Tests should protect capability contracts and meaningful boundaries rather than implementation details.
- Before finishing, re-read `README.md` against the change: every statement must still be true, and every new user-visible capability must be represented.

## Development workflow

The initial solution is `src/Kaffeine.slnx`. `Kaffeine.Api` is the ASP.NET Core host; `Kaffeine.Web` is the React SPA. The application-status capability demonstrates the feature boundary on both sides. Kafka operations are not implemented in this skeleton.

Prerequisites: .NET SDK 10.0.4xx (selected by `src/global.json`) and Node.js 24 with npm. The frontend dependency lockfile is committed; use `npm ci` for reproducible installs.

Run the backend from `src`:

```sh
dotnet run --project Kaffeine.Api
```

Run the frontend in a second terminal from `src/Kaffeine.Web`:

```sh
npm ci
npm run dev
```

Open `http://127.0.0.1:5173`. Vite proxies `/api` to the backend at `http://127.0.0.1:5080`. The development backend does not serve the unbuilt frontend.

To build and publish the combined application, run from `src`:

```sh
dotnet publish Kaffeine.Api -c Release -o ../artifacts/app
```

Publishing installs the locked frontend dependencies, checks TypeScript, builds the SPA, and includes its assets in the host's published `wwwroot`. Node.js is required to publish, not to run the published application.

Run from `artifacts/app`:

```sh
dotnet Kaffeine.Api.dll --urls http://127.0.0.1:5080
```

Open `http://127.0.0.1:5080` for the single-process application. Client routes fall back to the SPA; unknown `/api` routes return 404. `GET /api/application/status` reports application development status, not Kafka health.

The current shell has no authentication and performs no Kafka operations. Do not expose it as a secured team deployment. Container packaging and team authentication are not implemented yet.

## Application persistence

The API registers a scoped EF Core `KaffeineDbContext` in `Persistence/`. Inject it directly into feature operations; do not add repositories or another unit-of-work abstraction. Features own entities and `IEntityTypeConfiguration<T>` mappings, which the context discovers in the API assembly.

SQLite is the only implemented provider. `appsettings.json` sets `Persistence:Provider` to `Sqlite` and `ConnectionStrings:Kaffeine` to `Data Source=kaffeine.db;Foreign Keys=True`. Relative database paths resolve against the host content root, not the caller's working directory. Override configuration with `Persistence__Provider` and `ConnectionStrings__Kaffeine` environment variables; for example, set `ConnectionStrings__Kaffeine='Data Source=/var/lib/kaffeine/kaffeine.db;Foreign Keys=True'` for externally mounted storage.

Provision the database's parent directory with write access for the application identity. SQLite creates the file on first database use. Registration rejects unsupported providers, missing or malformed connection strings, temporary/in-memory databases, and SQLite URI filenames. It does not check filesystem access or database availability at startup.

No feature tables exist yet. Startup does not call `EnsureCreated` or apply schema changes. Add schema evolution alongside the first persisted feature; do not treat changing the provider setting as a data migration. PostgreSQL support requires its provider package, configuration, schema evolution, and real-provider verification.

Keep the SQLite file and journal files outside the container's writable layer and outside published frontend assets. The default `kaffeine.db` and its journal files are ignored by Git; custom locations must also be kept out of source control. Exclude live data from deployment artifacts and use SQLite-aware backups. SQLite persistence is single-instance; do not share the file across application replicas.

Persistence does not automatically encrypt entity properties or enforce user ownership. Features must explicitly use the secret-protection infrastructure for credentials and sensitive query values, and implement authorization before exposing saved data. Sensitive-data logging is not enabled. The native SQLite bundle is pinned explicitly to avoid the vulnerable dependency shipped transitively by EF Core 10.0.0.

## Secret protection

`Infrastructure/SecretProtection/` exposes `ISecretProtector` for recoverable secrets. It uses ASP.NET Core Data Protection, independent of the database provider. Features authorize operations, protect plaintext before assigning persistence entities, store only the returned ciphertext, and decrypt only for authorized backend use. Do not introduce a generic secrets CRUD API or return stored plaintext through read endpoints.

Every call supplies a `SecretProtectionContext` with stable, non-secret `OwnerId`, `ResourceId`, and `SecretName`. Derive these from trusted feature state, not unchecked request values. Separate cryptographic purpose segments bind ciphertext to all three identifiers; renaming display labels must not change them. Ownership transfer requires authorized decrypt/re-protect. The `v1:` format prefix and protection purposes are durable storage contracts. Unknown versions, tampered ciphertext, revoked/missing keys, or the wrong context fail without a plaintext fallback.

Configure all three settings externally:

| Environment variable | Purpose |
| --- | --- |
| `SecretProtection__KeyRingPath` | Existing private, writable directory on persistent storage for encrypted Data Protection keys |
| `SecretProtection__CertificatePath` | PEM X.509 certificate used to encrypt the key ring |
| `SecretProtection__PrivateKeyPath` | Matching unencrypted PEM RSA private key, at least 2048 bits, delivered through a restricted external secret mount |

Paths resolve against the host content root. Provision the key directory with access limited to the application identity; restrict private-key access similarly. Keep the private key separate from the database, key-ring volume, and their backups. Keep all of these outside source control, frontend assets, images, and build/publish directories. The private-key file is not password-encrypted: protection comes from the external secret delivery mechanism and filesystem permissions. No private key or certificate is generated automatically.

For local development, generate a dedicated self-signed RSA certificate once in an external private directory using `openssl req -x509 -newkey rsa:3072 -sha256 -days 365 -nodes -subj '/CN=Kaffeine local secret protection' -keyout private-key.pem -out certificate.pem`. Run under `umask 077`, never overwrite an existing key, and supply the resulting absolute paths through the settings above. This certificate protects stored keys; it is not an HTTPS certificate.

With no `SecretProtection` section, the shell starts but resolving `ISecretProtector` throws an actionable configuration error. Partial configuration is rejected. With configuration present, startup loads the certificate and performs a non-secret protection roundtrip before listening, exercising key-ring access. It does not prove that every historical encrypted record is recoverable; retain restore checks using representative stored ciphertext.

The application discriminator is fixed as `Kaffeine.StoredSecrets`, so content-root changes do not invalidate secrets. Separate installations/environments must use separate key rings and certificates. Replicas of the same installation need access to the same ring and certificate; this does not make SQLite or automation ownership replica-safe.

Data Protection rotates encryption keys automatically; expired keys remain needed for existing ciphertext and backups. Do not delete or revoke keys still needed for recovery. Back up the database and encrypted key ring, and maintain separate protected recovery of the original certificate/private key. Rotation does not re-encrypt existing records or revoke upstream Kafka/API credentials. The current configuration accepts one wrapping certificate: do not replace it while old key files depend on it; certificate rollover requires a deliberate migration with old-key recovery. Loss of the key ring or its private key makes existing secrets unrecoverable.

This protects against theft of the database, including when encrypted key-ring files are also stolen, but not compromise of a running backend or an account that can access the private key. Authentication, feature authorization, credential storage endpoints, and KMS/vault integration are not implemented by this infrastructure.

Run the security regression suite from `src` with `dotnet test Kaffeine.Api.Tests`. It uses temporary certificate/key-ring files and real Data Protection to check ownership isolation, tampering, restart/key rotation, missing keys, certificate separation, and absent/partial configuration.

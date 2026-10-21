# Roadmap

What Kaffeine plans to support, in the order it will be built. `README.md` describes what exists **today**; this document describes what is **planned**. Nothing here is a commitment to a date — versions are a sequence, not a schedule.

## Summary

| Version | Theme | Capability | Status |
| --- | --- | --- | --- |
| 0 | Skeleton | Application status only, no Kafka connectivity | Current |
| 1 | Connect and inspect | Saved connections, topic discovery, bounded message inspection | Planned |
| 2 | Personal workspace | Pins, folders, recent navigation, reading preferences and presets | Planned |
| 3 | Search and revisit | Bounded search, coverage, saved searches, optional history | Planned |
| 4 | Decode and validate | Schema Registry connections, schema-aware decoding, validation | Planned |
| 5 | Operational tools | Reviewed configuration changes, bounded record deletion, operation history | Planned |
| 6 | Extensibility | Plugins, automations | Planned |
| 7 | Shared team use | Sign-in, roles, shared context, attributable audit | Planned |

Discovery and personal organization are separate releases, as are searching and schema validation. Each major version delivers a complete user outcome that can be planned and verified independently. Operational tools and later releases move back in the sequence to make room for those boundaries and the durable workspace capabilities.

## Version 0 — Skeleton (current)

A running application with a single status view. No cluster connections, no Kafka operations.

## Version 1 — Connect and inspect

A developer can save a connection to a live cluster, find a topic, and inspect its messages. Connection management and inspection ship together so the first release completes a useful Kafka workflow.

| Capability | Description |
| --- | --- |
| `connect` | Save, test, edit, and remove cluster connections. Replace protected credentials without revealing saved values. Distinguish connection, authentication, and permission failures. Environments, clusters, and topics are first-class, switchable context — every action states what it targets. |
| `discover` | List topics in the selected cluster and find a topic by name. See partitions and offset ranges. Request bounded timestamp inspection rather than treating exact oldest/newest message ages as always-available metadata. |
| `read` | Preview messages from a topic. Filter by offset, timestamp, partition, and key. Reads are bounded, cancellable, and never commit consumer offsets or disturb a running consumer group. |
| `inspect` | Inspect message keys, values, headers, partition, offset, and timestamp. Distinguish tombstones, empty values, binary data, and decoding failures. Unsupported encodings remain visibly undecoded, not misrepresented as text. |

Saved connections belong to a stable local personal workspace and survive application restarts and upgrades. Switching clusters must not leave a stale selection that silently targets another cluster. Removing a connection makes the effect on saved references explicit. Timestamp inspection accounts for empty partitions, compaction, and producer-provided timestamps; it does not promise an exact measure of how long records have been stored.

**Done when:** a local user can save connections to dev, staging, and production, restart Kaffeine, switch between them, find a topic, and inspect representative messages without leaving the browser or affecting the running deployment.

## Version 2 — Personal workspace

Make large deployments manageable without repeatedly rebuilding the same working context.

| Capability | Description |
| --- | --- |
| `organize` | Pin environments, clusters, and topics. Group topic references into personal folders without renaming or changing Kafka topics. References distinguish the same topic name in different clusters; missing topics remain visibly unavailable. |
| `recent` | Reopen recently accessed topics and pages. Recent activity is bounded, expires, and can be cleared or disabled. |
| `preferences` | Remember display preferences, selected columns, and reading settings. Topic-specific settings stay scoped to their connection and topic. |
| `presets` | Name, reopen, edit, and delete saved reading-filter presets. Explicitly saved presets remain until deleted rather than expiring with activity history. |
| `restore` | Restore useful page context and open views across restarts without automatically starting reads, searches, or other operations. |

Organization and preferences are personal, durable state, not temporary login-session state. Restoring context does not retain message payloads automatically and never silently redirects missing resources to another connection or topic.

**Done when:** a user can organize a large deployment, revisit recent topics, reuse reading presets, and return to their working context after a restart without automatically rerunning reads.

## Version 3 — Search and revisit

Locate messages at scale, understand the limits of the investigation, and save useful searches for explicit reuse.

| Capability | Description |
| --- | --- |
| `search` | Find messages by key, header, or body content within explicit topic, partition, and offset/time bounds. Searches are bounded and cancellable. State supported encodings and decoding limitations rather than implying every binary payload can be searched as text. |
| `coverage` | Show search progress, partitions and ranges scanned, limits reached, and whether results are complete, cancelled, or partial. “No matches in the scanned range” must not imply that the entire topic has no matches. |
| `saved_searches` | Name, reopen, edit, and delete searches scoped to their connection and topic. Reopening does not execute them automatically; saved searches remain until deleted. |
| `history` | Optionally retain private, bounded, expiring search and filter history, with clear and disable controls. Treat query values as sensitive; do not automatically retain matching message payloads. |

**Done when:** a user can locate messages, understand exactly what was searched and what remains unknown, and explicitly rerun a saved investigation after restarting Kaffeine.

## Version 4 — Decode and validate

Interpret schema-encoded records and check whether their contents conform to an identified schema.

| Capability | Description |
| --- | --- |
| `schema_registry` | Save and manage Schema Registry connections where needed, protecting credentials without revealing saved values. Make the registry used for an investigation explicit. |
| `decode` | Decode supported schema-encoded messages and show the schema and version used. Distinguish an unsupported encoding or unavailable schema from a decoding failure. |
| `validate` | Validate message bodies against JSON Schema, Avro, or Protobuf schemas. Use a schema/version identified by the record where available, or one explicitly selected by the user, including schemas supplied without a registry. Distinguish validation failure from an inability to decode or validate. |

Do not assume that the latest schema is the correct schema for historical records. Inspection and validation remain bounded and cancellable.

**Done when:** a user can inspect supported schema-encoded messages, identify the schema/version used, and distinguish invalid data from missing schema information or decoding failures.

## Version 5 — Operational tools

Tools that change or destroy data. They need reviewed scope, explicit confirmation, and a clear account of what cannot be undone before they are safe to ship.

| Capability | Description |
| --- | --- |
| `configure` | Change topic settings such as retention and compression, or explicitly increase partition counts. Partition increases cannot be reversed and can affect key-to-partition routing and ordering assumptions. |
| `purge` | Delete records before reviewed offset boundaries in selected partitions. Records produced after those boundaries remain; the operation does not promise an empty topic or immediate physical erasure. Not reversible. |
| `review` | Show current and proposed settings or deletion boundaries, affected scope, and irreversible consequences before execution. Detect relevant changes since review and require another review instead of silently applying a stale proposal. |
| `outcomes` | Report success, failure, partial completion, or an unknown outcome. Cancelling a request does not undo an operation already accepted by Kafka. |
| `operation_history` | Record the requested operation, its target, time, and known outcome locally, without credentials or message payloads. Named-user attribution arrives with shared team use. |

Configuration and deletion tools are disabled by default. Every operation states its environment, cluster, topic, and applicable partition scope before it runs.

Per-user authorization does not exist yet at this point in the sequence, and confirmation in the interface is not authorization. These tools therefore assume a single operator on a local deployment; a shared deployment is not offered until Version 7.

**Done when:** an operator can review the exact change, execute it explicitly, and revisit what is known to have happened, including partial completion or uncertain outcomes.

## Version 6 — Extensibility

Kaffeine is an extensible graphical workspace, not a fixed set of screens.

| Capability | Description |
| --- | --- |
| `plugins` | Add tools in your own code. Plugins register alongside the built-ins and share the same environment, cluster, and topic context. |
| `automations` | Scheduled or event-triggered runs of any tool: continuous validation, retention sweeps, health checks. |

Automations run tools that can change data, so they land after those tools exist and after automation ownership is coordinated across replicas — running the same automation in every replica of a shared deployment is not safe by default.

## Version 7 — Shared team use

The deployment mode where Kaffeine serves a team rather than one person at a time: each person signs in as themselves, is granted only what their role allows over each environment, and leaves a record of what they did.

| Capability | Description |
| --- | --- |
| `sign_in` | Sign in through the team's identity provider. A shared deployment has no anonymous mode — every action belongs to a named user. |
| `authorize` | Roles and per-user permissions per environment and cluster: who may read, who may change settings, who may delete records. Granted in Kaffeine, enforced on the backend, independent of the credentials used to reach the broker. |
| `share` | Explicitly share selected connections, folders, searches, and views with the team. Personal organization, preferences, and activity history remain private by default. Shared resources have an owner and defined visibility. |
| `audit` | Extend local operation history with named-user attribution: who ran what, when, and against which environment, cluster, topic, and partition scope, with the outcome. Destructive and configuration operations are always recorded, without credentials or message payloads. |

Authentication is deferred because shared deployment is deferred, not because authorization or audit require a finished toolset. Permissions and audit coverage must evolve as tools are added.

Sign-in and per-user authorization appear here and nowhere earlier. `configure` and `purge` ship in Version 5 without them, on the assumption of a single operator on a local deployment. Personal ownership begins in Version 1 and local operation history in Version 5; this version adds team access rules and attributable audit. Concurrent use is a first-class condition — people reading and operating on the same cluster at the same time must not see or disturb each other's private state.

**Done when:** a team can point one deployment at their identity provider, grant each person the access their role allows, share the context worth sharing, and reconstruct who changed what.

## Across every version

- **Shared team deployments.** Authentication and user-level authorization arrive in Version 7 and are required before any shared deployment is offered. Versions 0-6 have no per-user authorization at all, so they are for local single-user use only — never a shared deployment. When Version 7 lands, authorization is enforced on the backend: confirmation in the interface is not authorization, and a shared Kafka service account is not user-level access control.
- **Deployment.** One application package that works locally and in a shared Kubernetes deployment, with configuration and secrets supplied from outside and saved state kept outside the package. Packaging is not yet exercised; it is not described as working until it is.
- **Trust.** Every tool is read-first, explicit about scope, and reversible where it can be. Reads and searches are bounded and cancellable; no unbounded buffering anywhere a user can trigger it.
- **Saved state and privacy.** Personal ownership starts with saved connections in Version 1. Explicitly saved work survives restarts and upgrades; automatic activity history is optional, bounded, expiring, and clearable. Credentials are protected and never revealed in read views, history, or exports. Restored views do not automatically execute operations.

## Keeping this document honest

- Planned capabilities live here. `README.md` describes only what a user can do today, in product terms, with no implementation detail (see `AGENTS.md`).
- When a capability ships, write its description into `README.md` in the same change, and mark it Shipped here. Entries stay in this document so the sequence reads as history.
- Update this file when a capability is added, dropped, reordered, or rescoped, and say why in the change.
- Status values: `Planned`, `In progress`, `Shipped in vN`.

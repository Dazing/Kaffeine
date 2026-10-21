# Kaffeine

Kafka developer toolkit for managing, debugging, and operating Kafka.

## Purpose

Give developers the visibility and control to manage, debug, and operate a Kafka deployment confidently — across many environments, clusters, and topics.

Two problems drive the design:

- **Scale.** Hand-checking topics across dev, staging, and production does not work at hundreds of clusters. Kaffeine keeps environments, clusters, and topics as first-class, switchable context, so one command runs where you intend.
- **Trust.** Operating a live deployment means acting on evidence, not guesses. Every tool is read-first, explicit about scope, and reversible where it can be.

Kaffeine is a platform, not a fixed command set. Automations, plugins, and the built-in toolkit are all extensible.

## Core Tools

| Tool | Description |
| --- | --- |
| `read` | Read messages from a topic. Filter by offset, timestamp, partition, and key. |
| `search` | Find specific messages in a topic by key, header, or body content. |
| `validate` | Validate message bodies against a schema (JSON Schema, Avro, Protobuf). |

## Optional Tools

Disabled by default. These change or destroy data.

| Tool | Description |
| --- | --- |
| `purge` | Empty a topic. |
| `configure` | Change topic settings: partitions, retention, compression. |

## Extensible

| Extension | Description |
| --- | --- |
| `automations` | Scheduled or event-triggered runs of any tool: continuous validation, retention sweeps, health checks. |
| `plugins` | Add tools in your own code. Plugins register alongside the built-ins and share the same context. |

## Status

Early development. No releases yet.

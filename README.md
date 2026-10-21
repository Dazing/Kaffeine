# Kaffeine

Browser-based graphical toolkit for managing, debugging, and operating Kafka.

## Purpose

Give developers the visibility and control to manage, debug, and operate a Kafka deployment confidently through a web UI — across many environments, clusters, and topics. Designed for individual developers working locally and teams sharing a deployment.

Two problems drive the design:

- **Scale.** Hand-checking topics across dev, staging, and production does not work at hundreds of clusters. Kaffeine keeps environments, clusters, and topics as first-class, switchable context, so every action targets the intended deployment.
- **Trust.** Operating a live deployment means acting on evidence, not guesses. Every tool is read-first, explicit about scope, and reversible where it can be.

Kaffeine is an extensible graphical workspace, not a fixed set of screens — the built-in toolkit, automations, and plugins are all designed to be extended.

## What exists today

The application runs and shows its own status. It does not connect to Kafka yet — no topic browsing, no message tools. The first release adds cluster connections, topic discovery, and message preview.

## Roadmap

The planned capabilities and the order they arrive in: [docs/roadmap.md](docs/roadmap.md).

## Status

Early development. No releases yet.

import { ApplicationStatus } from '../features/application';

export function App() {
  return (
    <>
      <a className="skip-link" href="#main">Skip to content</a>
      <header className="app-header">
        <a className="brand" href="/" aria-label="Kaffeine home">Kaffeine</a>
        <span className="release-label">Early development</span>
      </header>
      <main id="main" className="workspace">
        <div className="page-heading">
          <p className="eyebrow">Workspace overview</p>
          <h1>Your Kafka workspace</h1>
          <p className="intro">Visibility and control across environments, clusters, and topics.</p>
        </div>
        <section className="panel" aria-labelledby="workspace-title">
          <h2 id="workspace-title">A starting point for Kaffeine</h2>
          <p>This build contains the application shell. Kafka connections and message tools are not available yet.</p>
          <p className="muted">No broker connections are opened and no Kafka data is read or changed.</p>
        </section>
        <ApplicationStatus />
      </main>
    </>
  );
}

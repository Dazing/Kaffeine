import { useApplicationStatus } from './use-application-status';

export function ApplicationStatus() {
  const { state, refresh } = useApplicationStatus();

  return (
    <section className="panel" aria-labelledby="status-title">
      <div className="section-heading">
        <h2 id="status-title">Application connection</h2>
        <button type="button" onClick={refresh} disabled={state.kind === 'loading'}>
          {state.kind === 'error' ? 'Retry connection' : 'Check again'}
        </button>
      </div>
      <div className="connection-status" role="status" aria-live="polite">
        {state.kind === 'loading' && <p>Connecting to the backend…</p>}
        {state.kind === 'error' && <p className="error">Unable to reach the application backend. Check that it is running, then retry.</p>}
        {state.kind === 'ready' && (
          <p><span className="status-dot" aria-hidden="true" />Connected to {state.name} <span className="status-badge">{state.status}</span></p>
        )}
      </div>
      <p className="muted">This checks the application backend, not the health of a Kafka cluster.</p>
    </section>
  );
}

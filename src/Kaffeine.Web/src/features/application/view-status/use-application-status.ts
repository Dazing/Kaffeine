import { useEffect, useState } from 'react';

type StatusState =
  | { kind: 'loading' }
  | { kind: 'ready'; name: string; status: string }
  | { kind: 'error' };

export function useApplicationStatus() {
  const [state, setState] = useState<StatusState>({ kind: 'loading' });
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    const controller = new AbortController();

    async function load() {
      try {
        const response = await fetch('/api/application/status', { signal: controller.signal });
        if (!response.ok) throw new Error('Application status request failed.');
        const data: unknown = await response.json();
        if (
          typeof data !== 'object' || data === null ||
          !('name' in data) || typeof data.name !== 'string' ||
          !('status' in data) || typeof data.status !== 'string'
        ) throw new Error('Unexpected application status response.');
        if (!controller.signal.aborted) {
          setState({ kind: 'ready', name: data.name, status: data.status });
        }
      } catch {
        if (!controller.signal.aborted) setState({ kind: 'error' });
      }
    }

    void load();
    return () => controller.abort();
  }, [attempt]);

  function refresh() {
    setState({ kind: 'loading' });
    setAttempt((previous) => previous + 1);
  }

  return { state, refresh };
}

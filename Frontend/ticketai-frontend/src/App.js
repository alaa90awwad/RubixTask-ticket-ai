import React, { useState, useMemo } from 'react';
import { CATEGORIES } from './types';
import { useLocalStorage } from './hooks/useLocalStorage';

function App() {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [suggestion, setSuggestion] = useState(null);
  const [category, setCategory] = useState('General Feedback');
  const [tickets, setTickets] = useLocalStorage('tickets', []);
  const [loadingSuggest, setLoadingSuggest] = useState(false);
  const [loadingSubmit, setLoadingSubmit] = useState(false);
  const [errorSuggest, setErrorSuggest] = useState(null);
  const [errorSubmit, setErrorSubmit] = useState(null);

  const valid = useMemo(() => title.trim() && description.trim(), [title, description]);

  async function onSuggest() {
    setErrorSuggest(null);
    setLoadingSuggest(true);
    try {
      const res = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/suggest`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ title, description })
      });
      if (!res.ok) throw new Error(await res.text());
      const data = await res.json();
      setSuggestion(data.category);
      setCategory(data.category);
    } catch (e) {
      setErrorSuggest(e.message || 'Failed to get suggestion');
    } finally {
      setLoadingSuggest(false);
    }
  }

  async function onSubmit() {
    setErrorSubmit(null);
    setLoadingSubmit(true);
    try {
      const ticket = {
        id: crypto.randomUUID(),
        title,
        description,
        category,
        createdAt: new Date().toISOString()
      };
      setTickets([ticket, ...tickets]);
      setTitle('');
      setDescription('');
      setSuggestion(null);
      setCategory('General Feedback');
    } catch (e) {
      setErrorSubmit(e.message || 'Failed to submit');
    } finally {
      setLoadingSubmit(false);
    }
  }

  return (
    <div className="container my-4">
      <h1 className="h3 fw-bold mb-4">AI Support Ticket Categorizer</h1>

      <div className="card mb-4">
        <div className="card-body">
          <div className="mb-3">
            <label className="form-label">Title<span className="text-danger">*</span></label>
            <input
              className="form-control"
              value={title}
              onChange={e => setTitle(e.target.value)}
              placeholder="e.g., App crashes on save"
            />
          </div>

          <div className="mb-3">
            <label className="form-label">Description<span className="text-danger">*</span></label>
            <textarea
              className="form-control"
              rows="4"
              value={description}
              onChange={e => setDescription(e.target.value)}
              placeholder="After clicking Save on the profile page, the app closes."
            />
          </div>

          <div className="d-flex align-items-center gap-2 mb-3">
            <button
              onClick={onSuggest}
              disabled={!valid || loadingSuggest}
              className="btn btn-dark"
            >
              {loadingSuggest ? 'Suggesting…' : 'Suggest category'}
            </button>
            {errorSuggest && <span className="text-danger small">{errorSuggest}</span>}
          </div>

          {suggestion && (
            <div className="alert alert-secondary py-2">
              <strong>Suggested:</strong> {suggestion}
            </div>
          )}

          <div className="mb-3">
            <label className="form-label">Override category</label>
            <select
              className="form-select"
              value={category}
              onChange={e => setCategory(e.target.value)}
            >
              {CATEGORIES.map(c => (
                <option key={c} value={c}>{c}</option>
              ))}
            </select>
          </div>

          <div className="d-flex align-items-center gap-2">
            <button
              onClick={onSubmit}
              disabled={!valid || loadingSubmit}
              className="btn btn-primary"
            >
              {loadingSubmit ? 'Submitting…' : 'Submit ticket'}
            </button>
            {errorSubmit && <span className="text-danger small">{errorSubmit}</span>}
          </div>
        </div>
      </div>

      <section>
        <h2 className="h5 mb-2">Submitted Tickets</h2>
        <div className="vstack gap-2">
          {tickets.map(t => (
            <div key={t.id} className="card">
              <div className="card-body">
                <div className="d-flex justify-content-between">
                  <span className="fw-semibold">{t.title}</span>
                  <span className="text-muted small">
                    {new Date(t.createdAt).toLocaleString()}
                  </span>
                </div>
                <div className="small text-muted">{t.description}</div>
                <span className="badge bg-light text-dark mt-2">{t.category}</span>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}

export default App;
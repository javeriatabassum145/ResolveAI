
import { useCallback, useEffect, useMemo, useState } from "react";
import {
  getPayments,
  getPayment,
  investigatePayment,
} from "./api";


const money = (amount, currency = "EUR") => {
  const value = Number(amount ?? 0);

  return new Intl.NumberFormat("en-IE", {
    style: "currency",
    currency: currency || "EUR",
  }).format(Number.isFinite(value) ? value : 0);
};

const read = (obj, ...keys) => {
  for (const key of keys) {
    if (obj?.[key] !== undefined && obj?.[key] !== null) {
      return obj[key];
    }
  }
  return undefined;
};

const getId = (payment) =>
  read(payment, "id", "paymentId", "Id", "PaymentId");

const getAmount = (payment) =>
  read(payment, "amount", "Amount", "total", "Total") ?? 0;

const getCurrency = (payment) =>
  read(payment, "currency", "Currency") ?? "EUR";

const getStatus = (payment) =>
  String(read(payment, "status", "Status") ?? "UNKNOWN");

const getPayPalId = (payment) =>
  read(payment, "paypalOrderId", "payPalOrderId", "PayPalOrderId",
    "paypalPaymentId", "PayPalPaymentId", "orderId", "OrderId") ??
  `PAY-${getId(payment) ?? "UNKNOWN"}`;

const getRisk = (result) =>
  String(read(result, "riskLevel", "RiskLevel") ?? "UNKNOWN").toUpperCase();

const getScore = (result) =>
  Number(read(result, "riskScore", "RiskScore") ?? 0);

function StatusBadge({ status }) {
  const normalized = String(status || "UNKNOWN").toLowerCase();
  let type = "neutral";

  if (
    normalized.includes("completed") ||
    normalized.includes("success") ||
    normalized.includes("approved")
  ) {
    type = "success";
  } else if (
    normalized.includes("pending") ||
    normalized.includes("processing")
  ) {
    type = "warning";
  } else if (
    normalized.includes("failed") ||
    normalized.includes("denied") ||
    normalized.includes("cancel") ||
    normalized.includes("high")
  ) {
    type = "danger";
  }

  return (
    <span className={`badge badge-${type}`}>
      {String(status || "Unknown").replaceAll("_", " ")}
    </span>
  );
}

function RiskGauge({ score = 0 }) {
  const safeScore = Math.min(100, Math.max(0, Number(score) || 0));

  return (
    <div className="risk-gauge">
      <div
        className="risk-gauge-ring"
        style={{ "--score": `${safeScore * 3.6}deg` }}
      >
        <div className="risk-gauge-center">
          <strong>{safeScore}</strong>
          <span>/ 100</span>
        </div>
      </div>
      <span className="muted small">AI risk score</span>
    </div>
  );
}

function MetricCard({ label, value, detail, icon }) {
  return (
    <article className="metric-card">
      <div className="metric-top">
        <span className="metric-label">{label}</span>
        <span className="metric-icon">{icon}</span>
      </div>
      <div className="metric-value">{value}</div>
      <div className="metric-detail">{detail}</div>
    </article>
  );
}

function EmptyState({ title, description }) {
  return (
    <div className="empty-state">
      <div className="empty-icon">⌕</div>
      <strong>{title}</strong>
      <p>{description}</p>
    </div>
  );
}

export default function App() {
  const [payments, setPayments] = useState([]);
  const [selectedId, setSelectedId] = useState("");
  const [selectedPayment, setSelectedPayment] = useState(null);
  const [question, setQuestion] = useState(
    "Investigate this payment for anomalies, refund discrepancies, and potential fraud. Explain the evidence and recommend the next action."
  );
  const [investigation, setInvestigation] = useState(null);
  const [loadingPayments, setLoadingPayments] = useState(true);
  const [loadingPayment, setLoadingPayment] = useState(false);
  const [investigating, setInvestigating] = useState(false);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [search, setSearch] = useState("");

  const loadPayments = useCallback(async () => {
    setLoadingPayments(true);
    setError("");

    try {
      const data = await getPayments();
      const list = Array.isArray(data)
        ? data
        : read(data, "items", "payments", "Payments", "data") ?? [];

      if (!Array.isArray(list)) {
        throw new Error(
          "The payments API did not return a list. Check GET /api/Payments in Swagger."
        );
      }

      setPayments(list);

      setSelectedId((current) => {
        if (current && list.some((p) => String(getId(p)) === String(current))) {
          return current;
        }
        return list.length ? String(getId(list[0])) : "";
      });
    } catch (err) {
      setError(
        `Could not load payments. ${err.message}. Check that your API is running and the endpoint in src/api.js matches Swagger.`
      );
    } finally {
      setLoadingPayments(false);
    }
  }, []);

  useEffect(() => {
    loadPayments();
  }, [loadPayments]);

  useEffect(() => {
    if (!selectedId) {
      setSelectedPayment(null);
      setInvestigation(null);
      return;
    }

    let cancelled = false;

    async function loadSelectedPayment() {
      setLoadingPayment(true);
      setInvestigation(null);
      setNotice("");

      const fromList = payments.find(
        (p) => String(getId(p)) === String(selectedId)
      );

      try {
        const detail = await getPayment(selectedId);
        if (!cancelled) {
          setSelectedPayment(detail ?? fromList ?? null);
        }
      } catch {
        // The list endpoint may already contain the full payment object.
        if (!cancelled) {
          setSelectedPayment(fromList ?? null);
        }
      } finally {
        if (!cancelled) setLoadingPayment(false);
      }
    }

    loadSelectedPayment();

    return () => {
      cancelled = true;
    };
  }, [selectedId, payments]);

  const visiblePayments = useMemo(() => {
    const query = search.trim().toLowerCase();

    if (!query) return payments;

    return payments.filter((payment) => {
      const values = [
        getId(payment),
        getPayPalId(payment),
        getStatus(payment),
        getCurrency(payment),
        getAmount(payment),
      ];

      return values.some((value) =>
        String(value ?? "").toLowerCase().includes(query)
      );
    });
  }, [payments, search]);

  const totalVolume = useMemo(
    () =>
      payments.reduce((sum, payment) => {
        const amount = Number(getAmount(payment));
        return sum + (Number.isFinite(amount) ? amount : 0);
      }, 0),
    [payments]
  );

  const completedCount = payments.filter((p) =>
    getStatus(p).toLowerCase().includes("completed")
  ).length;

  const pendingCount = payments.filter((p) =>
    getStatus(p).toLowerCase().includes("pending")
  ).length;

  const selectedRefunds =
    read(selectedPayment, "refunds", "Refunds") ?? [];

  const selectedEvents =
    read(selectedPayment, "events", "Events") ?? [];

  const totalRefunded = Array.isArray(selectedRefunds)
    ? selectedRefunds.reduce((sum, refund) => {
        const amount = Number(read(refund, "amount", "Amount") ?? 0);
        return sum + (Number.isFinite(amount) ? amount : 0);
      }, 0)
    : 0;

  async function handleInvestigate(event) {
    event.preventDefault();

    if (!selectedId) {
      setError("Select a payment before starting an investigation.");
      return;
    }

    if (!question.trim()) {
      setError("Enter a question for the AI investigation.");
      return;
    }

    setInvestigating(true);
    setError("");
    setNotice("");
    setInvestigation(null);

    try {
      const result = await investigatePayment(selectedId, question.trim());
      setInvestigation(result);
      setNotice("Investigation completed. Review the findings before taking action.");
    } catch (err) {
      setError(
        `Investigation failed: ${err.message}. Verify the POST /api/Investigations route and request body in Swagger.`
      );
    } finally {
      setInvestigating(false);
    }
  }

  const riskLevel = investigation ? getRisk(investigation) : "—";
  const riskScore = investigation ? getScore(investigation) : 0;

  const summary = investigation
    ? read(investigation, "summary", "Summary")
    : "";

  const rootCause = investigation
    ? read(investigation, "rootCause", "RootCause")
    : "";

  const recommendation = investigation
    ? read(investigation, "recommendation", "Recommendation")
    : "";

  const confidence = investigation
    ? read(investigation, "confidence", "Confidence")
    : null;

  const evidence = investigation
    ? read(investigation, "evidence", "Evidence") ?? []
    : [];

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-mark">R</div>
          <div>
            <strong>ResolveAI</strong>
            <span>Payment intelligence</span>
          </div>
        </div>

        <div className="sidebar-section-label">WORKSPACE</div>
        <button className="nav-item active" type="button">
          <span>▦</span> Payment forensics
        </button>
        <button
          className="nav-item"
          type="button"
          onClick={() => {
            setInvestigation(null);
            setNotice("");
          }}
        >
          <span>⌁</span> Investigations
        </button>

        <div className="sidebar-bottom">
          <div className="connection-indicator">
            <span className="live-dot" />
            <div>
              <strong>ResolveAI API</strong>
              <span>Connection status updates on load</span>
            </div>
          </div>
          <div className="sidebar-footer">AI-assisted payment analysis</div>
        </div>
      </aside>

      <main className="main-content">
        <header className="topbar">
          <div>
            <div className="eyebrow">PAYMENTS / FORENSICS</div>
            <h1>Payment intelligence</h1>
            <p className="page-subtitle">
              Investigate payment anomalies with AI-powered evidence.
            </p>
          </div>

          <button
            className="button button-secondary"
            type="button"
            onClick={loadPayments}
            disabled={loadingPayments}
          >
            <span>{loadingPayments ? "↻" : "⟳"}</span>
            {loadingPayments ? "Refreshing..." : "Refresh data"}
          </button>
        </header>

        {error && (
          <div className="alert alert-error" role="alert">
            <span>!</span>
            <div>
              <strong>Something needs attention</strong>
              <p>{error}</p>
            </div>
            <button
              type="button"
              className="alert-close"
              onClick={() => setError("")}
              aria-label="Dismiss error"
            >
              ×
            </button>
          </div>
        )}

        {notice && (
          <div className="alert alert-success" role="status">
            <span>✓</span>
            <p>{notice}</p>
            <button
              type="button"
              className="alert-close"
              onClick={() => setNotice("")}
              aria-label="Dismiss notice"
            >
              ×
            </button>
          </div>
        )}

        <section className="metrics-grid">
          <MetricCard
            label="Total payments"
            value={loadingPayments ? "…" : payments.length}
            detail="Payments returned by your API"
            icon="◈"
          />
          <MetricCard
            label="Completed"
            value={loadingPayments ? "…" : completedCount}
            detail="Status marked completed"
            icon="✓"
          />
          <MetricCard
            label="Pending"
            value={loadingPayments ? "…" : pendingCount}
            detail="Awaiting final status"
            icon="◷"
          />
          <MetricCard
            label="Payment volume"
            value={
              loadingPayments
                ? "…"
                : money(
                    totalVolume,
                    payments[0] ? getCurrency(payments[0]) : "EUR"
                  )
            }
            detail="Sum of listed payment amounts"
            icon="€"
          />
        </section>

        <section className="workspace-grid">
          <div className="panel payments-panel">
            <div className="panel-heading">
              <div>
                <div className="eyebrow">TRANSACTION DATA</div>
                <h2>Payments</h2>
              </div>
              <span className="count-pill">{payments.length}</span>
            </div>

            <div className="search-box">
              <span>⌕</span>
              <input
                aria-label="Search payments"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Search ID, status, amount..."
              />
            </div>

            {loadingPayments ? (
              <div className="loading-state">
                <div className="spinner" />
                Loading payments...
              </div>
            ) : visiblePayments.length === 0 ? (
              <EmptyState
                title={payments.length ? "No matching payments" : "No payments found"}
                description={
                  payments.length
                    ? "Try a different search term."
                    : "Check the API response or create a sandbox payment."
                }
              />
            ) : (
              <div className="payment-list">
                {visiblePayments.map((payment) => {
                  const id = getId(payment);
                  const isSelected = String(id) === String(selectedId);

                  return (
                    <button
                      key={String(id)}
                      type="button"
                      className={`payment-row ${isSelected ? "selected" : ""}`}
                      onClick={() => setSelectedId(String(id))}
                    >
                      <div className="payment-row-top">
                        <span className="payment-id">Payment #{id}</span>
                        <StatusBadge status={getStatus(payment)} />
                      </div>
                      <div className="payment-row-bottom">
                        <span className="muted">
                          {String(getPayPalId(payment))}
                        </span>
                        <strong>
                          {money(getAmount(payment), getCurrency(payment))}
                        </strong>
                      </div>
                    </button>
                  );
                })}
              </div>
            )}
          </div>

          <div className="right-column">
            <section className="panel detail-panel">
              <div className="panel-heading">
                <div>
                  <div className="eyebrow">SELECTED TRANSACTION</div>
                  <h2>Payment details</h2>
                </div>
                {selectedPayment && (
                  <StatusBadge status={getStatus(selectedPayment)} />
                )}
              </div>

              {loadingPayment ? (
                <div className="loading-state">
                  <div className="spinner" />
                  Loading payment details...
                </div>
              ) : !selectedPayment ? (
                <EmptyState
                  title="Select a payment"
                  description="Choose a transaction from the list to inspect it."
                />
              ) : (
                <>
                  <div className="amount-block">
                    <span className="muted">Transaction amount</span>
                    <strong>
                      {money(
                        getAmount(selectedPayment),
                        getCurrency(selectedPayment)
                      )}
                    </strong>
                    <span className="transaction-reference">
                      {String(getPayPalId(selectedPayment))}
                    </span>
                  </div>

                  <div className="detail-grid">
                    <div className="detail-cell">
                      <span>Internal ID</span>
                      <strong>{String(getId(selectedPayment))}</strong>
                    </div>
                    <div className="detail-cell">
                      <span>Currency</span>
                      <strong>{String(getCurrency(selectedPayment))}</strong>
                    </div>
                    <div className="detail-cell">
                      <span>Refund records</span>
                      <strong>
                        {Array.isArray(selectedRefunds)
                          ? selectedRefunds.length
                          : 0}
                      </strong>
                    </div>
                    <div className="detail-cell">
                      <span>Event records</span>
                      <strong>
                        {Array.isArray(selectedEvents)
                          ? selectedEvents.length
                          : 0}
                      </strong>
                    </div>
                    <div className="detail-cell full-width">
                      <span>Total recorded refunds</span>
                      <strong>
                        {money(totalRefunded, getCurrency(selectedPayment))}
                      </strong>
                    </div>
                  </div>
                </>
              )}
            </section>

            <section className="panel investigate-panel">
              <div className="panel-heading">
                <div>
                  <div className="eyebrow">AI FORENSICS ENGINE</div>
                  <h2>Investigate payment</h2>
                </div>
                <span className="ai-chip">✦ AI</span>
              </div>

              <p className="panel-description">
                Ask ResolveAI to explain anomalies, examine refund activity,
                and recommend the next step.
              </p>

              <form onSubmit={handleInvestigate}>
                <label htmlFor="investigation-question">
                  Investigation question
                </label>
                <textarea
                  id="investigation-question"
                  rows={4}
                  value={question}
                  onChange={(event) => setQuestion(event.target.value)}
                  placeholder="What should the AI investigate?"
                  disabled={investigating}
                />

                <button
                  className="button button-primary full-button"
                  type="submit"
                  disabled={!selectedId || investigating || !question.trim()}
                >
                  {investigating ? (
                    <>
                      <span className="spinner spinner-light" />
                      Analysing payment...
                    </>
                  ) : (
                    <>
                      <span>✦</span>
                      Run AI investigation
                    </>
                  )}
                </button>
              </form>

              <div className="privacy-note">
                <span>ⓘ</span>
                AI findings are decision support, not proof of fraud.
              </div>
            </section>
          </div>
        </section>

        {investigation && (
          <section className="panel results-panel">
            <div className="panel-heading results-heading">
              <div>
                <div className="eyebrow">INVESTIGATION REPORT</div>
                <h2>AI findings</h2>
                <p className="panel-description">
                  Results for payment #{selectedId}
                </p>
              </div>
              <StatusBadge status={riskLevel} />
            </div>

            <div className="results-grid">
              <div className="risk-card">
                <RiskGauge score={riskScore} />
                <div className="risk-label">
                  <StatusBadge status={`${riskLevel} RISK`} />
                  <p>
                    {riskScore >= 70
                      ? "Requires careful manual review"
                      : riskScore >= 35
                        ? "Review the evidence and context"
                        : "No strong risk signal identified"}
                  </p>
                </div>
              </div>

              <div className="finding-card">
                <div className="finding-title">
                  <span className="finding-icon">◉</span>
                  <h3>Summary</h3>
                </div>
                <p>{summary || "The API did not return a summary."}</p>
              </div>

              <div className="finding-card">
                <div className="finding-title">
                  <span className="finding-icon">⌕</span>
                  <h3>Potential root cause</h3>
                </div>
                <p>{rootCause || "No root cause was returned."}</p>
              </div>

              <div className="finding-card recommendation-card">
                <div className="finding-title">
                  <span className="finding-icon">↗</span>
                  <h3>Recommended next action</h3>
                </div>
                <p>
                  {recommendation ||
                    "Review the payment records manually before taking action."}
                </p>
              </div>

              <div className="finding-card evidence-card">
                <div className="finding-title">
                  <span className="finding-icon">☷</span>
                  <h3>Evidence</h3>
                </div>

                {Array.isArray(evidence) && evidence.length > 0 ? (
                  <ul className="evidence-list">
                    {evidence.map((item, index) => (
                      <li key={`${index}-${String(item)}`}>
                        <span>✓</span>
                        {typeof item === "string"
                          ? item
                          : JSON.stringify(item)}
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p>No evidence list was returned by the API.</p>
                )}

                {confidence !== null && confidence !== undefined && (
                  <div className="confidence-row">
                    <span>AI confidence</span>
                    <strong>
                      {Number(confidence) <= 1
                        ? `${(Number(confidence) * 100).toFixed(0)}%`
                        : `${Number(confidence).toFixed(0)}%`}
                    </strong>
                  </div>
                )}
              </div>
            </div>

            <div className="results-footer">
              <span>ResolveAI · AI-generated analysis</span>
              <button
                type="button"
                className="button button-secondary"
                onClick={() => {
                  const report = {
                    paymentId: selectedId,
                    riskLevel,
                    riskScore,
                    summary,
                    rootCause,
                    recommendation,
                    evidence,
                    confidence,
                  };

                  const blob = new Blob(
                    [JSON.stringify(report, null, 2)],
                    { type: "application/json" }
                  );
                  const url = URL.createObjectURL(blob);
                  const link = document.createElement("a");
                  link.href = url;
                  link.download = `resolveai-investigation-${selectedId}.json`;
                  link.click();
                  URL.revokeObjectURL(url);
                }}
              >
                Export report ↓
              </button>
            </div>
          </section>
        )}

        <footer className="app-footer">
          <span>ResolveAI</span>
          <span>PayPal Sandbox · AI Payment Forensics</span>
        </footer>
      </main>
    </div>
  );
}
import { useEffect, useState } from "react";
import "./App.css";

interface HealthResponse {
  status: string;
  message: string;
}

function App() {
  const [backendStatus, setBackendStatus] = useState("Checking...");
  const [error, setError] = useState("");

  useEffect(() => {
    fetch("https://localhost:7120/api/health")
      .then((response) => {
        if (!response.ok) {
          throw new Error("Backend request failed");
        }

        return response.json();
      })
      .then((data: HealthResponse) => {
        setBackendStatus(data.message);
      })
      .catch((error) => {
        console.error(error);
        setBackendStatus("Backend connection failed");
        setError("Please make sure the .NET API is running.");
      });
  }, []);

  return (
    <div className="app">
      <div className="card">
        <h1>
          Resolve<span>AI</span>
        </h1>

        <p className="subtitle">
          AI Payment Forensics
        </p>

        <div className="status">
          <span className="status-dot"></span>

          <strong>{backendStatus}</strong>
        </div>

        {error && (
          <p className="error">
            {error}
          </p>
        )}

        <p className="description">
          When a payment doesn't make sense,
          ResolveAI investigates why.
        </p>
      </div>
    </div>
  );
}

export default App;
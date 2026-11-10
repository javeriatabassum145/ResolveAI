# ResolveAI – AI Payment Forensics

**AI-Powered Payment Investigation and Risk Analysis**

ResolveAI is an AI-powered payment forensics application designed to help investigate payment discrepancies, refund irregularities, and potentially suspicious transactions.

By combining PayPal Sandbox payment data with Gemini AI, ResolveAI analyzes transaction evidence, identifies potential risks, explains possible root causes, and recommends appropriate next steps.

## Problem Statement

Online payment systems process large numbers of transactions and refunds. Identifying unusual payment activity, duplicate refunds, partial refunds, and discrepancies can require manual investigation.

This process can be time-consuming and difficult, especially when payment information and refund records need to be reviewed together.

## Our Solution

ResolveAI simplifies payment investigation by combining transaction data, backend analysis, and generative AI.

The application retrieves payment information, analyzes associated refund records, and uses Gemini AI to generate structured investigation reports.

Each report provides:

- Risk level (LOW, MEDIUM, or HIGH)
- Risk score
- Investigation summary
- Possible root cause
- Supporting evidence
- Recommended next action
- AI confidence estimate

ResolveAI is designed to support human decision-making. Its risk assessments are indicators for further investigation, not proof of fraud.

## Key Features

### 1. Payment Dashboard
- View payment records.
- Review transaction amounts and statuses.
- Inspect payment and refund information.

### 2. AI-Powered Investigation
- Investigate individual payments using Gemini AI.
- Submit investigation questions.
- Analyze payment and refund discrepancies.
- Generate structured investigation reports.

### 3. Risk Assessment
- Identify potential payment irregularities.
- Generate a risk level and numerical risk score.
- Provide evidence-based explanations.

### 4. Recommendations
- Explain possible reasons for payment discrepancies.
- Suggest practical next steps.
- Help users decide which transactions need further review.

### 5. Investigation Results
- Display investigation summaries.
- Show supporting evidence.
- Present root causes and recommendations.
- Display the AI confidence estimate.

## Technology Stack

| Component | Technology |
|---|---|
| Frontend | React.js |
| Backend | ASP.NET Core Web API (.NET 8) |
| Database | PostgreSQL |
| ORM | Entity Framework Core |
| Payment Platform | PayPal Sandbox |
| AI Integration | Google Gemini API |
| API Testing | Swagger / OpenAPI |

## System Architecture

```text
             React Frontend
                   |
                   | HTTP Requests
                   v
          ASP.NET Core Web API
                   |
          +--------+--------+
          |                 |
          v                 v
     PostgreSQL        PayPal Sandbox
          |
          v
    Payment & Refund
        Evidence
          |
          v
       Gemini AI
          |
          v
   Investigation Report
          |
          v
      React Dashboard
```

## How It Works

1. Payment records are available through the application.
2. The user selects a payment to investigate.
3. The React frontend sends the payment ID and investigation question to the backend.
4. The ASP.NET Core API retrieves the payment and its associated refund and event information.
5. The backend prepares the transaction evidence.
6. Gemini AI analyzes the supplied evidence and generates a structured investigation report.
7. The backend saves the investigation result.
8. The React dashboard displays the risk assessment, evidence, explanation, and recommendation.

## Example Investigation

**Scenario:** A payment of €149.00 has one completed refund of €99.00.

**AI Investigation Result:**

| Field | Result |
|---|---|
| Payment Amount | €149.00 |
| Refund Amount | €99.00 |
| Remaining Difference | €50.00 |
| Risk Level | MEDIUM |
| Risk Score | 30/100 |
| AI Confidence | 95% |

**Summary:**

The payment has one completed partial refund, leaving a remaining difference of €50.00 that requires further context.

**Recommendation:**

Review merchant billing records or customer support notes to determine whether the partial refund was intentional or whether another refund is pending.

*Note: This is an illustrative investigation result. The remaining balance alone does not establish fraud.*

## Project Structure

```text
ResolveAI/
│
├── ResolveAI.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Services/
│   ├── DTOs/
│   ├── Program.cs
│   └── appsettings.json
│
├── resolveai-frontend/
│   ├── src/
│   │   ├── App.jsx
│   │   ├── api.js
│   │   └── ...
│   ├── public/
│   └── package.json
│
├── screenshots/
├── README.md
└── LICENSE
```

*The folder names above are illustrative. Adjust them to match your actual repository.*

## Getting Started

### Prerequisites

Install the following:

- .NET 8 SDK
- Node.js and npm
- PostgreSQL
- PayPal Developer Sandbox account
- Google Gemini API key

### 1. Clone the Repository

```bash
git clone YOUR_GITHUB_REPOSITORY_URL
cd ResolveAI
```

### 2. Configure the Backend

Open the backend project.

Configure the PostgreSQL connection string and required API settings using local configuration or environment variables.

Example configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ResolveAIDb;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY",
    "Model": "gemini-2.5-flash"
  }
}
```

Configure PayPal Sandbox credentials according to the backend configuration used by the application.

**Security:** Never commit real API keys, passwords, or PayPal secrets to GitHub.

### 3. Run the Backend

Navigate to the backend project directory:

```bash
dotnet restore
dotnet build
dotnet run
```

The API is configured to run locally at:

```text
https://localhost:7120
```

Swagger documentation:

```text
https://localhost:7120/swagger
```

### 4. Run the Frontend

Open another terminal and navigate to the frontend directory:

```bash
npm install
npm run dev
```

Open the React application:

```text
http://localhost:5173
```

### 5. Configure the Database

Make sure PostgreSQL is running and the configured database exists.

Apply the Entity Framework Core migrations if required by the project:

```bash
dotnet ef database update
```

## Example API Operations

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/Payments` | Retrieve payment records |
| GET | `/api/Payments/{id}` | Retrieve a specific payment |
| POST | `/api/Investigations/{paymentId}` | Investigate a payment using AI |

Example investigation request:

```json
{
  "question": "Investigate this payment for refund discrepancies. Explain the evidence and recommend the next action."
}
```

## Future Improvements

- Advanced anomaly detection.
- Historical payment trend analysis.
- More detailed investigation history and reporting.
- Additional payment provider integrations.
- Exportable investigation reports.
- Improved risk assessment using verified transaction patterns.

## Responsible AI

ResolveAI uses AI to assist with payment investigation and explain transaction evidence.

AI-generated findings should be reviewed by a human investigator. A risk score or unusual refund pattern should not independently be treated as confirmation of fraudulent activity.

## Project Information

**Project:** ResolveAI – AI Payment Forensics

**Category:** AI / FinTech / Payment Security

**Technologies:** React, ASP.NET Core, PostgreSQL, PayPal Sandbox, Google Gemini AI

---

*Built for the PayPal AI Hackathon.*

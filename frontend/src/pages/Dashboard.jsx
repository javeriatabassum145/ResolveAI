import { useEffect, useState } from "react";
import {
    getDashboard,
    getPayments
} from "../services/api";

function Dashboard() {

    const [dashboard, setDashboard] = useState(null);
    const [payments, setPayments] = useState([]);

    useEffect(() => {
        loadData();
    }, []);

    async function loadData() {
        try {
            const dashboardData =
                await getDashboard();

            const paymentData =
                await getPayments();

            setDashboard(dashboardData);
            setPayments(paymentData);
        }
        catch (error) {
            console.error(error);
        }
    }

    if (!dashboard) {
        return <div>Loading...</div>;
    }

    return (
        <div>

            <h1>ResolveAI</h1>

            <p>
                AI Payment Forensics
            </p>

            <div>

                <div>
                    <h2>
                        {dashboard.payments}
                    </h2>
                    <p>Payments</p>
                </div>

                <div>
                    <h2>
                        {dashboard.investigations}
                    </h2>
                    <p>Investigations</p>
                </div>

                <div>
                    <h2>
                        {dashboard.anomalies}
                    </h2>
                    <p>Anomalies</p>
                </div>

                <div>
                    <h2>
                        {dashboard.highRisk}
                    </h2>
                    <p>High Risk</p>
                </div>

            </div>

            <h2>Payments</h2>

            {payments.map(payment => (

                <div key={payment.id}>

                    <strong>
                        {payment.payPalOrderId}
                    </strong>

                    <p>
                        {payment.amount}
                        {" "}
                        {payment.currency}
                    </p>

                    <p>
                        Status:
                        {" "}
                        {payment.status}
                    </p>

                </div>

            ))}

        </div>
    );
}

export default Dashboard;
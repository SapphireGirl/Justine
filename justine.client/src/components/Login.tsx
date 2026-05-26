import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ToastContainer, toast, Bounce } from 'react-toastify';
import './css/login.css';

const Login: React.FC = () => {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [showPassword, setShowPassword] = useState(false);
    const navigate = useNavigate();

    // Removed logging from render (renders happen frequently, especially in StrictMode)
    // Only log minimal non-sensitive info in development when submitting.
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        try {
            // Debug-only: do not log the actual password. Log only length/occurrence.
            if (import.meta.env.DEV) {
                console.debug("Submitting login for user:", username, "passwordLength:", password.length);
            }

            const resp = await fetch('/api/Auth/Login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include', // allow HttpOnly cookie set by backend (recommended)
                body: JSON.stringify({
                    username,
                    password
                })
            });

            const body = await resp.json().catch(() => ({}));

            if (!resp.ok) {
                const msg = body?.message || 'Login failed';
                toast.error(msg);
                setError(msg);
                return;
            }

            // Navigate to Product List
            navigate("/productlist");
        } catch (err: unknown) {
            console.error("Login error:", err);
            const msg = err && typeof err === "object" && "message" in err ? (err as { message: string }).message : "Login failed.";
            toast.error(msg);
            setError(msg);
        }
    };

    return (
        <div className="login-page-background">
            <div className="login-container">
                <div>
                    <div>
                        <ToastContainer
                            position="top-center"
                            autoClose={5000}
                            hideProgressBar={false}
                            newestOnTop={false}
                            closeOnClick={false}
                            rtl={false}
                            pauseOnFocusLoss
                            draggable
                            pauseOnHover
                            theme="light"
                            transition={Bounce}
                        />
                    </div>

                    <h2 className="cognito-form-container">Sign in</h2>
                    <p className="cognito-form-description">Sign in to your account.</p>
                    <form onSubmit={handleSubmit}>
                        <div>
                            <label>Username</label>
                            <input
                                type="text"
                                value={username}
                                onChange={e => setUsername(e.target.value)}
                                required
                                placeholder="Enter username"
                            />
                        </div>
                        <div>
                            <label>Password</label>
                            <input
                                type={showPassword ? "text" : "password"}
                                value={password}
                                onChange={e => setPassword(e.target.value)}
                                required
                                placeholder="Enter password"
                            />
                            <div style={{ marginTop: "6px" }}>
                                <input
                                    type="checkbox"
                                    id="showPassword"
                                    checked={showPassword}
                                    onChange={() => setShowPassword(!showPassword)}
                                />
                                <label htmlFor="showPassword" style={{ marginLeft: "6px", fontSize: "14px" }}>
                                    Show Password
                                </label>
                            </div>
                        </div>
                        <button type="submit">Login</button>
                        {error && <p style={{ color: "red" }}>{error}</p>}
                    </form>
                </div>
            </div>
        </div>
    );
};

export default Login;

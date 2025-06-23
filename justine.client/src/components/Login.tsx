import React, { useState } from "react";
// InitiateAuthCommand
import { CognitoIdentityProviderClient, AuthFlowType, AdminInitiateAuthCommand } from "@aws-sdk/client-cognito-identity-provider";
import { useNavigate } from "react-router-dom";
import { ToastContainer, toast, Bounce } from 'react-toastify';
import './css/login.css';

const REGION = "us-east-1"; 
//const POOL_CLIENT_ID = "2kj2hsbqq2p69511uhqo63et24"; 
const USER_POOL_ID = "us-east-1_pi7FVAqX0"; // Replace with your actual User Pool ID
const APP_CLIENT_ID = "2kj2hsbqq2p69511uhqo63et24"; // Replace with your actual App Client ID
// Created Justine and my email
const SECRET_HASH = "nouo5umilaf0n5pp8dfhff0ecp2m3kpkta0mr6ciikvohqjsb4a"; // Replace with your actual secret hash if needed
const Login: React.FC = () => {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [showPassword, setShowPassword] = useState(false);
    const navigate = useNavigate();

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        const client = new CognitoIdentityProviderClient({ region: REGION });
        //const input = {
        //    AuthFlow: AuthFlowType.USER_PASSWORD_AUTH,
        //    ClientId: POOL_CLIENT_ID,

        //    AuthParameters: {
        //        USERNAME: username,
        //        PASSWORD: password
                
        //    },
        //};

        const params = {
            AuthFlow: AuthFlowType.USER_PASSWORD_AUTH, 
            ClientId: APP_CLIENT_ID,
            UserPoolId: USER_POOL_ID,
            AuthParameters: {
                USERNAME: "Justine",
                PASSWORD: "your-password", // For password-based flows
                SECRET_HASH: SECRET_HASH
            },
        };

        // I must set up
        // 1. A domain: justine-developer.net
        // 2. Use route53

        // 3. set up SES: Email service: Need domain
        // 4. Then I configure my user pool to send an email

        try {
            const command = new AdminInitiateAuthCommand(params);
            //const command = new InitiateAuthCommand(input);
            const response = await client.send(command);

            // are we using a token?
            if (response.AuthenticationResult && response.AuthenticationResult.AccessToken) {
                // Optionally store tokens in localStorage/sessionStorage
                // localStorage.setItem("accessToken", response.AuthenticationResult.AccessToken);
                console.log("Login successful:", JSON.stringify(response));
                // might not be able to do this toast b/c jof the navigate below
                toast.info('?? Success Logging in...', {
                    position: "top-center",
                    autoClose: 5000,
                    hideProgressBar: false,
                    closeOnClick: false,
                    pauseOnHover: true,
                    draggable: true,
                    progress: undefined,
                    theme: "light",
                    transition: Bounce,
                });
                navigate("/productlist");
            } else {
                console.log("Login failed. Please check your credentials.", JSON.stringify(response));
                toast.info('?? Success Logging in...', {
                    position: "top-center",
                    autoClose: 5000,
                    hideProgressBar: false,
                    closeOnClick: false,
                    pauseOnHover: true,
                    draggable: true,
                    progress: undefined,
                    theme: "light",
                    transition: Bounce,
                });
                setError("Login failed. Please check your credentials.");
            }
        } catch (err: unknown) {
            if (err && typeof err === "object" && "message" in err
            ) {
                setError((err as { message: string }).message);
            } else {
                setError("Login failed.");
            }
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
